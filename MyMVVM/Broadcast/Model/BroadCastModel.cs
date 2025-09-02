using MyMVVM.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Broadcast
{
    public class BroadCastModel : ViewModelsBase
    {

        public int Id { get; set; }

        public int GroupId { get; set; }

        public ObservableCollection<string> Users { get; set; }

        public string TTSText { get; set; }

        /**
        共有多种可供选择的值，可考虑设置 常量类、枚举类
            1. 人工广播：组内选人
            2. 人工广播：组内全部
            3. 多方广播：跨组选人
            4. 音乐广播：组内选人
            5. 音乐广播：组内全部
            6. TTS广播：组内选人
            7. TTS广播：组内全部
            8. 任务广播：组内选人TTS
            9. 任务广播：组内全部TTS
            10. 任务广播：组内选人音乐
            11. 任务广播：组内全部音乐
            12. 定时广播：组内选人TTS
            13. 定时广播：组内全部TTS
            14. 定时广播：组内选人音乐
            15. 定时广播：组内全部音乐

            16. 全体广播：人工广播
            17. 全体广播：音乐广播
         */
        public BroadcastTypeEnum Type { get; set; }

        public BroadCastPlayStatusEnum PlayStatus { get; set; }

        public string CreateTime { get; set; }

        public string BroadCastBeginTime { get; set; }

        public string BroadCastEndTime { get; set; }

        public string TimeStamp { get; set; }

        public string MusicPath { get; set; }

        public string DispatchNum { get; set; }

        /// <summary>
        /// 仅针对定时广播：设置一个定时广播的持续时间，以分钟为单位
        /// </summary>
        public int BroadcastDuration { get; set; }

        /// <summary>
        /// 音乐，TTS 播放次数
        /// </summary>
        public int PlayCount { get; set; }

    }
}

