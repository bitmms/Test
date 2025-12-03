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
using MyMVVM.Gateway.ViewModel;

namespace MyMVVM.Gateway.View
{
    /// <summary>
    /// GatewayView.xaml 的交互逻辑
    /// </summary>
    public partial class GatewayView : Window
    {
        public GatewayView()
        {
            InitializeComponent();
            this.DataContext = new GatewayAlarmViewModel();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
