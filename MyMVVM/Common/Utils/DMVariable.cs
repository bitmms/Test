using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.MainWindow.View;

namespace MyMVVM.Common.Utils
{
    /// <summary>
    /// 全局变量
    /// </summary>
    public class DMVariable
    {
        public static string POSTGRESQL_IP = "";
        public static string POSTGRESQL_PORT = "5432";
        public static string POSTGRESQL_USERNAME = "postgres";
        public static string POSTGRESQL_PASSWORD = "pg123456";
        public static string POSTGRESQL_DATABASE = "freeswitch";
        // public static string DB_STRING = $"Host={DMVariable.LOGIN_IP}; Port={DMVariable.POSTGRESQL_PORT}; User Id={DMVariable.POSTGRESQL_USERNAME}; Password={DMVariable.POSTGRESQL_PASSWORD}; Database={DMVariable.POSTGRESQL_DATABASE}";
        public static string DB_STRING = $"";

        public static string SSHIP = "";
        public static string SSHUsername = "root";
        public static string SSHPassword = "dm@123456";

        public static string NowLoginUserName = ""; // 记录当前登录的用户名称

        /// <summary>
        /// 音乐文件同步的状态
        /// </summary>
        public class MusicSyncState
        {
            /// <summary>
            /// 音乐文件同步中
            /// </summary>
            public static bool IsMusicSyncing;

            /// <summary>
            /// 音乐文件同步成功
            /// </summary>
            public static bool IsMusicSyncSuccess;
        }

        /// <summary>
        /// 音乐文件上传的状态
        /// </summary>
        public class MusicUploadState
        {
            /// <summary>
            /// 音乐文件同步中
            /// </summary>
            public static bool IsMusicUploading = false;

            /// <summary>
            /// 音乐文件同步成功
            /// </summary>
            public static bool IsMusicUploadSuccess = true;

            public static int NowNumber;

            public static int TotalNumber;
        }

        /// <summary>
        /// 监控视频窗口
        /// </summary>
        public static System.Windows.Forms.PictureBox broadcastVideoForm;
        public static int retFlag;

        /// <summary>
        /// 监控视频最近一次查看的用户
        /// </summary>
        public static List<Dictionary<string, string>> broadcastCameraVideoList = new List<Dictionary<string, string>>();
    }
}
