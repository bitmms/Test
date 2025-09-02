using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HaiKang;
using MyMVVM.Common.View;
using MyMVVM.Monitor.Model;
using MyMVVM.Monitor.Utils;
using MyMVVM.Monitor.ViewModel;

namespace MyMVVM.Monitor.View
{
    public partial class MoreMonitorView : Window
    {
        private int CurrentPage = 1;
        private int PageSize = 6;
        private List<MonitorModel> AllMonitorList;
        private List<Grid> BoxList = new List<Grid>();
        private List<TextBlock> MonitorTextList = new List<TextBlock>();
        private List<IntPtr> MonitorVideoPtrList = new List<IntPtr>();
        private List<Border> MonitorIconList = new List<Border>();

        private List<string> MonitorTypeList = new List<string>();

        // 实时预览的状态码
        private List<int> CurrentLoginCodeList = new List<int>();
        private List<int> CurrentPreViewCodeList = new List<int>();
        // 组播的状态码
        private List<int> GroupLoginCodeList = new List<int>();
        private List<int> GroupTalkCodeList = new List<int>();


        public MoreMonitorView(List<MonitorModel> SelectedMonitorList)
        {
            InitializeComponent();
            InitBox();
            AllMonitorList = SelectedMonitorList;

            CurrentPage = 1;
            LoadMonitorByPage();

            Task.Run(() => StartGroupTalk(AllMonitorList));
        }

        private void LoadMonitorByPage()
        {
            // 预处理
            int total = AllMonitorList.Count / PageSize;
            total += (AllMonitorList.Count % PageSize == 0) ? 0 : 1;
            CurrentPageText.Text = "" + CurrentPage;
            TotalPageText.Text = "" + total;
            for (int i = 0; i < CurrentPreViewCodeList.Count; i++) // 退出实时预览
            {
                MonitorUtil.StopPreView(CurrentPreViewCodeList[i], MonitorTypeList[i]);
            }
            for (int i = 0; i < CurrentLoginCodeList.Count; i++) // 实时预览设备退出登录
            {
                MonitorUtil.StopLogin(CurrentLoginCodeList[i], MonitorTypeList[i]);
            }
            for (int i = 0; i < 6; i++)
            {
                BoxList[i].Visibility = Visibility.Hidden;
                MonitorIconList[i].Visibility = Visibility.Hidden;
            }

            // 分页查询数据
            List<MonitorModel> list = new List<MonitorModel>();
            int startIndex = (CurrentPage - 1) * PageSize;
            int endIndex = (CurrentPage - 1) * PageSize + PageSize - 1;
            endIndex = (endIndex > AllMonitorList.Count - 1) ? AllMonitorList.Count - 1 : endIndex;
            for (int idx = startIndex; idx <= endIndex; idx++)
            {
                list.Add(AllMonitorList[idx]);
            }

            // 对分页查到的数据进行处理
            for (int i = 0; i < list.Count; i++)
            {
                BoxList[i].Visibility = Visibility.Visible;
                MonitorTextList[i].Text = list[i].Name;
                CurrentLoginCodeList[i] = MonitorUtil.LoginDevice(list[i].IP, (ushort)list[i].Port, list[i].Username, list[i].Password, list[i].Type);
                CurrentPreViewCodeList[i] = MonitorUtil.PreViewDevice(CurrentLoginCodeList[i], MonitorVideoPtrList[i], list[i].Type);
                MonitorIconList[i].Visibility = (list[i].IsShowTalk) ? Visibility.Visible : Visibility.Hidden;
                MonitorTypeList[i] = list[i].Type;
            }
        }

