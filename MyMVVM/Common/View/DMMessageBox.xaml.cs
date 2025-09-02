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

namespace MyMVVM.Common.View
{

    // 四种消息类型的枚举
    public enum DMMessageType
    {
        MESSAGE_SUCCESS = 1,
        MESSAGE_FAIL = 2,
        MESSAGE_WARING = 3,
        MESSAGE_INFO = 4,
    }


    // 消息窗口底部按钮的枚举
    public enum DMMessageButton
    {
        Confirm = 1,
        YesNo = 2,
        LeftRight = 3
    }


    public partial class DMMessageBox : Window
    {

        private readonly static Dictionary<DMMessageType, string> iconType = new Dictionary<DMMessageType, string>()
        {
            { DMMessageType.MESSAGE_SUCCESS, "\uE615"},
            { DMMessageType.MESSAGE_FAIL, "\uE613"},
            { DMMessageType.MESSAGE_WARING, "\uE616"},
            { DMMessageType.MESSAGE_INFO, "\uE614"},
        };
        private readonly static Dictionary<DMMessageType, SolidColorBrush> iconColor = new Dictionary<DMMessageType, SolidColorBrush>()
        {
            { DMMessageType.MESSAGE_SUCCESS, new SolidColorBrush(Color.FromRgb((byte)26, (byte)250, (byte)41))},
            { DMMessageType.MESSAGE_FAIL, new SolidColorBrush(Color.FromRgb((byte)216, (byte)30, (byte)6))},
            { DMMessageType.MESSAGE_WARING, new SolidColorBrush(Color.FromRgb((byte)206, (byte)104, (byte)27))},
            { DMMessageType.MESSAGE_INFO, new SolidColorBrush(Color.FromRgb((byte)44, (byte)44, (byte)44))},
        };


        // 构造器
        public DMMessageBox()
        {
            InitializeComponent();
        }

        // 属性的 get、set 方法
        public new string Title
        {
            get { return this.lblTitle.Text; }
            set { this.lblTitle.Text = value; }
        }

        public string Message
        {
            get { return this.lblMsg.Text; }
            set { this.lblMsg.Text = value; }
        }



        // 点击确定按钮
        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            isOpen = false;
            this.DialogResult = true;
        }


        // 点击是按钮
        private void NoButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


        // 点击否按钮
        private void YesButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }


        private void LeftClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void RightClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }


        /// <summary>
        /// 消息确认窗口，仅传入消息内容即可
        /// </summary>X
        public static bool ShowInfo(string msg)
        {
            return DMMessageBox.Show("提示", msg, DMMessageType.MESSAGE_INFO, DMMessageButton.Confirm);
        }


        /// <summary>
        /// 依次传入：标题、消息内容、消息类型【例子：DMMessageBox.Show("标题", "消息内容", DMMessageType.消息类型);】
        /// </summary>X
        public static bool Show(string title, string msg, DMMessageType flag)
        {
            return DMMessageBox.Show(title, msg, flag, DMMessageButton.Confirm);
        }


        /// <summary>
        /// 依次传入：标题、消息内容、消息类型、按钮类型【例子：DMMessageBox.Show("标题", "消息内容", DMMessageType.消息类型, DMMessageButton.按钮类型);】
        /// </summary>
        public static bool Show(string title, string msg, DMMessageType flag, DMMessageButton button)
        {
            DMMessageBox msgBox = new DMMessageBox();

            msgBox.Title = title;

            msgBox.Message = msg;

            TextBlock tt = msgBox.FindName("infoIcon") as TextBlock;
            tt.Text = iconType[flag];
            tt.Foreground = iconColor[flag];

            if (button == DMMessageButton.YesNo)
            {
                StackPanel ConfirmPanel = msgBox.FindName("ConfirmPanel") as StackPanel;
                StackPanel YesAndNoPanel = msgBox.FindName("YesAndNoPanel") as StackPanel;
                StackPanel LeftAndRight = msgBox.FindName("LeftAndRight") as StackPanel;
                LeftAndRight.Visibility = Visibility.Collapsed;
                YesAndNoPanel.Visibility = Visibility.Visible;
                ConfirmPanel.Visibility = Visibility.Collapsed;
            }
            else if (button == DMMessageButton.Confirm)
            {
                StackPanel ConfirmPanel = msgBox.FindName("ConfirmPanel") as StackPanel;
                StackPanel YesAndNoPanel = msgBox.FindName("YesAndNoPanel") as StackPanel;
                StackPanel LeftAndRight = msgBox.FindName("LeftAndRight") as StackPanel;
                YesAndNoPanel.Visibility = Visibility.Collapsed;
                LeftAndRight.Visibility = Visibility.Collapsed;
                ConfirmPanel.Visibility = Visibility.Visible;
            }
            else if (button == DMMessageButton.LeftRight)
            {
                StackPanel LeftAndRight = msgBox.FindName("LeftAndRight") as StackPanel;
                StackPanel ConfirmPanel = msgBox.FindName("ConfirmPanel") as StackPanel;
                StackPanel YesAndNoPanel = msgBox.FindName("YesAndNoPanel") as StackPanel;
                LeftAndRight.Visibility = Visibility.Visible;
                YesAndNoPanel.Visibility = Visibility.Collapsed;
                ConfirmPanel.Visibility = Visibility.Collapsed;
            }
            return (bool)msgBox.ShowDialog();
        }


        // 标记网络异常弹窗是否开启状态
        public static bool isOpen = false;
        public static bool Show_Network_Error(string title, string msg)
        {
            if (!isOpen)
            {
                isOpen = true;
                DMMessageBox msgBox = new DMMessageBox();
                {
                    msgBox.Title = title;
                    msgBox.Message = msg;
                    TextBlock tt = msgBox.FindName("infoIcon") as TextBlock;
                    tt.Text = iconType[DMMessageType.MESSAGE_FAIL];
                    tt.Foreground = iconColor[DMMessageType.MESSAGE_FAIL];
                }

                StackPanel ConfirmPanel = msgBox.FindName("ConfirmPanel") as StackPanel;
                StackPanel YesAndNoPanel = msgBox.FindName("YesAndNoPanel") as StackPanel;
                StackPanel LeftAndRight = msgBox.FindName("LeftAndRight") as StackPanel;
                YesAndNoPanel.Visibility = Visibility.Collapsed;
                LeftAndRight.Visibility = Visibility.Collapsed;
                ConfirmPanel.Visibility = Visibility.Visible;

                return (bool)msgBox.ShowDialog();

            }
            return true;

        }
    }
}
