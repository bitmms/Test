using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyMVVM.Battery.View;
using MyMVVM.Battery.ViewModel;
using MyMVVM.Broadcast.View;
using MyMVVM.Broadcast.ViewModel;
using MyMVVM.Common;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.Conference.View;
using MyMVVM.Conference.ViewModel;
using MyMVVM.Dispatch.View;
using MyMVVM.Dispatch.ViewModel;
using MyMVVM.Emotion;
using MyMVVM.Emotion.View;
using MyMVVM.Login.ViewModel;
using MyMVVM.MainWindow.Model;
using MyMVVM.MainWindow.Utils;
using MyMVVM.Map.View;
using MyMVVM.Map.ViewModel;
using MyMVVM.Monitor;
using MyMVVM.Monitor.Model;
using MyMVVM.Monitor.Utils;
using MyMVVM.Monitor.View;
using MyMVVM.Monitor.ViewModel;
using MyMVVM.Phone.Utils;
using MyMVVM.Phone.View;
using MyMVVM.Phone.ViewModel;
using MyMVVM.Setting.View;
using MyMVVM.Speak.View;
using MyMVVM.Speak.ViewModel;
using MyMVVM.WIFI.View;
using MyMVVM.WIFI.ViewModel;

namespace MyMVVM.MainWindow.View
{
    /// <summary>
    /// MainFormView.xaml 的交互逻辑
    /// </summary>
    public partial class MainFormView : Window
    {
        private ViewModelsBase _CurrentViewModel = null;
        private string _CurrentActiveName = "";

        public MainFormView()
        {
            InitializeComponent();

            if (DMConfig.IsShowLogo())
            {
                LogoTitle1.Visibility = Visibility.Collapsed;
                LogoTitle2.Visibility = Visibility.Visible;
                LogoText.Visibility = Visibility.Visible;
                TimeTextBlock2.Visibility = Visibility.Visible;
                TimeTextBlock1.Visibility = Visibility.Collapsed;
            }
            else
            {
                LogoTitle1.Visibility = Visibility.Visible;
                LogoTitle2.Visibility = Visibility.Collapsed;
                LogoText.Visibility = Visibility.Hidden;
                TimeTextBlock1.Visibility = Visibility.Visible;
                TimeTextBlock2.Visibility = Visibility.Collapsed;
            }

            // 加载修改的标题
            LoadTitle();

            // 定时更新时间
            UpdateTime();

            LoadSideButtonData();

            // 默认激活的控件
            DispatchView dispatchView = new DispatchView();
            dispatchView.DataContext = new DispatchViewModel();
            MainContentView.Content = dispatchView;

            // 初始化：恢复广播、判断软件过期时间、同步音乐、加载软电话用户
            new InitUtil().InitSystem();

            // 定时查询是否和监控打电话
            startBackgroundServiceOfQueryMinitorCall();
        }



        #region 加载侧边栏

