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
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
	public partial class InputDialog : Window
    {
        public string ResponseText { get; set; }
        public string Prompt { get; set; }

        public InputDialog(string prompt, string defaultText = "")
        {
            InitializeComponent();
            DataContext = this;
            Prompt = prompt;
            ResponseText = defaultText;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
