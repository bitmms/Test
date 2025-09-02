using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MyMVVM.Broadcast;
using MyMVVM.Common;
using MyMVVM.Common.Model;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;

namespace MyMVVM.Conference.ViewModel
{
    public class ConferenceViewModel : ViewModelsBase, IDisposable
    {


        #region 构造器
        public ConferenceViewModel()
        {
            // 初始化参数
            NowDispatchStatu = 0; // 仅可以初始化当前是左调度，但是左调度的具体号码需要后续动态加载
            LeftDispatchNumModel = new DispatchNumModel();
            RightDispatchNumModel = new DispatchNumModel();
            SelectedUserList = new ObservableCollection<string>();
            TempAllButtons = new ObservableCollection<DefaultUserModel>();

            BroadcastTypeButton = (Color)Application.Current.Resources["BroadcastTypeButton"];
            BroadcastControlButton = (Color)Application.Current.Resources["BroadcastControlButton"];
            BroadcastOtherButton = (Color)Application.Current.Resources["BroadcastOtherButton"];

            Selected = (Color)Application.Current.Resources["Selected"];
            GroupUnSelected = (Color)Application.Current.Resources["GroupUnSelected"];
            GroupSelected = (Color)Application.Current.Resources["GroupSelected"];
            UserDefault = (Color)Application.Current.Resources["UserDefault"];
            UserOnline = (Color)Application.Current.Resources["UserOnline"];
            UserCalling = (Color)Application.Current.Resources["UserCalling"];
            UserRinging = (Color)Application.Current.Resources["UserRinging"];
            UserUnSelected = (Color)Application.Current.Resources["UserUnSelected"];
            UserSelected = (Color)Application.Current.Resources["UserSelected"];

            DispatchButtonUnSelected = (Color)Application.Current.Resources["DispatchButtonUnSelected"];
            DispatchButtonSelected = (Color)Application.Current.Resources["DispatchButtonSelected"];

            // 先加载组
            GroupDataList = new ObservableCollection<GroupModel>();
            LoadGroupButton();

            // 再加载用户
            UserDataList = new ObservableCollection<DefaultUserModel>();
            if (SelectedGroupModel != null)
            {
                LoadButtonForm();
            }

            // 加载调度按钮
            DispatchButtonModelList = new ObservableCollection<DispatchButtonModel>();
            LoadDispatchButtonList();

            // 首次加载左右调度
            FirstLoadDispatchNum();

            // 定时任务
            _timer1 = new Timer(UpdateOnlineUser, null, 0, 1000);  // 用户状态
            _timer2 = new Timer(LoadDispatchNum, null, 0, 1000);   // 左右调度

            Dictionary<string, string> dict = CommonDB.GetFunctionNumber();
            queryNowNumber = dict["number"];
            queryNowTime = dict["date"];
            queryNowMissCall = dict["misscall"];
        }
        #endregion

        public void Dispose()
        {

        }


        #region 变量的定义

        private string queryNowNumber = "*114";
        private string queryNowTime = "*115";
        private string queryNowMissCall = "*117";


        private Color BroadcastTypeButton;
        private Color BroadcastControlButton;
        private Color BroadcastOtherButton;

        private Color Selected;

        private Color GroupUnSelected;
        private Color GroupSelected;

        private Color UserDefault;
        private Color UserOnline;
        private Color UserCalling;
        private Color UserRinging;
        private Color UserUnSelected;
        private Color UserSelected;

        private Color DispatchButtonUnSelected;
        private Color DispatchButtonSelected;



        // 组
        private GroupModel SelectedGroupModel;
        private ObservableCollection<GroupModel> _groupDataList;
        public ObservableCollection<GroupModel> GroupDataList { get => _groupDataList; set => SetProperty(ref _groupDataList, value); }


        // 用户
        public ObservableCollection<string> SelectedUserList;
        /// <summary>
        /// 当前组的全部用户
        /// </summary>
        private ObservableCollection<DefaultUserModel> TempAllButtons;

        private ObservableCollection<DefaultUserModel> _userDataList;
        /// <summary>
        /// 当前组在当前页的全部用户
        /// </summary>
        public ObservableCollection<DefaultUserModel> UserDataList { get => _userDataList; set => SetProperty(ref _userDataList, value); }


        // 调度号码
        /// <summary>
        /// 左右调度的优先状态
        ///     - 0 左调度优先
        ///     - 1 右调度优先
        /// </summary>
        private int NowDispatchStatu;
        private string NowDispatchNum;
        private DispatchNumModel _leftDispatchNumModel;
        private DispatchNumModel _rightDispatchNumModel;
        public DispatchNumModel LeftDispatchNumModel { get { return _leftDispatchNumModel; } set { SetProperty(ref _leftDispatchNumModel, value); } }
        public DispatchNumModel RightDispatchNumModel { get { return _rightDispatchNumModel; } set { SetProperty(ref _rightDispatchNumModel, value); } }


