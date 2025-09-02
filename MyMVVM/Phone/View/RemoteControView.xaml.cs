using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
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
using LibVLCSharp.Shared;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Monitor.Utils;
using MyMVVM.Phone.Model;
using MyMVVM.Phone.Utils;
using Newtonsoft.Json;

namespace MyMVVM.Phone.View
{
    /// <summary>
    /// RemoteControView.xaml 的交互逻辑
    /// </summary>
    public partial class RemoteControView : Window
    {

        String userNum = "";
        Boolean isOpenTalk = false;
        public RemoteControView(string _userNum)
        {
            InitializeComponent();

            isOpenTalk = false;
            OpenTalkButton.Visibility = Visibility.Hidden;
            CloseTalkButton.Visibility = Visibility.Visible;
            mainTitleText.Text = "无声介入 (" + _userNum + ")";
            userNum = _userNum;
            PlayVideo();
        }

        LibVLCSharp.Shared.MediaPlayer _mediaPlayer = null;
        LibVLC _libVLC = null;

        public void PlayVideo()
        {
            _libVLC = new LibVLC();
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            VideoPlayer.MediaPlayer = _mediaPlayer;
            var media = new Media(_libVLC, new Uri("rtmp://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port1 + "/live/livestreams_" + userNum));
            _mediaPlayer.Play(media);
            isOpenTalk = false;
            _mediaPlayer.Mute = true; // 默认静音
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseWindowCommand(object sender, RoutedEventArgs e)
        {
            this.Close();

            Task.Run(async () =>
            {
                _mediaPlayer.Stop();
                // 可选：释放媒体资源
                _mediaPlayer.Media = null;
                _mediaPlayer.Dispose();
                _libVLC.Dispose();
            });

            //Task.Run(async () =>
            //{
            //    if (userNum != "")
            //    {
            //        HttpClient client2 = new HttpClient();
            //        HttpResponseMessage response2 = await client2.GetAsync("http://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port2 + "/rtmp/" + userNum + "/2");
            //        response2.EnsureSuccessStatusCode();
            //        string responseBody2 = response2.Content.ReadAsStringAsync().Result;
            //        RTMPResponse rTMPResponse2 = JsonConvert.DeserializeObject<RTMPResponse>(responseBody2);
            //    }
            //});
        }

        private void OpenOrCloseTalkCommand(object sender, RoutedEventArgs e)
        {
            if (isOpenTalk)
            {
                // 关闭声音
                isOpenTalk = false;
                OpenTalkButton.Visibility = Visibility.Hidden;
                CloseTalkButton.Visibility = Visibility.Visible;
                _mediaPlayer.Mute = true;
            }
            else
            {
                // 开启声音
                isOpenTalk = true;
                OpenTalkButton.Visibility = Visibility.Visible;
                CloseTalkButton.Visibility = Visibility.Hidden;
                _mediaPlayer.Mute = false;
            }
        }

        public Boolean isFront = true;

        private void SwitchFrontCommand(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                HttpClient client2 = new HttpClient();
                HttpResponseMessage response2 = await client2.GetAsync("http://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port2 + "/rtmp/" + userNum + "/3");
                response2.EnsureSuccessStatusCode();
                string responseBody2 = response2.Content.ReadAsStringAsync().Result;
                RTMPResponse rTMPResponse2 = JsonConvert.DeserializeObject<RTMPResponse>(responseBody2);
            });
        }
    }
}
