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
        //  1. 没接电话      灰色 离线
        //  2. 短路、故障    红色 故障
        public string lineState { get; set; }
        // 线路长度
        public string lineLength { get; set; }
        // 终端结果
        public string terminationType { get; set; }
        // 故障描述
        public string desc { get; set; }
        // 显示类型
        //  1. 没有接电话
        //  2. 线路故障
        public int type { get; set; }
    }
}