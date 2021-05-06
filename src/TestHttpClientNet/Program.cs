using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace TestHttpClientNet
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Testing static HttpClientFactory");
                var staticFactoryHandlersCount = await CountMessageHandlers(() => HttpClientFactory.Create());
                Console.WriteLine($"Used HttpMessageHandlers count: {staticFactoryHandlersCount}");

                Console.WriteLine("Testing Asp.Net Core IHttpClientFactory");
                var services = new ServiceCollection().AddHttpClient().BuildServiceProvider();
                var aspNetFactoryHandlersCount = await CountMessageHandlers(() => services.GetRequiredService<IHttpClientFactory>().CreateClient());
                Console.WriteLine($"Used HttpMessageHandlers count: {aspNetFactoryHandlersCount}");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            Console.ReadLine();
        }

        private static async Task<int> CountMessageHandlers(Func<HttpClient> createClient)
        {
            var messageHandlers = new HashSet<HttpMessageHandler>();

            for (int i = 0; i < 10; i++)
            {
                var client = createClient();
                await client.GetAsync("http://www.example.com/");
                await Task.Delay(100);
                messageHandlers.Add(GetHandler(client));
            }

            return messageHandlers.Count;
        }

        private static FieldInfo HandlerField = typeof(HttpMessageInvoker).GetField("handler", BindingFlags.NonPublic | BindingFlags.Instance);

        private static HttpMessageHandler GetHandler(HttpClient client)
        {
            return (HttpMessageHandler)HandlerField.GetValue(client);
        }
    }
}
