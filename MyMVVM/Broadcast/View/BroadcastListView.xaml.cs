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
using MyMVVM.Broadcast.Utils;
using MyMVVM.Broadcast.ViewModel;
using MyMVVM.MainWindow.View;

namespace MyMVVM.Broadcast.View
{
    /// <summary>
    /// BroadcastListView.xaml 的交互逻辑
    /// </summary>
    public partial class BroadcastListView : Window
    {
        public BroadcastListView()
        {
            InitializeComponent();
            this.DataContext = new BroadcastListViewModel();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TimerPool.StopAndRemoveTimer("broadcastList");
            this.Close();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BroadCastListModel broadCastListModel = DataGridElement.SelectedItem as BroadCastListModel;
            if (broadCastListModel == null)
            {
                return;
            }
            BroadcastDetailView view = new BroadcastDetailView(broadCastListModel);
            view.ShowDialog();
        }
    }
}

