using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;
using System.Threading;
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
using TCPCSharpChat;

namespace TCPWPFTest
{
    
    public partial class MainContent : Page
    {

        private const string host = "127.0.0.1";        //переменная ip адресса
        private const int port = 55555;                 //переменная порта
        static TcpClient client;                        //переменная клиента
        static NetworkStream stream;                    //переменная потока

        public string current = "";         //переменая для имени

        public MainContent()
        {

            InitializeComponent();
            read4jsonName();
            label_UserNameMain.Text = current;
            start();

        }

        public void read4jsonName()            //функция чтения json файла имени
        {
            string json = File.ReadAllText("user.json");
            User users = JsonConvert.DeserializeObject<User>(json);
            current = users.CurrentUser;
        }

        private void start()
        {
            string userName = current;
            client = new TcpClient();
            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(receiveMessage));
                receiveThread.Start(); //старт потока
                send_notification($"{userName} присоеденился к чату");

            }
            catch (ObjectDisposedException ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            
        }

        private void receiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);
                    if (!(tb_read_message.Dispatcher.CheckAccess()))
                    {
                        tb_read_message.Dispatcher.BeginInvoke(new Action(delegate () { tb_read_message.AppendText(message); }));
                    }
                    else 
                    {
                        tb_read_message.AppendText(message);//вывод сообщения
                    }

                }
                catch (ObjectDisposedException ex)
                {
                    MessageBox.Show(ex.Message.ToString()); //соединение было прервано
                    disconnect();
                }
            }
        }

        private void disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }

        private void btn_send_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                string message = tb_editMessage.Text.ToString();
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

        public void send_notification(string notification) 
        {
            string message = notification;
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

    }
}
