using System;
using System.Collections.Generic;
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

namespace TCPWPFTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists("user.json"))
            {
                MainFrame.Navigate(new MainContent());   //открытие окна "регистрации" Нужно будет сделать проверку на "вхождение"
            }
            else if (!(File.Exists("user.json")))
            {
                MainFrame.Navigate(new PageSignIn());   //открытие окна "регистрации" Нужно будет сделать проверку на "вхождение"
            }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)      //кнопка закрыть
        {
            Application.Current.Shutdown();
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

        private void Label_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}
