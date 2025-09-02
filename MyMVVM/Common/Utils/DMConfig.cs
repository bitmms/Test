using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Common.Utils
{
    /// <summary>
    /// 系统配置
    /// </summary>
    public class DMConfig
    {
        // 统一加密key
        public static String KEY = "浙江达名科技调度系统";

        // 会议模块中设置主机的信息【如果这里进行修改操作，则 function.lua 脚本同样要改】
        public class ConferenceCallerInfo
        {
            /// <summary>
            /// 会议主机名称
            /// </summary>
            public static string OriginationCallerName = "zjdmkj(conference)";

            /// <summary>
            /// 会议主机号码
            /// </summary>
            public static string OriginationCallerNumber = "0000000000";
        }

        public static bool IsShowLogo()
        {
            return true;
        }
    }
}
