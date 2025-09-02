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
using System.Windows.Threading;
using MyMVVM.Broadcast.Utils;
using MyMVVM.Broadcast.ViewModel;
using MyMVVM.Common.Utils;

namespace MyMVVM.Broadcast.View
{
    /// <summary>
    /// BroadcastView.xaml 的交互逻辑
    /// </summary>
    public partial class BroadcastView : UserControl
    {
        public BroadcastView()
        {
            InitializeComponent();
            this.DataContext = new BroadCastViewModel();
            DMVariable.broadcastVideoForm = BroadcastCameraVideo;
        }



        #region 摄像头

        private void Button_All_Camera_Video(object sender, RoutedEventArgs e)
        {
            if (DMVariable.broadcastCameraVideoList.Count > 1)
                new CameraView().ShowDialog();
        }

        #endregion


        #region 音乐播放器

        // 点击音乐列表的某一首歌曲进行播放
        private void MusicDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MusicDataGrid.SelectedItem == null)
            {
                return;
            }

            MusicModel musicModel = MusicDataGrid.SelectedItem as MusicModel;

            // （1）音乐名称
            TextBlock MusicNameTextBloc = this.FindName("MusicNameElement") as TextBlock;
            MusicNameTextBloc.Text = musicModel.Name;
            // （2）音乐时长
            TextBlock MusicTimeTextBloc = this.FindName("MusicTimeElement") as TextBlock;
            MusicTimeTextBloc.Text = musicModel.Time;
            // （3）音源：用户有选择音乐时，使用用户选择的音乐
            mediaElement.Source = new Uri(musicModel.UploadLocalPath, UriKind.Absolute);
            // （4）ID
            PlayButton.Tag = musicModel.Id.ToString();

