using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Lifetime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using LibVLCSharp.Shared;
using MyMVVM.Broadcast;
using MyMVVM.Common;
using MyMVVM.Common.Model;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.Dispatch;
using MyMVVM.Monitor.View;
using MyMVVM.Phone.Model;
using MyMVVM.Phone.Utils;
using MyMVVM.Phone.View;
using NAudio.Wave;
using Newtonsoft.Json;

namespace MyMVVM.Phone.ViewModel
{
    public class PhoneViewModel : ViewModelsBase, IDisposable
    {

        public PhoneViewModel()
        {
            IsShowButtonOfRemoteContro = false;
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
            SelectedGroupModel = null;
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



            Dictionary<string, string> dict = CommonDB.GetFunctionNumber();
            queryNowNumber = dict["number"];
            queryNowTime = dict["date"];
            queryNowMissCall = dict["misscall"];
        }


        private string queryNowNumber = "*114";
        private string queryNowTime = "*115";
        private string queryNowMissCall = "*117";

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


        public bool IsKuaZu;

        //组
        private GroupModel SelectedGroupModel;
        private ObservableCollection<GroupModel> _groupDataList;
        public ObservableCollection<GroupModel> GroupDataList { get => _groupDataList; set => SetProperty(ref _groupDataList, value); }


        //用户展示
        private ObservableCollection<string> SelectedUserList;

        /// <summary>
        /// 当前组的全部用户
        /// </summary>
        private ObservableCollection<DefaultUserModel> TempAllButtons;

        private ObservableCollection<DefaultUserModel> _userDataList;
        public ObservableCollection<DefaultUserModel> UserDataList { get => _userDataList; set => SetProperty(ref _userDataList, value); }

        //调度号码
        private int NowDispatchStatu;
        private string NowDispatchNum;
        private DispatchNumModel _leftDispatchNumModel;
        private DispatchNumModel _rightDispatchNumModel;
        public DispatchNumModel LeftDispatchNumModel { get => _leftDispatchNumModel; set { SetProperty(ref _leftDispatchNumModel, value); } }
        public DispatchNumModel RightDispatchNumModel { get => _rightDispatchNumModel; set { SetProperty(ref _rightDispatchNumModel, value); } }


        //分页
        private int _totalPages;
        private int _currentPage;
        private bool _isShowPageButton;
        public int TotalPages { get => _totalPages; set => SetProperty(ref _totalPages, value); }
        public int CurrentPage { get => _currentPage; set { if (SetProperty(ref _currentPage, value)) { UpdatePage(); } } }
        public bool IsShowPageButton { get => _isShowPageButton; set => SetProperty(ref _isShowPageButton, value); }



        // 定时器
        private readonly Timer _timer1; // 用户状态
        private readonly Timer _timer2; // 左右调度


        //调度记录
        public ObservableCollection<DefaultUserModel> _dispatchCdr;
        public ObservableCollection<DefaultUserModel> DispatchCdr { get => _dispatchCdr; set { _dispatchCdr = value; OnPropertyChanged(nameof(DispatchCdr)); } }


        // 调度按钮
        private ObservableCollection<DispatchButtonModel> _dispatchButtonModelList;
        public ObservableCollection<DispatchButtonModel> DispatchButtonModelList { get => _dispatchButtonModelList; set => SetProperty(ref _dispatchButtonModelList, value); }


        // 无声介入
        private Boolean _IsShowButtonOfRemoteContro;
        public Boolean IsShowButtonOfRemoteContro { get => _IsShowButtonOfRemoteContro; set => SetProperty(ref _IsShowButtonOfRemoteContro, value); }







