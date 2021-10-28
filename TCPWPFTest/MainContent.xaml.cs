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
using System.Management;

namespace TCPWPFTest
{
    

    public partial class MainContent : Page
    {

        public string host = "";        //переменная ip адресса
        public int port = 0;                 //переменная порта
        static TcpClient client;                        //переменная клиента
        static NetworkStream stream;                    //переменная потока

        public static string current = "";         //переменая для имени
        public static string oldname = "";          //переменая старого имени
        public static string status = "";           //переменая статуса (Войдён, не войдён)

        public static string focus = "true";        // переменая фокуса на окне или этой странице

        public MainContent()
        {

            InitializeComponent();      
            read4jsonName();                        //чтения данных из джейсона
            changeName();                           //измеение ника в нижнем левом углу

            if (!(File.Exists("connection.json")))  //если не существует файла с настройками соединения создаёться новый с настройками по умолчанию
            {
                Connecting conn = new Connecting();
                var options = new JsonSerializerOptions         //настройка джейсона для utf-8
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };

                conn.ip = "127.0.0.1";
                conn.port = 55555;

                string jsonString = System.Text.Json.JsonSerializer.Serialize<Connecting>(conn, options);
                File.WriteAllText("connection.json", jsonString);

                string json = File.ReadAllText("connection.json");
                Connecting con = JsonConvert.DeserializeObject<Connecting>(json);
                host = con.ip;
                port = con.port;

            }
            else if (File.Exists("connection.json")) //иначе читает файл
            {
                string json = File.ReadAllText("connection.json");
                Connecting conn = JsonConvert.DeserializeObject<Connecting>(json);
                host = conn.ip;
                port = conn.port;
            }

            start(); // основная функция


        }

        public static void read4jsonName()            //функция чтения json файла имени
        {
            string json = File.ReadAllText("user.json");
            User users = JsonConvert.DeserializeObject<User>(json);
            current = users.CurrentUser;
            oldname = users.OldName;
            status = users.Status;
        }

        public static void pcNotification(string message)       //функция уведамлений windows 10 
        {
            string platform = Environment.OSVersion.Platform.ToString();

            if (focus == "False" & platform == "Win32NT")
            {
                new ToastContentBuilder()
                    .AddText("Чат")
                    .AddText("У вас есть сообщения")
                    .Show();
            }
        }

        public void changeName()            //функция изменения плашки имени
        {
            label_UserNameMain.Text = current;
        }