        // 分页
        private int _totalPages;
        private int _currentPage;
        private bool _isShowPageButton;
        public int TotalPages { get => _totalPages; set => SetProperty(ref _totalPages, value); }
        public int CurrentPage { get => _currentPage; set { if (SetProperty(ref _currentPage, value)) { UpdatePage(); } } }
        public bool IsShowPageButton { get => _isShowPageButton; set => SetProperty(ref _isShowPageButton, value); }


        // 调度按钮
        private ObservableCollection<DispatchButtonModel> _dispatchButtonModelList;
        public ObservableCollection<DispatchButtonModel> DispatchButtonModelList { get => _dispatchButtonModelList; set => SetProperty(ref _dispatchButtonModelList, value); }


        // 定时器
        private Timer _timer1; // 用户状态
        private Timer _timer2; // 左右调度



        #endregion



        #region 加载组，加载用户

        /// <summary>
        /// 先查组，从数据库查询数据，加载全部的组信息，为每个组按钮绑定 Command，并默认设置当前组为全部组的第一个组
        /// </summary>
        private void LoadGroupButton()
        {
            // 1. 查询所有的组
            GroupDataList = ConferenceDB.GetAllConferenceGroup();

            // 组名称修改
            foreach (var item in GroupDataList)
            {
                item.GroupName = item.GroupName + $"({item.CallId})";
            }

            // 2. 给所有的组按钮绑定 Command
            foreach (GroupModel model in GroupDataList)
            {
                model.GroupButtonCommand = new ViewModelCommand(param =>
                {
                    SelectedGroupModel = model;
                    ChangeGroupCommand();
                    SelectedUserList.Clear(); // 切换组用户时清空用户列表，跨组选人除外
                });
                model.GroupButtonColor = DMUtil.ColorToHex(GroupUnSelected);
            }

            // 3. 首次加载，设置第一个组为当前组
            if (GroupDataList != null && GroupDataList.Count > 0)
            {
                SelectedGroupModel = GroupDataList[0];

                // 第一个组按钮的颜色设置为选中状态
                GroupDataList[0].GroupButtonColor = DMUtil.ColorToHex(GroupSelected);
            }
        }


        /// <summary>
        /// 再查用户，从当前组查询组内的全部用户，利用页码大小、当前页码的变化来动态展示内容
        /// </summary>
        private void LoadButtonForm()
        {
            // 1. 查询组内所有用户
            TempAllButtons.Clear();
            ConferenceDB.GetUserListByGroupId(SelectedGroupModel.Id, TempAllButtons);
            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                if (userModel.CameraIP != null && userModel.CameraIP != "")
                {
                    userModel.IsShowCamera = true;
                }
                else
                {
                    userModel.IsShowCamera = false;
                }
                userModel.IsNotShowCamera = !userModel.IsShowCamera;

                userModel.BackgroundColor = DMUtil.ColorToHex(UserDefault);
                userModel.ButtonCommand = new ViewModelCommand(param => DianhuUserButtonCommand(userModel.Usernum));
            }
            // 2. 分页
            TotalPages = (int)Math.Ceiling((double)TempAllButtons.Count / 64);
            CurrentPage = 1; // 默认显示第一页
            IsShowPageButton = TempAllButtons.Count > 64; // 查看是否有第二页
        }


        /// <summary>
        /// 点击组按钮，触发对应的 Command，每次选择一个组时，重新展示用户列表
        /// </summary>
        private void ChangeGroupCommand()
        {
            // 1. 清空已经选择的用户
            SelectedUserList.Clear();
            // 2. 广播按钮颜色恢复默认
            ResetDispatchButtonBackgroundColor();

            // 1. 根据组按钮是否被选择去改变颜色
            foreach (GroupModel model in GroupDataList)
            {
                if (model.Id == SelectedGroupModel.Id)
                {
                    model.GroupButtonColor = DMUtil.ColorToHex(GroupSelected);
                }
                else
                {
                    model.GroupButtonColor = DMUtil.ColorToHex(GroupUnSelected);
                }
            }

            // 2. 查询所有在线用户的号码
            List<string> onlineUserList = CommonDB.GetOnlineUserNum();

            // 3. 每次切换组，都要清空所有的用户
            UserDataList.Clear();
            TempAllButtons.Clear();

            // 4. 重新查询指定组的所有用户
            ConferenceDB.GetUserListByGroupId(SelectedGroupModel.Id, TempAllButtons);
            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                if (userModel.CameraIP != null && userModel.CameraIP != "")
                {
                    userModel.IsShowCamera = true;
                }
                else
                {
                    userModel.IsShowCamera = false;
                }
                userModel.IsNotShowCamera = !userModel.IsShowCamera;

                userModel.BackgroundColor = DMUtil.ColorToHex(UserDefault);
                userModel.ButtonCommand = new ViewModelCommand(param => DianhuUserButtonCommand(userModel.Usernum));
            }

