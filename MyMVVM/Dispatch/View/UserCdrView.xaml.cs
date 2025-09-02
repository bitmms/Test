using MyMVVM.Dispatch.ViewModel;
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

namespace MyMVVM.Dispatch.View
{
    /// <summary>
    /// UserCdrView.xaml 的交互逻辑
    /// </summary>
    public partial class UserCdrView : Window
    {

        private bool _isClosing = false;

        public UserCdrView()
        {
            InitializeComponent();
            this.DataContext = new UserCdrViewModel();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

}