using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using TCPCSharpChat;

namespace TCPWPFTest
{
    /// <summary>
    /// Логика взаимодействия для PageHistory.xaml
    /// </summary>
    public partial class PageHistory : Page
    {
        public static string current = "";         //переменая для имени
        public static string oldname = "";          //переменная старого имени
        public static string status = "";                   // переменая статуса
        public PageHistory()
        {
            InitializeComponent();
            read4jsonName();
            if (System.IO.Directory.Exists("messages"))         //проверка существования папки с сообщениями
            {
                if (File.Exists($"messages/{current}.txt"))
                {
                    string messages = File.ReadAllText($"messages/{current}.txt");      //чтение сообщений
                    tb_readMessage.Text = messages; //заполнение
                    tb_readMessage.ScrollToEnd();
                }
            }
            else if (!(System.IO.Directory.Exists("messages")))
            {
                Directory.CreateDirectory("messages");
            }
            tb_readMessage.ScrollToEnd();


        }
        public static void read4jsonName()            //функция чтения json файла имени
        {
            string json = File.ReadAllText("user.json");
            User users = JsonConvert.DeserializeObject<User>(json);
            current = users.CurrentUser;
            oldname = users.OldName;
            status = users.Status;
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e) //возвращение в меню настроек
        {
            if (NavigationService.CanGoBack == true)
            {
                NavigationService.GoBack(); 
            }
        }

    }
}
