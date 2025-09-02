using MyMVVM.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.MainWindow.Model
{
    public class MissCallModel : ViewModelsBase
    {
        public string MissName { get; set; }
        public string MissNum { get; set; }
        public string MissTime { get; set; }

        public string SelectedItem { get; set; }
    }
}
