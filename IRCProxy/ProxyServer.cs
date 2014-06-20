using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace IRCProxy
{
    class ProxyServer
    {
        public string TargetServer { get; protected set; }
        public int TargetPort { get; protected set; }
        public int MyPort { get; protected set; }

        public ProxyServer(int my_port, string target_server, int target_port)
        {
            MyPort = my_port;
            TargetServer = target_server;
            TargetPort = target_port;

        }

        /// <summary>
        /// input stream에 있는 문자열을 output stream에 맞도록 변환하여 전달한다
        /// 만약 stream이 연결이 끊켰을 경우 false를 반환한다
        /// 에러가 난 경우 무시한다.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="input_codepage"></param>
        /// <param name="output_codepage"></param>
        /// <returns></returns>
        protected async Task<bool> ConvertStream(NetworkStream input, NetworkStream output, int input_codepage, int output_codepage)
        {
            try
            {
                byte[] buf = new byte[8196];

                int read_bytes = await input.ReadAsync(buf, 0, 8196);

                if ( read_bytes == 0 )
                {
                    return false;
                }

                string converted_string = Encoding.GetEncoding(input_codepage).GetString(buf, 0, read_bytes);
                byte[] converted_buf = Encoding.GetEncoding(output_codepage).GetBytes(converted_string);
                await output.WriteAsync(converted_buf, 0, converted_buf.Count());

                return true;
            }
            catch(Exception e)
            {
                // 인코딩 변환 실패는 에러만 출력하고 그냥 무시한다.
                Console.WriteLine("ConvertStream Fail: " + e.Message);

                return true;
            }

        }
        protected async Task ClientHandler(TcpClient client, params object[] args)
        {
            TcpClient server = null;
            try
            {
                using (NetworkStream client_stream = client.GetStream())
                {
                    server = new TcpClient();
                    await server.ConnectAsync(TargetServer, TargetPort);

                    using (NetworkStream server_stream = server.GetStream())
                    {
                        while (true)
                        {
                            if (server.Client.Poll(30, SelectMode.SelectRead))
                            {
                                if ( await ConvertStream(server_stream, client_stream, 949, 65001) == false )
                                {
                                    throw new Exception("disconnected");
                                }
                            }

                            if (client.Client.Poll(30, SelectMode.SelectRead))
                            {
                                if ( await ConvertStream(client_stream, server_stream, 65001, 949) == false )
                                {
                                    throw new Exception("disconnected");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("clean up");
                client.Close();
                if (server != null)
                {
                    server.Close();
                }
            }
        }
        protected async Task ListenJob()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, MyPort);
            listener.Start();

            while (true)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Task client_task = ClientHandler(client);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }
        public void RunForever()
        {
            ListenJob().Wait();
        }
    }
}
