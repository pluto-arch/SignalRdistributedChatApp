using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("请输入用户名称后回车：");
            var userName=Console.ReadLine();
            Console.Write("请输入要连接的服务器端口：");
            var port=Console.ReadLine();

            var connection = new HubConnectionBuilder()
                .WithUrl($"http://localhost:{port}/hubs/chat", options =>
                {
                    options.AccessTokenProvider = () => GetToken(userName, port);
                })
                .WithAutomaticReconnect()
                .Build();
            connection.Closed += async (error) =>
            {
                Console.WriteLine($"连接关闭：{error}。 稍后将重试");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            connection.On<string,string>("message", (name,message) =>
            {
                Console.WriteLine($"[{name}]说：{message}");
            });

            await connection.StartAsync();
            Console.WriteLine("连接成功");
            Console.Read();
        }

        private static readonly HttpClient HttpClient = new();

        private static async Task<string> GetToken(string username,string port)
        {
            var url = $"http://localhost:{port}/api/account";
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(username), "userName");
            content.Add(new StringContent("admin"), "password");
            var response = await HttpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<Rootobject>(res);
                return model.data.token;
            }
            throw new InvalidOperationException("获取token失败");
        }
    }



    public class Rootobject
    {
        public Data data { get; set; }
        public int code { get; set; }
        public string message { get; set; }
    }

    public class Data
    {
        public string token { get; set; }
    }


}