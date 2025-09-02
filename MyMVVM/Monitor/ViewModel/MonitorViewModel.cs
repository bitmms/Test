using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing.Printing;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FFmpeg.AutoGen;
using HaiKang;
using HandyControl.Controls;
using MyMVVM.Common;
using MyMVVM.Common.Model;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.Monitor.Model;
using MyMVVM.Monitor.Utils;
using MyMVVM.Monitor.View;
using Timer = System.Threading.Timer;

namespace MyMVVM.Monitor.ViewModel
{
    public class MonitorViewModel : ViewModelsBase, IDisposable
    {
        private Color MonitorTypeButton = (Color)Application.Current.Resources["MonitorControlButton"];
        private Color MonitorOtherButton = (Color)Application.Current.Resources["MonitorOtherButton"];
        private Color MonitorButtonSelected = (Color)Application.Current.Resources["Selected"];
        private Color MonitorOnLine = (Color)Application.Current.Resources["UserOnline"];
        private Color MonitorUnOnLine = (Color)Application.Current.Resources["UserDefault"];

        private Color GroupUnSelected = (Color)Application.Current.Resources["GroupUnSelected"];
        private Color GroupSelected = (Color)Application.Current.Resources["GroupSelected"];
        private Color MonitorUnSelected = (Color)Application.Current.Resources["UserUnSelected"];
        private Color MonitorSelected = (Color)Application.Current.Resources["UserSelected"];
        private Color DispatchButtonUnSelected = (Color)Application.Current.Resources["DispatchButtonUnSelected"];
        private Color DispatchButtonSelected = (Color)Application.Current.Resources["DispatchButtonSelected"];


        private int NowDispatchStatu;
        private string NowDispatchNum;
        private DispatchNumModel _leftDispatchNumModel;
        private DispatchNumModel _rightDispatchNumModel;
        public DispatchNumModel LeftDispatchNumModel { get { return _leftDispatchNumModel; } set { SetProperty(ref _leftDispatchNumModel, value); } }
        public DispatchNumModel RightDispatchNumModel { get { return _rightDispatchNumModel; } set { SetProperty(ref _rightDispatchNumModel, value); } }


        private Timer DispatchTimer;
        private int CurrentPage;
        private IntPtr VideoOfIntPtr;
        private MonitorModel CurrentModel;
        private ButtonClickHandler _ButtonClickHandler;
        private List<MonitorModel> _SelectedMonitorList;
        private delegate void ButtonClickHandler(MonitorModel monitorModel);


        private ObservableCollection<MonitorModel> _MonitorDataList;
        private ObservableCollection<MonitorButton> _MonitorButtonList;
        public ObservableCollection<MonitorModel> MonitorDataList { get => _MonitorDataList; set => SetProperty(ref _MonitorDataList, value); }
        public ObservableCollection<MonitorButton> MonitorButtonList { get => _MonitorButtonList; set => SetProperty(ref _MonitorButtonList, value); }


        public MonitorViewModel(IntPtr intPtr)
        {
            Task.Run(() =>
            {
                CurrentPage = 1;
                VideoOfIntPtr = intPtr;
                _SelectedMonitorList = new List<MonitorModel>();
                _ButtonClickHandler = new ButtonClickHandler(SetVideoToSmallWindow);

                // 加载操作按钮
                LoadButtonList();

                // 首次加载左右调度
                FirstLoadDispatchNum();

                // 定时刷新加载左右调度
                DispatchTimer = new Timer(LoadDispatchNum, null, 0, 2000);

                // 加载首页的 64 个数据
                LoadMonitorListByPage(CurrentPage, MonitorDB.GetPageSize());

                // 实时预览第一个摄像仪
                if (MonitorDataList != null && MonitorDataList.Count >= 1)
                {
                    CurrentModel = MonitorDataList[0];
                    CurrentModel.LoginCode = MonitorUtil.LoginDevice(CurrentModel.IP, ushort.Parse(CurrentModel.Port.ToString()), CurrentModel.Username, CurrentModel.Password, CurrentModel.Type);
                    CurrentModel.PreVirwCode = MonitorUtil.PreViewDevice(CurrentModel.LoginCode, VideoOfIntPtr, CurrentModel.Type);
                }
            });

            TimerUpdateStatus();
        }



        #region 更新状态，检查摄像仪是否在线

        private System.Timers.Timer timeTimer = null;

        private void TimerUpdateStatus()
        {
            timeTimer = new System.Timers.Timer(1000);
            timeTimer.Elapsed += UpdateStatus;
            timeTimer.Start();
        }

