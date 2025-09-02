using MyMVVM.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyMVVM.Broadcast
{
    public class TTSModel : ViewModelsBase
    {
        public int Id { get; set; }

        // 文件名称
        public string Name { get; set; }

        // 时长
        public string Time { get; set; }

        public string Path { get; set; }
        public string Text { get; set; }


        private ICommand _DeleteTTSCommand;
        public ICommand DeleteTTSCommand { get => _DeleteTTSCommand; set => SetProperty(ref _DeleteTTSCommand, value); }
    }
}
