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

namespace MyMVVM.MainWindow.View
{
    /// <summary>
    /// PasswordWindow.xaml 的交互逻辑
    /// </summary>
	public partial class PasswordWindow : Window
    {
        public PasswordWindow()
        {
            InitializeComponent();
        }

        public string EnteredPassword { get; private set; }
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            EnteredPassword = passwordBox.Password;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