        private void InitBox()
        {
            Grid Box_01 = Box1 as Grid;
            Grid Box_02 = Box2 as Grid;
            Grid Box_03 = Box3 as Grid;
            Grid Box_04 = Box4 as Grid;
            Grid Box_05 = Box5 as Grid;
            Grid Box_06 = Box6 as Grid;
            BoxList.Add(Box_01);
            BoxList.Add(Box_02);
            BoxList.Add(Box_03);
            BoxList.Add(Box_04);
            BoxList.Add(Box_05);
            BoxList.Add(Box_06);

            TextBlock MonitorText_01 = MonitorTitleText_01 as TextBlock;
            TextBlock MonitorText_02 = MonitorTitleText_02 as TextBlock;
            TextBlock MonitorText_03 = MonitorTitleText_03 as TextBlock;
            TextBlock MonitorText_04 = MonitorTitleText_04 as TextBlock;
            TextBlock MonitorText_05 = MonitorTitleText_05 as TextBlock;
            TextBlock MonitorText_06 = MonitorTitleText_06 as TextBlock;
            MonitorTextList.Add(MonitorText_01);
            MonitorTextList.Add(MonitorText_02);
            MonitorTextList.Add(MonitorText_03);
            MonitorTextList.Add(MonitorText_04);
            MonitorTextList.Add(MonitorText_05);
            MonitorTextList.Add(MonitorText_06);

            IntPtr MonitorVideoPtr_01 = MonitorVideoForm_01.Handle;
            IntPtr MonitorVideoPtr_02 = MonitorVideoForm_02.Handle;
            IntPtr MonitorVideoPtr_03 = MonitorVideoForm_03.Handle;
            IntPtr MonitorVideoPtr_04 = MonitorVideoForm_04.Handle;
            IntPtr MonitorVideoPtr_05 = MonitorVideoForm_05.Handle;
            IntPtr MonitorVideoPtr_06 = MonitorVideoForm_06.Handle;
            MonitorVideoPtrList.Add(MonitorVideoPtr_01);
            MonitorVideoPtrList.Add(MonitorVideoPtr_02);
            MonitorVideoPtrList.Add(MonitorVideoPtr_03);
            MonitorVideoPtrList.Add(MonitorVideoPtr_04);
            MonitorVideoPtrList.Add(MonitorVideoPtr_05);
            MonitorVideoPtrList.Add(MonitorVideoPtr_06);

            MonitorIconList.Add(MonitorTitleText_Icon_01);
            MonitorIconList.Add(MonitorTitleText_Icon_02);
            MonitorIconList.Add(MonitorTitleText_Icon_03);
            MonitorIconList.Add(MonitorTitleText_Icon_04);
            MonitorIconList.Add(MonitorTitleText_Icon_05);
            MonitorIconList.Add(MonitorTitleText_Icon_06);

            Box_01.Visibility = Visibility.Hidden;
            Box_02.Visibility = Visibility.Hidden;
            Box_03.Visibility = Visibility.Hidden;
            Box_04.Visibility = Visibility.Hidden;
            Box_05.Visibility = Visibility.Hidden;
            Box_06.Visibility = Visibility.Hidden;

            for (int i = 0; i < 6; i++)
            {
                CurrentLoginCodeList.Add(-1);
                CurrentPreViewCodeList.Add(-1);
                MonitorIconList[i].Visibility = Visibility.Hidden;
                MonitorTypeList.Add("");
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void PrePage(object sender, RoutedEventArgs e)
        {
            if (CurrentPage == 1)
            {
                return;
            }
            CurrentPage--;
            LoadMonitorByPage();
        }

        private void NextPage(object sender, RoutedEventArgs e)
        {
            int total = AllMonitorList.Count / PageSize;
            total += (AllMonitorList.Count % PageSize == 0) ? 0 : 1;
            if (CurrentPage == total)
            {
                return;
            }
            CurrentPage++;
            LoadMonitorByPage();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            // 将设备从广播组中删除
            foreach (var groupTalkCode in GroupTalkCodeList)
            {
                if (groupTalkCode != -1)
                    CHCNetSDK.NET_DVR_DelDVR_V30(groupTalkCode);
            }
            // 广播组设备退出登录
            foreach (var groupLoginCode in GroupLoginCodeList)
            {
                if (groupLoginCode != -1)
                    CHCNetSDK.NET_DVR_Logout(groupLoginCode);
            }
            // 退出实时预览
            for (int i = 0; i < CurrentPreViewCodeList.Count; i++)
            {
                MonitorUtil.StopPreView(CurrentPreViewCodeList[i], MonitorTypeList[i]);
            }
            // 退出登录
            for (int i = 0; i < CurrentLoginCodeList.Count; i++)
            {
                MonitorUtil.StopLogin(CurrentLoginCodeList[i], MonitorTypeList[i]);
            }
            this.Close();
        }

        private void StartGroupTalk(List<MonitorModel> AllDevice)
        {
            // 开启广播
            if (!CHCNetSDK.NET_DVR_ClientAudioStart_V30(null, IntPtr.Zero))
            {
                Thread.Sleep(1000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DMMessageBox.Show("警告", "开启语音广播失败，请检查音频设备是否连接正常", DMMessageType.MESSAGE_WARING, DMMessageButton.Confirm);
                });
                return;
            }

            // 登录设备，将设备添加到广播组
            for (int idx = 0; idx < AllDevice.Count; idx++)
            {
                var currentDevice = AllDevice[idx];
                if (currentDevice.IsShowTalk)
                {
                    int loginCode = MonitorUtil.LoginDevice(AllDevice[idx].IP, (ushort)AllDevice[idx].Port, AllDevice[idx].Username, AllDevice[idx].Password, AllDevice[idx].Type);
                    GroupLoginCodeList.Add(loginCode);
                    int groupCode = CHCNetSDK.NET_DVR_AddDVR_V30(loginCode, 1);
                    GroupTalkCodeList.Add(groupCode);
                }
                else
                {
                    GroupLoginCodeList.Add(-1);
                    GroupTalkCodeList.Add(-1);
                }
            }
        }
    }
}
