using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HaiKang;
using MyMVVM.Broadcast.View;
using MyMVVM.Common;
using MyMVVM.Common.Model;
using MyMVVM.Common.Utils;

namespace MyMVVM.Broadcast.Utils
{
    /// <summary>
    /// 广播工具类
    /// </summary>
    public class BroadCastUtil
    {


        /// <summary>
        /// 委托：将一个方法作为参数传递给另一个方法
        /// </summary>
        private delegate void FunctionParam(BroadCastModel dailyBroadCastModel);


        /// <summary>
        /// 任务广播
        /// </summary>
        private void TaskBroadCast1(FunctionParam functionParam, BroadCastModel dailyBroadCastModel)
        {
            // 1. 获取任务广播的执行时间
            DateTime nextRun = Convert.ToDateTime(dailyBroadCastModel.BroadCastBeginTime);

            // 2. 如果当前时间已经过了任务时间，设置超时未播放【此情况出现在重启客户端之后】
            if (DateTime.Now > nextRun)
            {
                BroadCastDB.UpdateBroadCastIsPlayed(dailyBroadCastModel.Id, (int)BroadCastPlayStatusEnum.TaskBroadcastTimeout);
                return;
            }

            // 3. 从当前时间开始计算距离任务开始的时间间隔，初始化一个 n 毫秒后执行的定时器
            double milliseconds = nextRun.Subtract(DateTime.Now).TotalMilliseconds;
            System.Timers.Timer myTimer = new System.Timers.Timer(milliseconds);

            // 4. 设置定时器在时间间隔到达之后是否重置间隔去重新执行回调函数，false表示仅执行一次回调函数
            myTimer.AutoReset = false;

            // 5. 执行任务
            myTimer.Elapsed += (source, e) =>
            {
                // (1) 播放任务广播
                FunctionParam Func = functionParam;
                Func(dailyBroadCastModel);

                BroadCastDB.UpdateBroadCastIsPlayed(dailyBroadCastModel.Id, (int)BroadCastPlayStatusEnum.Played);

                // (2) 到达指定的结束时间后，发送停止广播的命令
                double second = DateTime.Parse(dailyBroadCastModel.BroadCastEndTime).Subtract(DateTime.Now).TotalSeconds;
                Thread.Sleep(TimeSpan.FromSeconds(second));
                FSSocket.SendCommand($"bgapi conference {"conference-" + dailyBroadCastModel.Type + "-" + dailyBroadCastModel.Id + "-" + dailyBroadCastModel.TimeStamp} hup all");
                // SSH.ExecuteCommand($"fs_cli -x 'conference {"conference-" + dailyBroadCastModel.Type + "-" + dailyBroadCastModel.Id + "-" + dailyBroadCastModel.TimeStamp} hup all'");

                // (3) 释放定时器
                TimerPool.StopAndRemoveTimer("conference-" + dailyBroadCastModel.Type + "-" + dailyBroadCastModel.Id + "-" + dailyBroadCastModel.TimeStamp);
            };

            // 6. 保存定时器到容器中
            TimerPool.AddTimer("conference-" + dailyBroadCastModel.Type + "-" + dailyBroadCastModel.Id + "-" + dailyBroadCastModel.TimeStamp, myTimer);

            // 7. 开始定时器
            myTimer.Start();
        }


        /// <summary>
        /// 定时广播
        /// </summary>
        private void TaskBroadCast2(FunctionParam functionParam, BroadCastModel dailyBroadCastModel)
        {
            // 1. 获取当前时间
            DateTime now = DateTime.Now;

            // 2. 获取该定时广播在今天的执行时间
            dailyBroadCastModel.BroadCastBeginTime = DateTime.Now.ToString("yyyy-MM-dd") + " " + dailyBroadCastModel.BroadCastBeginTime;
            DateTime nextRun = Convert.ToDateTime(dailyBroadCastModel.BroadCastBeginTime);

            // 3. 如果超时，则计算第二天的任务时间
            while (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }
            dailyBroadCastModel.BroadCastBeginTime = nextRun.ToString("yyyy-MM-dd HH:mm:ss");

            // 4. 从当前时间开始计算距离任务开始的时间间隔，初始化一个 n 毫秒后执行的定时器
            double milliseconds = DateTime.Parse(dailyBroadCastModel.BroadCastBeginTime).Subtract(DateTime.Now).TotalMilliseconds;
            System.Timers.Timer myTimer = new System.Timers.Timer(milliseconds);

            // 5. 设置false表示仅执行一次回调函数
            myTimer.AutoReset = false;

            // 6. 执行回调函数
            myTimer.Elapsed += (source, e) =>
            {
                // (1) 播放任务广播
                FunctionParam Func = functionParam;
                Func(dailyBroadCastModel);

                // (2) 在广播开始后的指定时间关闭定时广播
                Task.Run(async () =>
                {
                    await Task.Delay(dailyBroadCastModel.BroadcastDuration * 60 * 1000); // 等待 n 秒
                    SSH.ExecuteCommand($"fs_cli -x 'conference {"conference-" + dailyBroadCastModel.Type + "-" + dailyBroadCastModel.Id + "-" + dailyBroadCastModel.TimeStamp} hup all'");
                });

                // (3) 重新开启下一轮定时广播
                DateTime dateTime1 = Convert.ToDateTime(DateTime.Now.ToString()); // 当前时间
                DateTime dateTime2 = Convert.ToDateTime(dailyBroadCastModel.BroadCastBeginTime).AddDays(1); // 一天后开始下一轮
                myTimer.Interval = dateTime2.Subtract(dateTime1).TotalMilliseconds; // 设置间隔
                myTimer.Start();
            };

            // 7. 保存定时器到容器中
            TimerPool.AddTimer("conference-" + dailyBroadCastModel.Type + "-" + dailyBroadCastModel.Id + "-" + dailyBroadCastModel.TimeStamp, myTimer);

            // 8. 开始定时器
            myTimer.Start();
        }


