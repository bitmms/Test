using MyMVVM.Phone.ViewModel;
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

namespace MyMVVM.Phone.View
{
    /// <summary>
    /// VideoView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoView : UserControl
    {
        public VideoView()
        {
            InitializeComponent();
            VideoViewModel = new VideoViewModel();
            DataContext = VideoViewModel;

        }


        private VideoViewModel VideoViewModel;


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string number = button.Content.ToString();
                VideoViewModel.AppendNumber(number);
            }
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            VideoViewModel.Backspace();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            VideoViewModel.Clear();
        }
    }
}

