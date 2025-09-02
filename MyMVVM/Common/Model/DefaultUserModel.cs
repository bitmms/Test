using MyMVVM.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyMVVM.Common.Model
{
    public class DefaultUserModel : ViewModelsBase
    {
        public string Username { get; set; }
        public string Usernum { get; set; }

        public string Cid_num { get; set; }
        public string Dest { get; set; }
        public string CallerNum { get; set; }
        public string CalleeNum { get; set; }
        public string Duration { get; set; }

        public string CallNum2 { get; set; }
        public string CallNum1 { get; set; }


        public string WaitUUID { get; set; }
        public string WaitDate { get; set; }
        public string WaitDisplay { get; set; }
        public string WaitUsernum { get; set; }
        /*public string WaitNum { get; set; }
        public string Wait { get; set; }*/


        public bool IsInCall { get; set; }

        public string GroupId { get; set; }
        public string GroupName { get; set; }


        private ICommand _buttonCommand;
        public ICommand ButtonCommand { get => _buttonCommand; set => SetProperty(ref _buttonCommand, value); }

        private string _backgoundColor;
        public string BackgroundColor { get => _backgoundColor; set => SetProperty(ref _backgoundColor, value); }

        private string _userDisplay;
        public string UserDisplay { get => _userDisplay; set => SetProperty(ref _userDisplay, value); }


        private ICommand _groupCommand;
        public ICommand GroupCommand { get => _groupCommand; set => SetProperty(ref _groupCommand, value); }


        private string _userButtonFontColor;
        public string UserButtonFontColor { get => _userButtonFontColor; set => SetProperty(ref _userButtonFontColor, value); }




        public bool IsShowCamera { get; set; }
        public bool IsNotShowCamera { get; set; }
        public string CameraIP { get; set; }
        public string CameraPort { get; set; }
        public string CameraAccount { get; set; }
        public string CameraPassword { get; set; }



    }

}

