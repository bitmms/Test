using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyMVVM.Common.ViewModel;

namespace MyMVVM.Emotion.Model
{
    public class EmotionModel : ViewModelsBase
    {
        public int Id { get; set; }
        public string FromNumber { get; set; }
        public string ToNumber { get; set; }
        public string CallTime { get; set; }
        public string FileName { get; set; }
        public string CallText { get; set; }
        public string EmotionStatu { get; set; }
        public int ActionStatu { get; set; }

        private ICommand _cancelCommand;
        public ICommand CancelCommand { get => _cancelCommand; set => SetProperty(ref _cancelCommand, value); }

        private ICommand _ShowMoreInfo;
        public ICommand ShowMoreInfo { get => _ShowMoreInfo; set => SetProperty(ref _ShowMoreInfo, value); }
    }
}
