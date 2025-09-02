using System.Collections.Generic;
using System.Timers;

namespace MyMVVM.Broadcast.Utils
{
    /// <summary>
    /// 广播定时器容器
    /// </summary>
    public class TimerPool
    {
        private static Dictionary<string, Timer> TimerList = new Dictionary<string, Timer>();

        /// <summary>
        /// 增加定时器到容器中
        /// </summary>
        public static void AddTimer(string name, Timer timer)
        {
            TimerList.Add(name, timer);
        }

        /// <summary>
        /// 停止、释放并移除容器中指定的定时器
        /// </summary>
        public static void StopAndRemoveTimer(string timerName)
        {
            if (TimerList.ContainsKey(timerName))
            {
                Timer timer = TimerList[timerName];
                timer.Stop();
                timer.Dispose();
                TimerList.Remove(timerName);
            }
        }

        /// <summary>
        /// 获取指定的定时器【不存在就返回 null 】
        /// </summary>
        public static Timer GetTimer(string timerName)
        {
            if (!TimerList.ContainsKey(timerName))
            {
                return TimerList[timerName];
            }
            return null;
        }
    }
}