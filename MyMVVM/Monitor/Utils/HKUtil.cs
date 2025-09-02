using System;
using System.Text;
using HaiKang;

namespace MyMVVM.Monitor.Utils
{
    internal class HKUtil
    {
        /// <summary>
        /// 登录设备，返回 LoginCode
        /// </summary>
        public static int LoginMonitorDevice(string host, ushort port, string username, string password)
        {
            byte[] _UserName = new byte[CHCNetSDK.NET_DVR_LOGIN_USERNAME_MAX_LEN];
            Encoding.ASCII.GetBytes(username, 0, username.Length, _UserName, 0);
            byte[] _Password = new byte[CHCNetSDK.NET_DVR_LOGIN_USERNAME_MAX_LEN];
            Encoding.ASCII.GetBytes(password, 0, password.Length, _Password, 0);
            byte[] _Host = new byte[CHCNetSDK.NET_DVR_DEV_ADDRESS_MAX_LEN];
            Encoding.ASCII.GetBytes(host, 0, host.Length, _Host, 0);
            ushort _Port = port;
            CHCNetSDK.NET_DVR_USER_LOGIN_INFO loginInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO()
            {
                sUserName = _UserName,
                sPassword = _Password,
                sDeviceAddress = _Host,
                wPort = _Port,
            };
            CHCNetSDK.NET_DVR_DEVICEINFO_V40 deviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();
            int loginCode = CHCNetSDK.NET_DVR_Login_V40(ref loginInfo, ref deviceInfo);
            return loginCode;
        }

        /// <summary>
        /// 开启实时预览，返回 PreViewCode
        /// </summary>
        public static int PreViewMonitorDevice(int loginCode, IntPtr intPtr)
        {
            // 开启预览
            CHCNetSDK.NET_DVR_PREVIEWINFO PreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO()
            {
                hPlayWnd = intPtr,      // 预览窗口 播放画面句柄  PictureBox组件句柄
                lChannel = 1,           // Int16.Parse(textBoxChannel.Text);//预览的设备通道
                dwStreamType = 0,       // 码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                dwLinkMode = 1,         // 连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                bBlocked = false,       // 0- 非阻塞取流，1- 阻塞取流
                dwDisplayBufNum = 1,    // 播放库播放缓冲区最大缓冲帧数
                byProtoType = 0,
                byPreviewMode = 0,
            };
            int preViewCode = CHCNetSDK.NET_DVR_RealPlay_V40(loginCode, ref PreviewInfo, null, IntPtr.Zero);
            return preViewCode;
        }

        /// <summary>
        /// 设备语音对讲，返回 TalkCode
        /// </summary>
        public static int StartTalkDevice(int loginCode)
        {
            int lUserID = loginCode;
            uint dwVoiceChan = 1;
            bool bNeedCBNoEncData = true;
            CHCNetSDK.VOICEDATACALLBACKV30 fVoiceDataCallBack = null;
            IntPtr pUser = IntPtr.Zero;
            int talkCode = CHCNetSDK.NET_DVR_StartVoiceCom_V30(lUserID, dwVoiceChan, bNeedCBNoEncData, fVoiceDataCallBack, pUser);
            return talkCode;
        }

        /// <summary>
        /// 停止语音对讲，传入 TalkCode
        /// </summary>
        public static void StopTalk(int talkCode)
        {
            if (talkCode != -1)
            {
                CHCNetSDK.NET_DVR_StopVoiceCom(talkCode);
            }
        }

        /// <summary>
        /// 停止实时预览，传入 PreViewCode
        /// </summary>
        public static void StopPreView(int preViewCode)
        {
            if (preViewCode != -1)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(preViewCode);
            }
        }

        /// <summary>
        /// 退出登录，传入 LoginCode
        /// </summary>
        public static void StopLogin(int loginCode)
        {
            if (loginCode != -1)
            {
                CHCNetSDK.NET_DVR_Logout(loginCode);
            }
        }

        /// <summary>
        /// 释放 SDK 资源
        /// </summary>
        public static void CleanSDK()
        {
            CHCNetSDK.NET_DVR_Cleanup();
        }
    }
}