        /// <summary>
        /// 实时的音乐广播【立刻开始，播放完成之后就停止】
        /// </summary>
        private void RealTimeMusicBroadCast(FunctionParam functionParam, BroadCastModel broadcastModel)
        {
            // 1. 延迟时间，2s
            int delayTime = 2;

            // 2. 初始化一个 10 毫秒后执行的定时器
            System.Timers.Timer myTimer = new System.Timers.Timer(10);

            // 3. 设置定时器在时间间隔到达之后是否重置间隔去重新执行回调函数，false表示仅执行一次回调函数
            myTimer.AutoReset = false;

            // 4. 执行任务
            myTimer.Elapsed += (source, e) =>
            {
                // (1) 播放广播
                FunctionParam Func = functionParam;
                Func(broadcastModel);

                // (2) 到达指定的结束时间后，发送停止广播的命令
                int secondCount = 0;
                StringBuilder sb = new StringBuilder();
                string[] tempMusicList = broadcastModel.MusicPath.Split('!');
                for (int i = 0; i < tempMusicList.Length; i++)
                {
                    string time = MusicDB.GeTimeBytMusicName(tempMusicList[i]);
                    int As = int.Parse(time.Split(':')[0]) * 60;
                    int Bs = int.Parse(time.Split(':')[1]);
                    secondCount = secondCount + As + Bs;
                }
                secondCount = secondCount * broadcastModel.PlayCount;

                Thread.Sleep(TimeSpan.FromSeconds(secondCount + delayTime));
                FSSocket.SendCommand($"bgapi conference {"conference-" + broadcastModel.Type + "-" + broadcastModel.Id + "-" + broadcastModel.TimeStamp} hup all");
                // SSH.ExecuteCommand($"fs_cli -x 'conference {"conference-" + dailyBroadCastModel.Type + "-" + dailyBroadCastModel.Id + "-" + dailyBroadCastModel.TimeStamp} hup all'");

                // (3) 释放定时器
                TimerPool.StopAndRemoveTimer("conference-" + broadcastModel.Type + "-" + broadcastModel.Id + "-" + broadcastModel.TimeStamp);
            };

            // 5. 保存定时器到容器中
            TimerPool.AddTimer("conference-" + broadcastModel.Type + "-" + broadcastModel.Id + "-" + broadcastModel.TimeStamp, myTimer);

            // 6. 开始定时器
            myTimer.Start();
        }


        /// <summary>
        /// 实时的TTS广播【立刻开始，播放完成之后就停止】
        /// </summary>
        private void RealTimeTTSBroadCast(FunctionParam functionParam, BroadCastModel broadcastModel)
        {
            // 1. 延迟时间，2s
            int delayTime = 2;

            // 2. 初始化一个 10 毫秒后执行的定时器
            System.Timers.Timer myTimer = new System.Timers.Timer(10);

            // 3. 设置定时器在时间间隔到达之后是否重置间隔去重新执行回调函数，false表示仅执行一次回调函数
            myTimer.AutoReset = false;

            // 4. 执行任务
            myTimer.Elapsed += (source, e) =>
            {
                // (1) 播放广播
                FunctionParam Func = functionParam;
                Func(broadcastModel);

                // (2) 到达指定的结束时间后，发送停止广播的命令
                int secondCount = 0;
                StringBuilder sb = new StringBuilder();
                string[] tempMusicList = broadcastModel.MusicPath.Split('!');
                for (int i = 0; i < tempMusicList.Length; i++)
                {
                    string time = TTSDB.GeTimeByPath(tempMusicList[i]);
                    int As = int.Parse(time.Split(':')[0]) * 60;
                    int Bs = int.Parse(time.Split(':')[1]);
                    secondCount = secondCount + As + Bs;
                }
                secondCount = secondCount * broadcastModel.PlayCount;

                Thread.Sleep(TimeSpan.FromSeconds(secondCount + delayTime));
                FSSocket.SendCommand($"bgapi conference {"conference-" + broadcastModel.Type + "-" + broadcastModel.Id + "-" + broadcastModel.TimeStamp} hup all");
                // SSH.ExecuteCommand($"fs_cli -x 'conference {"conference-" + dailyBroadCastModel.Type + "-" + dailyBroadCastModel.Id + "-" + dailyBroadCastModel.TimeStamp} hup all'");

                // (3) 释放定时器
                TimerPool.StopAndRemoveTimer("conference-" + broadcastModel.Type + "-" + broadcastModel.Id + "-" + broadcastModel.TimeStamp);
            };

            // 5. 保存定时器到容器中
            TimerPool.AddTimer("conference-" + broadcastModel.Type + "-" + broadcastModel.Id + "-" + broadcastModel.TimeStamp, myTimer);

            // 6. 开始定时器
            myTimer.Start();
        }


