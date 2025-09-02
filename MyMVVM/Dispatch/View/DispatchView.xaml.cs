using MyMVVM.Dispatch.ViewModel;
using MyMVVM.MainWindow.View;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyMVVM.Dispatch.View
{
    /// <summary>
    /// DispatchView.xaml 的交互逻辑
    /// </summary>
	public partial class DispatchView : UserControl
    {
        public DispatchView()
        {
            InitializeComponent();
            this.DataContext = new DispatchViewModel();
        }

        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UserCdrDetailView userCdrDetailView = new UserCdrDetailView();
            userCdrDetailView.ShowDialog();
        }
    }


}
