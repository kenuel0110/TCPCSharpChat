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
    /// Логика взаимодействия для MainContent.xaml
    /// </summary>
    public partial class MainContent : Page
    {

        public string current = "";         //переменая для имени

        public MainContent()
        {
            InitializeComponent();
            read4json();
            label_UserNameMain.Text = current;

        }

        private void read4json()            //функция чтения json файла
        {
            string json = File.ReadAllText("user.json");
            User users = JsonConvert.DeserializeObject<User>(json);
            current = users.CurrentUser;
        }
    }
}