        /// <summary>
        /// 广播发起逻辑
        /// </summary>
        public static class BroadCastTypeFunction
        {


            /// <summary>
            /// 根据传入的 BroadCastModel 对象的 type 值自动选择去执行什么广播
            /// </summary>
            public static void HandlerBroadcast(BroadCastModel broadCastModel)
            {

                if (broadCastModel.PlayCount == -1)
                {
                    broadCastModel.PlayCount = 60 * 60 * 24;
                }

                switch (broadCastModel.Type)
                {
                    // 人工广播
                    case BroadcastTypeEnum.RealTimeManualBroadcastOfGroupSelected: FunctionUtils.Fun01(broadCastModel); break;
                    case BroadcastTypeEnum.RealTimeManualBroadcastOfGroupAll: FunctionUtils.Fun02(broadCastModel); break;
                    case BroadcastTypeEnum.MultiGroupManualBroadcast: FunctionUtils.Fun07(broadCastModel); break;
                    case BroadcastTypeEnum.AllManualBroadcast: FunctionUtils.Fun10(broadCastModel); break;

                    // TTS广播
                    case BroadcastTypeEnum.RealTimeTTSBroadcastOfGroupSelected: FunctionUtils.Fun03(broadCastModel); break;
                    case BroadcastTypeEnum.RealTimeTTSBroadcastOfGroupAll: FunctionUtils.Fun04(broadCastModel); break;
                    case BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupSelected: FunctionUtils.Fun17(broadCastModel); break;
                    case BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupAll: FunctionUtils.Fun18(broadCastModel); break;
                    case BroadcastTypeEnum.TaskTTSBroadcastOfGroupSelected: FunctionUtils.Fun13(broadCastModel); break;
                    case BroadcastTypeEnum.TaskTTSBroadcastOfGroupAll: FunctionUtils.Fun14(broadCastModel); break;
                    case BroadcastTypeEnum.MultiGroupTTSBroadcast: FunctionUtils.Fun08(broadCastModel); break;
                    case BroadcastTypeEnum.AllTTSBroadcast: FunctionUtils.Fun12(broadCastModel); break;

                    // 音乐广播
                    case BroadcastTypeEnum.RealTimeMusicBroadcastOfGroupSelected: FunctionUtils.Fun05(broadCastModel); break;
                    case BroadcastTypeEnum.RealTimeMusicBroadcastOfGroupAll: FunctionUtils.Fun06(broadCastModel); break;
                    case BroadcastTypeEnum.TaskMusicBroadcastOfGroupSelected: FunctionUtils.Fun15(broadCastModel); break;
                    case BroadcastTypeEnum.TaskMusicBroadcastOfGroupAll: FunctionUtils.Fun16(broadCastModel); break;
                    case BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupSelected: FunctionUtils.Fun19(broadCastModel); break;
                    case BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupAll: FunctionUtils.Fun20(broadCastModel); break;
                    case BroadcastTypeEnum.MultiGroupMusicBroadcast: FunctionUtils.Fun09(broadCastModel); break;
                    case BroadcastTypeEnum.AllMusicBroadcast: FunctionUtils.Fun11(broadCastModel); break;
                }
            }


            /// <summary>
            /// 各种广播的具体实现【前提：广播里面不会出现调度台的两个高权限号码】
            /// </summary>
            private static class FunctionUtils
            {


                #region 配置变量


                /// <summary>
                /// 音乐广播对会议播放音乐时手动设置的延迟： n * 1000 毫秒
                /// </summary>
                readonly private static int MusicAndTTSDelayTime = 2 * 1000;


                /// <summary>
                /// 音乐文件无限时长播放
                /// </summary>
                // readonly private static int PlayTimes = 1000;


                /// <summary>
                /// 是否在广播中剔除左右调度号码
                ///     - 人工广播也会剔除左右调度号码，但是人工广播会单独将调度拉入会议
                /// </summary>
                readonly private static bool IsRemoveDispatchNum = true;


                /// <summary>
                /// 自动应答的请求头
                /// </summary>
                readonly private static string AnswerAfter = "answer-after=0";


                /// <summary>
                /// 会议主叫的号码和名称
                /// origination_caller_id_name=zjdmkj(conference)
                /// origination_caller_id_number=1234567890
                /// </summary>

                readonly private static string OriginationCallerName = "origination_caller_id_name=" + DMConfig.ConferenceCallerInfo.OriginationCallerName;
                readonly private static string OriginationCallerNumber = "origination_caller_id_number=" + DMConfig.ConferenceCallerInfo.OriginationCallerNumber;


