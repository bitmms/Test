using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using HandyControl.Tools.Extension;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using MyMVVM.Broadcast.Utils;
using MyMVVM.Broadcast.View;
using MyMVVM.Common;
using MyMVVM.Common.Model;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using NAudio.Wave;

namespace MyMVVM.Broadcast.ViewModel
{
    public class BroadCastViewModel : ViewModelsBase, IDisposable
    {



        #region 构造器

        public BroadCastViewModel()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (DMVariable.MusicUploadState.IsMusicUploadSuccess) break;
                    MusicUploadIngText = $"{DMVariable.MusicUploadState.NowNumber}/{DMVariable.MusicUploadState.TotalNumber}";
                }
            });


            // 初始化参数
            MusicUploadIngText = "";
            NowDispatchStatu = 0; // 仅可以初始化当前是左调度，但是左调度的具体号码需要后续动态加载
            LeftDispatchNumModel = new DispatchNumModel();
            RightDispatchNumModel = new DispatchNumModel();
            BroadcastType = "DefaultType";
            SelectedUserList = new ObservableCollection<string>();
            TempAllButtons = new ObservableCollection<DefaultUserModel>();
            isKuaZu = false;

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

            // 加载音乐广场
            MusicList = MusicDB.GetMusicList();
            IsMusicSynching = true;
            IsMusicSyncSuccess = false;
            Task.Run(() =>
            {
                while (true)
                {
                    if (DMVariable.MusicSyncState.IsMusicSyncSuccess && !DMVariable.MusicSyncState.IsMusicSyncing)
                    {
                        IsMusicSynching = false;
                        IsMusicSyncSuccess = true;
                        break;
                    }
                }
            });

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


        public void Dispose()
        {

        }

        #endregion



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
        private bool isKuaZu;
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


        // 音乐广场
        private bool _IsMusicSynching;
        public bool IsMusicSynching { get => _IsMusicSynching; set => SetProperty(ref _IsMusicSynching, value); }
        private bool _IsMusicSyncSuccess;
        public bool IsMusicSyncSuccess { get => _IsMusicSyncSuccess; set => SetProperty(ref _IsMusicSyncSuccess, value); }

        private MusicModel _selectedMusicItem;
        public MusicModel SelectedMusicItem
        {
            get { return _selectedMusicItem; }
            set
            {
                SetProperty(ref _selectedMusicItem, value);
            }
        }
        private List<MusicModel> _musicList;
        public List<MusicModel> MusicList
        {
            get { return _musicList; }
            set { SetProperty(ref _musicList, value); }
        }

        private string _MusicUploadIngText;
        public string MusicUploadIngText
        {
            get { return _MusicUploadIngText; }
            set { SetProperty(ref _MusicUploadIngText, value); }
        }


        // 广播
        private string BroadcastType;


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
            GroupDataList = CommonDB.GetGroupListByType("广播");

            // 2. 给所有的组按钮绑定 Command
            foreach (GroupModel model in GroupDataList)
            {
                model.GroupButtonCommand = new ViewModelCommand(param =>
                {
                    SelectedGroupModel = model;
                    ChangeGroupCommand();
                    if (!isKuaZu)
                    {
                        SelectedUserList.Clear(); // 切换组用户时清空用户列表，跨组选人除外
                    }

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
            CommonDB.GetUserListByGroupId(SelectedGroupModel.Id, TempAllButtons);
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
            if (!isKuaZu)
            {
                // 1. 清空已经选择的用户
                SelectedUserList.Clear();
                // 2. 广播按钮颜色恢复默认
                ResetDispatchButtonBackgroundColor();
                // 3. 选择的广播类型恢复默认
                BroadcastType = "DefaultType";
            }


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
            CommonDB.GetUserListByGroupId(SelectedGroupModel.Id, TempAllButtons);
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
                if (isKuaZu)
                {
                    userModel.ButtonCommand = new ViewModelCommand(param => DefaultUserButtonCommand(userModel.Usernum));
                }
                else
                {
                    userModel.ButtonCommand = new ViewModelCommand(param => DianhuUserButtonCommand(userModel.Usernum));
                }

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
                if (dispatchButtonModel.Id == 1 || dispatchButtonModel.Id == 2 || dispatchButtonModel.Id == 3 || dispatchButtonModel.Id == 4 || dispatchButtonModel.Id == 6 || dispatchButtonModel.Id == 7 || dispatchButtonModel.Id == 8)
                {
                    dispatchButtonModel.BackgroundColor = DMUtil.ColorToHex(BroadcastTypeButton);
                }
                else if (dispatchButtonModel.Id == 9 || dispatchButtonModel.Id == 11 || dispatchButtonModel.Id == 12 || dispatchButtonModel.Id == 13)
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
            // 人工广播
            DispatchButtonModel button1 = new DispatchButtonModel();
            button1.Id = 1;
            button1.Name = "人工广播";
            button1.Icon = "MicrophoneVariant";
            button1.IsShow = 1;
            button1.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand10(button1.Id));

            // 音乐广播
            DispatchButtonModel button2 = new DispatchButtonModel();
            button2.Id = 2;
            button2.Name = "音乐广播";
            button2.Icon = "FileMusic";
            button2.IsShow = 1;
            button2.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand11(button2.Id));

            // 多方广播
            DispatchButtonModel button3 = new DispatchButtonModel();
            button3.Id = 3;
            button3.Name = "多方广播";
            button3.Icon = "PeoplePlusOutline";
            button3.IsShow = 1;
            button3.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand13(button3.Id));

            // 全体广播
            DispatchButtonModel button4 = new DispatchButtonModel();
            button4.Id = 4;
            button4.Name = "全体广播";
            button4.Icon = "ContactlessPayment";
            button4.IsShow = 1;
            button4.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand14(button4.Id));

            // 停止广播
            DispatchButtonModel button5 = new DispatchButtonModel();
            button5.Id = 5;
            button5.Name = "停止广播";
            button5.Icon = "MathNorm";
            button5.IsShow = 1;
            button5.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand16(button5.Id));

            // TTS广播
            DispatchButtonModel button6 = new DispatchButtonModel();
            button6.Id = 6;
            button6.Name = "TTS广播";
            button6.Icon = "HeadsetMic";
            button6.IsShow = 1;
            button6.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand12(button6.Id));

            // 任务广播
            DispatchButtonModel button7 = new DispatchButtonModel();
            button7.Id = 7;
            button7.Name = "任务广播";
            button7.Icon = "ClipboardListOutline";
            button7.IsShow = 1;
            button7.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand22(button7.Id));

            // 定时广播
            DispatchButtonModel button8 = new DispatchButtonModel();
            button8.Id = 8;
            button8.Name = "定时广播";
            button8.Icon = "AccountClock";
            button8.IsShow = 1;
            button8.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand23(button8.Id));

            // 恢复默认
            DispatchButtonModel button9 = new DispatchButtonModel();
            button9.Id = 9;
            button9.Name = "恢复默认";
            button9.Icon = "ClockwiseArrows";
            button9.IsShow = 1;
            button9.ButtonCommand = new ViewModelCommand(param => ResetDefaultCommand(button9.Id));

            // 开始广播
            DispatchButtonModel button10 = new DispatchButtonModel();
            button10.Id = 10;
            button10.Name = "开始广播";
            button10.Icon = "MenuRight";
            button10.IsShow = 1;
            button10.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand15(button10.Id));

            // 上传音乐
            DispatchButtonModel button11 = new DispatchButtonModel();
            button11.Id = 11;
            button11.Name = "上传音乐";
            button11.Icon = "CloudUploadOutline";
            button11.IsShow = 1;
            button11.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand20(button11.Id));

            // 音乐广场
            DispatchButtonModel button12 = new DispatchButtonModel();
            button12.Id = 12;
            button12.Name = "音乐广场";
            button12.Icon = "MusicBoxMultipleOutline";
            button12.IsShow = 1;
            button12.ButtonCommand = new ViewModelCommand(param => ChangeButtonBehaviorCommand21(button12.Id));

            // TTS列表
            DispatchButtonModel TTSBGutton = new DispatchButtonModel();
            TTSBGutton.Id = 13;
            TTSBGutton.Name = "TTS列表";
            TTSBGutton.Icon = "ClipboardListOutline";
            TTSBGutton.IsShow = 1;
            TTSBGutton.ButtonCommand = new ViewModelCommand(param => TTSTextCommand(TTSBGutton.Id));

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
            dict.Add(3, button3);
            dict.Add(4, button4);

            dict.Add(5, button6);
            dict.Add(7, button7);
            dict.Add(6, button8);
            dict.Add(8, button10);

            dict.Add(9, button12);
            dict.Add(10, TTSBGutton);
            dict.Add(11, button11);
            dict.Add(12, button9);

            dict.Add(13, buttonTemp);
            dict.Add(14, buttonTemp);
            dict.Add(15, buttonTemp);
            dict.Add(16, button5);

            // 渲染到页面
            foreach (var item in dict.Values)
            {
                DispatchButtonModelList.Add(item);
            }

            ResetDispatchButtonBackgroundColor();


            // 从数据库动态加载、从数据库动态加载、从数据库动态加载
            /* 
            Dictionary<string, Action<int>> dict = new Dictionary<string, Action<int>>()
             {
                 {"mapping3-01",  ChangeButtonBehaviorCommand10}, // 人工广播
                 {"mapping3-02",  ChangeButtonBehaviorCommand11}, // 音乐广播
                 {"mapping3-03",  ChangeButtonBehaviorCommand13}, // 多方广播
                 {"mapping3-04",  ChangeButtonBehaviorCommand14}, // 全体广播
                 {"mapping3-05",  ChangeButtonBehaviorCommand16}, // 停止全部的广播：人工广播、音乐广播、实时的TTS广播
                 {"mapping3-06",  ChangeButtonBehaviorCommand12}, // TTS广播
                 {"mapping3-07",  ChangeButtonBehaviorCommand22}, // 任务广播
                 {"mapping3-08",  ChangeButtonBehaviorCommand23}, // 定时广播
                 {"mapping3-09",  ResetDefaultCommand},  // 恢复默认
                 {"mapping3-10",  ChangeButtonBehaviorCommand10}, // 未开启的功能，随便设置个方法给委托方法占位置
                 {"mapping3-11", ChangeButtonBehaviorCommand15}, // 开始广播
                 {"mapping3-12", ChangeButtonBehaviorCommand20}, // 上传音乐
                 {"mapping3-13", ChangeButtonBehaviorCommand21}, // 音乐列表
                 {"mapping3-14", XXXYYY}, // 音乐列表
                 {"mapping3-15", XXXYYY}, // 音乐列表
                 {"mapping3-16", XXXYYY}, // 音乐列表
             };
             List<Dictionary<string, string>> list = CommonDB.GetAllButtonsByType(3);

             list.ForEach(item =>
             {
                 DispatchButtonModelList.Add(new DispatchButtonModel()
                 {
                     Id = int.Parse(item["Id"]),
                     Name = item["Name"],
                     Icon = item["Icon"],
                     ButtonCommand = new ViewModelCommand(param =>
                     {
                         if (item["IsOk"] == "0")
                         {
                             DMMessageBox.ShowInfo("这是一个未开放功能");
                         }
                         else
                         {
                             dict[item["MappingName"]](int.Parse(item["Id"]));
                         }
                     }),
                     BackgroundColor = DMConstant.FuncationButtonStateColor.UnSelected,
                 });
             });*/
            // 没有功能的按钮是否展示
            /*
            for (int i = list.Count + 1; i <= 16; i++)
            {
                DispatchButtonModelList.Add(new DispatchButtonModel()
                {

                    Id = 99999 + i,
                    Name = "",
                    ButtonCommand = null,
                    BackgroundColor = "#87ceff",
                });
            }
            */
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



        #region 用户按钮: 选人，点呼


        /// <summary>
        /// 点击相应按钮后，SIP用户按钮绑定事件：选择用户、取消选择用户
        /// </summary>
        private void DefaultUserButtonCommand(string userNum)
        {
            if (SelectedUserList.Contains(userNum))
            {
                SelectedUserList.Remove(userNum);
                return;
            }
            SelectedUserList.Add(userNum);
        }


        /// <summary>
        /// 点击相应按钮后，SIP用户按钮绑定事件：点击发起点呼
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
        /// 点击全体广播按钮后，SIP用户按钮绑定事件：清空全部绑定的事件
        /// </summary>
        private void XXXYYY(string userNum)
        {
            DMMessageBox.ShowInfo("当前处于全体广播状态，无需选择用户");
        }


        #endregion



        #region 调度按钮一：人工广播、音乐广播、全体广播、多方广播、TTS广播、任务广播、定时广播


        /// <summary>
        /// 人工广播按钮
        /// </summary>
        public void ChangeButtonBehaviorCommand10(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param => DefaultUserButtonCommand(userModel.Usernum));
            }

            BroadcastType = "人工广播";
        }


        /// <summary>
        /// 音乐广播按钮
        /// </summary>
        public void ChangeButtonBehaviorCommand11(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param => DefaultUserButtonCommand(userModel.Usernum));
            }

            BroadcastType = "音乐广播";
        }


        /// <summary>
        /// 多方广播按钮
        /// </summary>
        public void ChangeButtonBehaviorCommand13(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param => DefaultUserButtonCommand(userModel.Usernum));
            }

            BroadcastType = "多方广播";

            isKuaZu = true;
        }


        /// <summary>
        /// 全体广播
        /// </summary>
        public void ChangeButtonBehaviorCommand14(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param => XXXYYY(userModel.Usernum));
            }

            BroadcastType = "全体广播";
        }


        /// <summary>
        /// TTS广播
        /// </summary>
        public void ChangeButtonBehaviorCommand12(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param => DefaultUserButtonCommand(userModel.Usernum));
            }

            BroadcastType = "TTS广播";
        }


        /// <summary>
        /// 任务广播按钮
        /// </summary>
        public void ChangeButtonBehaviorCommand22(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param => DefaultUserButtonCommand(userModel.Usernum));
            }

            BroadcastType = "任务广播";
        }


        /// <summary>
        /// 定时广播按钮
        /// </summary>
        public void ChangeButtonBehaviorCommand23(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param => DefaultUserButtonCommand(userModel.Usernum));
            }

            BroadcastType = "定时广播";
        }


        #endregion



        #region 调度按钮二：开始广播


        // 点击开始广播按钮，进行相关的判断处理
        public void ChangeButtonBehaviorCommand15(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            if (BroadcastType == null || BroadcastType == "" || BroadcastType == "DefaultType")
            {
                DMMessageBox.ShowInfo("当前未选择广播类型");
                // 收尾
                ResetDispatchButtonBackgroundColor();
                return;
            }

            DateTime dateTime = DateTime.Now;
            BroadCastModel broadCastModel = new BroadCastModel()
            {
                DispatchNum = NowDispatchNum,
                // CreateTime = DMUtil.GetDateTimeString(dateTime),
                // TimeStamp = DMUtil.GetTimeStamp(dateTime),
                GroupId = int.Parse(SelectedGroupModel.Id),
                Users = DMUtil.CopyObservableCollection(SelectedUserList),
            };

            if (BroadcastType == "人工广播")
            {
                BeginBroadCastOfPerson(broadCastModel);
            }
            else if (BroadcastType == "音乐广播")
            {
                BeginBroadCastOfMusic(broadCastModel);
            }
            else if (BroadcastType == "全体广播")
            {
                BeginBroadCastOfAllUser(broadCastModel);
            }
            else if (BroadcastType == "多方广播")
            {
                BeginBroadCastOfMutil(broadCastModel);
            }
            else if (BroadcastType == "TTS广播")
            {
                BeginBroadCastOfTTS(broadCastModel);
            }
            else if (BroadcastType == "定时广播")
            {
                BeginBroadCastOfScheduled(broadCastModel);
            }
            else if (BroadcastType == "任务广播")
            {
                BeginBroadCastOfTask(broadCastModel);
            }

            // 收尾工作
            ResetState();
        }

        // 开始执行人工广播
        private void BeginBroadCastOfPerson(BroadCastModel broadCastModel)
        {
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

            // 2. 左右调度均未鉴权，仅考虑调度优先打出
            if (!LeftIsAuth && !RightIsAuth)
            {
                // 【左优先】左空闲 以左直接打出
                // 【右优先】右空闲 以右直接打出
                if ((NowDispatchNum == LeftDispatchNumModel.Num && !LeftIsCalling) || (NowDispatchNum == RightDispatchNumModel.Num && !RightIsCalling))
                {
                    // 构建 Model
                    broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
                    broadCastModel.BroadCastBeginTime = broadCastModel.CreateTime;
                    broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);
                    broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
                    broadCastModel.Type = (broadCastModel.Users.Count > 0) ? BroadcastTypeEnum.RealTimeManualBroadcastOfGroupSelected : broadCastModel.Type = BroadcastTypeEnum.RealTimeManualBroadcastOfGroupAll;

                    // 处理广播
                    Task.Run(() =>
                    {
                        BroadCastDB.InsertBroadCast(broadCastModel);
                        BroadCastUtil.BroadCastTypeFunction.HandlerBroadcast(broadCastModel);
                    });


                    // 打开监控视频
                    if (broadCastModel.Type == BroadcastTypeEnum.RealTimeManualBroadcastOfGroupSelected)
                    {
                        // 获取全部广播对象
                        List<string> list = new List<string>(broadCastModel.Users);
                        // 摄像头
                        BroadCastUtil.CreateBroadcastCameraVideo(list);
                    }
                    else
                    {
                        // 获取全部广播对象
                        ObservableCollection<DefaultUserModel> users = new ObservableCollection<DefaultUserModel>();
                        CommonDB.GetUserListByGroupId(broadCastModel.GroupId.ToString(), users);
                        List<string> list = new List<string>();
                        for (int i = 0; i < users.Count; i++)
                        {
                            list.Add(users[i].Usernum);
                        }
                        // 摄像头
                        BroadCastUtil.CreateBroadcastCameraVideo(list);
                    }
                }

                // 【左优先】左忙碌 右空闲 提示右未鉴权
                // 【右优先】右忙碌 左空闲 提示左未鉴权
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
                // 左右都鉴权，说明左右都空闲
                Dictionary<string, string> dict = (NowDispatchNum == LeftDispatchNumModel.Num) ? CommonDB.GetUUIDByAuthing(LeftDispatchNumModel.Num) : CommonDB.GetUUIDByAuthing(RightDispatchNumModel.Num);

                // 谁优先使用谁
                if (dict != null)
                {
                    // 构建 Model
                    broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
                    broadCastModel.BroadCastBeginTime = broadCastModel.CreateTime;
                    broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);
                    broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
                    broadCastModel.Type = (broadCastModel.Users.Count > 0) ? BroadcastTypeEnum.RealTimeManualBroadcastOfGroupSelected : broadCastModel.Type = BroadcastTypeEnum.RealTimeManualBroadcastOfGroupAll;
                    // 存数据库，并生成 id
                    BroadCastDB.InsertBroadCast(broadCastModel);
                    // 发起广播
                    if (broadCastModel.Type == BroadcastTypeEnum.RealTimeManualBroadcastOfGroupSelected) // #31
                    {
                        // 在数据库保存：选择用户的列表，会议ID
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < broadCastModel.Users.Count; i++)
                        {
                            sb.Append(broadCastModel.Users[i]).Append(",");
                        }
                        sb.Length--;
                        sb.Append("=");
                        sb.Append("conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp);
                        BroadCastDB.AddSelectedUsersAndBroadcastName(sb.ToString(), SelectedGroupModel.CallId);

                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {dict["uuid"]} #31{SelectedGroupModel.CallId} '");
                    }
                    else // #32
                    {
                        // 在数据库保存：会议ID
                        BroadCastDB.AddSelectedUsersAndBroadcastName("conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp, SelectedGroupModel.CallId);
                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {dict["uuid"]} #32{SelectedGroupModel.CallId} '");
                    }

                    // 打开监控视频
                    if (broadCastModel.Type == BroadcastTypeEnum.RealTimeManualBroadcastOfGroupSelected)
                    {
                        // 获取全部广播对象
                        List<string> list = new List<string>(broadCastModel.Users);
                        // 摄像头
                        BroadCastUtil.CreateBroadcastCameraVideo(list);
                    }
                    else
                    {
                        // 获取全部广播对象
                        ObservableCollection<DefaultUserModel> users = new ObservableCollection<DefaultUserModel>();
                        CommonDB.GetUserListByGroupId(broadCastModel.GroupId.ToString(), users);
                        List<string> list = new List<string>();
                        for (int i = 0; i < users.Count; i++)
                        {
                            list.Add(users[i].Usernum);
                        }
                        // 摄像头
                        BroadCastUtil.CreateBroadcastCameraVideo(list);
                    }


                }

                return;
            }

            // 4. 仅一个鉴权，以鉴权为主【该状态下，哪个处于鉴权状态必定处于空闲状态，一定使用该调度号码发出点呼】
            //      - 同等状态下，鉴权的优先级 大于 左右优先
            //      - 比如：左鉴权，右优先且空闲，此时点呼从鉴权话机打出
            if ((LeftIsAuth && !RightIsAuth) || (!LeftIsAuth && RightIsAuth))
            {
                // 找出鉴权的UUID
                Dictionary<string, string> dict = LeftIsAuth ? CommonDB.GetUUIDByAuthing(LeftDispatchNumModel.Num) : CommonDB.GetUUIDByAuthing(RightDispatchNumModel.Num);

                // 发起广播
                if (dict != null)
                {
                    // 构建 Model
                    broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
                    broadCastModel.BroadCastBeginTime = broadCastModel.CreateTime;
                    broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);
                    broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
                    broadCastModel.Type = (broadCastModel.Users.Count > 0) ? BroadcastTypeEnum.RealTimeManualBroadcastOfGroupSelected : broadCastModel.Type = BroadcastTypeEnum.RealTimeManualBroadcastOfGroupAll;
                    // 存数据库，并生成 id
                    BroadCastDB.InsertBroadCast(broadCastModel);

                    // 发起广播
                    if (broadCastModel.Type == BroadcastTypeEnum.RealTimeManualBroadcastOfGroupSelected)
                    {
                        // 在数据库保存：选择用户的列表，会议ID
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < broadCastModel.Users.Count; i++)
                        {
                            sb.Append(broadCastModel.Users[i]).Append(",");
                        }
                        sb.Length--;
                        sb.Append("=");
                        sb.Append("conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp);
                        BroadCastDB.AddSelectedUsersAndBroadcastName(sb.ToString(), SelectedGroupModel.CallId);

                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {dict["uuid"]} #31{SelectedGroupModel.CallId}'");
                    }
                    else // #32
                    {
                        // 在数据库保存：会议ID
                        BroadCastDB.AddSelectedUsersAndBroadcastName("conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp, SelectedGroupModel.CallId);

                        SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {dict["uuid"]} #32{SelectedGroupModel.CallId} '");
                    }

                    // 打开监控视频
                    if (broadCastModel.Type == BroadcastTypeEnum.RealTimeManualBroadcastOfGroupSelected)
                    {
                        // 获取全部广播对象
                        List<string> list = new List<string>(broadCastModel.Users);
                        // 摄像头
                        BroadCastUtil.CreateBroadcastCameraVideo(list);
                    }
                    else
                    {
                        // 获取全部广播对象
                        ObservableCollection<DefaultUserModel> users = new ObservableCollection<DefaultUserModel>();
                        CommonDB.GetUserListByGroupId(broadCastModel.GroupId.ToString(), users);
                        List<string> list = new List<string>();
                        for (int i = 0; i < users.Count; i++)
                        {
                            list.Add(users[i].Usernum);
                        }
                        // 摄像头
                        BroadCastUtil.CreateBroadcastCameraVideo(list);
                    }
                }

                return;
            }
        }


        // 开始执行音乐广播
        private void BeginBroadCastOfMusic(BroadCastModel broadCastModel)
        {
            new MusicBroadcastView(broadCastModel).ShowDialog();
        }


        // 开始执行全体广播
        private void BeginBroadCastOfAllUser(BroadCastModel broadCastModel)
        {
            new AllPersonsBroadcastView(broadCastModel).ShowDialog();
        }


        // 开始执行多方广播
        private void BeginBroadCastOfMutil(BroadCastModel broadCastModel)
        {
            if (SelectedUserList.Count <= 0)
            {
                DMMessageBox.ShowInfo("未选择用户");
                ResetDispatchButtonBackgroundColor();
                return;
            }

            // broadCastModel.GroupId = -1; // 跨组广播，设置该组 id 为 -1【具体啥原因待查】
            broadCastModel.GroupId = int.Parse(SelectedGroupModel.Id); // 跨组广播，设置该组 id 为 -1【具体啥原因待查】

            new MultiBroadcastView(broadCastModel).ShowDialog();
        }


        // 开始执行TTS广播
        private void BeginBroadCastOfTTS(BroadCastModel broadCastModel)
        {
            new TTSRealTimeBroadcastView(broadCastModel).ShowDialog();
        }


        // 开始执行定时广播
        private void BeginBroadCastOfScheduled(BroadCastModel broadCastModel)
        {
            new ScheduledBroadcastView(broadCastModel).ShowDialog();
        }


        // 开始执行任务广播
        private void BeginBroadCastOfTask(BroadCastModel broadCastModel)
        {
            new TaskBroadcastView(broadCastModel).ShowDialog();
        }


        #endregion



        #region 调度按钮三： 停止广播、上传音乐、恢复默认



        /// <summary>
        /// 停止全部的广播
        /// </summary>
        public void ChangeButtonBehaviorCommand16(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            if (DMMessageBox.Show("停止广播", "是否停止全部正在播放的广播", DMMessageType.MESSAGE_WARING, DMMessageButton.YesNo))
            {
                // 广播的命名规则：dmkj-type-id-timestamp
                string ret = SSH.ExecuteCommand("fs_cli -x 'conference list'");

                // 查找所有匹配项  
                MatchCollection matches = new Regex(@"\+OK Conference ([a-zA-Z0-9-]+) \(").Matches(ret);

                // 捕获匹配到的会议室标识符  
                List<string> list = new List<string>();
                foreach (Match match in matches)
                {
                    list.Add(match.Groups[1].Value);
                }

                // 停止全部会议
                foreach (string conferenceName in list)
                {
                    SSH.ExecuteCommand($"fs_cli -x'conference {conferenceName} hup all'");
                    int id = int.Parse(conferenceName.Split('-')[2]);
                    int playStatus = (int)BroadCastPlayStatusEnum.StopAllBroadcastClicked;
                    BroadCastDB.UpdateBroadCastIsPlayed(id, playStatus);
                }

                DMMessageBox.ShowInfo($"成功停止全部的广播");
            }

            ResetState();
        }



        // 恢复默认：整个广播系统恢复默认状态
        public void ResetDefaultCommand(int Id)
        {
            ResetState();
        }



        private void ResetState()
        {
            // 1. 清空已经选择的用户
            SelectedUserList.Clear();

            // 2. 广播按钮颜色恢复默认
            ResetDispatchButtonBackgroundColor();

            // 3. 选择的广播类型恢复默认
            BroadcastType = "DefaultType";

            // 4. 跨组设置为否
            isKuaZu = false;

            // 5. 恢复当前组点呼
            foreach (var userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(param =>
                {
                    DianhuUserButtonCommand(userModel.Usernum);
                });
            }
        }



        // 上传音乐
        public void ChangeButtonBehaviorCommand20(int Id)
        {
            if (DMVariable.MusicUploadState.IsMusicUploading)
            {
                DMMessageBox.ShowInfo("请等待当前上传任务完成再执行操作");
                return;
            }

            ClickDispatchButtonChangeBackgroundColor(Id);

            // 异步上传音乐文件，同时刷新音乐广场
            UploadMusic();

            ResetDispatchButtonBackgroundColor();
        }



        // 上传音乐
        private async void UploadMusic()
        {
            // 打开选择音乐的窗口
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择音乐",
                Filter = "Audio files (*.wav; *.mp3)|*.wav;*.mp3", // 用户仅可以看到 .wav .mp3 类型的文件
                Multiselect = true, // 允许多选
            };

            // 用户打开文件选择窗口但是没有选择文件
            if (openFileDialog.ShowDialog() != true) return;

            List<string> list = new List<string>();
            for (int i = openFileDialog.FileNames.Length - 1; i >= 0; i--)
            {
                var filename = openFileDialog.FileNames[i];
                // 获取被选择文件的参数，并构造一个 Model
                MusicModel musicModel = new MusicModel()
                {
                    LocalPath = filename,
                    Name = SolveMusicFileName(filename),
                    Time = "",
                    UploadLocalPath = Path.Combine(MusicDB.GetUploadLocalPath(), SolveMusicFileName(filename)),
                    UploadRemotePath = $"{MusicDB.GetUploadRemotePath()}{SolveMusicFileName(filename)}",
                };

                // 已存在同名音乐，系统已自动过滤
                if (MusicDB.IsExistMusic(musicModel.Name))
                {
                    continue;
                }

                list.Add(filename);
            }

            DMVariable.MusicUploadState.IsMusicUploading = true;
            DMVariable.MusicUploadState.IsMusicUploadSuccess = false;
            DMVariable.MusicUploadState.TotalNumber = list.Count;
            DMVariable.MusicUploadState.NowNumber = 0;

            Task.Run(() =>
            {
                while (true)
                {
                    if (DMVariable.MusicUploadState.IsMusicUploadSuccess) break;
                    MusicUploadIngText = $"{DMVariable.MusicUploadState.NowNumber}/{DMVariable.MusicUploadState.TotalNumber}";
                }
            });

            foreach (string filename in list)
            {
                // 获取被选择文件的参数，并构造一个 Model
                MusicModel musicModel = new MusicModel()
                {
                    LocalPath = filename,
                    Time = "",
                    Name = SolveMusicFileName(filename),
                    UploadLocalPath = Path.Combine(MusicDB.GetUploadLocalPath(), SolveMusicFileName(filename)),
                    UploadRemotePath = $"{MusicDB.GetUploadRemotePath()}{SolveMusicFileName(filename)}",
                };

                // 上传文件
                await Task.Run(() =>
                {
                    new FileInfo(musicModel.LocalPath).CopyTo(musicModel.UploadLocalPath, true);
                    SSH.UploadFile(musicModel.LocalPath, musicModel.UploadRemotePath);
                    musicModel.Time = new AudioFileReader(musicModel.LocalPath).TotalTime.ToString("mm\\:ss");
                    MusicDB.AddMusic(musicModel);
                });
                DMVariable.MusicUploadState.NowNumber++;
            }

            DMVariable.MusicUploadState.IsMusicUploading = false;
            DMVariable.MusicUploadState.IsMusicUploadSuccess = true;
            MusicUploadIngText = "";

            DMMessageBox.ShowInfo("音乐文件上传完成");

            {
                // 获取数据库的音乐列表
                List<MusicModel> list1 = MusicDB.GetMusicList();

                // 获取本地音乐列表
                List<string> LocalList = new List<string>();
                foreach (string s in Directory.GetFiles(MusicDB.GetUploadLocalPath(), "*.wav"))
                {
                    string filename = Path.GetFileName(s);
                    LocalList.Add(filename);
                }

                // 判断本地文件夹是否存在，不存在就创建
                if (!Directory.Exists(MusicDB.GetUploadLocalPath()))
                {
                    Directory.CreateDirectory(MusicDB.GetUploadLocalPath());
                }

                // 查看哪些歌曲仅存在与服务器中
                List<MusicModel> tempList = new List<MusicModel>();
                foreach (MusicModel model in list1)
                {
                    string name = model.Name + ".wav";
                    if (!LocalList.Contains(name))
                    {
                        tempList.Add(model);
                    }
                }

                // 如果有文件差异，就执行同步操作
                Task.Run(() =>
                {
                    IsMusicSynching = true;
                    IsMusicSyncSuccess = false;
                    if (tempList.Count > 0)
                    {
                        foreach (var model in tempList)
                        {
                            SSH.DownLoadFile(model.UploadRemotePath, MusicDB.GetUploadLocalPath() + "\\" + Path.GetFileName(model.UploadLocalPath));
                        }
                    }
                    IsMusicSynching = false;
                    IsMusicSyncSuccess = true;
                });
            }

            MusicList = MusicDB.GetMusicList();
        }



        // 音乐广场
        public void ChangeButtonBehaviorCommand21(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            MusicListView musicListView = new MusicListView();
            MusicListViewModel musicListViewModel = new MusicListViewModel();
            musicListView.DataContext = musicListViewModel;

            musicListView.Closed += (sender, args) =>
            {
                if (musicListViewModel.IsEditor)
                {
                    MusicList = MusicDB.GetMusicList();
                }
            };

            musicListView.ShowDialog();

            ResetDispatchButtonBackgroundColor();
        }


        // TTS列表
        public void TTSTextCommand(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            TTSListView ttsListView = new TTSListView();
            TTSListViewModel viewModel = new TTSListViewModel();
            ttsListView.DataContext = viewModel;

            ttsListView.Closed += (sender, args) =>
            {
                // 如果tts文件被编辑过，关闭窗口后重新加载音乐广场
                /* if (ttsListView.IsEditor)
                 {
                     MusicList = MusicDB.GetMusicList();
                 }*/
            };

            ttsListView.ShowDialog();

            ResetDispatchButtonBackgroundColor();
        }


        public void XXXYYY(int Id)
        {

        }

        /// <summary>
        /// 处理文件名称中特殊的字符
        /// </summary>
        private string SolveMusicFileName(string filename)
        {
            // 去除空格
            filename = Path.GetFileName(filename).Replace(" ", "");

            // 去除 "!"
            filename = filename.Replace("!", "");

            return filename;
        }


        #endregion



        #region 监控视频




        #endregion

    }
}
