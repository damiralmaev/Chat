using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace server
{
    class Program
    {
        static Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static List<string> nicked = new List<string>();
        static List<Socket> clients = new List<Socket>();

        static void Main(string[] args)
        {
            server.Bind(new IPEndPoint(IPAddress.Any, 908));
            server.Listen(100);
            Thread thread = new Thread(new ThreadStart(NewConnect));
            thread.Start();

            Console.WriteLine("Сервер работает!");

            while (true)
                Console.ReadLine();
        }

        static void UpdateMessages(string mess)
        {
            foreach (Socket client in clients)
            {
                try
                {
                    client.Send(Encoding.UTF8.GetBytes(mess));
                }
                catch { }
            }
        }

        static void Message(object i)
        {
            object[] ii = (object[])i;
            string nick = (string)ii[0];
            Socket client = (Socket)ii[1];
            int messi;
            byte[] buffer = new byte[1024];
            string mess = "";
            bool clientconnect = true;

            while (clientconnect)
            {
                Task.Delay(10).Wait();

                try
                {
                    messi = client.Receive(buffer);
                    mess = Encoding.UTF8.GetString(buffer, 0, messi);
                    mess = $"Новое сообщение {nick}: {mess}";
                    UpdateMessages(mess);
                    Console.WriteLine(mess);
                }
                catch
                {
                    Console.WriteLine($"Ошибка отправка сообщение: {nick}");
                    if (CheckClient(client, nick) == false)
                    {
                        Console.WriteLine($"Клиент отключён: {nick}");
                        UpdateMessages($"Клиент отключён: {nick}");
                        client.Close();
                        nicked.Remove(nick);
                        clients.Remove(client);
                        clientconnect = false;
                    }
                }
            }
        }

        static bool CheckClient(Socket client, string nick)
        {
            if (client.Connected == false)
                return false;

            return true;
        }

        static void CheckNewConnect(object iii)
        {
            int i;
            byte[] buffer = new byte[1024];
            Socket client = (Socket)iii;
            string nick;
            bool yes = true;

            try
            {
                i = client.Receive(buffer);
                nick = Encoding.UTF8.GetString(buffer, 0, i);

                foreach (string nicks in nicked)
                {
                    if (nick == nicks)
                        yes = false;
                }

                if (yes == false)
                {
                    client.Close();
                    Console.WriteLine($"Ошибка одинаковый ник! Ник: {nick}");
                }
                else
                {

                    Console.WriteLine($"Новое подключение! Ник: {nick}");
                    UpdateMessages($"Новое подключение! Ник: {nick}");

                    object[] ii = { nick, client };
                    nicked.Add(nick);
                    clients.Add(client);

                    Thread thread = new Thread(new ParameterizedThreadStart(Message));
                    thread.Start(ii);
                }
            }
            catch
            {
                Console.WriteLine("Ошибка нового подключение");
            }

            Thread.Sleep(0);
        }

        static void NewConnect()
        {
            while (true)
            {
                Task.Delay(10).Wait();

                try
                {
                    Socket client = server.Accept();
                    Thread th = new Thread(new ParameterizedThreadStart(CheckNewConnect));
                    th.Start(client);
                }
                catch { Console.WriteLine("Ошибка нового подключение"); }
            }
        }
    }
}