                /// <summary>
                /// 封装广播的扩展请求头【不可以有空格存在】
                /// {{sip_h_Call-Info=<http://example.com>;answer-after=0}}
                /// </summary>
                readonly public static string ExtendHeader = $"{{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}";



                #endregion



                #region 人工广播


                /// <summary>
                /// 对被选择的用户执行人工广播
                /// </summary>
                public static void Fun01(BroadCastModel broadCastModel)
                {
                    // 1. 会议参数：名称、规则
                    string name = "conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp;
                    string conrefrenceName1 = name + "@dmkj+flags{mute|nomoh}";
                    string conrefrenceName2 = name + "@dmkj+flags{moderator|endconf}";

                    // 2. 单独剔除调度号码
                    if (IsRemoveDispatchNum)
                    {
                        Dictionary<string, string> dict = CommonDB.GetDispatchNum();
                        broadCastModel.Users.Remove(dict["left"]);
                        broadCastModel.Users.Remove(dict["right"]);
                    }

                    // 3. 调度号码拉入会议
                    // FSSocket.SendCommand($"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{broadCastModel.DispatchNum} &conference({conrefrenceName2})");
                    FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{broadCastModel.DispatchNum} &conference({conrefrenceName2})");
                    // SSH.ExecuteCommand($"fs_cli -x \"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{broadCastModel.DispatchNum} &conference({conrefrenceName2})\""); // 主持人加入会议：主持人、不静音、离开结束会议

                    // 4. 其他广播用户拉入会议
                    foreach (string user in broadCastModel.Users)
                    {
                        // FSSocket.SendCommand($"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{user} &conference({conrefrenceName1})");
                        FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{user} &conference({conrefrenceName1})");
                        // SSH.ExecuteCommand($"fs_cli -x \"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{user} &conference({conrefrenceName1})\""); // 普通参会人加入会议：静音，仅加入存在的会议
                    }

                    // 5. 开始广播之后设置该条广播被执行
                    BroadCastDB.UpdateBroadCastIsPlayed(broadCastModel.Id, (int)BroadCastPlayStatusEnum.Played);
                }


                /// <summary>
                /// 对被选择组的全部用户执行人工广播
                /// </summary>
                public static void Fun02(BroadCastModel broadCastModel)
                {
                    // 1. 会议参数：名称、规则
                    string name = "conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp;
                    string conrefrenceName1 = name + "@dmkj+flags{mute}";
                    string conrefrenceName2 = name + "@dmkj+flags{moderator|endconf}";

                    // 2. 查询指定组的全部用户
                    ObservableCollection<DefaultUserModel> users = new ObservableCollection<DefaultUserModel>();
                    CommonDB.GetUserListByGroupId(broadCastModel.GroupId.ToString(), users);
                    List<string> userList = new List<string>();
                    foreach (var item in users)
                    {
                        userList.Add(item.Usernum);
                    }

                    // 3. 单独剔除调度号码
                    if (IsRemoveDispatchNum)
                    {
                        Dictionary<string, string> dict = CommonDB.GetDispatchNum();
                        userList.Remove(dict["left"]);
                        userList.Remove(dict["right"]);
                    }

                    // 4. 调度号码拉入会议
                    FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{broadCastModel.DispatchNum} &conference({conrefrenceName2})");
                    // SSH.ExecuteCommand($"fs_cli -x \"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{broadCastModel.DispatchNum} &conference({conrefrenceName2})\""); // 主持人加入会议：主持人、不静音、离开结束会议

                    // 5. 其他广播将用户拉入会议
                    foreach (string number in userList)
                    {
                        FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{number} &conference({conrefrenceName1})");
                        // SSH.ExecuteCommand($"fs_cli -x \"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{number} &conference({conrefrenceName1})\""); // 普通参会人加入会议：静音，仅加入存在的会议
                    }

                    // 6. 开始广播之后设置该条广播被执行
                    BroadCastDB.UpdateBroadCastIsPlayed(broadCastModel.Id, (int)BroadCastPlayStatusEnum.Played);
                }


                /// <summary>
                /// 多方广播：人工广播
                /// </summary>
                public static void Fun07(BroadCastModel broadCastModel)
                {
                    Fun01(broadCastModel);
                }


