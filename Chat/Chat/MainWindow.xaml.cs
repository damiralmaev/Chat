using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Chat
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Btconnect_Click(object sender, RoutedEventArgs e)//для подключение
        {
            if (string.IsNullOrWhiteSpace(tbip.Text))
            {
                MessageBox.Show("Ведите ip адрес сервера!", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                tbip.Text = "";
            }
            else if (string.IsNullOrWhiteSpace(tbport.Text))
            {
                MessageBox.Show("Ведите порт сервера!", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                tbport.Text = "";
            }
            else if (string.IsNullOrWhiteSpace(tbnick.Text))
            {
                MessageBox.Show("Ведите свой ник!", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                tbnick.Text = "";
            }
            else
            {
                await Task.Run(() =>
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {

                        int port = 0;
                        try { port = Convert.ToInt32(tbport.Text); }
                        catch { MessageBox.Show("Ведите нормальный порт!", Title, MessageBoxButton.OK, MessageBoxImage.Error); }

                        client.Connect(new IPEndPoint(IPAddress.Parse(tbip.Text), port));
                        client.Send(Encoding.UTF8.GetBytes(tbnick.Text));
                        Task.Delay(30);
                        if (client.Connected == false)
                        {
                            MessageBox.Show("Ошибка одинаковый ник!", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            Thread thread = new Thread(new ThreadStart(UpdateMessages));
                            thread.Start();
                            btsend.IsEnabled = true;
                            btconnect.IsEnabled = false;
                        }
                    }));
                });
            }
        }

        void UpdateMessages()//Обновление сообщений
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                Task.Delay(10).Wait();

                int i = client.Receive(buffer);
                this.Dispatcher.Invoke(new Action(() => { tbchat.Text += $"{Encoding.UTF8.GetString(buffer, 0, i)}{Environment.NewLine}"; }));
            }
        }

        private void Btsend_Click(object sender, RoutedEventArgs e)//Отправить сообщение
        {
            client.Send(Encoding.UTF8.GetBytes(tbsend.Text));
            tbsend.Text = "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Tbsend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Btsend_Click(btsend, null);
            }
        }

        private void Btabout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }
    }
}
