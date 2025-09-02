using MyMVVM.Common.ViewModel;
using System.Windows.Input;

namespace MyMVVM.Common.Model
{
    public class DispatchButtonModel : ViewModelsBase
    {
        public int Id;
        public string Name { get; set; }
        public string Icon { get; set; }

        private ICommand _buttonCommand;

        public ICommand ButtonCommand { get => _buttonCommand; set => SetProperty(ref _buttonCommand, value); }

        private string _backgroundColor;

        public string BackgroundColor { get => _backgroundColor; set => SetProperty(ref _backgroundColor, value); }

        public int IsShow { get; set; }

    }
}