                /// <summary>
                /// 全体广播：人工广播
                /// </summary>
                public static void Fun10(BroadCastModel broadCastModel)
                {
                    // 1. 会议参数：名称、规则
                    string name = "conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp;
                    string conrefrenceName1 = name + "@dmkj+flags{mute}";
                    string conrefrenceName2 = name + "@dmkj+flags{moderator|endconf}";

                    // 2. 查询全部广播用户
                    List<string> userList = BroadCastDB.GetAllBroadcastUser();

                    // 3. 单独剔除调度号码
                    if (IsRemoveDispatchNum)
                    {
                        Dictionary<string, string> dict = CommonDB.GetDispatchNum();
                        userList.Remove(dict["left"]);
                        userList.Remove(dict["right"]);
                    }

                    // 4. 调度号码拉入会议
                    FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{broadCastModel.DispatchNum} &conference({conrefrenceName2})");

                    // 5. 其他广播用户拉入会议
                    foreach (string user in userList)
                    {
                        FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{user} &conference({conrefrenceName1})");
                        // SSH.ExecuteCommand($"fs_cli -x \"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{user} &conference({conrefrenceName1})\""); // 普通参会人加入会议：静音，仅加入存在的会议
                    }

                    // 6. 开始广播之后设置该条广播被执行
                    BroadCastDB.UpdateBroadCastIsPlayed(broadCastModel.Id, (int)BroadCastPlayStatusEnum.Played);
                }


                #endregion



                #region 音乐广播


                #region 基础工具

                /// <summary>
                /// 基础工具：对被选择的用户发起音乐广播
                /// </summary>
                private static void _Fun05(BroadCastModel broadCastModel)
                {
                    // 1. 会议参数：名称、规则
                    string name = "conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp;
                    string conrefrenceName1 = name + "@dmkj+flags{mute|nomoh}";

                    // 2. 单独剔除调度号码
                    if (IsRemoveDispatchNum)
                    {
                        Dictionary<string, string> dict = CommonDB.GetDispatchNum();
                        broadCastModel.Users.Remove(dict["left"]);
                        broadCastModel.Users.Remove(dict["right"]);
                    }

                    // 3. 其他广播用户拉入会议
                    foreach (string user in broadCastModel.Users)
                    {
                        FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{user} &conference({conrefrenceName1})"); // 参会人加入会议：静音
                        // SSH.ExecuteCommand($"fs_cli -x 'bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{user} &conference({conrefrenceName1})'");
                    }

                    // 4. 延迟 n 秒后给会议播放音乐
                    Thread.Sleep(MusicAndTTSDelayTime);
                    StringBuilder sb = new StringBuilder();
                    string[] tempMusicList = broadCastModel.MusicPath.Split('!');
                    for (int cnt = 1; cnt <= broadCastModel.PlayCount; cnt++)
                    {
                        for (int i = 0; i < tempMusicList.Length; i++)
                        {
                            sb.Append(MusicDB.GetUploadRemotePath()).Append(tempMusicList[i]).Append(".wav!");
                        }
                    }
                    sb.Length--;
                    string temp = "file_string://" + sb.ToString();
                    FSSocket.SendCommand($"bgapi conference {name} play {temp}");
                    // SSH.ExecuteCommand($"fs_cli -x 'conference {name} play {temp}'");

                    // 5. 开始广播之后设置该条广播被执行
                    if (broadCastModel.Type == BroadcastTypeEnum.RealTimeMusicBroadcastOfGroupSelected && broadCastModel.Type == BroadcastTypeEnum.TaskMusicBroadcastOfGroupSelected) // 仅修改实时广播、任务广播
                    {
                        BroadCastDB.UpdateBroadCastIsPlayed(broadCastModel.Id, (int)BroadCastPlayStatusEnum.Played);
                    }
                }

                /// <summary>
                /// 基础工具：对被选择的组的全部用户发起音乐广播
                /// </summary>
                private static void _Fun06(BroadCastModel broadCastModel)
                {
                    // 1. 会议参数：名称、规则
                    string name = "conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp;
                    string conrefrenceName1 = name + "@dmkj+flags{mute|nomoh}";

                    // 2. 查询指定组的全部用户
                    ObservableCollection<DefaultUserModel> users = new ObservableCollection<DefaultUserModel>();
                    CommonDB.GetUserListByGroupId(broadCastModel.GroupId.ToString(), users);
                    List<string> userList = new List<string>();
                    foreach (var item in users)
                    {
                        userList.Add(item.Usernum);
                    }

                    // 3. 单独剔除调度号码
                    if (IsRemoveDispatchNum)
                    {
                        Dictionary<string, string> dict = CommonDB.GetDispatchNum();
                        userList.Remove(dict["left"]);
                        userList.Remove(dict["right"]);
                    }

                    // 4. 将用户拉入会议
                    foreach (string number in userList)
                    {
                        FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{number} &conference({conrefrenceName1})");
                        // SSH.ExecuteCommand($"fs_cli -x \"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{number} &conference({conrefrenceName1})\""); // 参会人加入会议：静音
                    }

                    // 5. 延迟 n 秒后给会议播放音乐
                    Thread.Sleep(MusicAndTTSDelayTime);
                    StringBuilder sb = new StringBuilder();
                    string[] tempMusicList = broadCastModel.MusicPath.Split('!');
                    for (int cnt = 1; cnt <= broadCastModel.PlayCount; cnt++)
                    {
                        for (int i = 0; i < tempMusicList.Length; i++)
                        {
                            sb.Append(MusicDB.GetUploadRemotePath()).Append(tempMusicList[i]).Append(".wav!");
                        }
                    }
                    sb.Length--;
                    string temp = "file_string://" + sb.ToString();
                    FSSocket.SendCommand($"bgapi conference {name} play {temp}");
                    // SSH.ExecuteCommand($"fs_cli -x 'conference {name} play {temp}'");

                    // 6. 开始广播之后设置该条广播被执行: isPlayed=2
                    if (broadCastModel.Type == BroadcastTypeEnum.RealTimeMusicBroadcastOfGroupAll && broadCastModel.Type == BroadcastTypeEnum.TaskMusicBroadcastOfGroupAll) // 仅修改实时广播、任务广播
                    {
                        BroadCastDB.UpdateBroadCastIsPlayed(broadCastModel.Id, (int)BroadCastPlayStatusEnum.Played);
                    }
                }

