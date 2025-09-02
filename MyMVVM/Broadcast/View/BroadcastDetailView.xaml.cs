using MyMVVM.Broadcast.ViewModel;
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

namespace MyMVVM.Broadcast.View
{
    /// <summary>
    /// BroadcastDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class BroadcastDetailView : Window
    {
        public BroadcastDetailView(BroadCastListModel broadCastListModel)
        {
            InitializeComponent();
            this.DataContext = new BroadcastDetailViewModel(broadCastListModel);
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
