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
using MyMVVM.Common.View;
using MyMVVM.Monitor.Model;
using MyMVVM.Monitor.Utils;

namespace MyMVVM.MainWindow.View
{
    /// <summary>
    /// VideoCallWebSocketView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoCallWebSocketView : Window
    {
        private int loginCode = -1;
        private int preViewCode = -1;
        private string type;

        public VideoCallWebSocketView(Dictionary<string, string> monitorDict)
        {
            InitializeComponent();
            VideoCallNumber.Text = $"{monitorDict["sipNumber"]}({monitorDict["sipName"]})";
            type = monitorDict["type"];
            loginCode = MonitorUtil.LoginDevice(monitorDict["ip"], ushort.Parse(monitorDict["port"]), monitorDict["account"], monitorDict["password"], type);
            preViewCode = MonitorUtil.PreViewDevice(loginCode, OneMonitorVideoForm.Handle, type);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        public void CloseWindowCommand(object sender, RoutedEventArgs e)
        {
            MonitorUtil.StopPreView(preViewCode, type);
            MonitorUtil.StopLogin(loginCode, type);
            this.Close();
        }
    }
}
