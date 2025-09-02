using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HaiKang;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.Monitor.Model;
using MyMVVM.Monitor.Utils;
using MyMVVM.Monitor.ViewModel;

namespace MyMVVM.Monitor.View
{
    public partial class OneMonitorView : Window
    {
        private bool IsTalkIng;
        private int loginCode = -1;
        private int preViewCode = -1;
        private int talkCode = -1;
        private string type;
        private MonitorModel NowMonitorModel;

        public OneMonitorView(MonitorModel monitorModel, bool isNowOpenTalk = false)
        {
            InitializeComponent();

            IsTalkIng = false;
            NowMonitorModel = monitorModel;

            if (monitorModel.IsShowTalk)
            {
                OpenTalkButton.Visibility = Visibility.Hidden;
                CloseTalkButton.Visibility = Visibility.Visible;
            }
            else
            {
                OpenTalkButton.Visibility = Visibility.Hidden;
                CloseTalkButton.Visibility = Visibility.Hidden;
            }

            loginCode = MonitorUtil.LoginDevice(NowMonitorModel.IP, ushort.Parse(NowMonitorModel.Port.ToString()), NowMonitorModel.Username, NowMonitorModel.Password, NowMonitorModel.Type);
            preViewCode = MonitorUtil.PreViewDevice(loginCode, OneMonitorVideoForm.Handle, NowMonitorModel.Type);
            type = NowMonitorModel.Type;

            if (isNowOpenTalk)
            {
                OpenTalk(true);
            }
        }



        private void OpenTalk(bool isFirst = false)
        {
            if (!NowMonitorModel.IsShowTalk) // 无语音对讲功能
            {
                return;
            }
            if (IsTalkIng) // 开启 -->> 关闭
            {
                MonitorUtil.StopTalk(talkCode, type);
                IsTalkIng = false;

                OpenTalkButton.Visibility = Visibility.Hidden;
                CloseTalkButton.Visibility = Visibility.Visible;
            }
            else // 关闭 -->> 开启
            {
                talkCode = MonitorUtil.TalkDevice(loginCode, type);

                if (talkCode < 0)
                {
                    if (!isFirst)
                    {
                        DMMessageBox.Show("警告", "开启语音对讲失败，请检查音频设备是否连接正常", DMMessageType.MESSAGE_WARING, DMMessageButton.Confirm);
                    }
                }
                else
                {
                    IsTalkIng = true;
                    OpenTalkButton.Visibility = Visibility.Visible;
                    CloseTalkButton.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseWindowCommand(object sender, RoutedEventArgs e)
        {
            MonitorUtil.StopTalk(talkCode, type);
            MonitorUtil.StopPreView(preViewCode, type);
            MonitorUtil.StopLogin(loginCode, type);

            this.Close();
        }

        private void OpenOrCloseTalkCommand(object sender, RoutedEventArgs e)
        {
            OpenTalk();
        }
    }
}
