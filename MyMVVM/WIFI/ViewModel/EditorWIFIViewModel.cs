using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.WIFI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyMVVM.WIFI.ViewModel
{
    public class EditorWIFIViewModel : ViewModelsBase
    {
        private WIFIModel _wifi;

        private string _Name;
        public string Name { get => _Name; set => SetProperty(ref _Name, value); }

        private string _IP;
        public string IP { get => _IP; set => SetProperty(ref _IP, value); }

        private string _MAC;
        public string MAC { get => _MAC; set => SetProperty(ref _MAC, value); }


        public EditorWIFIViewModel(WIFIModel wifi)
        {
            Name = wifi.WIFIName;
            IP = wifi.WIFIIP;
            MAC = wifi.WIFIMAC;
            _wifi = wifi;
        }


        // 确定按钮
        public ICommand ConfirmButtonCommand => new ViewModelCommand(param =>
        {
            if (Name == null || Name == "")
            {
                DMMessageBox.ShowInfo("请输入格式正确的名称");
                return;
            }
            _wifi.WIFIName = Name;
            WIFIDB.UpdateWIFIModelName(_wifi);

            // 获得窗口
            Window window = (Window)param;
            window.Close();
        });


        // 取消按钮
        public ICommand CancelButtonCommand => new ViewModelCommand(param =>
        {
            Window window = (Window)param;
            window.Close();
        });
    }
}

