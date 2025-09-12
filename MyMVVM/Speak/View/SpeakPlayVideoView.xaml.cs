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
using System.Windows.Threading;

namespace MyMVVM.Speak.View
{
    /// <summary>
    /// SpeakPlayVideoView.xaml 的交互逻辑
    /// </summary>
    public partial class SpeakPlayVideoView : Window
    {

        private string videoPath = "";

        public SpeakPlayVideoView(string path)
        {
            InitializeComponent();

            videoPath = path;

            mediaElement.Volume = 1;
            mediaElement.MediaEnded += mediaElement_MediaEnded;
            mediaElement.MediaOpened += mediaElement_MediaOpened;
        }

        private DispatcherTimer timerTo = new DispatcherTimer();


        //播放
        private void PlayMusic(object sender, RoutedEventArgs e)
        {
            if (PlayStartPause.Content.ToString() == "播放")
            {
                // 初始状态没有选择音乐，无法播放
                if (mediaElement.Source == null)
                {
                    mediaElement.Source = new Uri(videoPath, UriKind.Absolute);
                }
                PlayStartPause.Content = "暂停";
                mediaElement.Play();
            }
            else
            {
                PlayStartPause.Content = "播放";
                mediaElement.Pause();
            }

        }
        // 当音乐文件加载完成后触发，实时更新进度条  
        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            // 设置进度条的最大值为媒体文件的总秒数  
            sliderPosition.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;

            aa();
        }

        public void aa()
        {
            timerTo.Interval = TimeSpan.FromSeconds(1);
            timerTo.Tick += (sender, e) =>
            {
                Console.WriteLine(mediaElement.Position.TotalSeconds);
                // 将进度条的Value属性设置为当前媒体播放的位置（以秒为单位）  
                sliderPosition.Value = mediaElement.Position.TotalSeconds;
            };// 每秒触发一次
            timerTo.Start();
        }


        // 当进度条的值改变时触发
        private void sliderPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 根据进度条的值（以秒为单位），设置MediaElement的当前播放位置  
            mediaElement.Position = TimeSpan.FromSeconds(sliderPosition.Value);
        }

        // 当音乐文件播放完成后触发，自动跳转到下一首歌曲
        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            timerTo.Stop();
            sliderPosition.Value = 0;
            mediaElement.SpeedRatio = 1;
            PlayStartPause.Content = "播放";
            mediaElement.Source = null;
        }
    }
}
