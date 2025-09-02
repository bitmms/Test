using System;

namespace MyMVVM.Monitor.Utils
{
    public class MonitorUtil
    {
        /// <summary>
        /// 登录设备，返回 LoginCode
        /// </summary>
        public static int LoginDevice(string host, ushort port, string username, string password, string type)
        {
            var ret = -1;
            switch (type)
            {
                case "海康":
                    ret = HKUtil.LoginMonitorDevice(host, port, username, password);
                    break;
                case "大华":
                    ret = DHUtil.LoginMonitorDevice(host, port, username, password);
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 开启实时预览，返回 PreViewCode
        /// </summary>
        public static int PreViewDevice(int loginCode, IntPtr intPtr, string type)
        {
            var ret = -1;
            switch (type)
            {
                case "海康":
                    ret = HKUtil.PreViewMonitorDevice(loginCode, intPtr);
                    break;
                case "大华":
                    ret = DHUtil.PreViewMonitorDevice(loginCode, intPtr);
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 设备语音对讲，返回 TalkCode
        /// </summary>
        public static int TalkDevice(int loginCode, string type)
        {
            var ret = -1;
            switch (type)
            {
                case "海康":
                    ret = HKUtil.StartTalkDevice(loginCode);
                    break;
                case "大华":
                    ret = DHUtil.StartTalkDevice(loginCode);
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 停止语音对讲，传入 TalkCode
        /// </summary>
        public static void StopTalk(int talkCode, string type)
        {
            switch (type)
            {
                case "海康":
                    HKUtil.StopTalk(talkCode);
                    break;
                case "大华":
                    DHUtil.StopTalk(talkCode);
                    break;
            }
        }

        /// <summary>
        /// 停止实时预览，传入 PreViewCode
        /// </summary>
        public static void StopPreView(int preViewCode, string type)
        {
            switch (type)
            {
                case "海康":
                    HKUtil.StopPreView(preViewCode);
                    break;
                case "大华":
                    DHUtil.StopPreView(preViewCode);
                    break;
            }
        }

        /// <summary>
        /// 退出登录，传入 LoginCode
        /// </summary>
        public static void StopLogin(int loginCode, string type)
        {
            switch (type)
            {
                case "海康":
                    HKUtil.StopLogin(loginCode);
                    break;
                case "大华":
                    DHUtil.StopLogin(loginCode);
                    break;
            }
        }

        /// <summary>
        /// 释放 SDK 资源
        /// </summary>
        public static void CleanMonitorSDK()
        {
            HKUtil.CleanSDK(); // 释放海康
            DHUtil.CleanSDK(); // 释放大华
        }
    }
}