using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibVLCSharp.Shared;

namespace MyMVVM.Phone.Utils
{
    public class RemoteControCommonVar
    {
        public static LibVLC _libVLC = null;
        public static LibVLCSharp.WPF.VideoView phoneVideoPlayer = null;
        public static LibVLCSharp.Shared.MediaPlayer _mediaPlayer = null;

        public static String Port1 = "19358"; // RTMP 服务器的端口号
        public static String Port2 = "8080";  // 无线通信后台端口，用来控制手机是否推流的
    }
}
