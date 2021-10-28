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
using System.Windows.Media.Effects;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.Uwp.Notifications;

namespace TCPWPFTest
{
    

    public partial class MainContent : Page
    {

        public string host = "";        //переменная ip адресса
        public int port = 0;                 //переменная порта
        static TcpClient client;                        //переменная клиента
        static NetworkStream stream;                    //переменная потока

        public static string current = "";         //переменая для имени
        public static string oldname = "";
        public static string status = "";

        public MainContent()
        {

            InitializeComponent();
            read4jsonName();
            changeName();

            if (!(File.Exists("connection.json")))
            {
                Connecting conn = new Connecting();
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };

                conn.ip = "127.0.0.1";
                conn.port = 55555;

                string jsonString = System.Text.Json.JsonSerializer.Serialize<Connecting>(conn, options);
                File.WriteAllText("connection.json", jsonString);
            }
            else if (File.Exists("connection.json")) 
            {
                string json = File.ReadAllText("connection.json");
                Connecting conn = JsonConvert.DeserializeObject<Connecting>(json);
                host = conn.ip;
                port = conn.port;
            }

            start();


        }

        public static void read4jsonName()            //функция чтения json файла имени
        {
            string json = File.ReadAllText("user.json");
            User users = JsonConvert.DeserializeObject<User>(json);
            current = users.CurrentUser;
            oldname = users.OldName;
            status = users.Status;
        }

        public static void pcNotification(string message) 
        {
            new ToastContentBuilder()
            .AddArgument("action", "viewConversation")
            .AddArgument("conversationId", 9813)
            .AddText("Andrew sent you a picture")
            .AddText(Regex.Replace(message, "<.*?>", String.Empty))
            .Show();
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
                File.WriteAllText("fail.json", "Траблы");
                MessageBox.Show("Проблемы с соединением, перезапустите приложение");
                disconnect();
                Application.Current.Shutdown();
            }
                
                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(receiveMessage));
                receiveThread.Start(); //старт потока
                string notification_startup = $"<html><head/><body><table width = '100%' cellpadding='3' cellspacing='0'><tr><td valign='center' align = 'center'><font color = '#ffffff'><b>{userName}</b> присоеденился к чату</font></tr></td></table><br></body></html>";
                send_notification(notification_startup);

      
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

                    if (System.IO.Directory.Exists("messages"))
                    {
                        if (File.Exists($"messages/{current}.txt"))
                        {
                            File.AppendAllText($"messages/{current}.txt", message);
                            pcNotification(message);
                        }
                        else 
                        {
                            File.WriteAllText($"messages/{current}.txt", message);
                            pcNotification(message);
                        }
                    }
                    else if (!(System.IO.Directory.Exists("messages")))
                    {
                        Directory.CreateDirectory("messages");
                        File.WriteAllText($"messages/{current}.txt", message);
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

            Canvas.SetZIndex(grid_rename, 2);
            grid_rename.Visibility = Visibility.Visible;
            BlurEffect blur = new BlurEffect();
            blur.Radius = 4;
            grid_MainContent.Effect = blur;

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

        private void btn_setting_Click(object sender, RoutedEventArgs e)
        {

            Canvas.SetZIndex(grid_settings, 2);
            grid_settings.Visibility = Visibility.Visible;
            frame_settings.Source = new Uri("Page_Settings.xaml", UriKind.Relative); 
            BlurEffect blur = new BlurEffect();
            blur.Radius = 4;
            grid_MainContent.Effect = blur;

        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {

             Canvas.SetZIndex(grid_settings, 0);
             grid_settings.Visibility = Visibility.Hidden;
             BlurEffect blur = new BlurEffect();
             blur.Radius = 0;
             grid_MainContent.Effect = blur;
            
        }

        private void tboxNickEventHandler(object sender, TextChangedEventArgs args)
        {
            tboxNewNick.Foreground = Brushes.White;        //исправления цвета текста при ошибке
        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            string nickname = tboxNewNick.Text;        //изьятие из текстбокса
            string[] allFoundFiles = Directory.GetFiles("messages", $"{nickname}.txt");

            if (nickname.Length == 0) { tboxNewNick.Foreground = Brushes.Red; tboxNewNick.ToolTip = "Введите хоть что-нибудь"; }      //проверка данных
            else if (nickname.Length < 3) { tboxNewNick.Foreground = Brushes.Red; tboxNewNick.ToolTip = "Длина ника должна быть больше 3-х символов"; }
            else if (Regex.IsMatch(nickname, @"[\%\/\\\&\?\,\'\;\:\!\-\0-9]+") == true) { tboxNewNick.Foreground = Brushes.Red; tboxNewNick.ToolTip = "Ник имеет недопустимые символы"; }
            else if (allFoundFiles.Any() == true) { tboxNewNick.Foreground = Brushes.Red; tboxNewNick.ToolTip = "Этот ник уже используеться одним из пользователей этого приложения"; }
            else
            {
                rename(nickname);
            }
        }

        private void rename(string nickname) 
        {
            read4jsonName();
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            User user = new User();

            user.CurrentUser = nickname;
            user.OldName = oldname;
            user.Status = status;

            string jsonString = System.Text.Json.JsonSerializer.Serialize<User>(user, options);
            File.WriteAllText("user.json", jsonString);

            string message = $"<html><head/><body><table width = '100%' cellpadding='3' cellspacing='0'><tr><td valign='center' align = 'center'><font color = '#ffffff'><b>{current}</b> изменил имя на <b>{nickname}</b></font></tr></td></table><br></body></html>";
            send_notification(message);
            current = nickname;
            changeName();

            tboxNewNick.Text = "";

            Canvas.SetZIndex(grid_rename, 0);
            grid_rename.Visibility = Visibility.Hidden;
            BlurEffect blur = new BlurEffect();
            blur.Radius = 0;
            grid_MainContent.Effect = blur;

        }

        private void btnRenameCancel_Click(object sender, RoutedEventArgs e)
        {
            tboxNewNick.Text = "";
            Canvas.SetZIndex(grid_rename, 0);
            grid_rename.Visibility = Visibility.Hidden;
            BlurEffect blur = new BlurEffect();
            blur.Radius = 0;
            grid_MainContent.Effect = blur;
        }
    }
}
