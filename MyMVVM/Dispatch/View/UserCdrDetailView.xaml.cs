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
    /// UserCdrDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class UserCdrDetailView : Window
    {
        public UserCdrDetailView()
        {
            InitializeComponent();
            this.DataContext = new UserCdrDetailViewModel();
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
