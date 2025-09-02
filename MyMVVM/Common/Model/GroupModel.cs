using MyMVVM.Common.ViewModel;
using System.Windows.Input;

namespace MyMVVM.Common.Model
{
    public class GroupModel : ViewModelsBase
    {
        public string Id { get; set; }

        public string CallId { get; set; }

        public string GroupName { get; set; }



        private ICommand _groupButtonCommand;

        public ICommand GroupButtonCommand { get { return _groupButtonCommand; } set { SetProperty(ref _groupButtonCommand, value); } }



        private string _groupButtonColor;

        public string GroupButtonColor { get => _groupButtonColor; set => SetProperty(ref _groupButtonColor, value); }
    }
}