                /// <summary>
                /// 基础工具：对全体广播用户发起音乐广播
                /// </summary>
                private static void _Fun11(BroadCastModel broadCastModel)
                {
                    // 1. 会议参数：名称、规则
                    string name = "conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp;
                    string conrefrenceName1 = name + "@dmkj+flags{mute|nomoh}";

                    // 2. 查询全部广播用户
                    List<string> userList = BroadCastDB.GetAllBroadcastUser();

                    // 3. 单独剔除调度号码
                    if (IsRemoveDispatchNum)
                    {
                        Dictionary<string, string> dict = CommonDB.GetDispatchNum();
                        userList.Remove(dict["left"]);
                        userList.Remove(dict["right"]);
                    }

                    // 4. 其他广播用户拉入会议
                    foreach (string user in userList)
                    {
                        FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{user} &conference({conrefrenceName1})");
                        // SSH.ExecuteCommand($"fs_cli -x \"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{user} &conference({conrefrenceName1})\""); // 普通参会人加入会议：静音，仅加入存在的会议
                    }

                    // 5. 延迟 n 秒后给会议播放音乐
                    Thread.Sleep(MusicAndTTSDelayTime);
                    StringBuilder sb = new StringBuilder();
                    string[] tempMusicList = broadCastModel.MusicPath.Split('!');
                    for (int cnt = 1; cnt <= broadCastModel.PlayCount; cnt++)
                    {
                        for (int i = 0; i < tempMusicList.Length; i++)
                        {
                            sb.Append(MusicDB.GetUploadRemotePath()).Append(tempMusicList[i]).Append(".wav!");
                        }
                    }
                    sb.Length--;
                    string temp = "file_string://" + sb.ToString();
                    FSSocket.SendCommand($"bgapi conference {name} play {temp}");
                    // SSH.ExecuteCommand($"fs_cli -x 'conference {name} play {temp}'");

                    // 6. 开始广播之后设置该条广播被执行
                    BroadCastDB.UpdateBroadCastIsPlayed(broadCastModel.Id, (int)BroadCastPlayStatusEnum.Played);
                }

                #endregion


                #region 实时 👇



                /// <summary>
                /// 对被选择的用户执行音乐广播
                /// </summary>
                public static void Fun05(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().RealTimeMusicBroadCast(_Fun05, broadCastModel);
                }


                /// <summary>
                /// 对被选择组的全部用户执行音乐广播
                /// </summary>
                public static void Fun06(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().RealTimeMusicBroadCast(_Fun06, broadCastModel);
                }


                /// <summary>
                /// 多方广播：音乐广播
                /// </summary>
                public static void Fun09(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().RealTimeMusicBroadCast(_Fun05, broadCastModel);
                }


                /// <summary>
                /// 全体广播：音乐广播
                /// </summary>
                public static void Fun11(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().RealTimeMusicBroadCast(_Fun11, broadCastModel);
                }


                #endregion


                #region 任务 👇


                /// <summary>
                /// 组内选人的音乐任务广播
                /// </summary>
                public static void Fun15(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().TaskBroadCast1(_Fun05, broadCastModel);
                }


                /// <summary>
                /// 组内全部的音乐任务广播
                /// </summary>
                public static void Fun16(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().TaskBroadCast1(_Fun06, broadCastModel);
                }

                #endregion


                #region 定时 👇



                /// <summary>
                /// 组内选人的音乐定时广播
                /// </summary>
                public static void Fun19(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().TaskBroadCast2(_Fun05, broadCastModel);
                }


                /// <summary>
                /// 组内全部的音乐定时广播
                /// </summary>
                public static void Fun20(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().TaskBroadCast2(_Fun06, broadCastModel);
                }

                #endregion


                #endregion



                #region TTS 广播


