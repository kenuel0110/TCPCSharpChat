using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace TCPCSharpChat
{
    /// <summary>
    /// Логика взаимодействия для QuestionDialog.xaml
    /// </summary>
    public partial class QuestionDialog : Window
    {
        public QuestionDialog(string question)
        {
            InitializeComponent();
            lbl_Question.Text = question;           //получение вопроса
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)      //перемещение окна
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void btn_yes_Click(object sender, RoutedEventArgs e)        //положительный ответ
        {
            this.DialogResult = true;
        }

        private void btn_no_Click(object sender, RoutedEventArgs e)     //отрицательный ответ
        {
            this.DialogResult = false;
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)      //закрытие
        {
            this.Close();
        }
    }
}
