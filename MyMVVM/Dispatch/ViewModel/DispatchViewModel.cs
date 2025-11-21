using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Lifetime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MyMVVM.Broadcast;
using MyMVVM.Common;
using MyMVVM.Common.Model;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.Dispatch.Model;
using NAudio.Wave;

namespace MyMVVM.Dispatch.ViewModel
{
    public class DispatchViewModel : ViewModelsBase, IDisposable
    {

        #region 构造器

        public DispatchViewModel()
        {
            // 初始化参数
            NowDispatchStatu = 0; // 仅可以初始化当前是左调度,但是左调度的具体号码需要后续动态加载
            LeftDispatchNumModel = new DispatchNumModel();
            RightDispatchNumModel = new DispatchNumModel();
            TempAllButtons = new ObservableCollection<DefaultUserModel>();
            SelectedUserList = new ObservableCollection<string>();
            IsKuaZu = false;


            Type1Color = (Color)Application.Current.Resources["Type1Color"];
            Type2Color = (Color)Application.Current.Resources["Type2Color"];
            Type3Color = (Color)Application.Current.Resources["Type3Color"];

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

            // 首次加载
            FirstLoadDispatchNum();

            // 定时任务

            _timer1 = new Timer(UpdateOnlineUser, null, 0, 1000);  // 用户状态
            _timer2 = new Timer(LoadDispatchNum, null, 0, 1000);   // 左右调度
            _timer3 = new Timer(LoadCallingUsers, null, 0, 1000);  // 通话记录
            _timer4 = new Timer(LoadDispatchCdr, null, 0, 1000);  // 调度记录
            _timer5 = new Timer(LoadWaitUser, null, 0, 1000);  // 来电等候
            _timer6 = new Timer(LoadGatewayAlarmRecord, null, 0, 2000);  // 网关报警记录

            Dictionary<string, string> dict = CommonDB.GetFunctionNumber();
            queryNowNumber = dict["number"];
            queryNowTime = dict["date"];
            queryNowMissCall = dict["misscall"];
        }

        #endregion




        #region 变量的定义

        private string queryNowNumber;
        private string queryNowTime;
        private string queryNowMissCall;


        private Color Type1Color;
        private Color Type2Color;
        private Color Type3Color;

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

        private bool IsKuaZu; // 是否跨组


        // 组
        private GroupModel SelectedGroupModel;
        private ObservableCollection<GroupModel> _groupDataList;
        public ObservableCollection<GroupModel> GroupDataList { get => _groupDataList; set => SetProperty(ref _groupDataList, value); }


        // 用户展示
        private ObservableCollection<string> SelectedUserList;
        private ObservableCollection<DefaultUserModel> TempAllButtons; // 当前组的全部用户
        private ObservableCollection<DefaultUserModel> _userDataList;
        public ObservableCollection<DefaultUserModel> UserDataList { get => _userDataList; set => SetProperty(ref _userDataList, value); } // 当前组在当前页的全部用户


        // 调度号码
        private int NowDispatchStatu;
        private string NowDispatchNum;
        private DispatchNumModel _leftDispatchNumModel;
        private DispatchNumModel _rightDispatchNumModel;
        public DispatchNumModel LeftDispatchNumModel { get => _leftDispatchNumModel; set { SetProperty(ref _leftDispatchNumModel, value); } }
        public DispatchNumModel RightDispatchNumModel { get => _rightDispatchNumModel; set { SetProperty(ref _rightDispatchNumModel, value); } }

        // 分页
        private int _totalPages;
        private int _currentPage;
        private bool _isShowPageButton;
        public int TotalPages { get => _totalPages; set => SetProperty(ref _totalPages, value); }
        public int CurrentPage { get => _currentPage; set { if (SetProperty(ref _currentPage, value)) { UpdatePage(); } } }

        public bool IsShowPageButton { get => _isShowPageButton; set => SetProperty(ref _isShowPageButton, value); }


