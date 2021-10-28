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
using TCPWPFTest;

namespace TCPCSharpChat
{
    /// <summary>
    /// Логика взаимодействия для Page_Settings.xaml
    /// </summary>
    public partial class Page_Settings : Page
    {
        public Page_Settings()
        {
            InitializeComponent();
        }

        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageConnect());
        }

        private void btn_Message_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageHistory());
        }
    }
}
