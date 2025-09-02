using MyMVVM.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyMVVM.WIFI.Model
{
    public class WIFIModel : ViewModelsBase
    {
        public int Id { get; set; }
        public string WIFIIP { get; set; }

        private string _wifiMac; // 私有字段来存储MAC地址  
        public string WIFIMAC
        {
            get
            {
                return _wifiMac;
            }
            set
            {
                if (value.Contains(":"))
                {
                    _wifiMac = value;
                }
                else
                {
                    _wifiMac = Regex.Replace(value, @"(.{2})", "$1:").TrimEnd(':'); // Mac 地址格式化
                }
            }
        }

        private ICommand _wifiCommand;

        public ICommand WifiCommand { get { return _wifiCommand; } set { SetProperty(ref _wifiCommand, value); } }

        private string _WIFIName;
        public string WIFIName { get { return _WIFIName; } set { SetProperty(ref _WIFIName, value); } }

        private string _backGround;

        public string BackGround { get => _backGround; set => SetProperty(ref _backGround, value); }



        private ICommand _SaveEditorCommand;

        public ICommand SaveEditorCommand { get { return _SaveEditorCommand; } set { SetProperty(ref _SaveEditorCommand, value); } }

        private ICommand _DeleteCommand;

        public ICommand DeleteCommand { get { return _DeleteCommand; } set { SetProperty(ref _DeleteCommand, value); } }

        private string _background;

        public string Background { get { return _background; } set { SetProperty(ref _background, value); } }

    }
}
