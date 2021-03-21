using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;


namespace Server
{
    class Program
    {
        private static TcpListener listener;
        private const int serverPort = 2365;
        private static bool run;
        private static int step;
        private static int gameCount;
        private static bool isGaming = true;
        private static TcpClient client;
        private static NetworkStream stream;
        private static StreamReader reader;
        private static StreamWriter writer;
        private static int iteration = 0;
        private static int clientStep;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Server start");
            listener = new TcpListener(IPAddress.Any, serverPort);
            run = true;
            await Listen();
        }

        private static async Task Listen()
        {
            listener.Start();
            client = await listener.AcceptTcpClientAsync();
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            do
            {
                string fullmessage = await reader.ReadLineAsync();
                if (fullmessage != null)
                {
                    string[] listMessage = fullmessage.Split(",");
                    if (int.TryParse(listMessage[0], out gameCount) && int.TryParse(listMessage[1], out step))
                    {
                        break;
                    }
                }
            } while (gameCount <= step);
            await BasheAI();
        }

        private static async Task BasheAI()
        {
            while (isGaming)
            {
                if (gameCount > 0)
                {
                    var serverRemoveCount = CheckStatus();
                    gameCount -= serverRemoveCount;
                    if (gameCount < 1)
                    {
                        writer = new StreamWriter(stream);
                        await writer.WriteLineAsync("Server is win!");
                        await writer.FlushAsync();
                        isGaming = false;
                        break;
                    }
                    var str = "Server choice is " + serverRemoveCount + " Left count " + gameCount; 
                    await writer.WriteLineAsync(str);
                    await writer.FlushAsync();
                    if (int.TryParse(await reader.ReadLineAsync(), out clientStep))
                    {
                        gameCount -= clientStep;
                        if (gameCount < 1)
                        {
                            writer = new StreamWriter(stream);
                            await writer.WriteLineAsync("Client is win!");
                            await writer.FlushAsync();
                            isGaming = false;
                            break;
                        }
                    }
                }
            }
        }
        private static int CheckStatus()
        {
            Random rnd = new Random();
            if (iteration == 0)
            {
                var needForWin = gameCount - ((gameCount / (step + 1)) * (step + 1));
                iteration += 1;
                if (needForWin == 0)
                {
                    return rnd.Next(1, (int) step);
                }
                else
                {
                    return needForWin;
                }
            }
            else
            {
                return (step + 1) - clientStep;
            }
        }
    }
}