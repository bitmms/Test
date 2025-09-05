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
using MyMVVM.Common.Utils;

namespace MyMVVM.Common.View
{
    /// <summary>
    /// PingNetworkDeviceView.xaml 的交互逻辑
    /// </summary>
    public partial class PingNetworkDeviceView : Window
    {
        public PingNetworkDeviceView()
        {
            InitializeComponent();

            string ip1 = CommonDB.GetHostIPByType("主机");
            string ip2 = CommonDB.GetHostIPByType("备机");

            HostIP1.Text = ip1;
            HostIP2.Text = ip2;

            HostIcon1.Source = new BitmapImage(new Uri(DMUtil.PingNetworkDevice(ip1) ? "pack://application:,,,/Common/Images/ping_ok.png" : "pack://application:,,,/Common/Images/ping_no.png", UriKind.Absolute));
            HostIcon2.Source = new BitmapImage(new Uri(DMUtil.PingNetworkDevice(ip2) ? "pack://application:,,,/Common/Images/ping_ok.png" : "pack://application:,,,/Common/Images/ping_no.png", UriKind.Absolute));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
