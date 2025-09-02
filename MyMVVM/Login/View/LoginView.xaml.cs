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
using MyMVVM.Common;
using MyMVVM.Common.Utils;

namespace MyMVVM.Login.View
{
    /// <summary>
    /// LoginView.xaml 的交互逻辑
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            if (DMConfig.IsShowLogo())
            {
                LogoImage1.Visibility = Visibility.Collapsed;
                LogoImage2.Visibility = Visibility.Visible;
            }
            else
            {
                LogoImage1.Visibility = Visibility.Visible;
                LogoImage2.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        /// <summary>
        /// 鼠标点击用户名输入框
        /// </summary>
        private void txtUser_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtUser.CaretIndex = txtUser.Text.Length;
        }
        /// <summary>
        /// 鼠标点击密码输入框
        /// </summary>
        private void password_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            password.CaretIndex = password.Text.Length;
        }
        /// <summary>
        /// 鼠标点击IP输入框
        /// </summary>
        private void ip_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ip.CaretIndex = ip.Text.Length;
        }
    }
}