            // 暂停当前的播放，即使当前正在播放音乐，一旦选择音乐之后，则停止
            PlayButtonTextBloc.Text = MusicConfig.MusicState.PauseIcon;
            mediaElement.Pause();
        }

        // 点击播放按钮
        private void PlayMusic(object sender, RoutedEventArgs e)
        {
            // 当前是处于暂停按钮状态，点击之后播放音乐
            if (PlayButtonTextBloc.Text.ToString() == MusicConfig.MusicState.PauseIcon)
            {
                // 初始状态没有选择音乐，默认播放第一首
                if (PlayButton.Tag == null || PlayButton.Tag.ToString() == "")
                {
                    ClickPreMusicButton(null, null); // 默认播放第一首
                    return;
                }

                // 已选择音乐
                // 1. 找ID
                string musicFIleId = PlayButton.Tag.ToString();
                // 2. 根据 ID 查找数据库中的音乐记录
                MusicModel musicModel = new MusicModel();
                foreach (var item in MusicDB.GetMusicList())
                {
                    if (item.Id.ToString() == musicFIleId)
                    {
                        musicModel = item;
                        break;
                    }
                }
                // 3. 基本信息
                // （1）音乐名称
                TextBlock MusicNameTextBlock = this.FindName("MusicNameElement") as TextBlock;
                MusicNameTextBlock.Text = musicModel.Name;
                // （2）音乐时长
                TextBlock MusicTimeTextBloc = this.FindName("MusicTimeElement") as TextBlock;
                MusicTimeTextBloc.Text = musicModel.Time;
                // （3）音源：用户有选择音乐时，使用用户选择的音乐【仅未选择音乐时重新设置音源】
                if (mediaElement.Source == null)
                {
                    mediaElement.Source = new Uri(musicModel.UploadLocalPath, UriKind.Absolute);
                }
                // 4. 播放
                PlayButtonTextBloc.Text = MusicConfig.MusicState.PlayIcon;
                mediaElement.Play();
            }

            // 当前是处于播放中按钮状态，点击之后暂停播放音乐
            else
            {
                PlayButtonTextBloc.Text = MusicConfig.MusicState.PauseIcon;
                mediaElement.Pause();
            }
        }

        // 当进度条的值改变时触发
        private void sliderPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 根据进度条的值（以秒为单位），设置MediaElement的当前播放位置  
            mediaElement.Position = TimeSpan.FromSeconds(sliderPosition.Value);

            // 实时更新已经播放的时间
            int totalMinutes = (int)sliderPosition.Value; // 假设slider的值代表分钟  
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            MusicRunTimeElement.Text = $"{hours:D2}:{minutes:D2}";
        }

        // DispatcherTimer的Tick事件处理，用于更新进度条  
        private void timer_tick(object sender, EventArgs e)
        {
            // 将进度条的Value属性设置为当前媒体播放的位置（以秒为单位）  
            sliderPosition.Value = mediaElement.Position.TotalSeconds;
        }

        // 当音乐文件加载完成后触发，实时更新进度条  
        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            // 设置进度条的最大值为媒体文件的总秒数  
            sliderPosition.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;

            // 初始化并启动一个DispatcherTimer，用于每秒更新进度条  
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // 设置定时器间隔为1秒  
            timer.Tick += new EventHandler(timer_tick); // 定时器触发时调用timer_tick方法  
            timer.Start(); // 启动定时器
        }

        // 当音乐文件播放完成后触发，自动跳转到下一首歌曲
        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            ClickNextMusicButton(null, null);
        }

        // 上一首
        public void ClickPreMusicButton(object sender, RoutedEventArgs e)
        {
            List<MusicModel> MusicList = MusicDB.GetMusicList();

            if (MusicList.Count <= 0)
            {
                return;
            }

            // 1. 找上一首歌曲index
            int NowIndex = -1;
            if (PlayButton.Tag == null || PlayButton.Tag.ToString() == "")
            {
                NowIndex = 0;
            }
            else
            {
                for (int i = 0; i < MusicList.Count; i++)
                {
                    if (MusicList[i].Id.ToString() == PlayButton.Tag.ToString())
                    {
                        NowIndex = i;
                        break;
                    }
                }

                if (NowIndex == 0)
                {
                    NowIndex = MusicList.Count - 1;
                }
                else
                {
                    NowIndex--;
                }
            }

            // 2. 歌曲
            MusicModel musicModel = MusicList[NowIndex];

            // 3. 基本信息
            // （1）音乐名称
            TextBlock MusicNameTextBloc = this.FindName("MusicNameElement") as TextBlock;
            MusicNameTextBloc.Text = musicModel.Name;
            // （2）音乐时长
            TextBlock MusicTimeTextBloc = this.FindName("MusicTimeElement") as TextBlock;
            MusicTimeTextBloc.Text = musicModel.Time;
            // （3）音源：用户有选择音乐时，使用用户选择的音乐
            mediaElement.Source = new Uri(musicModel.UploadLocalPath, UriKind.Absolute);
            // （4）ID
            PlayButton.Tag = musicModel.Id.ToString();

            // 4. 播放
            PlayButtonTextBloc.Text = MusicConfig.MusicState.PlayIcon;
            mediaElement.Play();
        }

        // 下一首
        public void ClickNextMusicButton(object sender, RoutedEventArgs e)
        {
            List<MusicModel> MusicList = MusicDB.GetMusicList();

            if (MusicList.Count <= 0)
            {
                return;
            }

            // 1. 找下一首歌曲index
            int NowIndex = -1;
            if (PlayButton.Tag == null || PlayButton.Tag.ToString() == "")
            {
                NowIndex = 0;
            }
            else
            {
                for (int i = 0; i < MusicList.Count; i++)
                {
                    if (MusicList[i].Id.ToString() == PlayButton.Tag.ToString())
                    {
                        NowIndex = i;
                        break;
                    }
                }

                if (NowIndex == MusicList.Count - 1)
                {
                    NowIndex = 0;
                }
                else
                {
                    NowIndex++;
                }
            }

            // 2. 歌曲
            MusicModel musicModel = MusicList[NowIndex];

            // 3. 基本信息
            // （1）音乐名称
            TextBlock MusicNameTextBloc = this.FindName("MusicNameElement") as TextBlock;
            MusicNameTextBloc.Text = musicModel.Name;
            // （2）音乐时长
            TextBlock MusicTimeTextBloc = this.FindName("MusicTimeElement") as TextBlock;
            MusicTimeTextBloc.Text = musicModel.Time;
            // （3）音源：用户有选择音乐时，使用用户选择的音乐
            mediaElement.Source = new Uri(musicModel.UploadLocalPath, UriKind.Absolute);
            // （4）ID
            PlayButton.Tag = musicModel.Id.ToString();

            // 4. 播放
            PlayButtonTextBloc.Text = MusicConfig.MusicState.PlayIcon;
            mediaElement.Play();
        }




        #endregion
    }
}
