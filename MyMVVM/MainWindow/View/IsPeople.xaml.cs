using MyMVVM.Common.View;
using MyMVVM.Common;
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
    /// IsPeople.xaml 的交互逻辑
    /// </summary>
    public partial class IsPeople : Window
    {


        public IsPeople()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public event Action UpdateMainForm;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string u = peopleNum.Text.ToString();
            if (u == "" && u == null)
            {
                DMMessageBox.Show("警告", "无人值守号码为空", DMMessageType.MESSAGE_WARING);
                return;
            }
            else
            {
                bool result = DMMessageBox.Show("提示", "确认开启无人值守?", DMMessageType.MESSAGE_INFO, DMMessageButton.YesNo);
                if (result)
                {
                    UpdateMainForm?.Invoke();
                    IsPeopleDB.UpdateIsPeopleConfig1(u);
                    // DB.ExecuteNonQuery($"update config set ispeople = 0,transfernum={u} where id = 1");
                    DMMessageBox.Show("提示", "无人值守开启成功!", DMMessageType.MESSAGE_SUCCESS);
                    this.Close();
                }
            }
        }
    }
}
