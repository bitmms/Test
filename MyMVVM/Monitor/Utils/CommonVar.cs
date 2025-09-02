using MyMVVM.Monitor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MyMVVM.Monitor
{
    public class CommonVar
    {
        //public static List<MonitorModel> _MonitorList = new List<MonitorModel>();
        //public static List<IntPtr> _IntPtrList = new List<IntPtr>(); // 句柄列表
        //public static List<int> _LoginCode = new List<int>();        // 登录状态码列表
        //public static List<int> _PreviewCode = new List<int>();      // 实时预览状态码列表
        //public static List<int> _TalkCode = new List<int>();         // 实时对讲状态码列表
        //public static List<Timer> _TimerList = new List<Timer>();


        // one
        public static MonitorModel _CurrentModel;
        public static IntPtr _MonitorIntPtr;
        public static int _LoginCode;
        public static int _PreviewCode;
        public static string _CurrentIP;
    }
}
