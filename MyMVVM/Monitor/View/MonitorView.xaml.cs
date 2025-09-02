using HaiKang;
using MyMVVM.Monitor.ViewModel;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Threading.Tasks;
using MyMVVM.Monitor;
using System.Collections.Generic;

namespace MyMVVM.Monitor.View
{
    public partial class MonitorView : UserControl
    {
        public MonitorView()
        {
            InitializeComponent();
            this.DataContext = new MonitorViewModel(MonitorVideoForm1212.Handle);
        }
    }
}