        private void start()
        {
            string userName = current;      //переменая имени
            client = new TcpClient();

            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток
            }
            catch //окно настройки соедиения если подключиться не получилось
            {
                File.WriteAllText("fail.json", "Траблы");   //создание файла, чтобы при перезагрузке открыть страницу с настройками соединения
                MessageBox.Show("Проблемы с соединением, перезапустите приложение");
                disconnect();
                Application.Current.Shutdown();
            }
                
                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(receiveMessage));
                receiveThread.Start(); //старт потока
                string notification_startup = $"<html><head/><body><table width = '100%' cellpadding='3' cellspacing='0'><tr><td valign='center' align = 'center'><font color = '#ffffff'><b>{userName}</b> присоеденился к чату</font></tr></td></table><br></body></html>";
                send_notification(notification_startup);    //функция "внутренних уведомлений"

      
        }

        private void receiveMessage()       //функция получения сообщений (другой поток)
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

                    if (!(tb_readMessage.Dispatcher.CheckAccess()))     //проверка доступа к объекту из другого потока
                    {
                        tb_readMessage.Dispatcher.BeginInvoke(new Action(delegate () { tb_readMessage.AppendText(message); })); //получение доступа к элементу из другого потока
                    }
                    else 
                    {
                        tb_readMessage.AppendText(message);//вывод сообщения
                    }

                    if (System.IO.Directory.Exists("messages"))     //запись сообщений в файл
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
                        pcNotification(message);
                    }

                }
                catch
                {
                    disconnect();
                }
            }
        }

        public static void disconnect() //функция отключения от сервера
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }

        public void btn_send_Click(object sender, ExecutedRoutedEventArgs e)   //отправка сообщения
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

        public static void send_notification(string notification)       // отправка "внутреннего уведомления"
        {
            string message = notification;
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        private void btn_nick_mainContent_Click(object sender, RoutedEventArgs e)   //кнопка настроек пользователя
        {
            ContextMenu cm = this.FindResource("CMUser") as ContextMenu;    //закрепление контекстного меню
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

        private void btn_rename(object sender, RoutedEventArgs e)       //открытие окна переименования
        {

            Canvas.SetZIndex(grid_rename, 2);               //перенос на передний план
            grid_rename.Visibility = Visibility.Visible;        
            BlurEffect blur = new BlurEffect();         //создание эффекта блюр на "фон"
            blur.Radius = 6;
            grid_MainContent.Effect = blur;             //применение эффекта

        }



        private void btn_signout(object sender, RoutedEventArgs e)      //кнопка выхода
        {

            var options = new JsonSerializerOptions         //обновление данных в json 
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

        private void btn_setting_Click(object sender, RoutedEventArgs e)        //открытие окна настроек 
        {

            Canvas.SetZIndex(grid_settings, 2);
            grid_settings.Visibility = Visibility.Visible;
            frame_settings.Source = new Uri("Page_Settings.xaml", UriKind.Relative); 
            BlurEffect blur = new BlurEffect();
            blur.Radius = 6;
            grid_MainContent.Effect = blur;

        }

        private void btn_close_Click(object sender, RoutedEventArgs e)          //кнопка закрытия окна настроек
        {

             Canvas.SetZIndex(grid_settings, 0);                //перенос на задний план
             grid_settings.Visibility = Visibility.Hidden;      //скрытие окна
             BlurEffect blur = new BlurEffect();
             blur.Radius = 0;                                   //убирание блюра
             grid_MainContent.Effect = blur;                    //применение эффекта
            
        }



        private void btn_close_about_Click(object sender, RoutedEventArgs e)        // закрытие окна "о программе"
        {

            Canvas.SetZIndex(grid_about, 0);
            grid_about.Visibility = Visibility.Hidden;
            BlurEffect blur = new BlurEffect();
            blur.Radius = 0;
            grid_MainContent.Effect = blur;

        }

        private void tboxNickEventHandler(object sender, TextChangedEventArgs args)     //испровление цвета шрифта в окне переименования
        {
            tboxNewNick.Foreground = Brushes.White;        //исправления цвета текста при ошибке
        }

        private void btnRename_Click(object sender, RoutedEventArgs e)          //кнопка переименовать в окне переименования
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

        private void rename(string nickname)    //функция изменения имени
        {
            read4jsonName();        //чтение текущих данных
            var options = new JsonSerializerOptions     //их исправление
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

            File.Move($"messages/{current}.txt", $"messages/{nickname}.txt");  //переименование файла с историей сообщений

            string message = $"<html><head/><body><table width = '100%' cellpadding='3' cellspacing='0'><tr><td valign='center' align = 'center'><font color = '#ffffff'><b>{current}</b> изменил имя на <b>{nickname}</b></font></tr></td></table><br></body></html>";
            send_notification(message);     //уведомление о изменении имени
            current = nickname;         //обновление данных
            changeName();               //изменение имени в меню

            tboxNewNick.Text = "";

            Canvas.SetZIndex(grid_rename, 0);       // скрытие окна
            grid_rename.Visibility = Visibility.Hidden;
            BlurEffect blur = new BlurEffect();
            blur.Radius = 0;
            grid_MainContent.Effect = blur;

        }

        private void btnRenameCancel_Click(object sender, RoutedEventArgs e)  //кнопка закрытия окна переименования
        {
            tboxNewNick.Text = "";
            Canvas.SetZIndex(grid_rename, 0);
            grid_rename.Visibility = Visibility.Hidden;
            BlurEffect blur = new BlurEffect();
            blur.Radius = 0;
            grid_MainContent.Effect = blur;
        }

        private void Page_LostFocus(object sender, RoutedEventArgs e)       //событие потери фокуса на странице
        {
            focus = "False";
        }

        private void Page_GotFocus(object sender, RoutedEventArgs e)        //событие появления фокуса на странице
        {
            focus = "True";
        }

        private void btn_About_Click(object sender, RoutedEventArgs e)      //открытие окна "о программе"
        {
            Canvas.SetZIndex(grid_about, 2);
            grid_about.Visibility = Visibility.Visible;
            BlurEffect blur = new BlurEffect();
            blur.Radius = 6;
            grid_MainContent.Effect = blur;
        }
    }
}
