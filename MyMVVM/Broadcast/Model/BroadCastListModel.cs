using MyMVVM.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyMVVM.Broadcast
{
    public class BroadCastListModel : ViewModelsBase
    {
        public int Id { get; set; }
        public string DisplayType { get; set; }
        public string DisplayContent { get; set; }
        public string DisplayObject { get; set; }
        public string DisplayTime { get; set; }
        public string DisplayPlayStatus { get; set; }
        private bool _isShowCancelButton;
        public bool IsShowCancelButton { get => _isShowCancelButton; set => SetProperty(ref _isShowCancelButton, value); }
        private ICommand _cancelCommand;
        public ICommand CancelCommand { get => _cancelCommand; set => SetProperty(ref _cancelCommand, value); }
        private ICommand _stopCommand;
        public ICommand StopCommand { get => _stopCommand; set => SetProperty(ref _stopCommand, value); }
        private bool _isShowStopButton;
        public bool IsShowStopButton { get => _isShowStopButton; set => SetProperty(ref _isShowStopButton, value); }
        public string PlayCount { get; set; }



        public string RealObject { get; set; }
        public string RealContent { get; set; }






    }
}

