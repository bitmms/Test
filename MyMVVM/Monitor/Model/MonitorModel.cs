using System.Windows.Input;
using MyMVVM.Common.ViewModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace MyMVVM.Monitor.Model
{
    public class MonitorModel : ViewModelsBase
    {
        private string _ButtonText;
        private int _Id;
        private string _Name;
        private string _IP;
        private int _Port;
        private string _Username;
        private string _Password;
        private string _FontColor;
        private string _Background;
        private bool _IsShow;
        private bool _IsShowTalk;
        private bool _IsNotShowTalk;


        public int LoginCode { get; set; }
        public bool IsOnline { get; set; }
        public string Type { get; set; }
        public int PreVirwCode { get; set; }
        public int TalkCode { get; set; }
        public bool IsTalkIng { get; set; }
        private ICommand _ButtonCommand;


        public int Id { get => _Id; set { _Id = value; OnPropertyChanged(nameof(Id)); } }
        public string Name { get => _Name; set { _Name = value; OnPropertyChanged(nameof(Name)); } }
        public string FontColor { get => _FontColor; set { _FontColor = value; OnPropertyChanged(nameof(FontColor)); } }
        public string ButtonText { get => _ButtonText; set { _ButtonText = value; OnPropertyChanged(nameof(ButtonText)); } }
        public string IP { get => _IP; set { _IP = value; OnPropertyChanged(nameof(IP)); } }
        public int Port { get => _Port; set { _Port = value; OnPropertyChanged(nameof(Port)); } }
        public bool IsShowTalk { get => _IsShowTalk; set { _IsShowTalk = value; OnPropertyChanged(nameof(IsShowTalk)); } }
        public bool IsNotShowTalk { get => _IsNotShowTalk; set { _IsNotShowTalk = value; OnPropertyChanged(nameof(IsNotShowTalk)); } }
        public string Username { get => _Username; set { _Username = value; OnPropertyChanged(nameof(Username)); } }
        public string Password { get => _Password; set { _Password = value; OnPropertyChanged(nameof(Password)); } }
        public string Background { get => _Background; set { _Background = value; OnPropertyChanged(nameof(Background)); } }
        public bool IsShow { get => _IsShow; set { _IsShow = value; OnPropertyChanged(nameof(IsShow)); } }
        public ICommand ButtonCommand { get => _ButtonCommand; set => SetProperty(ref _ButtonCommand, value); }
    }
}