                #region 基础工具
                /// <summary>
                /// 对被选择的用户执行TTS实时广播
                /// </summary>
                public static void _Fun03(BroadCastModel broadCastModel)
                {
                    // 1. 文本转语音
                    string path = broadCastModel.MusicPath;


                    // 2. 会议规则
                    string name = "conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp;
                    string conrefrenceName1 = name + "@dmkj+flags{mute|nomoh}";

                    // 3. 单独剔除调度号码
                    if (IsRemoveDispatchNum)
                    {
                        Dictionary<string, string> dict = CommonDB.GetDispatchNum();
                        broadCastModel.Users.Remove(dict["left"]);
                        broadCastModel.Users.Remove(dict["right"]);
                    }

                    // 4. 将用户拉入会议
                    foreach (string user in broadCastModel.Users)
                    {
                        FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{user} &conference({conrefrenceName1})");
                        // SSH.ExecuteCommand($"fs_cli -x \"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{user} &conference({conrefrenceName1})\""); // 参会人加入会议：静音
                    }

                    // 5. 延迟 n 秒后给会议播放音乐
                    Thread.Sleep(MusicAndTTSDelayTime);

                    StringBuilder sb = new StringBuilder();
                    for (int cnt = 1; cnt <= broadCastModel.PlayCount; cnt++)
                    {
                        sb.Append(path).Append("!");
                    }
                    sb.Length--;
                    string temp = "file_string://" + sb.ToString();
                    FSSocket.SendCommand($"bgapi conference {name} play {temp}");
                    // SSH.ExecuteCommand($"fs_cli -x 'conference {name} play {temp}'");

                    // 6. 开始广播之后设置该条广播被执行: isPlayed=2
                    if (broadCastModel.Type == BroadcastTypeEnum.RealTimeTTSBroadcastOfGroupSelected && broadCastModel.Type == BroadcastTypeEnum.TaskTTSBroadcastOfGroupSelected) // 仅修改实时广播、任务广播
                    {
                        BroadCastDB.UpdateBroadCastIsPlayed(broadCastModel.Id, (int)BroadCastPlayStatusEnum.Played);
                    }
                }



                /// <summary>
                /// 对被选择组的全部用户执行TTS实时广播
                /// </summary>
                public static void _Fun04(BroadCastModel broadCastModel)
                {
                    // 1. 文本转语音
                    string path = broadCastModel.MusicPath;

                    // 2. 会议参数
                    string name = "conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp;
                    string conrefrenceName1 = name + "@dmkj+flags{mute|nomoh}";

                    // 3. 查询指定组的全部用户
                    ObservableCollection<DefaultUserModel> users = new ObservableCollection<DefaultUserModel>();
                    CommonDB.GetUserListByGroupId(broadCastModel.GroupId.ToString(), users);
                    List<string> userList = new List<string>();
                    foreach (var item in users)
                    {
                        userList.Add(item.Usernum);
                    }

                    // 4. 单独剔除调度号码
                    if (IsRemoveDispatchNum)
                    {
                        Dictionary<string, string> dict = CommonDB.GetDispatchNum();
                        userList.Remove(dict["left"]);
                        userList.Remove(dict["right"]);
                    }

                    // 5. 将用户拉入会议
                    foreach (string number in userList)
                    {
                        FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{number} &conference({conrefrenceName1})"); // 普通参会人加入会议：静音
                    }

                    // 6. 延迟 n 秒后给会议播放音乐
                    Thread.Sleep(MusicAndTTSDelayTime);
                    StringBuilder sb = new StringBuilder();
                    for (int cnt = 1; cnt <= broadCastModel.PlayCount; cnt++)
                    {
                        sb.Append(path).Append("!");
                    }
                    sb.Length--;
                    string temp = "file_string://" + sb.ToString();
                    FSSocket.SendCommand($"bgapi conference {name} play {temp}");
                    // SSH.ExecuteCommand($"fs_cli -x 'conference {name} play {temp}'");

                    // 7. 开始广播之后设置该条广播被执行: isPlayed=2
                    if (broadCastModel.Type == BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupAll && broadCastModel.Type == BroadcastTypeEnum.TaskTTSBroadcastOfGroupAll) // 仅修改实时广播、任务广播
                    {
                        BroadCastDB.UpdateBroadCastIsPlayed(broadCastModel.Id, (int)BroadCastPlayStatusEnum.Played);
                    }
                }


                /// <summary>
                /// 全体广播：TTS广播
                /// </summary>
                public static void _Fun12(BroadCastModel broadCastModel)
                {
                    // 1. 文本转语音
                    string path = broadCastModel.MusicPath;

                    // 2. 会议规则
                    string name = "conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp;
                    string conrefrenceName1 = name + "@dmkj+flags{mute|nomoh}";

                    // 3. 查询全部广播用户
                    List<string> userList = BroadCastDB.GetAllBroadcastUser();

                    // 4. 单独剔除调度号码
                    if (IsRemoveDispatchNum)
                    {
                        Dictionary<string, string> dict = CommonDB.GetDispatchNum();
                        userList.Remove(dict["left"]);
                        userList.Remove(dict["right"]);
                    }

                    // 5. 其他广播用户拉入会议
                    foreach (string user in userList)
                    {
                        FSSocket.SendCommand($"bgapi originate {ExtendHeader}user/{user} &conference({conrefrenceName1})");
                        // SSH.ExecuteCommand($"fs_cli -x \"bgapi originate {{sip_h_Call-Info=<http://example.com>;{AnswerAfter}}}user/{user} &conference({conrefrenceName1})\""); // 普通参会人加入会议：静音，仅加入存在的会议
                    }

                    // 6. 延迟 n 秒后给会议播放音乐
                    Thread.Sleep(MusicAndTTSDelayTime);
                    StringBuilder sb = new StringBuilder();
                    for (int cnt = 1; cnt <= broadCastModel.PlayCount; cnt++)
                    {
                        sb.Append(path).Append("!");

                    }
                    sb.Length--;
                    string temp = "file_string://" + sb.ToString();
                    FSSocket.SendCommand($"bgapi conference {name} play {temp}");
                    // SSH.ExecuteCommand($"fs_cli -x 'conference {name} play {temp}'");

                    // 7. 开始广播之后设置该条广播被执行
                    BroadCastDB.UpdateBroadCastIsPlayed(broadCastModel.Id, (int)BroadCastPlayStatusEnum.Played);
                }


