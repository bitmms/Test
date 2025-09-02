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
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Emotion.Model;

namespace MyMVVM.Emotion.View
{
    /// <summary>
    /// PlayAudioView.xaml 的交互逻辑
    /// </summary>
    public partial class PlayAudioView : Window
    {
        private bool _isPlaying = false;

        private bool _isUserDragging = false;
        private DispatcherTimer _updateMusicTimer;
        private DispatcherTimer _updateTime;

        public PlayAudioView(EmotionModel emotionModel)
        {
            InitializeComponent();

            _updateMusicTimer = new DispatcherTimer();
            _updateMusicTimer.Interval = TimeSpan.FromSeconds(0.5);
            _updateMusicTimer.Tick += _updateMusicTimer_Tick;

            _updateTime = new DispatcherTimer();
            _updateTime.Interval = TimeSpan.FromMilliseconds(500);
            _updateTime.Tick += Timer_Tick;


            from_number_textblock.Text = emotionModel.FileName.Split('_')[2].Split('.')[0];
            to_number_textblock.Text = emotionModel.FileName.Split('_')[1];
            keyword_textblock.Text = emotionModel.CallText;
            call_time_textblock.Text = emotionModel.CallTime;

            mediaElement.Source = new Uri("http://" + DMVariable.SSHIP + ":90/home/" + emotionModel.FileName);
        }


        // 点击播放、暂停按钮
        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            // 1. 检查媒体源是否存在
            if (mediaElement.Source == null) return;
            // 2. 播放、暂停
            if (mediaElement.IsLoaded && !_isPlaying)
            {
                mediaElement.Play();
                btnPlayPause.Content = "暂停";
                _isPlaying = !_isPlaying;
            }
            else
            {
                mediaElement.Pause();
                btnPlayPause.Content = "播放";
                _isPlaying = !_isPlaying;
            }
        }

        // 进度条改变
        private void SliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isUserDragging || !mediaElement.NaturalDuration.HasTimeSpan) return;

            var timeSpanTotalSeconds =
                (sliderProgress.Value / 100) * mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            mediaElement.Position = TimeSpan.FromSeconds(timeSpanTotalSeconds);
            ;
        }

        // 在媒体成功加载并准备播放时触发
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                sliderProgress.Maximum = 100;
                _updateMusicTimer.Start();
            }

            // 显示总时长
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                var naturalDurationTimeSpan = mediaElement.NaturalDuration.TimeSpan;
                txtTotalTime.Text =
                    $"{(int)naturalDurationTimeSpan.TotalMinutes:00}:{naturalDurationTimeSpan.Seconds:00}";
            }

            _updateTime.Start(); // 启动定时器
        }

        // 在媒体播放完毕时触发，可以显示一个消息框告诉用户媒体已经播放完毕，或者自动加载下一个媒体文件
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Stop();
            btnPlayPause.Content = "播放";
            sliderProgress.Value = 0;
            _isPlaying = false;

            _updateTime.Stop(); // 停止定时器
            txtCurrentTime.Text = "00:00"; // 重置显示
        }

        // 用户开始拖拽进度条
        private void SliderProgress_DragStarted(object sender,
            System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isUserDragging = true;
        }

        // 用户拖拽进度条结束
        private void SliderProgress_DragCompleted(object sender,
            System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _isUserDragging = false;
        }

        private void _updateMusicTimer_Tick(object sender, EventArgs e)
        {
            if (_isUserDragging || !mediaElement.NaturalDuration.HasTimeSpan)
            {
                return;
            }

            var positionTotalSeconds = mediaElement.Position.TotalSeconds;
            var spanTotalSeconds = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            var timeSpanTotalSeconds = (positionTotalSeconds / spanTotalSeconds) * 100;
            sliderProgress.Value = timeSpanTotalSeconds;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!mediaElement.NaturalDuration.HasTimeSpan)
            {
                return;
            }

            // 更新当前播放时间
            var mediaElementPosition = mediaElement.Position;
            txtCurrentTime.Text = $"{(int)mediaElementPosition.TotalMinutes:00}:{mediaElementPosition.Seconds:00}";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}