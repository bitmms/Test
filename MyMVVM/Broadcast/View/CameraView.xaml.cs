using HaiKang;
using MyMVVM.Common.Utils;
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
    /// CameraView.xaml 的交互逻辑
    /// </summary>
    public partial class CameraView : Window
    {
        List<int> retList = new List<int>();


        public CameraView()
        {
            InitializeComponent();

            for (int i = 1; i <= 6; i++)
            {
                (this.FindName("Box" + i) as Grid).Visibility = Visibility.Hidden;
            }


            // 总页数

            int totalPage = (DMVariable.broadcastCameraVideoList.Count / 6) + ((DMVariable.broadcastCameraVideoList.Count % 6) == 0 ? 0 : 1);
            TotalPage.Text = totalPage + "";

            // 当前页数
            int currentPage = 1;
            CurrentPage.Text = currentPage + "";

            // 首页
            int startIndex = (currentPage - 1) * 6;
            int endindex = startIndex + 6;
            endindex = (endindex >= DMVariable.broadcastCameraVideoList.Count) ? DMVariable.broadcastCameraVideoList.Count : endindex;
            for (int i = startIndex, idx = 1; i < endindex; i++, idx++)
            {
                (this.FindName("Box" + idx) as Grid).Visibility = Visibility.Visible;
                (this.FindName("videoTitleText" + idx) as TextBlock).Text = $"{DMVariable.broadcastCameraVideoList[i]["usernum"]}({DMVariable.broadcastCameraVideoList[i]["username"]})";
                System.Windows.Forms.PictureBox pictureBox = this.FindName("BroadcastVideo" + idx) as System.Windows.Forms.PictureBox;
                int ret = Init(DMVariable.broadcastCameraVideoList[i]["camera_ip"], Int16.Parse(DMVariable.broadcastCameraVideoList[i]["camera_port"]), DMVariable.broadcastCameraVideoList[i]["camera_account"], DMVariable.broadcastCameraVideoList[i]["camera_password"], pictureBox);
                retList.Add(ret);
            }
        }


        private void Button_Click_Pre_Page(object sender, RoutedEventArgs e)
        {
            int currentPage = int.Parse(CurrentPage.Text);
            int totalPage = int.Parse(TotalPage.Text);

            if (currentPage == 1)
            {
                return;
            }

            StopBroadcastVideo();

            for (int i = 1; i <= 6; i++)
            {
                (this.FindName("Box" + i) as Grid).Visibility = Visibility.Hidden;
            }

            currentPage--;
            CurrentPage.Text = currentPage + "";

            int startIndex = (currentPage - 1) * 6;
            int endindex = startIndex + 6;
            endindex = (endindex >= DMVariable.broadcastCameraVideoList.Count) ? DMVariable.broadcastCameraVideoList.Count : endindex;
            for (int i = startIndex, idx = 1; i < endindex; i++, idx++)
            {
                (this.FindName("Box" + idx) as Grid).Visibility = Visibility.Visible;
                System.Windows.Forms.PictureBox pictureBox = this.FindName("BroadcastVideo" + idx) as System.Windows.Forms.PictureBox;
                int ret = Init(DMVariable.broadcastCameraVideoList[i]["ip"], Int16.Parse(DMVariable.broadcastCameraVideoList[i]["port"]), DMVariable.broadcastCameraVideoList[i]["username"], DMVariable.broadcastCameraVideoList[i]["password"], pictureBox);
                retList.Add(ret);
            }
        }

        private void Button_Click_Next_Page(object sender, RoutedEventArgs e)
        {
            int currentPage = int.Parse(CurrentPage.Text);
            int totalPage = int.Parse(TotalPage.Text);

            if (currentPage == totalPage)
            {
                return;
            }

            StopBroadcastVideo();

            for (int i = 1; i <= 6; i++)
            {
                (this.FindName("Box" + i) as Grid).Visibility = Visibility.Hidden;
            }

            currentPage++;
            CurrentPage.Text = currentPage + "";

            int startIndex = (currentPage - 1) * 6;
            int endindex = startIndex + 6;
            endindex = (endindex >= DMVariable.broadcastCameraVideoList.Count) ? DMVariable.broadcastCameraVideoList.Count : endindex;
            for (int i = startIndex, idx = 1; i < endindex; i++, idx++)
            {
                (this.FindName("Box" + idx) as Grid).Visibility = Visibility.Visible;
                System.Windows.Forms.PictureBox pictureBox = this.FindName("BroadcastVideo" + idx) as System.Windows.Forms.PictureBox;
                int ret = Init(DMVariable.broadcastCameraVideoList[i]["ip"], Int16.Parse(DMVariable.broadcastCameraVideoList[i]["port"]), DMVariable.broadcastCameraVideoList[i]["username"], DMVariable.broadcastCameraVideoList[i]["password"], pictureBox);
                retList.Add(ret);
            }
        }

        private void Button_Click_To_First_Page(object sender, RoutedEventArgs e)
        {
            int currentPage = int.Parse(CurrentPage.Text);
            int totalPage = int.Parse(TotalPage.Text);

            if (currentPage == 1)
            {
                return;
            }

            StopBroadcastVideo();

            for (int i = 1; i <= 6; i++)
            {
                (this.FindName("Box" + i) as Grid).Visibility = Visibility.Hidden;
            }

            currentPage = 1;
            CurrentPage.Text = currentPage + "";

            int startIndex = (currentPage - 1) * 6;
            int endindex = startIndex + 6;
            endindex = (endindex >= DMVariable.broadcastCameraVideoList.Count) ? DMVariable.broadcastCameraVideoList.Count : endindex;
            for (int i = startIndex, idx = 1; i < endindex; i++, idx++)
            {
                (this.FindName("Box" + idx) as Grid).Visibility = Visibility.Visible;
                System.Windows.Forms.PictureBox pictureBox = this.FindName("BroadcastVideo" + idx) as System.Windows.Forms.PictureBox;
                int ret = Init(DMVariable.broadcastCameraVideoList[i]["ip"], Int16.Parse(DMVariable.broadcastCameraVideoList[i]["port"]), DMVariable.broadcastCameraVideoList[i]["username"], DMVariable.broadcastCameraVideoList[i]["password"], pictureBox);
                retList.Add(ret);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            StopBroadcastVideo(true);
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private int Init(string ip, Int16 port, string username, string password, System.Windows.Forms.PictureBox formName)
        {
            // 初始化
            if (!CHCNetSDK.NET_DVR_Init())
            {
                return -1;
            }

            // 登录摄像头
            CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            Int32 userId = CHCNetSDK.NET_DVR_Login_V30(ip, port, username, password, ref DeviceInfo);
            if (userId < 0)
            {
                return (int)CHCNetSDK.NET_DVR_GetLastError();
            }

            // 预览
            CHCNetSDK.NET_DVR_PREVIEWINFO PreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO()
            {
                hPlayWnd = formName.Handle,
                lChannel = Int16.Parse("1"),
                dwStreamType = 0,
                dwLinkMode = 0,
                bBlocked = true,
                dwDisplayBufNum = 1,
                byProtoType = 0,
                byPreviewMode = 0,
            };
            return CHCNetSDK.NET_DVR_RealPlay_V40(userId, ref PreviewInfo, null, new IntPtr());
        }

        private void StopBroadcastVideo(bool isEnd = false)
        {
            retList.ForEach(item =>
            {
                CHCNetSDK.NET_DVR_StopRealPlay(item);
            });
            retList.Clear();
            if (isEnd)
            {
                CHCNetSDK.NET_DVR_Cleanup();
            }
        }

    }
}
