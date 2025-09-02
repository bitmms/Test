using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Broadcast
{
    public enum BroadCastPlayStatusEnum
    {
        /*
            实时广播
                1 2 3 4
            任务广播
                1 2 3 4 5 6
            定时广播
                5 7
         */

        /// <summary>
        /// 广播未播放
        /// </summary>
        [Description("未播放")]
        Unplayed = 1,

        /// <summary>
        /// 广播已播放
        /// </summary>
        [Description("已播放")]
        Played = 2,

        /// <summary>
        /// 停止全部的广播
        /// </summary>
        [Description("广播被停止【停止全部】")]
        StopAllBroadcastClicked = 3,

        /// <summary>
        /// 停止指定的单个广播被停止
        /// </summary>
        [Description("广播被停止【停止单个】")]
        StopOneBroadcastClicked = 4,

        /// <summary>
        /// 广播被取消
        /// </summary>
        [Description("广播被取消")]
        BroadcastCancelled = 5,

        /// <summary>
        /// 任务广播超时未播放
        /// </summary>
        [Description("任务广播超时未播放")]
        TaskBroadcastTimeout = 6,

        /// <summary>
        /// 定时广播激活中
        /// </summary>
        [Description("定时广播激活中")]
        ScheduledBroadcastActive = 7,
    }
}
