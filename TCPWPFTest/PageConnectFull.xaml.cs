using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
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
using TCPWPFTest;

namespace TCPCSharpChat
{
    /// <summary>
    /// Логика взаимодействия для PageConnect.xaml
    /// </summary>
    public partial class PageConnectFull : Page
    {

        public string ip = "";
        public int port = 0;

        public PageConnectFull()
        {
            InitializeComponent();
            if (File.Exists("connection.json"))
            {
                readjson();
                tb_port.Text = port.ToString();
                String[] words = ip.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                tb_oneninetwo.Text = words[0];
                tb_onesixeight.Text = words[1];
                tb_zero.Text = words[2];
                tb_ten.Text = words[3];
            }
        }
        public void readjson()            //функция чтения json файла
        {
            string json = File.ReadAllText("connection.json");
            Connecting conn = JsonConvert.DeserializeObject<Connecting>(json);
            ip = conn.ip;
            port = conn.port;
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            Connecting conn = new Connecting();
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            string ipAdress = $"{tb_oneninetwo.Text}.{tb_onesixeight.Text}.{tb_zero.Text}.{tb_ten.Text}";

            conn.ip = ipAdress;
            conn.port = Convert.ToInt32(tb_port.Text);

            string jsonString = System.Text.Json.JsonSerializer.Serialize<Connecting>(conn, options);
            File.WriteAllText("connection.json", jsonString);

            QuestionDialog questionDialog = new QuestionDialog("Вы уверены, что хотите применить настройки? (Программа закроется)");
            if (questionDialog.ShowDialog() == true)
            {
                MainContent.disconnect();
                Application.Current.Shutdown();
            }

        }

        
    }
}