        public void LoadSideButtonData()
        {
            // 声明 View 字典
            Dictionary<string, Type> ViewDict = new Dictionary<string, Type>()
            {
                {"调度", typeof(DispatchView)},
                {"扩播", typeof(BroadcastView)},
                {"会议", typeof(ConferenceView)},
                {"手机", typeof(PhoneView)},
                {"监控", typeof(MonitorView)},
                {"基站", typeof(WIFIView)},
                {"地图", typeof(MapView)},
                {"电源", typeof(BatteryView)},
                {"对讲", typeof(SpeakView)},
            };
            // 声明 ViewModel 字典
            Dictionary<string, Type> ViewModelDict = new Dictionary<string, Type>()
            {
                {"调度", typeof(DispatchViewModel)},
                {"扩播", typeof(BroadCastViewModel)},
                {"会议", typeof(ConferenceViewModel)},
                {"手机", typeof(PhoneViewModel)},
                {"监控", typeof(MonitorViewModel)},
                {"基站", typeof(WIFIViewModel)},
                {"地图", typeof(MapViewModel)},
                {"电源", typeof(BatteryViewModel)},
                {"对讲", typeof(SpeakViewModel)},
            };
            // 声明 Icon 字典
            Dictionary<string, string> IconDict = new Dictionary<string, string>()
            {
                {"调度", "/Common/Images/icon-dispatch.png"},
                {"扩播", "/Common/Images/icon-broadcast.png"},
                {"手机", "/Common/Images/icon-phone.png"},
                {"会议", "/Common/Images/icon-conference.png"},
                {"监控", "/Common/Images/icon-monitor.png"},
                {"地图", "/Common/Images/icon-map.png"},
                {"基站", "/Common/Images/icon-wifi.png"},
                {"电源", "/Common/Images/icon-battery.png"},
                {"对讲", "/Common/Images/icon-speak.png"},
            };
            // 加载侧边栏按钮
            ObservableCollection<SideButtonModel> SideButtonList = new ObservableCollection<SideButtonModel>();
            List<SideButtonModel> tempList = SideButtonDB.GetAllSideButtons();
            foreach (SideButtonModel item in tempList)
            {
                if (IconDict.ContainsKey(item.Name))
                {
                    if (item.IsShow == 1)
                    {
                        item.Icon = IconDict[item.Name];
                        item.ShowUserControlCommand = new ViewModelCommand(p =>
                        {
                            if (item.IsOk == 1)
                            {
                                if (_CurrentActiveName == item.Name)
                                {
                                    return;
                                }
                                if (_CurrentViewModel != null && _CurrentViewModel is IDisposable disposable)
                                {
                                    disposable.Dispose();
                                }
                                _CurrentActiveName = item.Name;

                                if ("监控".Equals(item.Name))
                                {
                                    var newView = (MonitorView)Activator.CreateInstance(ViewDict[item.Name]);
                                    _CurrentViewModel = (MonitorViewModel)(newView.DataContext);
                                    MainContentView.Content = newView;
                                }
                                else if ("扩播".Equals(item.Name))
                                {
                                    var newView = (BroadcastView)Activator.CreateInstance(ViewDict[item.Name]);
                                    _CurrentViewModel = (BroadCastViewModel)Activator.CreateInstance(ViewModelDict[item.Name]);
                                    MainContentView.Content = newView;
                                }
                                else if ("会议".Equals(item.Name))
                                {
                                    var newView = (ConferenceView)Activator.CreateInstance(ViewDict[item.Name]);
                                    _CurrentViewModel = (ConferenceViewModel)Activator.CreateInstance(ViewModelDict[item.Name]);
                                    MainContentView.Content = newView;
                                }
                                else if ("基站".Equals(item.Name))
                                {
                                    var newView = (WIFIView)Activator.CreateInstance(ViewDict[item.Name]);
                                    _CurrentViewModel = (WIFIViewModel)Activator.CreateInstance(ViewModelDict[item.Name]);
                                    MainContentView.Content = newView;
                                }
                                else if ("调度".Equals(item.Name))
                                {
                                    var newView = (DispatchView)Activator.CreateInstance(ViewDict[item.Name]);
                                    _CurrentViewModel = (DispatchViewModel)Activator.CreateInstance(ViewModelDict[item.Name]);
                                    MainContentView.Content = newView;
                                }
                                else if ("地图".Equals(item.Name))
                                {
                                    var newView = (MapView)Activator.CreateInstance(ViewDict[item.Name]);
                                    _CurrentViewModel = (MapViewModel)Activator.CreateInstance(ViewModelDict[item.Name]);
                                    MainContentView.Content = newView;
                                }
                                else if ("电源".Equals(item.Name))
                                {
                                    var newView = (BatteryView)Activator.CreateInstance(ViewDict[item.Name]);
                                    _CurrentViewModel = (BatteryViewModel)Activator.CreateInstance(ViewModelDict[item.Name]);
                                    MainContentView.Content = newView;
                                }
                                else if ("手机".Equals(item.Name))
                                {
                                    var newView = (PhoneView)Activator.CreateInstance(ViewDict[item.Name]);
                                    _CurrentViewModel = (PhoneViewModel)Activator.CreateInstance(ViewModelDict[item.Name]);
                                    MainContentView.Content = newView;
                                }
                                else if ("对讲".Equals(item.Name))
                                {
                                    var newView = (SpeakView)Activator.CreateInstance(ViewDict[item.Name]);
                                    _CurrentViewModel = (SpeakViewModel)Activator.CreateInstance(ViewModelDict[item.Name]);
                                    MainContentView.Content = newView;
                                }
                            }
                            else
                            {
                                DMMessageBox.ShowInfo("功能未开放!");
                            }
                        });
                        SideButtonList.Add(item);
                    }
                }
            }
            SideButtonListView.ItemsSource = SideButtonList;
        }

        #endregion



        #region 更新时间

        private System.Timers.Timer timeTimer = null;

        private void UpdateTime()
        {
            timeTimer = new System.Timers.Timer(1000);
            timeTimer.Elapsed += TimerUpdateTime;
            timeTimer.Start();
        }