                #endregion


                #region 实时

                /// <summary>
                /// 对被选择的用户执行TTS实时广播
                /// </summary>
                public static void Fun03(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().RealTimeTTSBroadCast(_Fun03, broadCastModel);
                }



                /// <summary>
                /// 对被选择组的全部用户执行TTS实时广播
                /// </summary>
                public static void Fun04(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().RealTimeTTSBroadCast(_Fun04, broadCastModel);
                }


                /// <summary>
                /// 多方广播：TTS广播
                /// </summary>
                public static void Fun08(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().RealTimeTTSBroadCast(_Fun03, broadCastModel);
                }


                /// <summary>
                /// 全体广播：TTS广播
                /// </summary>
                public static void Fun12(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().RealTimeTTSBroadCast(_Fun12, broadCastModel);
                }

                #endregion


                #region 任务

                /// <summary>
                /// 组内选人的TTS任务广播
                /// </summary>
                public static void Fun13(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().TaskBroadCast1(_Fun03, broadCastModel); // new 新的对象保证不同的线程
                }

                /// <summary>
                /// 组内全部的TTS任务广播
                /// </summary>
                public static void Fun14(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().TaskBroadCast1(_Fun04, broadCastModel);
                }

                #endregion


                #region 定时

                /// <summary>
                /// 组内选人的TTS定时广播
                /// </summary>
                public static void Fun17(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().TaskBroadCast2(_Fun03, broadCastModel);
                }

                /// <summary>
                /// 组内全部的TTS定时广播
                /// </summary>
                public static void Fun18(BroadCastModel broadCastModel)
                {
                    new BroadCastUtil().TaskBroadCast2(_Fun04, broadCastModel);
                }

                #endregion


                #endregion


            }


        }



        /// <summary>
        /// 广播摄像头
        /// </summary>
        public static void CreateBroadcastCameraVideo(List<string> list)
        {
            // 查询有摄像头的广播对象
            List<Dictionary<string, string>> objList = new List<Dictionary<string, string>>();
            for (int i = 0; i < list.Count; i++)
            {
                Dictionary<string, string> dict = BroadCastDB.GetCameraInfoByUsernum(list[i]);
                if (dict["camera_ip"] != "")
                    objList.Add(dict);
            }

            if (objList.Count <= 0) return;

            // 仅仅展示第一个
            if (objList.Count == 1)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(DMVariable.retFlag);
                DMVariable.retFlag = ShowFirstCameraVideo(objList[0]["camera_ip"], Int16.Parse(objList[0]["camera_port"]), objList[0]["camera_account"], objList[0]["camera_password"], DMVariable.broadcastVideoForm);
            }
            else
            {
                CHCNetSDK.NET_DVR_StopRealPlay(DMVariable.retFlag);
                DMVariable.retFlag = ShowFirstCameraVideo(objList[0]["camera_ip"], Int16.Parse(objList[0]["camera_port"]), objList[0]["camera_account"], objList[0]["camera_password"], DMVariable.broadcastVideoForm);
                // 保存有摄像头的广播对象
                DMVariable.broadcastCameraVideoList = objList;
                if (DMVariable.broadcastCameraVideoList.Count > 0)
                {
                    new CameraView().ShowDialog();
                }
            }
        }

        /// <summary>
        /// 广播摄像头
        /// </summary>
        private static int ShowFirstCameraVideo(string ip, Int16 port, string username, string password, System.Windows.Forms.PictureBox formName)
        {
            // 初始化
            if (!CHCNetSDK.NET_DVR_Init())
            {
                return -1;
            }

            // 登录摄像头
            CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            Int32 userId = CHCNetSDK.NET_DVR_Login_V30(ip, port, username, password, ref DeviceInfo);
            if (userId < 0)
            {
                return (int)CHCNetSDK.NET_DVR_GetLastError();
            }

            // 预览
            CHCNetSDK.NET_DVR_PREVIEWINFO PreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO()
            {
                hPlayWnd = formName.Handle,
                lChannel = Int16.Parse("1"),
                dwStreamType = 0,
                dwLinkMode = 0,
                bBlocked = true,
                dwDisplayBufNum = 1,
                byProtoType = 0,
                byPreviewMode = 0,
            };
            return CHCNetSDK.NET_DVR_RealPlay_V40(userId, ref PreviewInfo, null, new IntPtr());
        }

    }
}
