using System;
using System.IO;
using System.Net.Sockets;

namespace Pixa.Soundbridge.Shell
{
    static class Module1
    {
        private static TcpClient _client;

        public static void Main(string[] args)
        {
            _client = new TcpClient(args[0], 5555);
            var d = new Action(ReadCon);
            d.BeginInvoke(null, null);
            string input;
            var sw = new StreamWriter(_client.GetStream());
            do
            {
                input = Console.ReadLine();
                if (input == "exit")
                    return;
                sw.Write(input + "\r\n");
                sw.Flush();
                if (!_client.Connected)
                    return;
            }
            while (true);
        }

        public static void ReadCon()
        {
            var sr = new StreamReader(_client.GetStream());
            while (_client.Connected)
                Console.WriteLine(sr.ReadLine());
        }
    }
}