using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HaiKang;
using MyMVVM.Common.Utils;
using MyMVVM.Common.ViewModel;
using MyMVVM.Monitor;
using MyMVVM.WIFI.Model;
using MyMVVM.WIFI.View;

namespace MyMVVM.WIFI.ViewModel
{
    public class WIFIViewModel : ViewModelsBase, IDisposable
    {

        private Color UserDefault = (Color)Application.Current.Resources["UserDefault"];
        private Color UserOnline = (Color)Application.Current.Resources["UserOnline"];

        private ObservableCollection<WIFIModel> _wifiDateList;

        public ObservableCollection<WIFIModel> WIFIDateList { get => _wifiDateList; set => SetProperty(ref _wifiDateList, value); }

        public void Dispose()
        {

        }

        public WIFIViewModel()
        {
            WIFIDateList = new ObservableCollection<WIFIModel>();
            LoadWIFIButton();
        }

        /// <summary>
        /// 从数据库查询已保存的基站信息
        /// </summary>
        private void LoadWIFIButton()
        {
            WIFIDateList = WIFIDB.GetWifiList();

            // 从数据库加载已保存的基站设备信息
            foreach (WIFIModel model in WIFIDateList)
            {
                model.WifiCommand = new ViewModelCommand(jj =>
                {
                    Process.Start(new ProcessStartInfo($"http://{model.WIFIIP}/") { UseShellExecute = true });
                });
                model.Background = DMUtil.ColorToHex(UserDefault);
            }

            // 异步扫描哪台设备掉线了
            Task.Run(() =>
            {
                foreach (WIFIModel model in WIFIDateList)
                {
                    if (CheckDeviceOnlineStatus(model.WIFIIP))
                    {
                        model.Background = DMUtil.ColorToHex(UserOnline);
                    }
                }
            });

        }


        private bool CheckDeviceOnlineStatus(string IP)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            PingReply reply = pingSender.Send(IP, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 点击扫描基站按钮
        /// </summary>
        public ICommand ScanWIFICommand => new ViewModelCommand(param =>
        {
            ScanWIFIView scanWIFI = new ScanWIFIView();
            ScanWIFIViewModel scanWIFIViewModel = new ScanWIFIViewModel(WIFIDateList);
            scanWIFI.DataContext = scanWIFIViewModel;
            scanWIFI.Closed += (sender, args) =>
            {
                if (scanWIFIViewModel.IsAdd)
                {
                    LoadWIFIButton();
                }
            };
            scanWIFI.ShowDialog();
        });
    }
}

