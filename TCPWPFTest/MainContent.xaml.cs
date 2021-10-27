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
using System.Text.Json;
using System.Text.Encodings.Web;

namespace TCPWPFTest
{
    
    public partial class MainContent : Page
    {

        private const string host = "127.0.0.1";        //переменная ip адресса
        private const int port = 55555;                 //переменная порта
        static TcpClient client;                        //переменная клиента
        static NetworkStream stream;                    //переменная потока

        public static string current = "";         //переменая для имени

        public MainContent()
        {

            InitializeComponent();
            read4jsonName();
            changeName();
            start();

        }

        public static void read4jsonName()            //функция чтения json файла имени
        {
            string json = File.ReadAllText("user.json");
            User users = JsonConvert.DeserializeObject<User>(json);
            current = users.CurrentUser;
        }

        public void changeName()            //функция чтения json файла имени
        {
            label_UserNameMain.Text = current;
        }


        private void start()
        {
            string userName = current;
            client = new TcpClient();

            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток
            }
            catch //окно настройки соедиения
            {
            
            }
                /*string message = userName;
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);*/

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(receiveMessage));
                receiveThread.Start(); //старт потока
                send_notification($"<html><head/><body><table width = '100%' cellpadding='3' cellspacing='0'><tr><td valign='center' align = 'center'><font color = '#ffffff'><b>{userName}</b> присоеденился к чату</font></tr></td></table><br></body></html>");

      
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
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();

                    if (!(tb_readMessage.Dispatcher.CheckAccess()))
                    {
                        tb_readMessage.Dispatcher.BeginInvoke(new Action(delegate () { tb_readMessage.AppendText(message); }));
                    }
                    else 
                    {
                        tb_readMessage.AppendText(message);//вывод сообщения
                    }

                }
                catch
                {
                    disconnect();
                }
            }
        }

        public static void disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }

        public void btn_send_Click(object sender, ExecutedRoutedEventArgs e)
        {
            string messageText = tb_editMessage.Text.ToString();

            if (messageText != "") 
            {
                string message = $"<table width = '100%' cellpadding='3' cellspacing='0'><tr><td valign='top' bgcolor='#5c4200' width = 86%><b><font color = #bda980>{current}</b></font></td><td valign='top' bgcolor='#5c4200' width = 14% align = 'right'><font size='5px' color = #bda980>{DateTime.Now.ToString("HH:mm dd.MM.yy")}</td></font><tr><td class = 'message' valign='top' bgcolor='#5c4200' width = 100% colspan= '2'>{messageText}</td></tr></table><br>"; // шаблон для сообщения

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                tb_read_message.ScrollToEnd();
                tb_editMessage.Text = "";
            }

        }

        public static void send_notification(string notification) 
        {
            string message = notification;
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        private void btn_nick_mainContent_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("CMUser") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

        private void btn_rename(object sender, RoutedEventArgs e)
        {

        }

        private void btn_signout(object sender, RoutedEventArgs e)
        {

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            User user = new User();       

            user.OldName = current;
            user.CurrentUser = null;
            user.Status = "Out";

            string jsonString = System.Text.Json.JsonSerializer.Serialize<User>(user, options);
            File.WriteAllText("user.json", jsonString);

            tb_readMessage.Text = "";

            send_notification($"<html><head/><body><table width = '100%' cellpadding='3' cellspacing='0'><tr><td valign='center' align = 'center'><font color = '#ffffff'><b>{current}</b> покинул чат</font></tr></td></table><br></body></html>");

            disconnect();

            Application.Current.Shutdown();
        }

        
    }
}