        private void UpdateStatus(object sender, ElapsedEventArgs e)
        {
            if (MonitorDataList.Count > 4)
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < MonitorDataList.Count / 2 + 1; i++)
                    {
                        if (i < MonitorDataList.Count)
                        {
                            XXXYYY(MonitorDataList[i]);
                        }
                    }
                });
                Task.Run(() =>
                {
                    for (int i = MonitorDataList.Count / 2; i < MonitorDataList.Count; i++)
                    {
                        if (i < MonitorDataList.Count)
                        {
                            XXXYYY(MonitorDataList[i]);
                        }
                    }
                });
                Task.Run(() =>
                {
                    for (int i = MonitorDataList.Count - 1; i >= MonitorDataList.Count / 2 - 1; i--)
                    {
                        if (i < MonitorDataList.Count)
                        {
                            XXXYYY(MonitorDataList[i]);
                        }
                    }
                });
                Task.Run(() =>
                {
                    for (int i = MonitorDataList.Count / 2; i >= 0; i--)
                    {
                        if (i < MonitorDataList.Count)
                        {
                            XXXYYY(MonitorDataList[i]);
                        }
                    }
                });
            }
            else
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < MonitorDataList.Count; i++)
                    {
                        if (i < MonitorDataList.Count)
                        {
                            XXXYYY(MonitorDataList[i]);
                        }
                    }
                });
            }
        }

        public bool StatusQuery(String ip)//检查计算机是否能正常连接
        {
            bool message = false;
            Ping p = new Ping();
            try
            {
                PingReply r = p.Send(ip, 88);
                if (r.Status == IPStatus.Success)
                {
                    message = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return message;
        }


        private void XXXYYY(MonitorModel item)
        {
            if (StatusQuery(item.IP))
            {
                item.IsOnline = true;
                item.Background = DMUtil.ColorToHex(MonitorOnLine);
            }
            else
            {
                item.IsOnline = false;
                item.Background = DMUtil.ColorToHex(MonitorUnOnLine);
            }
        }


        #endregion




        #region 点击监控列表数据的回调函数


        // 点击后在小屏幕实时预览
        public void SetVideoToSmallWindow(MonitorModel monitorModel)
        {
            if (monitorModel.IsOnline)
            {
                if (CurrentModel != null && CurrentModel.IP != monitorModel.IP)
                {
                    MonitorUtil.StopPreView(CurrentModel.PreVirwCode, CurrentModel.Type);
                    MonitorUtil.StopLogin(CurrentModel.LoginCode, CurrentModel.Type);
                    CurrentModel = monitorModel;
                    CurrentModel.LoginCode = MonitorUtil.LoginDevice(CurrentModel.IP, ushort.Parse(CurrentModel.Port.ToString()), CurrentModel.Username, CurrentModel.Password, CurrentModel.Type);
                    CurrentModel.PreVirwCode = MonitorUtil.PreViewDevice(CurrentModel.LoginCode, VideoOfIntPtr, CurrentModel.Type);
                }
            }
        }

        // 点击后在大屏幕实时预览并自动开启语音对讲
        public void SetVideoToMaxWindowAndOpenTalk(MonitorModel monitorModel)
        {
            if (!monitorModel.IsOnline)
            {
                DMMessageBox.ShowInfo("该摄像仪处于掉线状态！");
            }
            else
            {
                new OneMonitorView(monitorModel, true).ShowDialog();
            }

            ResetAllButtonBackgroundColor(); // 关闭参考时刷新按钮颜色

            foreach (var item in MonitorDataList) // 同时刷新按钮的回调函数
            {
                item.ButtonCommand = new ViewModelCommand(_param =>
                {
                    if (item.IsOnline)
                    {
                        if (CurrentModel != null && CurrentModel.IP != item.IP)
                        {
                            MonitorUtil.StopPreView(CurrentModel.PreVirwCode, CurrentModel.Type);
                            MonitorUtil.StopLogin(CurrentModel.LoginCode, CurrentModel.Type);
                            CurrentModel = item;
                            CurrentModel.LoginCode = MonitorUtil.LoginDevice(CurrentModel.IP, ushort.Parse(CurrentModel.Port.ToString()), CurrentModel.Username, CurrentModel.Password, CurrentModel.Type);
                            CurrentModel.PreVirwCode = MonitorUtil.PreViewDevice(CurrentModel.LoginCode, VideoOfIntPtr, CurrentModel.Type);
                        }

                        _ButtonClickHandler = new ButtonClickHandler(SetVideoToSmallWindow);
                    }
                });
            }
        }

        // 点击后选择用户
        public void SelectMonitorToList(MonitorModel monitorModel)
        {
            if (monitorModel.IsOnline)
            {
                for (int i = 0; i < _SelectedMonitorList.Count; i++)
                {
                    if (_SelectedMonitorList[i].Id == monitorModel.Id)
                    {
                        _SelectedMonitorList.RemoveAt(i);
                        monitorModel.FontColor = DMUtil.ColorToHex(MonitorUnSelected);
                        return;
                    }
                }
                _SelectedMonitorList.Add(monitorModel);
                monitorModel.FontColor = DMUtil.ColorToHex(MonitorSelected);
            }
        }


        #endregion


        #region 操作按钮


        // 动态加载按钮
        public void LoadButtonList()
        {
            MonitorButtonList = new ObservableCollection<MonitorButton>();
            // 语音对讲
            MonitorButton button1 = new MonitorButton();
            button1.Id = 1;
            button1.Name = "语音对讲";
            button1.Icon = "CarHorn";
            button1.IsShow = true;
            button1.ButtonCommand = new ViewModelCommand(param => TalkButton(button1.Id));

            // 多方对讲
            MonitorButton button2 = new MonitorButton();
            button2.Id = 2;
            button2.Name = "多方对讲";
            button2.Icon = "CarHorn";
            button2.IsShow = true;
            button2.ButtonCommand = new ViewModelCommand(param => MoreTalkButton(button2.Id));

            // 开始多方对讲
            MonitorButton button3 = new MonitorButton();
            button3.Id = 3;
            button3.Name = "开始多方";
            button3.Icon = "CarHorn";
            button3.IsShow = true;
            button3.ButtonCommand = new ViewModelCommand(param => StartMoreTalkButton(button3.Id));

            // 恢复默认
            MonitorButton button4 = new MonitorButton();
            button4.Id = 4;
            button4.Name = "恢复默认";
            button4.Icon = "CarHorn";
            button4.IsShow = true;
            button4.ButtonCommand = new ViewModelCommand(param => ResetButton(button4.Id));

            // 占位按钮
            MonitorButton buttonTemp = new MonitorButton();
            buttonTemp.Id = 12;
            buttonTemp.Name = "占位按钮";
            buttonTemp.Icon = "Men";
            buttonTemp.IsShow = false;
            buttonTemp.ButtonCommand = new ViewModelCommand(param => XXXYYY(buttonTemp.Id));

            // 调整顺序：
            Dictionary<int, MonitorButton> dict = new Dictionary<int, MonitorButton>();
            dict.Add(1, button1);
            dict.Add(2, button2);
            dict.Add(3, button3);
            dict.Add(4, button4);
            dict.Add(5, buttonTemp);
            dict.Add(7, buttonTemp);
            dict.Add(6, buttonTemp);
            dict.Add(8, buttonTemp);

            dict.Add(9, buttonTemp);
            dict.Add(10, buttonTemp);
            dict.Add(11, buttonTemp);
            dict.Add(12, buttonTemp);
            dict.Add(13, buttonTemp);
            dict.Add(14, buttonTemp);
            dict.Add(15, buttonTemp);
            dict.Add(16, buttonTemp);

            // 渲染到页面
            foreach (var item in dict.Values)
            {
                MonitorButtonList.Add(item);
            }
            ResetAllButtonBackgroundColor();
        }

        // 语音对讲按钮
        private void TalkButton(int buttonId)
        {
            ClickButtonChangeBackgroundColor(buttonId);

            _ButtonClickHandler = new ButtonClickHandler(SetVideoToMaxWindowAndOpenTalk);

            foreach (var monitorModel in MonitorDataList)
            {
                monitorModel.ButtonCommand = new ViewModelCommand(param =>
                {
                    _ButtonClickHandler(monitorModel);
                });
            }
        }

        // 多方对讲按钮
        private void MoreTalkButton(int buttonId)
        {
            ClickButtonChangeBackgroundColor(buttonId);

            _ButtonClickHandler = new ButtonClickHandler(SelectMonitorToList);

            foreach (var monitorModel in MonitorDataList)
            {
                monitorModel.ButtonCommand = new ViewModelCommand(param =>
                {
                    _ButtonClickHandler(monitorModel);
                });
            }
        }

        // 开始多方按钮
        private void StartMoreTalkButton(int buttonId)
        {
            ClickButtonChangeBackgroundColor(buttonId);

            if (_SelectedMonitorList.Count == 0)
            {
                DMMessageBox.ShowInfo("未选择摄像仪");
            }
            else if (_SelectedMonitorList.Count == 1)
            {
                SetVideoToMaxWindowAndOpenTalk(_SelectedMonitorList[0]);
                _SelectedMonitorList.Clear();
            }
            else
            {
                for (int i = _SelectedMonitorList.Count - 1; i >= 0; i--)
                {
                    if (!_SelectedMonitorList[i].IsOnline)
                    {
                        _SelectedMonitorList.RemoveAt(i);
                    }
                }
                new MoreMonitorView(_SelectedMonitorList).ShowDialog();
                _SelectedMonitorList.Clear();
            }

            UpdateMonitorStatusBySelectedList();

            ResetAllButtonBackgroundColor();

            foreach (var item in MonitorDataList) // 同时刷新按钮的回调函数
            {
                item.ButtonCommand = new ViewModelCommand(_param =>
                {
                    if (item.IsOnline)
                    {
                        if (CurrentModel != null && CurrentModel.IP != item.IP)
                        {
                            MonitorUtil.StopPreView(CurrentModel.PreVirwCode, CurrentModel.Type);
                            MonitorUtil.StopLogin(CurrentModel.LoginCode, CurrentModel.Type);
                            CurrentModel = item;
                            CurrentModel.LoginCode = MonitorUtil.LoginDevice(CurrentModel.IP, ushort.Parse(CurrentModel.Port.ToString()), CurrentModel.Username, CurrentModel.Password, CurrentModel.Type);
                            CurrentModel.PreVirwCode = MonitorUtil.PreViewDevice(CurrentModel.LoginCode, VideoOfIntPtr, CurrentModel.Type);
                        }

                        _ButtonClickHandler = new ButtonClickHandler(SetVideoToSmallWindow);
                    }
                });
            }
        }

        // 恢复默认按钮
        private void ResetButton(int buttonId)
        {
            ResetAllButtonBackgroundColor();

            _ButtonClickHandler = new ButtonClickHandler(SetVideoToSmallWindow);

            foreach (var item in MonitorDataList)
            {
                item.ButtonCommand = new ViewModelCommand(param =>
                {
                    _ButtonClickHandler(item);
                });

                item.FontColor = DMUtil.ColorToHex(MonitorUnSelected);
            }
            _SelectedMonitorList.Clear();
        }

        // 占位按钮
        private void XXXYYY(int _)
        {

        }

        // 设置全部按钮到默认的背景颜色
        public void ResetAllButtonBackgroundColor()
        {
            foreach (var button in MonitorButtonList)
            {
                if (button.Id == 1 || button.Id == 2 || button.Id == 3)
                {
                    button.BackgroundColor = DMUtil.ColorToHex(MonitorTypeButton);
                }
                else
                {
                    button.BackgroundColor = DMUtil.ColorToHex(MonitorOtherButton);
                }
            }
        }

        // 点击某个按钮后改变当前按钮的背景颜色
        public void ClickButtonChangeBackgroundColor(int buttonId)
        {
            ResetAllButtonBackgroundColor();
            foreach (var item in MonitorButtonList)
            {
                if (item.Id == buttonId)
                {
                    item.BackgroundColor = DMUtil.ColorToHex(MonitorButtonSelected);
                    break;
                }
            }
        }

        // 小屏实时预览 -->> 大屏实时预览
        public ICommand OpenOneMonitorView => new ViewModelCommand(param =>
        {
            new OneMonitorView(CurrentModel).ShowDialog();
        });


        #endregion


        #region 分页加载主区域列表的数据


        public ICommand PreviousPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage > 1)
            {
                LoadMonitorListByPage(--CurrentPage, MonitorDB.GetPageSize());
            }
        });


        public ICommand NextPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage < MonitorDB.GetTotalPages())
            {
                LoadMonitorListByPage(++CurrentPage, MonitorDB.GetPageSize());
            }
        });


        private void LoadMonitorListByPage(int pageIndex, int pageSize)
        {
            List<MonitorModel> monitorModels = MonitorDB.GetAllMonitorByPage(pageIndex, pageSize);
            foreach (var item in monitorModels)
            {
                item.ButtonCommand = new ViewModelCommand(param =>
                {
                    _ButtonClickHandler(item);
                });
                item.FontColor = DMUtil.ColorToHex(MonitorUnSelected);
                item.Background = DMUtil.ColorToHex(MonitorOnLine);
                item.IsOnline = true;
            }
            MonitorDataList = new ObservableCollection<MonitorModel>(monitorModels);
            UpdateMonitorStatusBySelectedList();
        }


        private void UpdateMonitorStatusBySelectedList()
        {
            foreach (var item in MonitorDataList)
            {
                bool f = true;
                for (int i = 0; i < _SelectedMonitorList.Count; i++)
                {
                    if (item.Id == _SelectedMonitorList[i].Id)
                    {
                        f = false;
                        break;
                    }
                }
                if (f)
                {
                    item.FontColor = DMUtil.ColorToHex(MonitorUnSelected);
                }
                else
                {
                    item.FontColor = DMUtil.ColorToHex(MonitorSelected);
                }
            }
        }


        #endregion


        #region 左右调度


        /// <summary>
        /// 首次加载左右调度
        /// </summary>
        private void FirstLoadDispatchNum()
        {
            LeftDispatchNumModel = new DispatchNumModel();
            RightDispatchNumModel = new DispatchNumModel();

            // 1. 查数据库
            Dictionary<string, string> dispatchNum = CommonDB.GetDispatchNum();

            // 2. 设置 Model 的初始信息信息
            LeftDispatchNumModel.Name = "左调度";
            LeftDispatchNumModel.Num = dispatchNum["left"];
            LeftDispatchNumModel.Image = "pack://application:,,,/Common/Images/Lphone.png";
            LeftDispatchNumModel.FontColor = DMUtil.ColorToHex(DispatchButtonUnSelected);
            LeftDispatchNumModel.DispatchNumCommand = new ViewModelCommand(param =>
            {
                NowDispatchStatu = 0; // 此时不需要使用默认值，而是手动设置的左右值
                LoadDispatchNum(null); // 点击切换时从数据库重新加载调度号
            });
            RightDispatchNumModel.Name = "右调度";
            RightDispatchNumModel.Num = dispatchNum["right"];
            RightDispatchNumModel.Image = "pack://application:,,,/Common/Images/Rphone.png";
            RightDispatchNumModel.FontColor = DMUtil.ColorToHex(DispatchButtonUnSelected);
            RightDispatchNumModel.DispatchNumCommand = new ViewModelCommand(param =>
            {
                NowDispatchStatu = 1; // 此时不需要使用默认值，而是手动设置的左右值
                LoadDispatchNum(null); // 点击切换时从数据库重新加载调度号
            });
        }


        /// <summary>
        /// 定时加载左右调度
        /// </summary>
        private void LoadDispatchNum(object obj)
        {
            // 1. 查数据库
            Dictionary<string, string> dispatchNum = CommonDB.GetDispatchNum();

            // 2. 加载从数据库获取到的实时调度号码
            LeftDispatchNumModel.Num = dispatchNum["left"];
            RightDispatchNumModel.Num = dispatchNum["right"];

            // 3. 修改调度优先状态
            if (NowDispatchStatu == 0)
            {
                NowDispatchNum = LeftDispatchNumModel.Num;
                LeftDispatchNumModel.FontColor = DMUtil.ColorToHex(DispatchButtonSelected);
                RightDispatchNumModel.FontColor = DMUtil.ColorToHex(DispatchButtonUnSelected);
            }
            else
            {
                NowDispatchNum = RightDispatchNumModel.Num;
                LeftDispatchNumModel.FontColor = DMUtil.ColorToHex(DispatchButtonUnSelected);
                RightDispatchNumModel.FontColor = DMUtil.ColorToHex(DispatchButtonSelected);
            }

            // 4. 修改图标状态，共两种状态：【空闲】或者【振铃、通话、鉴权】
            if (CommonDB.GetDispatchNumStatus(LeftDispatchNumModel.Num)) // 振铃、通话、鉴权时
            {
                LeftDispatchNumModel.Image = "pack://application:,,,/Common/Images/ring.png";
            }
            else
            {
                LeftDispatchNumModel.Image = "pack://application:,,,/Common/Images/Lphone.png";
            }
            if (CommonDB.GetDispatchNumStatus(RightDispatchNumModel.Num)) // 振铃、通话、鉴权时
            {
                RightDispatchNumModel.Image = "pack://application:,,,/Common/Images/ring.png";
            }
            else
            {
                RightDispatchNumModel.Image = "pack://application:,,,/Common/Images/Rphone.png";
            }
        }


        #endregion


        #region 释放资源


        public void Dispose()
        {
            if (CurrentModel != null)
            {
                MonitorUtil.StopPreView(CurrentModel.PreVirwCode, CurrentModel.Type);
                MonitorUtil.StopLogin(CurrentModel.LoginCode, CurrentModel.Type);
            }
            timeTimer.Stop();
            timeTimer.Elapsed -= UpdateStatus;
            timeTimer.Dispose();
            timeTimer = null;
        }


        #endregion
    }
}