using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.Common.ViewModel;

namespace MyMVVM.Speak.ViewModel
{
    // 对讲不使用 ViewModel
    public class SpeakViewModel : ViewModelsBase, IDisposable
    {
        public void Dispose()
        {

        }
    }
}
