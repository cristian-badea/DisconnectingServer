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
            StartPolicyServer();

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

        private static void StartPolicyServer()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += WorkerOnDoWork;
            worker.RunWorkerAsync();
        }

        private static void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            var socket = new TcpListener(new IPEndPoint(IPAddress.Any, 943));
            try
            {
                socket.Start();
                while (true)
                {
                    using (var client = socket.AcceptTcpClient())
                    {
                        var stream = client.GetStream();

                        var reader = new StreamReader(stream);
                        var request = reader.ReadLine();
                        Console.WriteLine("Got request on 943: {0}", request);

                        var writer = new StreamWriter(stream);
                        writer.Write(@"<?xml version=""1.0"" encoding =""utf-8""?>
<access-policy>
  <cross-domain-access>
    <policy>
      <allow-from>
        <domain uri=""*"" />
      </allow-from>
      <grant-to>
        <socket-resource port=""4500-4599"" protocol=""tcp"" />
      </grant-to>
    </policy>
  </cross-domain-access>
</access-policy>");
                        writer.Flush();
                    }

                }
            }
            finally
            {
                socket.Stop();
            }
        }
    }
}
