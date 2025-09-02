using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Broadcast
{
    public enum BroadcastTypeEnum
    {
        // 六大实时广播
        [Description("实时广播-人工")]
        RealTimeManualBroadcastOfGroupSelected = 1,
        [Description("实时广播-人工")]
        RealTimeManualBroadcastOfGroupAll = 2,
        [Description("实时广播-TTS")]
        RealTimeTTSBroadcastOfGroupSelected = 3,
        [Description("实时广播-TTS")]
        RealTimeTTSBroadcastOfGroupAll = 4,
        [Description("实时广播-音乐")]
        RealTimeMusicBroadcastOfGroupSelected = 5,
        [Description("实时广播-音乐")]
        RealTimeMusicBroadcastOfGroupAll = 6,


        // 多方广播
        [Description("多方广播-人工")]
        MultiGroupManualBroadcast = 7,
        [Description("多方广播-TTS")]
        MultiGroupTTSBroadcast = 8,
        [Description("多方广播-音乐")]
        MultiGroupMusicBroadcast = 9,


        // 全体广播
        [Description("全体广播-人工")]
        AllManualBroadcast = 10,
        [Description("全体广播-音乐")]
        AllMusicBroadcast = 11,
        [Description("全体广播-TTS")]
        AllTTSBroadcast = 12,


        // 任务广播
        [Description("任务广播-TTS")]
        TaskTTSBroadcastOfGroupSelected = 13,
        [Description("任务广播-TTS")]
        TaskTTSBroadcastOfGroupAll = 14,
        [Description("任务广播-音乐")]
        TaskMusicBroadcastOfGroupSelected = 15,
        [Description("任务广播-音乐")]
        TaskMusicBroadcastOfGroupAll = 16,


        // 定时广播
        [Description("定时广播-TTS")]
        ScheduledTTSBroadcastOfGroupSelected = 17,
        [Description("定时广播-TTS")]
        ScheduledTTSBroadcastOfGroupAll = 18,
        [Description("定时广播-音乐")]
        ScheduledMusicBroadcastOfGroupSelected = 19,
        [Description("定时广播-音乐")]
        ScheduledMusicBroadcastOfGroupAll = 20,
    }
}
