using System;

namespace MyMVVM.Monitor.Utils
{
    public class DHUtil
    {
        /// <summary>
        /// 登录设备，返回 LoginCode
        /// </summary>
        public static int LoginMonitorDevice(string host, ushort port, string username, string password)
        {
            return -1;
        }

        /// <summary>
        /// 开启实时预览，返回 PreViewCode
        /// </summary>
        public static int PreViewMonitorDevice(int loginCode, IntPtr intPtr)
        {
            return -1;
        }

        /// <summary>
        /// 设备语音对讲，返回 TalkCode
        /// </summary>
        public static int StartTalkDevice(int loginCode)
        {
            return -1;
        }

        /// <summary>
        /// 停止语音对讲，传入 TalkCode
        /// </summary>
        public static void StopTalk(int talkCode)
        {
        }

        /// <summary>
        /// 停止实时预览，传入 PreViewCode
        /// </summary>
        public static void StopPreView(int preViewCode)
        {
        }

        /// <summary>
        /// 退出登录，传入 LoginCode
        /// </summary>
        public static void StopLogin(int loginCode)
        {
        }

        /// <summary>
        /// 释放 SDK 资源
        /// </summary>
        public static void CleanSDK()
        {
        }
    }
}