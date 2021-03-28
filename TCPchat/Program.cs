using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPchat
{
    class Program
    {
        static int port = 8005;
        static List<Socket> clients = new List<Socket>();
        private static List<string> nicknames = new List<string>();
        static void Main(string[] args)
        {
            
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"),port);
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listenSocket.Bind(ipEndPoint);
                listenSocket.Listen(10);
                Console.WriteLine("Сервер запущен. Ожидаю подключения...");
                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    Console.WriteLine($"{handler.RemoteEndPoint} только что подключился.");

                    string nick_required = "NICK";
                    byte[] data = Encoding.Unicode.GetBytes(nick_required);
                    handler.Send(data);
                    
                    StringBuilder nick = new StringBuilder();
                    int bytes = 0;

                    byte[] nick_b = new byte[256];

                    do
                    {
                        bytes = handler.Receive(nick_b);
                        nick.Append(Encoding.Unicode.GetString(nick_b, 0, bytes));

                    } while (handler.Available>0);
                    
                    clients.Add(handler);
                    nicknames.Add(nick.ToString());
                    
                    Thread myThread = new Thread(new ParameterizedThreadStart(Handler));
                    myThread.Start(handler);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                
            }

            Console.Read();

        }

        static void Handler(object client)
        {
            Socket handler = (Socket) client;
            Console.WriteLine(clients.Count);
            try
            {
                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;

                    byte[] data = new byte[256];
                    builder.Append(nicknames[clients.IndexOf(handler)]);

                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));

                    } while (handler.Available>0);
                    
                    
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());
                    Broadcast(builder.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Поток закрывся");
                nicknames.RemoveAt(clients.IndexOf(handler));
                clients.Remove(handler);
                
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                return;
            }
            
        }

        static void Broadcast(string message)
        {
            
            foreach (var client in clients)
            {
                string message_client = $"{client.RemoteEndPoint}: {message}";
                byte[] data = Encoding.Unicode.GetBytes(message_client);
                client.Send(data);
            }
        }

        
    }
}