using MyMVVM.Battery.ViewModel;
using MyMVVM.Broadcast.ViewModel;
using MyMVVM.Common.Utils;
using MyMVVM.Common.ViewModel;
using MyMVVM.Common;
using MyMVVM.Conference.ViewModel;
using MyMVVM.Dispatch.ViewModel;
using MyMVVM.MainWindow.Model;
using MyMVVM.Map.ViewModel;
using MyMVVM.Monitor.ViewModel;
using MyMVVM.Phone.ViewModel;
using MyMVVM.WIFI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.Common.View;
using MyMVVM.MainWindow.Util;

namespace MyMVVM.MainWindow.ViewModel
{
    public class MainFormViewModel : ViewModelsBase
    {

        private ViewModelsBase _activeViewModel;

        public ViewModelsBase ActiveViewModel { get { return _activeViewModel; } set { SetProperty(ref _activeViewModel, value); } }

        private ObservableCollection<SideButtonModel> _sideButtonList;
        public ObservableCollection<SideButtonModel> SideButtonList { get => _sideButtonList; set => SetProperty(ref _sideButtonList, value); }



        public MainFormViewModel()
        {
            // 加载侧边栏按钮
            LoadSideButtons();

            // 默认激活的控件
            ActiveViewModel = new DispatchViewModel();


            // 初始化
            // 恢复广播、判断软件过期时间、同步音乐、加载软电话用户
            new InitUtil().InitSystem();
        }


        /// <summary>
        /// 切换视图模型
        /// </summary>
        private void SwitchViewModel(ViewModelsBase newViewModel)
        {
            if (ActiveViewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }

            ActiveViewModel = newViewModel;
        }

        /// <summary>
        /// 加载侧边栏按钮
        /// </summary>
        public void LoadSideButtons()
        {
            // 声明 ViewModel 字典
            Dictionary<string, Type> ViewModelDict = new Dictionary<string, Type>()
            {
                {"调度", typeof(DispatchViewModel)},
                {"扩播", typeof(BroadCastViewModel)},
                {"会议", typeof(ConferenceViewModel)},
                {"监控", typeof(MonitorViewModel)},
                {"基站", typeof(WIFIViewModel)},
                {"地图", typeof(MapViewModel)},
                {"电源", typeof(BatteryViewModel)},
                {"手机", typeof(PhoneViewModel)},
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
            };

            SideButtonList = new ObservableCollection<SideButtonModel>();
            List<SideButtonModel> tempList = SideButtonDB.GetAllSideButtons();
            foreach (SideButtonModel item in tempList)
            {
                if (IconDict.ContainsKey(item.Name))
                {
                    item.Icon = IconDict[item.Name];
                    item.ShowUserControlCommand = new ViewModelCommand(p =>
                    {
                        if (item.IsOk == 1)
                        {
                            var newViewModel = (ViewModelsBase)Activator.CreateInstance(ViewModelDict[item.Name]);
                            SwitchViewModel(newViewModel);
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

    }
}
