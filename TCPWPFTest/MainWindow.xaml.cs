using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string current = "";     //переменая имени
        public string oldname = "";     //переменная старого имени
        public string status = "";          // переменная статуса

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists("fail.json"))       //если есть файл с ошибкой запуск страницы с настройкой соединения
            {
                MainFrame.Navigate(new PageConnectFull());
                File.Delete("fail.json");               //удаление файла
            }

            else if (File.Exists("user.json"))      //проверка существования файла пользователя
            {
                readjson();                        // чтение данных
                if (status == "In")
                {
                    MainFrame.Navigate(new MainContent());   //открытие главного окна 
                }
                else if (status == "Out") 
                {
                    MainFrame.Navigate(new PageSignIn());   //открытие окна для входа
                }
            }
            else if (!(File.Exists("user.json")))
            {
                MainFrame.Navigate(new PageSignIn());   //открытие окна для входа
            }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)      //кнопка закрыть
        {
            this.Close();
        }

        private void btn_maximize_Click(object sender, RoutedEventArgs e)   //кнопка развернуть
        {
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)       //кнопка свернуть
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        
        private void Window_Closing(object sender, CancelEventArgs e)   //событие закрытие окна
        {
            if (File.Exists("user.json"))
            {
                string json = File.ReadAllText("user.json");
                User users = JsonConvert.DeserializeObject<User>(json);
                string userName = users.CurrentUser;

                string discont_notification = $"<html><head/><body><table width = '100%' cellpadding='3' cellspacing='0'><tr><td valign='center' align = 'center'><font color = '#ffffff'><b>{userName}</b> покинул чат</font></tr></td></table><br></body></html>";

                if (System.IO.Directory.Exists("messages")) //"дозапись" файла сообщения 
                {
                    if (File.Exists($"messages/{userName}.txt"))
                    {
                        File.AppendAllText($"messages/{userName}.txt", discont_notification);
                    }
                }

                MainContent.send_notification(discont_notification);    //отправка уведомления
                MainContent.disconnect();           //отключение
            }
            else 
            {
                MainContent.disconnect();
            }

        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)      // перемещение окна
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        public void readjson()            //функция чтения json файла
        {
            string json = File.ReadAllText("user.json");
            User users = JsonConvert.DeserializeObject<User>(json);
            current = users.CurrentUser;
            oldname = users.OldName;
            status = users.Status;

        }

        private void Window_Activated(object sender, EventArgs e)       //фокус на окне
        {
            MainContent.focus = "True";
        }

        private void Window_Deactivated(object sender, EventArgs e)         // нет фокуса на окне
        {
            MainContent.focus = "False";
        }
    }
}