            // 5. 计算页面大小，并动态展示
            TotalPages = (int)Math.Ceiling((double)TempAllButtons.Count / 64);
            CurrentPage = 1; // 默认显示第一页的用户
            foreach (var button in TempAllButtons.Skip((CurrentPage - 1) * 64).Take(64))
            {
                UserDataList.Add(button);
            }

            // 6. 更新 Buttons 中按钮的状态变化
            UpdateOnlineUser(null);
            IsShowPageButton = TempAllButtons.Count > 64; // 查看是否有第二页
        }


        #endregion



        #region 用户状态的变化


        private void UpdateOnlineUser(object obj)
        {
            // 用户被选中
            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.UserButtonFontColor = DMUtil.ColorToHex(UserUnSelected);
                if (SelectedUserList.Contains(userModel.Usernum))
                {
                    userModel.UserButtonFontColor = DMUtil.ColorToHex(UserSelected);
                }
            }

            // 获取在线用户 
            List<string> onlineUserList = new List<string>();
            foreach (DataRow row in CommonDB.GetOnlineUser().Rows)
            {
                onlineUserList.Add(row["sip_user"].ToString());
            }

            // 获取通话中的用户
            List<string> callUserList = new List<string>();
            foreach (DataRow row in CommonDB.GetDetailedCalls().Rows)
            {
                callUserList.Add(row["cid_num"].ToString());
                callUserList.Add(row["dest"].ToString());
            }

            // 用户状态变化
            for (int i = 0; i < TempAllButtons.Count; i++)
            {
                var userModel = TempAllButtons[i];
                //如果用户处于通话中,则无需更新为在线状态
                if (callUserList.Contains(userModel.Usernum))
                {
                    continue;
                }
                // 如果用户已经不在通话表中但在线,则更新为在线状态
                if (onlineUserList.Contains(userModel.Usernum))
                {
                    userModel.BackgroundColor = DMUtil.ColorToHex(UserOnline);
                    userModel.UserDisplay = userModel.Usernum;
                }
            }

            // 判断是否存在振铃、通话中的用户
            if (!CommonDB.IsBFieldEmpty())
            {
                DataTable dt = CommonDB.GetDetailedCalls();
                foreach (DataRow row in dt.Rows)
                {
                    string cidNum = row["cid_num"].ToString();
                    string cidName = row["cid_name"].ToString();
                    string destNum = row["dest"].ToString();
                    string application = row["application"].ToString();
                    string calleeNum = row["callee_num"].ToString();
                    string calleeName = row["callee_name"].ToString();
                    string callState = row["callstate"].ToString();
                    UpdateButtonColor(cidNum, cidName, destNum, application, calleeNum, calleeName, callState);
                }
            }
        }

        private void UpdateButtonColor(string cidNum, string cidName, string destNum, string application, string calleeNum, string calleeName, string callState)
        {
            // 按 # 呼调度
            //if (destNum == "#")
            //{
            //    UpdateSingleButtonColor(TempAllButtons.FirstOrDefault(temp => temp.Usernum == cidNum), cidNum, calleeNum, callState);
            //    // UpdateSingleButtonColor(TempAllButtons.FirstOrDefault(temp => temp.Usernum == calleeNum), cidNum, calleeNum, callState);
            //    return;
            //}

            // 临时会议组【√】
            if (calleeName.Contains("会议") || destNum.Length == 2)
            {
                Match match = Regex.Match(calleeName, @"\((\d+)\)");
                string conferenceNum = "";
                if (match.Success)
                {
                    conferenceNum = $"({match.Groups[1].Value})";
                }

                if (destNum.Length > 2)
                {
                    DefaultUserModel SIPUserButton = TempAllButtons.FirstOrDefault(temp => temp.Usernum == destNum);
                    if (SIPUserButton != null)
                    {
                        SIPUserButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                        SIPUserButton.UserDisplay = $"会议中{conferenceNum}";
                    }
                }
                else
                {
                    DefaultUserModel SIPUserButton = TempAllButtons.FirstOrDefault(temp => temp.Usernum == cidNum);
                    if (SIPUserButton != null)
                    {
                        SIPUserButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                        SIPUserButton.UserDisplay = $"会议中({destNum})";
                    }
                }
                return;
            }


            // 组呼【√】
            if (calleeName.Contains("组呼") || destNum.StartsWith("#17") || cidName.Contains("组呼"))
            {
                string toButton = "";
                if (destNum.StartsWith("#17"))
                {
                    toButton = cidNum;
                }
                else
                {
                    toButton = destNum;
                }
                DefaultUserModel userButton = TempAllButtons.FirstOrDefault(temp => temp.Usernum == toButton);
                if (userButton == null) return;
                switch (callState)
                {
                    case "RINGING":
                        userButton.BackgroundColor = DMUtil.ColorToHex(UserRinging);
                        userButton.UserDisplay = $"组呼中";
                        break;
                    case "ACTIVE":
                        userButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                        userButton.UserDisplay = $"组呼中";
                        break;
                    default:
                        break;
                }
                return;
            }


            // 多方通话【√】
            if (calleeName.Contains("多方通话") || destNum.StartsWith("#16") || cidName.Contains("多方通话"))
            {
                string toButton = "";
                if (destNum.StartsWith("#16"))
                {
                    toButton = cidNum;
                }
                else
                {
                    toButton = destNum;
                }

                DefaultUserModel userButton = TempAllButtons.FirstOrDefault(temp => temp.Usernum == toButton);
                if (userButton == null) return;
                switch (callState)
                {
                    case "RINGING":
                        userButton.BackgroundColor = DMUtil.ColorToHex(UserRinging);
                        userButton.UserDisplay = $"多方通话中";
                        break;
                    case "ACTIVE":
                        userButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                        userButton.UserDisplay = $"多方通话中";
                        break;
                    default:
                        break;
                }

                return;
            }


            // 全呼【√】
            if (calleeName.Contains("全呼") || destNum.StartsWith("666888") || cidName.Contains("全呼"))
            {
                string toButton = "";
                if (destNum.StartsWith("666888"))
                {
                    toButton = cidNum;
                }
                else
                {
                    toButton = destNum;
                }

                DefaultUserModel userButton = TempAllButtons.FirstOrDefault(temp => temp.Usernum == toButton);
                if (userButton == null) return;
                switch (callState)
                {
                    case "RINGING":
                        userButton.BackgroundColor = DMUtil.ColorToHex(UserRinging);
                        userButton.UserDisplay = $"全呼中";
                        break;
                    case "ACTIVE":
                        userButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                        userButton.UserDisplay = $"全呼中";
                        break;
                    default:
                        break;
                }

                return;
            }


            // 带 * 的功能号码【√】
            if (destNum.StartsWith("*"))
            {
                if (destNum == queryNowNumber)
                {
                    DefaultUserModel SIPUserButton = TempAllButtons.FirstOrDefault(temp => temp.Usernum == cidNum);
                    if (SIPUserButton != null)
                    {
                        SIPUserButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                        SIPUserButton.UserDisplay = "查询号码中";
                    }
                }

                else if (destNum == queryNowTime)
                {
                    DefaultUserModel SIPUserButton = TempAllButtons.FirstOrDefault(temp => temp.Usernum == cidNum);
                    if (SIPUserButton != null)
                    {
                        SIPUserButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                        SIPUserButton.UserDisplay = "查询时间中";
                    }
                }

                else if (destNum == queryNowMissCall)
                {
                    DefaultUserModel SIPUserButton = TempAllButtons.FirstOrDefault(temp => temp.Usernum == cidNum);
                    if (SIPUserButton != null)
                    {
                        SIPUserButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                        SIPUserButton.UserDisplay = "查询未接来电中";
                    }
                }

                else
                {
                    DefaultUserModel SIPUserButton = TempAllButtons.FirstOrDefault(temp => temp.Usernum == cidNum);
                    if (SIPUserButton != null)
                    {
                        SIPUserButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                        SIPUserButton.UserDisplay = $"{cidNum} ➔ {destNum}";
                    }
                }

                return;
            }


            // 广播中【√】
            if (cidNum == DMConfig.ConferenceCallerInfo.OriginationCallerNumber || calleeNum == DMConfig.ConferenceCallerInfo.OriginationCallerNumber)
            {
                DefaultUserModel SIPUserButton = TempAllButtons.FirstOrDefault(temp => temp.Usernum == destNum);
                if (SIPUserButton != null)
                {
                    SIPUserButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                    SIPUserButton.UserDisplay = $"广播中";
                }
                return;
            }


            // 普通的 A->B，A点呼B
            UpdateSingleButtonColor(TempAllButtons.FirstOrDefault(temp => temp.Usernum == cidNum), cidNum, destNum, callState);
            UpdateSingleButtonColor(TempAllButtons.FirstOrDefault(temp => temp.Usernum == destNum), cidNum, destNum, callState);
        }



        private void UpdateSingleButtonColor(DefaultUserModel userButton, string user1, string user2, string callState)
        {
            if (userButton == null) return;
            switch (callState)
            {
                case "RINGING":
                    userButton.BackgroundColor = DMUtil.ColorToHex(UserRinging);
                    userButton.UserDisplay = $"{user1} ➔ {user2}";
                    break;
                case "ACTIVE":
                    userButton.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                    userButton.UserDisplay = $"{user1} ➔ {user2}";
                    break;
                default:
                    break;
            }
        }


        #endregion



        #region 动态加载调度按钮，方便后续点击变色

        /// <summary>
        /// 复原调度按钮颜色
        /// </summary>
        public void ResetDispatchButtonBackgroundColor()
        {
            foreach (DispatchButtonModel dispatchButtonModel in DispatchButtonModelList)
            {
                if (dispatchButtonModel.Id >= 1 && dispatchButtonModel.Id <= 4)
                {
                    dispatchButtonModel.BackgroundColor = DMUtil.ColorToHex(BroadcastTypeButton);
                }
                else if (dispatchButtonModel.Id == 5)
                {
                    dispatchButtonModel.BackgroundColor = DMUtil.ColorToHex(BroadcastControlButton);
                }
                else
                {
                    dispatchButtonModel.BackgroundColor = DMUtil.ColorToHex(BroadcastOtherButton);
                }
            }
        }

        /// <summary>
        /// 调度按钮点击变色：先复原其他按钮颜色，再改变被点击的按钮颜色
        /// </summary>
        public void ClickDispatchButtonChangeBackgroundColor(int Id)
        {
            ResetDispatchButtonBackgroundColor();
            foreach (var item in DispatchButtonModelList)
            {
                if (item.Id == Id)
                {
                    item.BackgroundColor = DMUtil.ColorToHex(Selected);
                    break;
                }
            }
        }


        /// <summary>
        /// 动态加载调度按钮
        /// </summary>
        private void LoadDispatchButtonList()
        {
            List<DispatchButtonModel> buttons = new List<DispatchButtonModel>();

            // ID 不可变
            // 邀请用户
            DispatchButtonModel button1 = new DispatchButtonModel();
            button1.Id = 1;
            button1.Name = "邀请用户";
            button1.Icon = "AccountMultiplePlus";
            button1.IsShow = 1;
            button1.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand10(button1.Id));


            // 移除用户
            DispatchButtonModel button2 = new DispatchButtonModel();
            button2.Id = 2;
            button2.Name = "移除用户";
            button2.Icon = "AccountMultipleRemove";
            button2.IsShow = 1;
            button2.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand11(button2.Id));

            // 快速会议
            DispatchButtonModel button4 = new DispatchButtonModel();
            button4.Id = 4;
            button4.Name = "快速会议";
            button4.Icon = "ClockwiseArrows";
            button4.IsShow = 1;
            button4.ButtonCommand = new ViewModelCommand(param => QuickCreateConference(button4.Id));

            // 停止会议
            DispatchButtonModel button3 = new DispatchButtonModel();
            button3.Id = 3;
            button3.Name = "停止会议";
            button3.Icon = "ClockwiseArrows";
            button3.IsShow = 1;
            button3.ButtonCommand = new ViewModelCommand(param => QuickStopConference(button3.Id));

            // 恢复默认
            DispatchButtonModel button5 = new DispatchButtonModel();
            button5.Id = 5;
            button5.Name = "恢复默认";
            button5.Icon = "ClockwiseArrows";
            button5.IsShow = 1;
            button5.ButtonCommand = new ViewModelCommand(param => ResetDefaultCommand(button5.Id));

            // 占位按钮
            DispatchButtonModel buttonTemp = new DispatchButtonModel();
            buttonTemp.Id = 14;
            buttonTemp.Name = "占位按钮";
            buttonTemp.Icon = "Men";
            buttonTemp.IsShow = 0;
            buttonTemp.ButtonCommand = new ViewModelCommand(param => XXXYYY(buttonTemp.Id));

            // 调整顺序
            Dictionary<int, DispatchButtonModel> dict = new Dictionary<int, DispatchButtonModel>();
            dict.Add(1, button1);
            dict.Add(2, button2);
            dict.Add(3, button4);
            dict.Add(4, button3);

            dict.Add(5, button5);
            dict.Add(6, buttonTemp);
            dict.Add(7, buttonTemp);
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
                DispatchButtonModelList.Add(item);
            }

            ResetDispatchButtonBackgroundColor();
        }

        #endregion



        #region 翻页
        // 更新当前页面内容
        //  - 1. 用户状态改变时手动执行
        //  - 2. 当前页码变量 CurrentPage 改变时自动执行
        private void UpdatePage()
        {
            UserDataList.Clear();
            foreach (var button in TempAllButtons.Skip((CurrentPage - 1) * 64).Take(64))
            {
                UserDataList.Add(button);
            }
        }

        // 处理上一页按钮的命令
        public ICommand PreviousPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        });

        // 处理下一页按钮的命令
        public ICommand NextPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
        });
        #endregion



        #region 左右调度



        /// <summary>
        /// 首次加载左右调度
        /// </summary>
        private void FirstLoadDispatchNum()
        {
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



        #region 邀请用户、移除用户、恢复默认、快速会议、停止会议


        /// <summary>
        /// 邀请用户
        /// </summary>
        public void ChangeButtonBehaviorCommand10(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param => InviteUserButtonCommand(userModel.Usernum));
            }
        }


        /// <summary>
        /// 移除用户
        /// </summary>
        public void ChangeButtonBehaviorCommand11(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);
            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param => RemoveUserButtonCommand(userModel.Usernum));
            }
        }



        // 恢复默认：整个广播系统恢复默认状态
        public void ResetDefaultCommand(int Id)
        {
            ResetState();
        }


        /// <summary>
        /// 开启当前会议组的快速会议，带着调度号一起
        /// </summary>
        public void QuickCreateConference(int Id)
        {
            // 注意：下面 1234 的顺序不可修改、下面的 1234 的顺序不可修改、下面的 1234 的顺序不可修改
            // 注意：下面 1234 的顺序不可修改、下面的 1234 的顺序不可修改、下面的 1234 的顺序不可修改
            // 注意：下面 1234 的顺序不可修改、下面的 1234 的顺序不可修改、下面的 1234 的顺序不可修改

            // 0. 各种状态的判断
            bool LeftIsCalling = CommonDB.IsCallingByNumber(LeftDispatchNumModel.Num);
            bool RightIsCalling = CommonDB.IsCallingByNumber(RightDispatchNumModel.Num);
            bool LeftIsAuth = CommonDB.IsAuthingOfNumber(LeftDispatchNumModel.Num);
            bool RightIsAuth = CommonDB.IsAuthingOfNumber(RightDispatchNumModel.Num);

            // 1. 左调度、右调度均处于忙碌状态
            if (LeftIsCalling && RightIsCalling)
            {
                DMMessageBox.Show("警告", "左调度、右调度均处于忙碌状态!!!", DMMessageType.MESSAGE_FAIL);
                return;
            }

            // 2. 左右调度均未鉴权，仅考虑调度优先打出【该状态下，左右至少有一个处于空闲状态】
            if (!LeftIsAuth && !RightIsAuth)
            {
                // 【左优先】左空闲 以左直接打出
                // 【右优先】右空闲 以右直接打出
                if ((NowDispatchNum == LeftDispatchNumModel.Num && !LeftIsCalling) || (NowDispatchNum == RightDispatchNumModel.Num && !RightIsCalling))
                {
                    SSH.ExecuteCommand($"fs_cli -x 'bgapi originate {{origination_caller_id_name=会议组({SelectedGroupModel.CallId}),origination_caller_id_number={SelectedGroupModel.CallId}}}user/{NowDispatchNum} {SelectedGroupModel.CallId}'");
                }

                // 【左优先】左忙碌 提示右未鉴权
                // 【右优先】右忙碌 提示左未鉴权
                else
                {
                    string ret = (NowDispatchNum != LeftDispatchNumModel.Num) ? "左" : "右";
                    DMMessageBox.Show("警告", ret + "调度未鉴权!!!", DMMessageType.MESSAGE_FAIL);
                }

                return;
            }

            // 3. 左右都鉴权，以调度优先为主【该状态下，说明左右均处于空闲状态】
            if (LeftIsAuth && RightIsAuth)
            {
                // 左优先 以左鉴权打出
                if (NowDispatchNum == LeftDispatchNumModel.Num)
                {
                    Dictionary<string, string> dict = CommonDB.GetUUIDByAuthing(LeftDispatchNumModel.Num);
                    if (dict != null)
                    {
                        string uuid = dict["uuid"];
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {SelectedGroupModel.CallId} '");
                    }
                }
                // 右优先 以右鉴权打出
                else
                {
                    Dictionary<string, string> dict = CommonDB.GetUUIDByAuthing(RightDispatchNumModel.Num);
                    if (dict != null)
                    {
                        string uuid = dict["uuid"];
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {SelectedGroupModel.CallId} '");
                    }
                }
                return;
            }

            // 4. 仅一个鉴权，以鉴权为主【该状态下，哪个处于鉴权状态必定处于空闲状态，一定使用该调度号码发出点呼】
            //      - 同等状态下，鉴权的优先级 大于 左右优先
            //      - 比如：左鉴权，右优先且空闲，此时点呼从鉴权话机打出
            if ((LeftIsAuth && !RightIsAuth) || (!LeftIsAuth && RightIsAuth))
            {
                // 左鉴权，代表左一定处于空闲状态
                if (LeftIsAuth)
                {
                    Dictionary<string, string> dict = CommonDB.GetUUIDByAuthing(LeftDispatchNumModel.Num);
                    if (dict != null)
                    {
                        string uuid = dict["uuid"];
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {SelectedGroupModel.CallId} '");
                    }
                }
                // 右鉴权，代表右一定处于空闲状态
                else if (RightIsAuth)
                {
                    Dictionary<string, string> dict = CommonDB.GetUUIDByAuthing(RightDispatchNumModel.Num);
                    if (dict != null)
                    {
                        string uuid = dict["uuid"];
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {SelectedGroupModel.CallId} '");
                    }
                }
                return;
            }
        }


        /// <summary>
        /// 停止当前会议组的会议
        /// </summary>
        public void QuickStopConference(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);
            if (DMMessageBox.Show("停止会议", $"是否停止“{SelectedGroupModel.GroupName}”的会议通话", DMMessageType.MESSAGE_WARING, DMMessageButton.YesNo))
            {
                SSH.ExecuteCommand($"fs_cli -x'conference conferencegroup{SelectedGroupModel.CallId} hup all'");
            }
            ResetState();
        }


        /// <summary>
        /// 恢复默认
        /// </summary>
        private void ResetState()
        {
            // 1. 清空已经选择的用户
            SelectedUserList.Clear();

            // 2. 广播按钮颜色恢复默认
            ResetDispatchButtonBackgroundColor();

            // 5. 恢复当前组点呼
            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param =>
                {
                    DianhuUserButtonCommand(userModel.Usernum);
                });
            }
        }

        public void XXXYYY(int Id)
        {

        }

        #endregion



        #region 用户按钮: 邀请用户、移除用户、点呼

        /// <summary>
        /// 邀请用户
        /// </summary>
        /// <param name="userNum"></param>
        private void InviteUserButtonCommand(string userNum)
        {
            int flag1 = 0;
            int flag2 = 0;
            string ret = SSH.ExecuteCommand("fs_cli -x 'conference list'");
            string[] retList = ret.Split('\n');

            for (int i = 0; i < retList.Length; i++)
            {
                if (retList[i].Trim().Length > 0)
                {
                    if (retList[i].Contains("conferencegroup" + SelectedGroupModel.CallId))
                    {
                        flag1 = 1;
                        continue;
                    }
                    if (flag1 == 1)
                    {
                        if (retList[i].Contains("/" + userNum + "@"))
                        {
                            flag2 = 1;
                            break;
                        }
                    }
                    if (retList[i].Contains("OK Conference")) break;
                }
            }

            if (flag2 == 0)
            {
                // 广播用户，不可以自动应答的拉入会议，需要手动接听
                FSSocket.SendCommand($"bgapi originate {{origination_caller_id_name=会议组({SelectedGroupModel.CallId}),origination_caller_id_number={SelectedGroupModel.CallId}}}user/{userNum} &conference(conferencegroup{SelectedGroupModel.CallId}@dmkj)");
            }
            ResetState();
        }

        /// <summary>
        /// 移除会议
        /// </summary>
        private void RemoveUserButtonCommand(string userNum)
        {
            int flag1 = 0;
            int flag2 = 0;
            string ret = SSH.ExecuteCommand("fs_cli -x 'conference list'");
            string[] retList = ret.Split('\n');
            for (int i = 0; i < retList.Length; i++)
            {
                if (retList[i].Trim().Length > 0)
                {
                    if (retList[i].Contains("conferencegroup" + SelectedGroupModel.CallId))
                    {
                        flag1 = 1;
                        continue;
                    }
                    if (flag1 == 1)
                    {
                        if (retList[i].Contains("/" + userNum + "@"))
                        {
                            FSSocket.SendCommand($"bgapi conference conferencegroup{SelectedGroupModel.CallId} kick {retList[i].Split(';')[0]}");
                            break;
                        }
                    }
                    if (retList[i].Contains("OK Conference")) break;
                }
            }
            ResetState();
        }

        /// <summary>
        /// 点呼
        /// </summary>
        private void DianhuUserButtonCommand(string userNum)
        {
            // 注意：下面 1234 的顺序不可修改、下面的 1234 的顺序不可修改、下面的 1234 的顺序不可修改
            // 注意：下面 1234 的顺序不可修改、下面的 1234 的顺序不可修改、下面的 1234 的顺序不可修改
            // 注意：下面 1234 的顺序不可修改、下面的 1234 的顺序不可修改、下面的 1234 的顺序不可修改

            // 0. 各种状态的判断
            bool LeftIsCalling = CommonDB.IsCallingByNumber(LeftDispatchNumModel.Num);
            bool RightIsCalling = CommonDB.IsCallingByNumber(RightDispatchNumModel.Num);
            bool LeftIsAuth = CommonDB.IsAuthingOfNumber(LeftDispatchNumModel.Num);
            bool RightIsAuth = CommonDB.IsAuthingOfNumber(RightDispatchNumModel.Num);

            // 1. 左调度、右调度均处于忙碌状态
            if (LeftIsCalling && RightIsCalling)
            {
                DMMessageBox.Show("警告", "左调度、右调度均处于忙碌状态!!!", DMMessageType.MESSAGE_FAIL);
                return;
            }

            // 2. 左右调度均未鉴权，仅考虑调度优先打出【该状态下，左右至少有一个处于空闲状态】
            if (!LeftIsAuth && !RightIsAuth)
            {
                // 【左优先】左空闲 以左直接打出
                // 【右优先】右空闲 以右直接打出
                if ((NowDispatchNum == LeftDispatchNumModel.Num && !LeftIsCalling) || (NowDispatchNum == RightDispatchNumModel.Num && !RightIsCalling))
                {
                    SSH.ExecuteCommand($"fs_cli -x 'bgapi originate {{origination_caller_id_name=调度号（{NowDispatchNum}）,origination_caller_id_number={NowDispatchNum}}}user/{NowDispatchNum} {userNum}'");
                }

                // 【左优先】左忙碌 提示右未鉴权
                // 【右优先】右忙碌 提示左未鉴权
                else
                {
                    string ret = (NowDispatchNum != LeftDispatchNumModel.Num) ? "左" : "右";
                    DMMessageBox.Show("警告", ret + "调度未鉴权!!!", DMMessageType.MESSAGE_FAIL);
                }

                return;
            }

            // 3. 左右都鉴权，以调度优先为主【该状态下，说明左右均处于空闲状态】
            if (LeftIsAuth && RightIsAuth)
            {
                // 左优先 以左鉴权打出
                if (NowDispatchNum == LeftDispatchNumModel.Num)
                {
                    Dictionary<string, string> dict = CommonDB.GetUUIDByAuthing(LeftDispatchNumModel.Num);
                    if (dict != null)
                    {
                        string uuid = dict["uuid"];
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {userNum} '");
                    }
                }
                // 右优先 以右鉴权打出
                else
                {
                    Dictionary<string, string> dict = CommonDB.GetUUIDByAuthing(RightDispatchNumModel.Num);
                    if (dict != null)
                    {
                        string uuid = dict["uuid"];
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {userNum} '");
                    }
                }
                return;
            }

            // 4. 仅一个鉴权，以鉴权为主【该状态下，哪个处于鉴权状态必定处于空闲状态，一定使用该调度号码发出点呼】
            //      - 同等状态下，鉴权的优先级 大于 左右优先
            //      - 比如：左鉴权，右优先且空闲，此时点呼从鉴权话机打出
            if ((LeftIsAuth && !RightIsAuth) || (!LeftIsAuth && RightIsAuth))
            {
                // 左鉴权，代表左一定处于空闲状态
                if (LeftIsAuth)
                {
                    Dictionary<string, string> dict = CommonDB.GetUUIDByAuthing(LeftDispatchNumModel.Num);
                    if (dict != null)
                    {
                        string uuid = dict["uuid"];
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {userNum} '");
                    }
                }
                // 右鉴权，代表右一定处于空闲状态
                else if (RightIsAuth)
                {
                    Dictionary<string, string> dict = CommonDB.GetUUIDByAuthing(RightDispatchNumModel.Num);
                    if (dict != null)
                    {
                        string uuid = dict["uuid"];
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {userNum} '");
                    }
                }
                return;
            }
        }


        #endregion

    }
}