using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private const int serverPort = 2365;
        private static TcpClient client;
        private static string fullmessage;
        private static bool isRunning = true;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Tcp User start");
            var gameCount = 0;
            var maxStep = 0;
            var count = "";
            var step = "";
            do
            {
                Console.Write("Enter game count: ");
                count = Console.ReadLine();
                Console.Write("Enter max step: ");
                step = Console.ReadLine();
                if (int.TryParse(count, out gameCount) && int.TryParse(step, out maxStep) && gameCount > maxStep)
                {
                    fullmessage = count + ',' + step;
                }
                else
                {
                    Console.WriteLine("Input correct numbers");
                }
            } while (!int.TryParse(count, out gameCount) || !int.TryParse(step, out maxStep) || gameCount <= maxStep);
            client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, serverPort);
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream);
            await writer.WriteLineAsync(fullmessage);
            await writer.FlushAsync();
            string answer = await reader.ReadLineAsync();
            Console.WriteLine(answer);
            while (isRunning)
            {
                var nowStep = "";
                var stepClient = 0;
                do
                {
                    Console.Write("Enter your step: ");
                    nowStep = Console.ReadLine();
                    if (int.TryParse(nowStep, out stepClient) && (stepClient > 0 && stepClient <= maxStep))
                    {
                        Console.WriteLine("OK");
                    }
                    else
                    {
                        Console.WriteLine("Please, input correct step");
                    }
                } while (!int.TryParse(nowStep, out stepClient) || stepClient <= 0 || stepClient > maxStep);
                await writer.WriteLineAsync(nowStep);
                await writer.FlushAsync();
                answer = await reader.ReadLineAsync();
                if (answer == "Server is win!" || answer == "Client is win!")
                {
                    isRunning = false;
                }
                Console.WriteLine(answer);
            }
            client.Close();
        }
    }
}