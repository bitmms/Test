using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyMVVM.Common.ViewModel;

namespace MyMVVM.MainWindow.Model
{
    public class SideButtonModel : ViewModelsBase
    {
        public int Id { get; set; }
        public int IsOk { get; set; }
        public int IsShow { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }

        private ICommand _showUserControlCommand;

        public ICommand ShowUserControlCommand { get { return _showUserControlCommand; } set { SetProperty(ref _showUserControlCommand, value); } }

    }
}
