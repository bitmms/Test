using MyMVVM.Common.ViewModel;
using System.Windows.Input;

namespace MyMVVM.Common.Model
{
    public class DispatchNumModel : ViewModelsBase
    {
        public string Name { get; set; }

        public string _image { get; set; }
        public string Image
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    _image = value;
                    OnPropertyChanged(nameof(Image));
                }
            }
        }

        public string _num { get; set; }

        public string Num { get => _num; set { if (_num != value) { _num = value; OnPropertyChanged(nameof(Num)); } } }

        private string _fontColor { get; set; }

        public string FontColor { get => _fontColor; set { if (_fontColor != value) { _fontColor = value; OnPropertyChanged(nameof(FontColor)); } } }


        private ICommand _dispatchNumCommand;

        public ICommand DispatchNumCommand { get { return _dispatchNumCommand; } set { SetProperty(ref _dispatchNumCommand, value); } }
    }
}
