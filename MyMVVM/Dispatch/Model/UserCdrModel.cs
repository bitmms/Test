using MyMVVM.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Dispatch.Model
{
    public class UserCdrModel : ViewModelsBase
    {
        public string CallerNum { get; set; }
        public string CalleeNum { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}