        private void TimerUpdateTime(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TimeTextBlock1.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                TimeTextBlock2.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                int count = EmotionAlarmDB.getCount();
                if (count != 0)
                {
                    EmotionAlarmNumber.Text = "(" + EmotionAlarmDB.getCount() + ")";
                }
                else
                {
                    EmotionAlarmNumber.Text = "";
                }

            });
        }

        #endregion



        #region 定时查询是否和监控打电话


        private System.Timers.Timer QueryMinitorCallTimer = null;
        private string nowOpenMonitorNumber = null;
        private VideoCallWebSocketView nowOpenMonitorWindow = null;

        private void startBackgroundServiceOfQueryMinitorCall()
        {
            QueryMinitorCallTimer = new System.Timers.Timer(800);
            QueryMinitorCallTimer.Elapsed += BackgroundServiceOfQueryMinitorCall;
            QueryMinitorCallTimer.Start();
        }

        public void BackgroundServiceOfQueryMinitorCall(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // 查询左右调度
                Dictionary<string, string> dict = CommonDB.GetDispatchNum();
                string leftDispatchNumber = dict["left"];
                string rightDispatchNumber = dict["right"];

                // 处理与调度通话的号码
                List<string> list = CommonDB.QueryAllCallingNumberByDispatchNumber(leftDispatchNumber, rightDispatchNumber);

                // 存在新查询的通话
                if (list.Count > 0)
                {
                    string nowCallNumber = list[0];

                    // 场景一：当前没有通话中的电话
                    if (nowOpenMonitorNumber == null && nowOpenMonitorWindow == null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Dictionary<string, string> monitorDict = CommonDB.getMonitorDataByNumber(nowCallNumber);
                            VideoCallWebSocketView videoCallWebSocketView = new VideoCallWebSocketView(monitorDict);
                            videoCallWebSocketView.Show();

                            nowOpenMonitorNumber = nowCallNumber;
                            nowOpenMonitorWindow = videoCallWebSocketView;

                            Console.WriteLine("打开 " + nowOpenMonitorNumber);
                        });
                    }

                    // 场景二：当前有通话中的电话
                    else if (nowOpenMonitorNumber != null && nowOpenMonitorWindow != null)
                    {
                        Console.WriteLine($"当前已有{nowOpenMonitorNumber}，新查询{nowCallNumber} 忽略");
                    }

                    // 其他
                    else
                    {
                        Console.WriteLine($"其他 忽略");
                    }
                }
                // 不存在
                else
                {
                    if (nowOpenMonitorNumber != null)
                    {
                        Console.WriteLine("关闭 " + nowOpenMonitorNumber);
                    }
                    nowOpenMonitorNumber = null;
                    if (nowOpenMonitorWindow != null)
                    {
                        nowOpenMonitorWindow.CloseWindowCommand(null, null);
                        nowOpenMonitorWindow = null;
                    }
                }
            });
        }


        #endregion



        #region 窗口控制

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void quit_Click(object sender, RoutedEventArgs e)
        {
            PasswordWindow passwordWindow = new PasswordWindow();
            if (passwordWindow.ShowDialog() == true)
            {
                LoginViewModel viewModel = new LoginViewModel();
                if (passwordWindow.EnteredPassword == viewModel.Password)
                {
                    base.OnClosed(e);
                    Application.Current.Shutdown();

                    // 退出时 清理资源
                    SipUtil.videoClosing();
                    MonitorUtil.CleanMonitorSDK();



                    // 关闭显示时间的定时器
                    if (timeTimer != null)
                    {
                        timeTimer.Close();
                    }
                }
                else
                {
                    DMMessageBox.Show("警告", "密码错误,请重试!!", DMMessageType.MESSAGE_WARING);
                }
            }
        }

        #endregion



        #region 侧边栏状态
        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Visible;
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
        }

        private void GridTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
            ButtonCloseMenu.Visibility = Visibility.Visible;
        }
        #endregion



        #region 操作
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UserCdrView view = new UserCdrView();
            view.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MissCallView view = new MissCallView();
            view.ShowDialog();
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            BroadcastListView view = new BroadcastListView();
            view.ShowDialog();
        }


        // 情绪报警
        private void Button_Click_emotion_alarm(object sender, RoutedEventArgs e)
        {
            var view = new EmotionALarmView();
            view.ShowDialog();
        }


        private void OnUpdateMainForm()
        {
            this.people.Content = "无人值守";
            people.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BEBEBE"));
        }
        private void Button_Click_Setting(object sender, RoutedEventArgs e)
        {
            SettingView view = new SettingView();
            view.ShowDialog();
        }
        #endregion



        #region 有人、无人值守

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (people.Content.ToString() == "有人值守")
            {
                IsPeople isPeople = new IsPeople();
                isPeople.UpdateMainForm += OnUpdateMainForm;
                isPeople.ShowDialog();
            }
            else if (people.Content.ToString() == "无人值守")
            {
                // DB.ExecuteNonQuery("update config set ispeople = 1 where id = 1");
                IsPeopleDB.UpdateIsPeopleConfig2();
                people.Foreground = new SolidColorBrush(Colors.Green);
                people.Content = "有人值守";
            }
        }

        #endregion



        #region 标题修改

        private void LoadTitle()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ButtonText))
            {
                EditableButton.Content = Properties.Settings.Default.ButtonText;
            }
        }

        private void EditableButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 创建一个简单的输入对话框
            var inputDialog = new InputDialog("请输入新的标题:", EditableButton.Content.ToString());
            if (inputDialog.ShowDialog() == true)
            {
                // 更新按钮的内容
                EditableButton.Content = inputDialog.ResponseText;

                // 保存新的内容到应用程序设置
                Properties.Settings.Default.ButtonText = inputDialog.ResponseText;
                Properties.Settings.Default.Save();
            }
        }

        #endregion

    }

}
