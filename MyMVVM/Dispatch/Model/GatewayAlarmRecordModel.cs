using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.Common.ViewModel;

namespace MyMVVM.Dispatch.Model
{
    public class GatewayAlarmRecordModel : ViewModelsBase
    {
        // id
        public int id { get; set; }
        // 号码
        public string telno { get; set; }
        // 线路状态
        public string lineState { get; set; }
        // 线路长度
        public string lineLength { get; set; }
        // 终端结果
        public string terminationType { get; set; }
    }
}