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
        public PageSignIn()
        {
            InitializeComponent();
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)  //войти
        {

            string nickname = tboxNick.Text;        //изьятие из текстбокса

            if (nickname.Length == 0) { tboxNick.Foreground = Brushes.Red; tboxNick.ToolTip = "Введите хоть что-нибудь"; }      //проверка данных
            else if (nickname.Length < 3) { tboxNick.Foreground = Brushes.Red; tboxNick.ToolTip = "Длина ника должна быть больше 3-х символов"; }
            else if (Regex.IsMatch(nickname, @"[\%\/\\\&\?\,\'\;\:\!\-\0-9]+") == true) { tboxNick.Foreground = Brushes.Red; tboxNick.ToolTip = "Ник имеет недопустимые символы"; }
            else
            {

                save2file(nickname);
                NavigationService.Navigate(new MainContent());      //переход на главную страницу
                
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
            string jsonString = System.Text.Json.JsonSerializer.Serialize<User>(user, options);
            File.WriteAllText("user.json", jsonString);
                
        }

        private void tboxNickEventHandler(object sender, TextChangedEventArgs args)
        {
            tboxNick.Foreground = Brushes.White;        //исправления цвета текста при ошибке
        }
    }
}
