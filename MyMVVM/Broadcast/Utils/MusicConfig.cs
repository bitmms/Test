using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Broadcast.Utils
{
    public class MusicConfig
    {
        /// <summary>
        /// 音乐播放器的播放状态
        /// </summary>
        public class MusicState
        {
            /// <summary>
            /// 音乐播放中图标
            /// </summary>
            public static string PlayIcon = "\uE669";

            /// <summary>
            /// 音乐暂停中图标
            /// </summary>
            public static string PauseIcon = "\uE66e";
        }
    }
}
