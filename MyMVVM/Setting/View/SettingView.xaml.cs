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
using MyMVVM.MainWindow.View;
using MyMVVM.WIFI.View;

namespace MyMVVM.Setting.View
{
    /// <summary>
    /// SettingView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingView : Window
    {

        private ThemeSetting _ThemeSettingCache = null;
        public SettingView()
        {
            _ThemeSettingCache = new ThemeSetting();
            InitializeComponent();
            ContentArea.Content = _ThemeSettingCache;
            WIFIMenuItem.Visibility = SettingDB.IsShowWIFIMenuItem() ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string tag = menuItem.Tag.ToString();
                switch (tag)
                {
                    case "ThemeSettings":
                        {
                            ContentArea.Content = _ThemeSettingCache;
                            break;
                        }
                    case "WiFiManagement":
                        {
                            ContentArea.Content = new WIFISettingView();
                            break;
                        }
                    case "Logout":
                        {
                            this.Close();
                            break;
                        }
                }
            }
        }
    }

}