        /// <summary>
        /// 先查组,从数据库查询数据,加载全部的组信息,为每个组按钮绑定Command,并默认设置当前组为全部组的第一个组
        /// </summary>
        private void LoadGroupButton()
        {
            // 1.查询所有的组
            GroupDataList = CommonDB.GetGroupListByType("手机");

            // 2.给所有的组按钮绑定 Command
            foreach (GroupModel model in GroupDataList)
            {
                model.GroupButtonCommand = new ViewModelCommand(p =>
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
                userModel.ButtonCommand = new ViewModelCommand(p =>
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
                userModel.ButtonCommand = new ViewModelCommand(p =>
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
            TotalPages = (int)Math.Ceiling((Double)TempAllButtons.Count / 64);

            // 默认显示第一页的用户
            CurrentPage = 1;

            foreach (DefaultUserModel button in TempAllButtons.Skip((CurrentPage - 1) * 64).Take(64))
            {
                UserDataList.Add(button);
            }

            // 6.更新Buttons中按钮的状态变化
            UpdateOnlineUser(null);

            // 7.更新翻页按钮的隐藏、显示状态;查看是否有第二页
            IsShowPageButton = TempAllButtons.Count > 64;
        }



        #region 用户状态的变化

        private void UpdateOnlineUser(object obj)
        {
            for (int i = 0; i < TempAllButtons.Count; i++)
            {
                if (TempAllButtons.Count >= i + 1 && TempAllButtons[i] != null)
                {
                    SetOneUserButtonBackgroundState(TempAllButtons[i]);
                }
            }
        }

        /// <summary>
        /// 更新用户按钮的文本和图标的颜色
        /// </summary>
        private void SetOneUserButtonTextAndIconColor(DefaultUserModel userModel)
        {
            // 用户被选中
            if (SelectedUserList.Contains(userModel.Usernum))
            {
                userModel.UserButtonFontColor = DMUtil.ColorToHex(UserSelected);
            }
            // 用户没有被选中
            else
            {
                userModel.UserButtonFontColor = DMUtil.ColorToHex(UserUnSelected);
            }
        }

        /// <summary>
        /// 更新用户按钮的状态
        /// </summary>
        private void SetOneUserButtonBackgroundState(DefaultUserModel userModel)
        {
            Dictionary<string, string> map1 = CommonDB.IsCalling(userModel.Usernum);
            Dictionary<string, string> map2 = CommonDB.IsRinging(userModel.Usernum);

            // 通话中
            if (map1 != null)
            {
                userModel.BackgroundColor = DMUtil.ColorToHex(UserCalling);
                userModel.UserDisplay = DMUtil.SetOneUserButtonDisplayText(map1["cidNum"], map1["dest"]);
                SetOneUserButtonTextAndIconColor(userModel);
            }
            // 振铃中
            else if (map2 != null)
            {
                userModel.BackgroundColor = DMUtil.ColorToHex(UserRinging);
                userModel.UserDisplay = DMUtil.SetOneUserButtonDisplayText(map2["cidNum"], map2["dest"]);
                SetOneUserButtonTextAndIconColor(userModel);
            }
            // 在线
            else if (CommonDB.IsRegSuccess(userModel.Usernum))
            {
                userModel.BackgroundColor = DMUtil.ColorToHex(UserOnline);
                userModel.UserDisplay = userModel.Usernum;
                SetOneUserButtonTextAndIconColor(userModel);
            }
            // 默认：离线、没有接电话
            else
            {
                userModel.BackgroundColor = DMUtil.ColorToHex(UserDefault);
                userModel.UserDisplay = userModel.Usernum;
                SetOneUserButtonTextAndIconColor(userModel);
            }
        }

        #endregion



        #region 动态加载调度按钮，方便后续点击变色
        public void ResetDispatchButtonBackgroundColor()
        {
            foreach (DispatchButtonModel dispatchButtonModel in DispatchButtonModelList)
            {
                // 类型1
                if (dispatchButtonModel.Id >= 2 && dispatchButtonModel.Id <= 11)
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

        public void ClickDispatchButtonChangeBackgroundColor(int Id)
        {
            ResetDispatchButtonBackgroundColor();
            foreach (DispatchButtonModel item in DispatchButtonModelList)
            {
                if (item.Id == Id)
                {
                    item.BackgroundColor = DMUtil.ColorToHex(Selected);
                    break;
                }
            }
        }


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

            // 无声介入
            DispatchButtonModel button11 = new DispatchButtonModel();
            button11.Id = 11;
            button11.Name = "无声介入";
            button11.Icon = "Men";
            button11.IsShow = 1;
            button11.ButtonCommand = new ViewModelCommand(param => RemoteContro(button11.Id));

            // 恢复默认
            DispatchButtonModel button12 = new DispatchButtonModel();
            button12.Id = 12;
            button12.Name = "恢复默认";
            button12.Icon = "ClockwiseArrows";
            button12.IsShow = 1;
            button12.ButtonCommand = new ViewModelCommand(param => ResetDefaultCommand(button12.Id));


            // 占位按钮
            DispatchButtonModel buttonTemp = new DispatchButtonModel();
            buttonTemp.Id = 13;
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
            dict.Add(12, button12);
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



        ///// <summary>
        ///// 点呼
        ///// </summary>
        //private void DefaultUserButtonCommand(string userNum)
        //{
        //    //if (!DMMessageBox.Show("提示", $"呼叫到{userNum}？", DMMessageType.MESSAGE_WARING, DMMessageButton.YesNo))
        //    //{
        //    //    return;
        //    //}

        //    // 若左调度优先
        //    if (NowDispatchNum == LeftDispatchNumModel.Num)
        //    {
        //        // 左调度空闲，无需鉴权即可直接发出，但是也可以进行鉴权呼叫
        //        // 【左调度空闲、右调度空闲】 或者 【左调度空闲、右调度忙碌】 ===》 左调度发起点呼
        //        if (!DispatchDB.DispatchStatus(LeftDispatchNumModel.Num))
        //        {
        //            DataTable dt = DispatchDB.GetNum();
        //            if (dt.Rows.Count == 0) // 判断此次点呼为鉴权点呼还是直接点呼
        //            {
        //                SSH.ExecuteCommand($"fs_cli -x 'bgapi originate user/{NowDispatchNum} {userNum}'");
        //            }
        //            else if (dt.Rows.Count > 0)
        //            {
        //                string uuid = dt.Rows[0]["uuid"].ToString();
        //                SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {userNum} '");
        //            }
        //        }
        //        // 【左调度忙碌、右调度空闲】，并且右调度已经鉴权 ===》 右调度发起点呼
        //        else if (!DispatchDB.DispatchStatus(RightDispatchNumModel.Num))
        //        {
        //            DataTable dt = DispatchDB.GetNum();
        //            if (dt.Rows.Count == 0)
        //            {
        //                DMMessageBox.Show("警告", "右调度未鉴权!!", DMMessageType.MESSAGE_WARING);
        //            }
        //            else
        //            {
        //                string uuid = dt.Rows[0]["uuid"].ToString();
        //                SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {userNum} '");
        //            }
        //        }




        //        // 【左调度忙碌,右调度忙碌】
        //        else if (DispatchDB.DispatchStatus(LeftDispatchNumModel.Num) && DispatchDB.DispatchStatus(RightDispatchNumModel.Num))
        //        {
        //            DMMessageBox.Show("警告", "左调度,右调度均处于忙碌状态!!!", DMMessageType.MESSAGE_FAIL);
        //        }

        //    }

        //    // 若右调度优先
        //    if (NowDispatchNum == RightDispatchNumModel.Num)
        //    {
        //        // 右调度空闲，无需鉴权即可直接发出，但是也可以进行鉴权呼叫
        //        // 【左调度空闲、右调度空闲】 或者 【左调度忙碌、右调度空闲】 ===》 右调度发起点呼
        //        if (!DispatchDB.DispatchStatus(RightDispatchNumModel.Num))
        //        {
        //            DataTable dt = DispatchDB.GetNum();
        //            if (dt.Rows.Count == 0) // 判断右调度此次点呼为鉴权点呼还是直接点呼
        //            {
        //                SSH.ExecuteCommand($"fs_cli -x 'bgapi originate user/{NowDispatchNum} {userNum} '");
        //            }
        //            else if (dt.Rows.Count > 0)
        //            {
        //                string uuid = dt.Rows[0]["uuid"].ToString();
        //                SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {userNum} '");
        //            }
        //        }
        //        // 【左调度空闲、右调度忙碌】，并且左调度已经鉴权 ===》 左调度发起点呼
        //        else if (!DispatchDB.DispatchStatus(LeftDispatchNumModel.Num))
        //        {
        //            DataTable dt = DispatchDB.GetNum();
        //            if (dt.Rows.Count == 0)
        //            {
        //                DMMessageBox.Show("警告", "左调度未鉴权!!", DMMessageType.MESSAGE_WARING);
        //            }
        //            else
        //            {
        //                string uuid = dt.Rows[0]["uuid"].ToString();
        //                SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {uuid} {userNum} '");
        //            }
        //        }


        //        // 【左调度忙碌,右调度忙碌】
        //        else if (DispatchDB.DispatchStatus(LeftDispatchNumModel.Num) && DispatchDB.DispatchStatus(RightDispatchNumModel.Num))
        //        {
        //            DMMessageBox.Show("警告", "左调度、右调度均处于忙碌状态!!!", DMMessageType.MESSAGE_WARING);
        //        }


        //    }

        //}


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
        /// 远程操控，无声接入
        /// </summary>
        private async void Remote(string userNum)
        {
            try
            {
                // 1. 判断是否在通话中
                if (CommonDB.IsCallingByNumber(userNum))
                {
                    DMMessageBox.Show("警告", "通话中无法进行无声介入", DMMessageType.MESSAGE_WARING);
                    ResetUserButtonCommand();
                    return;
                }

                // 2. 无声介入失败
                {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response = await client.GetAsync("http://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port2 + "/rtmp/" + userNum + "/1");
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    RTMPResponse rTMPResponse = JsonConvert.DeserializeObject<RTMPResponse>(responseBody);
                    if (!rTMPResponse.success)
                    {
                        DMMessageBox.Show("警告", "无声介入失败", DMMessageType.MESSAGE_WARING);
                        HttpClient client556 = new HttpClient();
                        HttpResponseMessage response556 = await client556.GetAsync("http://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port2 + "/rtmp/" + userNum + "/2");
                        response556.EnsureSuccessStatusCode();
                        string responseBody556 = response556.Content.ReadAsStringAsync().Result;
                        RTMPResponse rTMPResponse556 = JsonConvert.DeserializeObject<RTMPResponse>(responseBody556);
                        ResetUserButtonCommand();
                        return;
                    }
                }

                // 3. 无声介入成功
                {
                    IsShowButtonOfRemoteContro = true;
                    if (RemoteControCommonVar._libVLC == null || RemoteControCommonVar._mediaPlayer == null || RemoteControCommonVar.phoneVideoPlayer.MediaPlayer == null)
                    {
                        RemoteControCommonVar._libVLC = new LibVLC();
                        RemoteControCommonVar._mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(RemoteControCommonVar._libVLC);
                        RemoteControCommonVar.phoneVideoPlayer.MediaPlayer = RemoteControCommonVar._mediaPlayer;
                    }
                    Uri uri = new Uri("rtmp://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port1 + "/live/livestreams_" + userNum);
                    Media media = new Media(RemoteControCommonVar._libVLC, uri);
                    RemoteControCommonVar._mediaPlayer.Play(media);
                    RemoteControVideoMute = true;
                    RemoteControCommonVar._mediaPlayer.Mute = true; // 默认静音
                    NowPhoneRemoteControNumber = userNum;

                    ResetUserButtonCommand();
                }
            }
            catch (Exception ex)
            {
                DMMessageBox.Show("警告", "无声介入失败", DMMessageType.MESSAGE_WARING);
                ResetUserButtonCommand();
            }
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
            //DataTable dt = DispatchDB.GetNum();
            //if (dt.Rows.Count == 0)
            //{
            //	DMMessageBox.Show("警告", "未鉴权!!", DMMessageType.MESSAGE_WARING);
            //}
            //else
            //{
            //	DataTable dt1 = DispatchDB.GetUUid(userNum);
            //	string uuid = dt1.Rows[0]["uuid"].ToString();
            //	SSH.ExecuteCommand($"fs_cli -x 'uuid_hold {uuid}'");
            //}
            DataTable dt1 = DispatchDB.GetUUid(userNum);
            string uuid = dt1.Rows[0]["uuid"].ToString();
            SSH.ExecuteCommand($"fs_cli -x 'uuid_hold {uuid}'");

            ResetUserButtonCommand();
        }

        /// <summary>
        /// 呼叫恢复
        /// </summary>
        private void CallRecovery(string userNum)
        {
            //DataTable dt = DispatchDB.GetNum();
            //if (dt.Rows.Count == 0)
            //{
            //	DMMessageBox.Show("警告", "未鉴权!!", DMMessageType.MESSAGE_WARING);
            //}
            //else
            //{
            //	DataTable dt1 = DispatchDB.GetUUid(userNum);
            //	string uuid = dt1.Rows[0]["uuid"].ToString();
            //	SSH.ExecuteCommand($"fs_cli -x 'uuid_hold off {uuid}'");
            //}
            DataTable dt1 = DispatchDB.GetUUid(userNum);
            string uuid = dt1.Rows[0]["uuid"].ToString();
            SSH.ExecuteCommand($"fs_cli -x 'uuid_hold off {uuid}'");

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

        /*/// <summary>
        /// 点击多方通话按钮，给用户按钮绑定选择事件
        /// </summary>
        /// <param name="userNum"></param>
        private void MultiCall(string userNum)
        {
            ResetUserButtonCommand();
        }*/

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

            // 4. 停止无声介入
            Task.Run(async () =>
            {
                if (NowPhoneRemoteControNumber != "")
                {
                    HttpClient client2 = new HttpClient();
                    HttpResponseMessage response2 = await client2.GetAsync("http://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port2 + "/rtmp/" + NowPhoneRemoteControNumber + "/2");
                    response2.EnsureSuccessStatusCode();
                    string responseBody2 = response2.Content.ReadAsStringAsync().Result;
                    RTMPResponse rTMPResponse2 = JsonConvert.DeserializeObject<RTMPResponse>(responseBody2);
                    IsShowButtonOfRemoteContro = false;
                    if (RemoteControCommonVar._mediaPlayer != null)
                        RemoteControCommonVar._mediaPlayer.Mute = true;
                    NowPhoneRemoteControNumber = "";
                }
            });
        }

        /// <summary>
        /// 无声介入
        /// </summary>
        public void RemoteContro(int Id)
        {
            ClickDispatchButtonChangeBackgroundColor(Id);

            Task.Run(async () =>
            {
                if (NowPhoneRemoteControNumber != "")
                {
                    HttpClient client2 = new HttpClient();
                    HttpResponseMessage response2 = await client2.GetAsync("http://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port2 + "/rtmp/" + NowPhoneRemoteControNumber + "/2");
                    response2.EnsureSuccessStatusCode();
                    string responseBody2 = response2.Content.ReadAsStringAsync().Result;
                    RTMPResponse rTMPResponse2 = JsonConvert.DeserializeObject<RTMPResponse>(responseBody2);
                    IsShowButtonOfRemoteContro = false;
                    if (RemoteControCommonVar._mediaPlayer != null)
                        RemoteControCommonVar._mediaPlayer.Mute = true;
                    NowPhoneRemoteControNumber = "";
                }
            });

            foreach (DefaultUserModel userModel in TempAllButtons)
            {
                userModel.ButtonCommand = new ViewModelCommand(par =>
                {
                    Remote(userModel.Usernum);
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



        #region 右上角的无声介入


        public Boolean RemoteControVideoMute = true;

        public String NowPhoneRemoteControNumber = "";

        public ICommand PhoneMUTECommand => new ViewModelCommand(param =>
        {
            if (RemoteControCommonVar._mediaPlayer == null)
            {
                DMMessageBox.ShowInfo("当前未处于无声介入状态");
                return;
            }

            // 静音 -> 不静音
            if (RemoteControVideoMute)
            {
                RemoteControVideoMute = false;
                RemoteControCommonVar._mediaPlayer.Mute = false;
            }
            // 不静音 -> 静音
            else
            {
                RemoteControVideoMute = true;
                RemoteControCommonVar._mediaPlayer.Mute = true;
            }
        });

        public ICommand PhoneSWITCHFRONTCommand => new ViewModelCommand(async param =>
        {
            if (RemoteControCommonVar._mediaPlayer == null)
            {
                DMMessageBox.ShowInfo("当前未处于无声介入状态");
                return;
            }

            // 切换摄像头
            HttpClient client2 = new HttpClient();
            HttpResponseMessage response2 = await client2.GetAsync("http://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port2 + "/rtmp/" + NowPhoneRemoteControNumber + "/3");
            response2.EnsureSuccessStatusCode();
            string responseBody2 = response2.Content.ReadAsStringAsync().Result;
            RTMPResponse rTMPResponse2 = JsonConvert.DeserializeObject<RTMPResponse>(responseBody2);
        });

        public ICommand PhoneRemoteControFullScreen => new ViewModelCommand(async param =>
        {
            if (RemoteControCommonVar._mediaPlayer == null)
            {
                DMMessageBox.ShowInfo("当前未处于无声介入状态");
                return;
            }

            IsShowButtonOfRemoteContro = false;

            RemoteControView remoteControView = new RemoteControView(NowPhoneRemoteControNumber);
            remoteControView.Closed += (sender, args) =>
            {
                IsShowButtonOfRemoteContro = true;
                if (RemoteControCommonVar._libVLC == null || RemoteControCommonVar._mediaPlayer == null || RemoteControCommonVar.phoneVideoPlayer.MediaPlayer == null)
                {
                    RemoteControCommonVar._libVLC = new LibVLC();
                    RemoteControCommonVar._mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(RemoteControCommonVar._libVLC);
                    RemoteControCommonVar.phoneVideoPlayer.MediaPlayer = RemoteControCommonVar._mediaPlayer;
                }
                Uri uri = new Uri("rtmp://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port1 + "/live/livestreams_" + NowPhoneRemoteControNumber);
                Media media = new Media(RemoteControCommonVar._libVLC, uri);
                RemoteControCommonVar._mediaPlayer.Play(media);
                RemoteControVideoMute = true;
                RemoteControCommonVar._mediaPlayer.Mute = true; // 默认静音
            };
            remoteControView.ShowDialog();
        });

        public ICommand CloseSmallScreenRemoteContro => new ViewModelCommand(async param =>
        {
            Task.Run(async () =>
            {
                if (NowPhoneRemoteControNumber != "")
                {
                    HttpClient client2 = new HttpClient();
                    HttpResponseMessage response2 = await client2.GetAsync("http://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port2 + "/rtmp/" + NowPhoneRemoteControNumber + "/2");
                    response2.EnsureSuccessStatusCode();
                    string responseBody2 = response2.Content.ReadAsStringAsync().Result;
                    RTMPResponse rTMPResponse2 = JsonConvert.DeserializeObject<RTMPResponse>(responseBody2);
                    IsShowButtonOfRemoteContro = false;
                    if (RemoteControCommonVar._mediaPlayer != null)
                        RemoteControCommonVar._mediaPlayer.Mute = true;
                    NowPhoneRemoteControNumber = "";
                }
            });
        });


        #endregion



        #region 转换窗口时销毁定时器

        public void Dispose()
        {
            _timer1?.Dispose();
            _timer2?.Dispose();

            if (RemoteControCommonVar.phoneVideoPlayer != null && RemoteControCommonVar.phoneVideoPlayer.MediaPlayer != null)
            {
                RemoteControCommonVar.phoneVideoPlayer.MediaPlayer.Stop();
                RemoteControCommonVar.phoneVideoPlayer.MediaPlayer.Media = null;
                RemoteControCommonVar.phoneVideoPlayer.MediaPlayer.Dispose();
                RemoteControCommonVar.phoneVideoPlayer.MediaPlayer = null;
                RemoteControCommonVar.phoneVideoPlayer = null;
            }
            if (RemoteControCommonVar._libVLC != null)
            {
                RemoteControCommonVar._libVLC.Dispose();
                RemoteControCommonVar._libVLC = null;
            }

            Task.Run(async () =>
            {
                if (NowPhoneRemoteControNumber != "")
                {
                    HttpClient client2 = new HttpClient();
                    HttpResponseMessage response2 = await client2.GetAsync("http://" + DMVariable.SSHIP + ":" + RemoteControCommonVar.Port2 + "/rtmp/" + NowPhoneRemoteControNumber + "/2");
                    response2.EnsureSuccessStatusCode();
                    string responseBody2 = response2.Content.ReadAsStringAsync().Result;
                    RTMPResponse rTMPResponse2 = JsonConvert.DeserializeObject<RTMPResponse>(responseBody2);
                }
            });
        }

        #endregion



        /*
        private readonly Timer _timer3; // 通话记录
         
         _timer3 = new Timer(LoadCallingUsers, null, 0, 1000);  // 通话记录
         
        //正在进行的通话记录
        public ObservableCollection<DefaultUserModel> _callingUser;
        public ObservableCollection<DefaultUserModel> CallingUser { get => _callingUser; set { _callingUser = value; OnPropertyChanged(nameof(CallingUser)); } }

        #region 通话显示


        private void LoadCallingUsers(object e)
        {
            List<DefaultUserModel> models = DispatchDB.GetCallingUser(queryNowNumber, queryNowTime, queryNowMissCall);
            ObservableCollection<DefaultUserModel> _callingUser = new ObservableCollection<DefaultUserModel>(models);
            CallingUser = _callingUser;
        }
        #endregion

        _timer3?.Dispose();
         */
    }
}