        // 定时器
        private readonly Timer _timer1; // 用户状态
        private readonly Timer _timer2; // 左右调度
        private readonly Timer _timer3; // 通话记录
        private readonly Timer _timer4; // 调度记录
        private readonly Timer _timer5; // 来电等候
        private readonly Timer _timer6; // 来电等候




        // 正在进行的通话记录
        public ObservableCollection<DefaultUserModel> _callingUser;
        public ObservableCollection<DefaultUserModel> CallingUser { get => _callingUser; set { _callingUser = value; OnPropertyChanged(nameof(CallingUser)); } }
        // 调度记录
        public ObservableCollection<DefaultUserModel> _dispatchCdr;
        public ObservableCollection<DefaultUserModel> DispatchCdr { get => _dispatchCdr; set { _dispatchCdr = value; OnPropertyChanged(nameof(DispatchCdr)); } }
        // 网关报警记录
        public ObservableCollection<GatewayAlarmRecordModel> _GatewayAlarmRecord;
        public ObservableCollection<GatewayAlarmRecordModel> GatewayAlarmRecord { get => _GatewayAlarmRecord; set { _GatewayAlarmRecord = value; OnPropertyChanged(nameof(GatewayAlarmRecord)); } }

        // 调度按钮
        private ObservableCollection<DispatchButtonModel> _dispatchButtonModelList;
        public ObservableCollection<DispatchButtonModel> DispatchButtonModelList { get => _dispatchButtonModelList; set => SetProperty(ref _dispatchButtonModelList, value); }


        private DefaultUserModel _waitListItem;
        public DefaultUserModel WaitListItem
        {
            get { return _waitListItem; }
            set
            {
                if (_waitListItem != value)
                {
                    _waitListItem = value;
                    OnPropertyChanged(nameof(WaitListItem));

                    if (_waitListItem != null)
                    {
                        bool ret = DMMessageBox.Show("选择呼叫队列", $"是否对 {WaitListItem.WaitUsernum} 发起呼叫", DMMessageType.MESSAGE_WARING, DMMessageButton.YesNo);
                        if (ret)
                        {
                            DMMessageBox.ShowInfo("Yes");

                            // 1. 判断是否两台调度机均忙碌

                            // 2. XXXXX



                        }
                        else
                        {
                            DMMessageBox.ShowInfo("No");
                        }
                    }
                }
            }
        }




        // { get => _waitListItem; set => SetProperty(ref _waitListItem, value); }

        private ObservableCollection<DefaultUserModel> _waitList;
        public ObservableCollection<DefaultUserModel> WaitList { get => _waitList; set => SetProperty(ref _waitList, value); }


