using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using System.IO;
using TCPCSharpChat;
using Newtonsoft.Json;
using System.Text.Encodings.Web;

namespace TCPWPFTest
{
    /// <summary>
    /// Логика взаимодействия для PageSignIn.xaml
    /// </summary>
    public partial class PageSignIn : Page
    {

        public string current = "";     //переменая имени
        public string oldname = "";     //переменная старого имени
        public string status = "";          //переменая статуса


        public PageSignIn()
        {
            InitializeComponent();

            if (File.Exists("user.json"))       //проверка существования файла пользователя
            {
                readjson();
                if (oldname != null) 
                {
                    tboxNick.Text = oldname;        //если он есть вставляеться старое имя
                }
            }

        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)  //войти
        {
            if (!(System.IO.Directory.Exists("messages"))) 
            {
                Directory.CreateDirectory("messages");
            }
            string nickname = tboxNick.Text;        //изьятие из текстбокса
            string[] allFoundFiles = Directory.GetFiles("messages", $"{nickname}.txt");

            if (nickname.Length == 0) { tboxNick.Foreground = Brushes.Red; tboxNick.ToolTip = "Введите хоть что-нибудь"; }      //проверка данных
            else if (nickname.Length < 3) { tboxNick.Foreground = Brushes.Red; tboxNick.ToolTip = "Длина ника должна быть больше 3-х символов"; }
            else if (Regex.IsMatch(nickname, @"[\%\/\\\&\?\,\'\;\:\!\-\0-9]+") == true) { tboxNick.Foreground = Brushes.Red; tboxNick.ToolTip = "Ник имеет недопустимые символы"; }
            else
            {
                if (allFoundFiles.Any() == true) 
                {
                    QuestionDialog questionDialog = new QuestionDialog("Вы уверены, что хотите продолжить под этим ником? (Он уже кем-то используеться)");
                    if (questionDialog.ShowDialog() == true)
                    {
                        save2file(nickname); //сохранение в джейсон
                        NavigationService.Navigate(new MainContent());      //переход на главную страницу
                    }
                }
                else
                {
                    save2file(nickname);
                    NavigationService.Navigate(new MainContent());      //переход на главную страницу
                }
            }
        }

        private void save2file(string nick)         //Сохранение данных о пользователe
        {

            User user = new User();
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            user.CurrentUser = nick;
            user.OldName = null;
            user.Status = "In";
            string jsonString = System.Text.Json.JsonSerializer.Serialize<User>(user, options);
            File.WriteAllText("user.json", jsonString);
                
        }

        public void readjson()            //функция чтения json файла
        {
            string json = File.ReadAllText("user.json");
            User users = JsonConvert.DeserializeObject<User>(json);
            current = users.CurrentUser;
            oldname = users.OldName;
            status = users.Status;

        }


        private void tboxNickEventHandler(object sender, TextChangedEventArgs args)
        {
            tboxNick.Foreground = Brushes.White;        //исправления цвета текста при ошибке
        }
    }
}
