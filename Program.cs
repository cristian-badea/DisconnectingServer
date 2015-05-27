using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace DisconnectingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = args.Length > 0 ? int.Parse(args[0]) : 1234;
            var socket = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            try
            {
                socket.Start();
                while (true)
                {
                    var client = socket.AcceptTcpClient();
                    var worker = new BackgroundWorker();
                    worker.DoWork += (sender, eventArgs) =>
                    {
                        using (client)
                        {
                            var stream = client.GetStream();
                            var reader = new StreamReader(stream);
                            var line = reader.ReadLine();
                            Console.WriteLine("Received: {0}", line);
                        }
                    };
                    worker.RunWorkerAsync();
                }
            }
            finally
            {
                socket.Stop();
            }
        }
    }
}