        public DefaultUserModel _selectedItem;
        public DefaultUserModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    if (_selectedItem != null)
                    {

                        if (_selectedItem.CallNum1 == LeftDispatchNumModel.Num || _selectedItem.CallNum1 == RightDispatchNumModel.Num)
                        {
                            ExecuteSshCommand(_selectedItem.CallNum2);
                        }

                        if (_selectedItem.CallNum2 == LeftDispatchNumModel.Num || _selectedItem.CallNum2 == RightDispatchNumModel.Num)
                        {
                            ExecuteSshCommand(_selectedItem.CallNum1);
                        }


                    }
                }
            }
        }


        #endregion




        #region 加载组，加载用户

        /// <summary>
        /// 先查组,从数据库查询数据,加载全部的组信息,为每个组按钮绑定Command,并默认设置当前组为全部组的第一个组
        /// </summary>
        private void LoadGroupButton()
        {
            // 1.查询所有的组
            GroupDataList = CommonDB.GetGroupListByType("座机");

            // 2.给所有的组按钮绑定 Command
            foreach (GroupModel model in GroupDataList)
            {
                model.GroupButtonCommand = new ViewModelCommand(param =>
                {
                    SelectedGroupModel = model;
                    ChangeGroupCommand();
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
        /// 再查用户,从当前组查询组内的全部用户,利用页码大小,当前页码的变化来动态展示内容
        /// </summary>
        private void LoadButtonForm()
        {
            // 1.查询组内所有用户
            TempAllButtons.Clear();
            CommonDB.GetUserListByGroupId(SelectedGroupModel.Id, TempAllButtons);
            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.BackgroundColor = DMUtil.ColorToHex(UserDefault);
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    if (IsKuaZu)
                    {
                        SelectUser(userModel.Usernum);
                    }
                    else
                    {
                        DefaultUserButtonCommand(userModel.Usernum);
                    }
                });
            }

            // 分页
            TotalPages = (int)Math.Ceiling((double)TempAllButtons.Count / 64);

            // 默认显示第一页
            CurrentPage = 1;

            // 更新翻页按钮的隐藏、显示状态; 查看是否有第二页
            IsShowPageButton = TempAllButtons.Count > 64;
        }


        /// <summary>
        /// 点击组按钮,触发对应的 Command,每次选择一个组时,重新展示用户列表
        /// </summary>
        private void ChangeGroupCommand()
        {
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

            // 2.查询所有在线用户的号码
            List<string> onlineUserList = CommonDB.GetOnlineUserNum();

            // 3.每次切换组，都要清空所有的用户
            UserDataList.Clear();
            TempAllButtons.Clear();

            // 4.重新查询指定组的所有用户
            CommonDB.GetUserListByGroupId(SelectedGroupModel.Id, TempAllButtons);
            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.BackgroundColor = DMUtil.ColorToHex(UserDefault);
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    if (IsKuaZu)
                    {
                        SelectUser(userModel.Usernum);
                    }
                    else
                    {
                        DefaultUserButtonCommand(userModel.Usernum);
                    }
                });
            }

            // 5. 计算页面大小,并动态展示
            TotalPages = (int)Math.Ceiling((double)TempAllButtons.Count / 64);

            // 默认显示第一页的用户
            CurrentPage = 1;

            foreach (DefaultUserModel button in TempAllButtons.Skip((CurrentPage - 1) * 64).Take(64))
            {
                UserDataList.Add(button);
            }

            // 6.更新Buttons中按钮的状态变化
            UpdateOnlineUser(null);

            // 7. 更新翻页按钮的隐藏、显示状态;查看是否有第二页
            IsShowPageButton = TempAllButtons.Count > 64;
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
                // 类型1
                if (dispatchButtonModel.Id >= 2 && dispatchButtonModel.Id <= 10)
                {

                    dispatchButtonModel.BackgroundColor = DMUtil.ColorToHex(Type1Color);
                }
                else if (dispatchButtonModel.Id == 1)
                {

                    dispatchButtonModel.BackgroundColor = DMUtil.ColorToHex(Type3Color);
                }
                // 类型2
                else
                {

                    dispatchButtonModel.BackgroundColor = DMUtil.ColorToHex(Type2Color);
                }
            }
        }

        /// <summary>
        /// 调度按钮点击变色：先复原其他按钮颜色,再改变被点击的按钮颜色
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
        /// 动态加载调度按钮,后续新增按钮的功能在这配置
        /// </summary>
        private void LoadDispatchButtonList()
        {
            List<DispatchButtonModel> buttons = new List<DispatchButtonModel>();

            // ID 不可变
            // 全呼
            DispatchButtonModel button1 = new DispatchButtonModel();
            button1.Id = 1;
            button1.Name = "全呼";
            button1.Icon = "CarHorn";
            button1.IsShow = 1;
            button1.ButtonCommand = new ViewModelCommand(param => AllCallCommand(button1.Id));

            // 挂断
            DispatchButtonModel button2 = new DispatchButtonModel();
            button2.Id = 2;
            button2.Name = "挂断";
            button2.Icon = "PhoneOff";
            button2.IsShow = 1;
            button2.ButtonCommand = new ViewModelCommand(param => HangupCommand(button2.Id));

            // 监听
            DispatchButtonModel button3 = new DispatchButtonModel();
            button3.Id = 3;
            button3.Name = "监听";
            button3.Icon = "Headphones";
            button3.IsShow = 1;
            button3.ButtonCommand = new ViewModelCommand(param => TappingCommand(button3.Id));

            // 强拆
            DispatchButtonModel button4 = new DispatchButtonModel();
            button4.Id = 4;
            button4.Name = "强拆";
            button4.Icon = "PhoneAlertOutline";
            button4.IsShow = 1;
            button4.ButtonCommand = new ViewModelCommand(param => DisconnectCommand(button4.Id));

            // 强插
            DispatchButtonModel button5 = new DispatchButtonModel();
            button5.Id = 5;
            button5.Name = "强插";
            button5.Icon = "PhonePlusOutline";
            button5.IsShow = 1;
            button5.ButtonCommand = new ViewModelCommand(param => InsertCommand(button5.Id));

            // 组呼
            DispatchButtonModel button6 = new DispatchButtonModel();
            button6.Id = 6;
            button6.Name = "组呼";
            button6.Icon = "Men";
            button6.IsShow = 1;
            button6.ButtonCommand = new ViewModelCommand(param => GroupCallCommand(button6.Id));

            // 多方通话
            DispatchButtonModel button7 = new DispatchButtonModel();
            button7.Id = 7;
            button7.Name = "多方通话";
            button7.Icon = "UserAdd";
            button7.IsShow = 1;
            button7.ButtonCommand = new ViewModelCommand(param => MultiCallCommand(button7.Id));

            // 开始多方
            DispatchButtonModel button8 = new DispatchButtonModel();
            button8.Id = 8;
            button8.Name = "开始多方";
            button8.Icon = "UserSupervisor";
            button8.IsShow = 1;
            button8.ButtonCommand = new ViewModelCommand(param => BeginMultiCallCommand(button8.Id));

            // 直接转接
            DispatchButtonModel button9 = new DispatchButtonModel();
            button9.Id = 9;
            button9.Name = "直接转接";
            button9.Icon = "PhoneForwardOutline";
            button9.IsShow = 1;
            button9.ButtonCommand = new ViewModelCommand(param => TransferCommand(button9.Id));

            // 协商转接
            DispatchButtonModel button10 = new DispatchButtonModel();
            button10.Id = 10;
            button10.Name = "协商转接";
            button10.Icon = "PhoneForwardOutline";
            button10.IsShow = 1;
            button10.ButtonCommand = new ViewModelCommand(param => CallTransferCommand(button10.Id));

            // 恢复默认
            DispatchButtonModel button11 = new DispatchButtonModel();
            button11.Id = 11;
            button11.Name = "恢复默认";
            button11.Icon = "ClockwiseArrows";
            button11.IsShow = 1;
            button11.ButtonCommand = new ViewModelCommand(param => ResetDefaultCommand(button11.Id));

            // 占位按钮
            DispatchButtonModel buttonTemp = new DispatchButtonModel();
            buttonTemp.Id = 12;
            buttonTemp.Name = "占位按钮";
            buttonTemp.Icon = "Men";
            buttonTemp.IsShow = 0;
            buttonTemp.ButtonCommand = new ViewModelCommand(param => XXXYYY(buttonTemp.Id));

            // 调整顺序：
            Dictionary<int, DispatchButtonModel> dict = new Dictionary<int, DispatchButtonModel>();
            dict.Add(1, button1);
            dict.Add(2, button2);
            dict.Add(3, button3);
            dict.Add(4, button4);
            dict.Add(5, button5);
            dict.Add(7, button6);
            dict.Add(6, button7);
            dict.Add(8, button8);

            dict.Add(9, button9);
            dict.Add(10, button10);
            dict.Add(11, button11);
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

            // 初始状态的颜色
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
                //MessageBox.Show($"当前调度号码为{NowDispatchNum}");
            });
            RightDispatchNumModel.Name = "右调度";
            RightDispatchNumModel.Num = dispatchNum["right"];
            RightDispatchNumModel.Image = "pack://application:,,,/Common/Images/Rphone.png";
            RightDispatchNumModel.FontColor = DMUtil.ColorToHex(DispatchButtonUnSelected);
            RightDispatchNumModel.DispatchNumCommand = new ViewModelCommand(param =>
            {
                NowDispatchStatu = 1; // 此时不需要使用默认值，而是手动设置的左右值
                LoadDispatchNum(null); // 点击切换时从数据库重新加载调度号
                //MessageBox.Show($"当前调度号码为{NowDispatchNum}");
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



        #region 功能点

        /// <summary>
        /// 功能执行完毕后,恢复点呼Command,恢复调度按钮颜色
        /// </summary>
        private void ResetUserButtonCommand()
        {
            foreach (DefaultUserModel userModel in UserDataList)
            {
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    DefaultUserButtonCommand(userModel.Usernum);
                });
            }

            ResetDispatchButtonBackgroundColor();
        }


        /// <summary>
        /// 点击相应按钮后，SIP用户按钮绑定事件：点击发起点呼
        /// </summary>
        private void DefaultUserButtonCommand(string userNum)
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
                    SSH.ExecuteCommand($"fs_cli -x 'bgapi originate user/{NowDispatchNum} {userNum}'");
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



        /// <summary>
        /// 盲转
        /// </summary>
        /// <param name="userNum"></param>
        private void Transfer(string userNum)
        {
            try
            {
                bool result = DMMessageBox.Show("提示", "请选择转接的调度", DMMessageType.MESSAGE_INFO, DMMessageButton.LeftRight);
                if (result)
                {
                    DataTable dt = DispatchDB.GetUUid_Transfer1(RightDispatchNumModel.Num);
                    string uuid = dt.Rows[0]["uuid"].ToString();
                    SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {userNum}'");
                }
                if (result == false)
                {
                    DataTable dt = DispatchDB.GetUUid_Transfer1(LeftDispatchNumModel.Num);
                    string uuid = dt.Rows[0]["uuid"].ToString();
                    SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {userNum}'");
                }
                ResetUserButtonCommand();
            }
            catch (Exception ex)
            {
                DMMessageBox.Show("错误", "请规范操作行为!!!", DMMessageType.MESSAGE_FAIL);
                ResetUserButtonCommand();
            }
        }

        /// <summary>
        /// 协商转接
        /// </summary>
        /// <param name="userNum"></param>
        private void CallTransfer(string userNum)
        {
            try
            {
                bool result = DispatchDB.TransferNum(userNum);
                if (result)
                {
                    bool rs = DMMessageBox.Show("提示", "请选择转接的调度", DMMessageType.MESSAGE_INFO, DMMessageButton.LeftRight);
                    if (rs)
                    {
                        DataTable dt = DispatchDB.GetUUid_Transfer(RightDispatchNumModel.Num);
                        string uuid = dt.Rows[0]["uuid"].ToString();
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_broadcast {uuid} lua::ditransfer.lua'");
                    }
                    else
                    {
                        DataTable dt = DispatchDB.GetUUid_Transfer(LeftDispatchNumModel.Num);
                        string uuid = dt.Rows[0]["uuid"].ToString();
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_broadcast {uuid} lua::ditransfer.lua'");
                    }
                }

                ResetUserButtonCommand();
            }
            catch (Exception ex)
            {

                DMMessageBox.Show("错误", "请规范操作行为!!!", DMMessageType.MESSAGE_FAIL);
                ResetUserButtonCommand();
            }
        }

        /// <summary>
        /// 监听
        /// </summary>
        /// <param name="userNum"></param>
        private void Tapping(string userNum)
        {
            DataTable dt = DispatchDB.GetNum();
            if (dt.Rows.Count == 0)
            {
                DMMessageBox.Show("警告", "未鉴权!!", DMMessageType.MESSAGE_WARING);
            }
            else
            {
                string uuid = dt.Rows[0]["uuid"].ToString();
                SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} #13{userNum} '");
            }

            ResetUserButtonCommand();
        }

        /// <summary>
        /// 强拆
        /// </summary>
        /// <param name="userNum"></param>
        private void Disconnect(string userNum)
        {
            DataTable dt = DispatchDB.GetNum();
            if (dt.Rows.Count == 0)
            {
                DMMessageBox.Show("警告", "未鉴权!!", DMMessageType.MESSAGE_WARING);
            }
            else
            {
                string uuid = dt.Rows[0]["uuid"].ToString();
                SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} #12{userNum} '");
            }

            ResetUserButtonCommand();
        }

        /// <summary>
        /// 强插
        /// </summary>
        /// <param name="userNum"></param>
        private void Insert(string userNum)
        {
            DataTable dt = DispatchDB.GetNum();
            if (dt.Rows.Count == 0)
            {
                DMMessageBox.Show("警告", "未鉴权!!", DMMessageType.MESSAGE_WARING);
            }
            else
            {
                string uuid = dt.Rows[0]["uuid"].ToString();
                SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} #11{userNum} '");
            }

            ResetUserButtonCommand();
        }


        /// <summary>
        /// 呼叫保持
        /// </summary>
        private void CallHold(string userNum)
        {
            DataTable dt = DispatchDB.GetNum();
            if (dt.Rows.Count == 0)
            {
                DMMessageBox.Show("警告", "未鉴权!!", DMMessageType.MESSAGE_WARING);
            }
            else
            {
                DataTable dt1 = DispatchDB.GetUUid(userNum);
                string uuid = dt1.Rows[0]["uuid"].ToString();
                SSH.ExecuteCommand($"fs_cli -x 'uuid_hold {uuid}'");
            }

            ResetUserButtonCommand();
        }

        /// <summary>
        /// 呼叫恢复
        /// </summary>
        private void CallRecovery(string userNum)
        {
            DataTable dt = DispatchDB.GetNum();
            if (dt.Rows.Count == 0)
            {
                DMMessageBox.Show("警告", "未鉴权!!", DMMessageType.MESSAGE_WARING);
            }
            else
            {
                DataTable dt1 = DispatchDB.GetUUid(userNum);
                string uuid = dt1.Rows[0]["uuid"].ToString();
                SSH.ExecuteCommand($"fs_cli -x 'uuid_hold off {uuid}'");
            }

            ResetUserButtonCommand();
        }

        /// <summary>
        /// 挂断
        /// </summary>
        private async void Hangup(string userNum)
        {
            try
            {
                DataTable dt1 = DispatchDB.GetUUid(userNum);
                string uuid = dt1.Rows[0]["uuid"].ToString();
                SSH.ExecuteCommand($"fs_cli -x 'uuid_broadcast {uuid} /usr/local/tts/hangup.wav both'");
                await Task.Delay(2350);
                SSH.ExecuteCommand($"fs_cli -x 'uuid_kill {uuid}'");
                ResetUserButtonCommand();
            }
            catch (Exception ex)
            {
                DMMessageBox.Show("警告", "请选择通话中的用户进行挂断", DMMessageType.MESSAGE_WARING);
                ResetUserButtonCommand();
            }

        }


        private void SelectUser(string usernum)
        {
            if (!SelectedUserList.Contains(usernum))
            {
                SelectedUserList.Add(usernum);
            }
            else
            {
                SelectedUserList.Remove(usernum);
            }
        }

        /// <summary>
        /// 全呼
        /// </summary>
        private void AllCall()
        {
            DataTable dt = DispatchDB.GetNum();
            if (dt.Rows.Count == 0)
            {
                DMMessageBox.Show("警告", "未鉴权!!", DMMessageType.MESSAGE_WARING);
            }
            else
            {
                string uuid = dt.Rows[0]["uuid"].ToString();
                SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} 666888 '");
            }

            ResetUserButtonCommand();
        }

        #endregion



        #region 调度按钮






        /// <summary>
        /// 占位按钮
        /// </summary>
        public void XXXYYY(int Id)
        {

        }


        /// <summary>
        /// 恢复默认
        /// </summary>
        public void ResetDefaultCommand(int Id)
        {
            // 1. 清空已经选择的用户
            SelectedUserList.Clear();

            // 2. 广播按钮颜色恢复默认
            ResetDispatchButtonBackgroundColor();

            // 3. 修改本组用户全部功能为点呼
            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    DefaultUserButtonCommand(userModel.Usernum);
                });
            }


        }


        /// <summary>
        /// 监听
        /// </summary>
        public void TappingCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    Tapping(userModel.Usernum);
                });
            }
        }

        /// <summary>
        /// 强拆
        /// </summary>
        public void DisconnectCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    Disconnect(userModel.Usernum);
                });
            }
        }

        /// <summary>
        /// 强插
        /// </summary>
        public void InsertCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    Insert(userModel.Usernum);
                });
            }
        }


        /// <summary>
        /// 呼叫保持
        /// </summary>
        public void CallHoldCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    CallHold(userModel.Usernum);
                });
            }
        }


        /// <summary>
        /// 呼叫恢复
        /// </summary>
        public void CallRecoveryCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    CallRecovery(userModel.Usernum);
                });
            }
        }

        /// <summary>
        /// 挂断
        /// </summary>
        public void HangupCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(par => { Hangup(userModel.Usernum); });
            }
        }


        /// <summary>
        /// 盲转
        /// </summary>
        /// <param name="Id"></param>
        public void TransferCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(p => { Transfer(userModel.Usernum); });
            }
        }


        public void CallTransferCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(p => { CallTransfer(userModel.Usernum); });
            }
        }

        /// <summary>
        /// 多方通话
        /// </summary>
        /// <param name="Id"></param>
        public void MultiCallCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            // 点击多方通话后，用户按钮可以选择
            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    SelectUser(userModel.Usernum);
                });
            }
            IsKuaZu = true;
        }


        /// <summary>
        /// 开始多方通话
        /// </summary>
        /// <param name="Id"></param>
        public void BeginMultiCallCommand(int Id)
        {
            DataTable dt = DispatchDB.GetNum();
            if (dt.Rows.Count == 0)
            {
                DMMessageBox.Show("警告", "未鉴权!!", DMMessageType.MESSAGE_WARING);
                return;
            }

            ResetDispatchButtonBackgroundColor();

            CommonDB.AddSelectedUsers(SelectedUserList, SelectedGroupModel.CallId);

            SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {dt.Rows[0]["uuid"]} #16{SelectedGroupModel.CallId} '");

            IsKuaZu = false;
            SelectedUserList.Clear(); // 开始多方通话之后，清空已选择用户列表，并恢复用户按钮的默认功能为点呼
            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    DefaultUserButtonCommand(userModel.Usernum);
                });
            }
        }


        /// <summary>
        /// 组呼
        /// </summary>
        public void GroupCallCommand(int Id)
        {
            ResetDispatchButtonBackgroundColor();

            DataTable dt = DispatchDB.GetNum();
            if (dt.Rows.Count == 0)
            {
                DMMessageBox.Show("警告", "未鉴权!!", DMMessageType.MESSAGE_WARING);
                return;
            }
            // CommonDB.AddSelectedUsers(SelectedUserList, SelectedGroupModel.CallId);
            SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {dt.Rows[0]["uuid"]} #17{SelectedGroupModel.CallId} '");
        }


        /// <summary>
        /// 全呼
        /// </summary>
        public void AllCallCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            AllCall();
        }

        #endregion





        /// <summary>
        /// 通话显示
        /// </summary>
        /// <param name="e"></param>

        private void LoadCallingUsers(object e)
        {
            List<DefaultUserModel> models = DispatchDB.GetCallingUser(queryNowNumber, queryNowTime, queryNowMissCall);
            ObservableCollection<DefaultUserModel> _callingUser = new ObservableCollection<DefaultUserModel>(models);
            CallingUser = _callingUser;
        }


        /// <summary>
        /// 调度记录
        /// </summary>
        /// <param name="e"></param>
        private void LoadDispatchCdr(object e)
        {
            DataTable dt = DispatchDB.DispatchCdr(LeftDispatchNumModel.Num, RightDispatchNumModel.Num);
            ObservableCollection<DefaultUserModel> _dispatchCdr = new ObservableCollection<DefaultUserModel>();
            foreach (DataRow row in dt.Rows)
            {
                _dispatchCdr.Add(new DefaultUserModel
                {
                    CallerNum = $"{row["caller_name"]} ({row["caller_id_number"]})",
                    CalleeNum = $"{row["callee_name"]} ({row["destination_number"]})",
                    Duration = Convert.ToDateTime(row["start_stamp"]).ToString("HH:mm:ss"),
                    CallNum1 = row["caller_id_number"].ToString(),
                    CallNum2 = row["destination_number"].ToString()
                }); ;
            }

            DispatchCdr = _dispatchCdr;
        }



        /// <summary>
        /// 网关报警记录
        /// </summary>
        private void LoadGatewayAlarmRecord(object e)
        {
            GatewayAlarmRecord = new ObservableCollection<GatewayAlarmRecordModel>(DispatchDB.GetGatewayAlarmRecorList());
        }


        /// <summary>
        /// 从调度记录呼叫
        /// </summary>
        /// <param name="value"></param>
		private async void ExecuteSshCommand(string value)
        {
            bool result = DMMessageBox.Show("呼叫选择", $"是否呼叫: {value}?", DMMessageType.MESSAGE_INFO, DMMessageButton.YesNo);
            if (result)
            {
                DataTable dt = await Task.Run(() => DispatchDB.DispatchNum());
                string left = dt.Rows[0]["left_dispatch"].ToString();
                string right = dt.Rows[0]["right_dispatch"].ToString();

                bool isLeftAvailable = await Task.Run(() => DispatchDB.DispatchStatus(left, left));

                string command = isLeftAvailable
                    ? $"fs_cli -x 'bgapi originate user/{right} {value}'"
                    : $"fs_cli -x 'bgapi originate user/{left} {value}'";

                await Task.Run(() => SSH.ExecuteCommand(command));
            }
        }


        /// <summary>
        /// 未接来电+自动应答
        /// </summary>
        /// <param name="e"></param>
        private void LoadWaitUser(object e)
        {
            DataTable dt = DispatchDB.WaitUser();
            ObservableCollection<DefaultUserModel> _waitUser = new ObservableCollection<DefaultUserModel>();

            if (dt == null || dt.Rows.Count == 0)
            {
                return;
            }


            foreach (DataRow row in dt.Rows)
            {
                _waitUser.Add(new DefaultUserModel
                {
                    WaitDate = row["created"].ToString(),
                    WaitDisplay = $"{row["username"]} ({row["cid_num"]})",

                    WaitUUID = row["uuid"].ToString(),
                    WaitUsernum = row["cid_num"].ToString(),
                });
            }

            WaitList = _waitUser;


            string wait_uuid = dt.Rows[0]["uuid"].ToString();
            if (DispatchDB.DispatchStatus(LeftDispatchNumModel.Num, LeftDispatchNumModel.Num) == false)
            {
                SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {wait_uuid} {LeftDispatchNumModel.Num}'");
            }
            if (DispatchDB.DispatchStatus(RightDispatchNumModel.Num, RightDispatchNumModel.Num) == false)
            {
                SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {wait_uuid} {RightDispatchNumModel.Num}'");
            }
            else
            {
                return;
            }

        }

        /// <summary>
        /// 转换窗口时销毁定时器
        /// </summary>
		public void Dispose()
        {
            _timer1?.Dispose();
            _timer2?.Dispose();
            _timer3?.Dispose();
            _timer4?.Dispose();
        }
    }
}

