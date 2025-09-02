using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HaiKang;
using MyMVVM.Broadcast;
using MyMVVM.Broadcast.Utils;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Login;
using MyMVVM.Phone.Utils;

namespace MyMVVM.MainWindow.Utils
{
    public class InitUtil
    {
        /// <summary>
        /// 将所有需要初始化的操作进行统一管理
        /// </summary>
        public void InitSystem()
        {
            // 初始化 海康 SDK
            CHCNetSDK.NET_DVR_Init();

            // 客户端第一次打开时：加载任务广播、定时广播
            LoadBroadCast();

            // 客户端第一次打开时：停止全部的实时广播
            StopAllRealTimeBroadcast();

            // 软件的过期时间判断
            LoadSoftRegTime();

            // 从服务器将歌曲同步到本地
            LoadMusic();

            try
            {
                LoadSoftPhoneSipUser();
            }
            catch (System.Exception e)
            {

                MessageBox.Show(e.Message);
            }
        }


        /// <summary>
        /// 客户端第一次打开时：从服务器将歌曲同步到本地
        /// </summary>
        private void LoadMusic()
        {
            // 获取数据库的音乐列表
            List<MusicModel> list1 = MusicDB.GetMusicList();

            // 获取本地音乐列表
            List<string> LocalList = new List<string>();
            foreach (string s in Directory.GetFiles(MusicDB.GetUploadLocalPath(), "*.wav"))
            {
                string filename = Path.GetFileName(s);
                LocalList.Add(filename);
            }

            // 判断本地文件夹是否存在，不存在就创建
            if (!Directory.Exists(MusicDB.GetUploadLocalPath()))
            {
                Directory.CreateDirectory(MusicDB.GetUploadLocalPath());
            }

            // 查看哪些歌曲仅存在与服务器中
            List<MusicModel> tempList = new List<MusicModel>();
            foreach (MusicModel model in list1)
            {
                string name = model.Name + ".wav";
                if (!LocalList.Contains(name))
                {
                    tempList.Add(model);
                }
            }

            // 如果有文件差异，就执行同步操作
            Task.Run(() =>
            {
                DMVariable.MusicSyncState.IsMusicSyncing = true;
                DMVariable.MusicSyncState.IsMusicSyncSuccess = false;
                if (tempList.Count > 0)
                {
                    foreach (var model in tempList)
                    {
                        SSH.DownLoadFile(model.UploadRemotePath, MusicDB.GetUploadLocalPath() + "\\" + Path.GetFileName(model.UploadLocalPath));
                    }
                }
                DMVariable.MusicSyncState.IsMusicSyncing = false;
                DMVariable.MusicSyncState.IsMusicSyncSuccess = true;
            });
        }


        /// <summary>
        /// 客户端第一次打开时：加载等待播放的任务广播、定时广播，使得未执行的任务被恢复
        /// </summary>
        private void LoadBroadCast()
        {
            Task.Run(() =>
            {
                List<BroadCastModel> broadcastList = BroadCastDB.GetBroadCastOfAction();
                foreach (BroadCastModel broadcast in broadcastList)
                {
                    BroadCastUtil.BroadCastTypeFunction.HandlerBroadcast(broadcast);
                }
            });
        }


        /// <summary>
        /// 客户端第一次打开时：停止全部的实时广播
        /// </summary>
        private void StopAllRealTimeBroadcast()
        {
            Task.Run(() =>
            {
                string ret = SSH.ExecuteCommand("fs_cli -x 'conference list'");
                MatchCollection matches = new Regex(@"\+OK Conference ([a-zA-Z0-9-]+) \(").Matches(ret);
                foreach (Match match in matches)
                {
                    string conferenceName = match.Groups[1].Value;
                    // 利用 '-' 字符出现三次来筛选出广播会议的ID
                    int times = 0;
                    for (int i = 0; i < conferenceName.Length; i++)
                    {
                        if (conferenceName[i] == '-')
                        {
                            times++;
                        }
                    }
                    if (times == 3)
                    {
                        SSH.ExecuteCommand($"fs_cli -x'conference {conferenceName} hup all'");
                        /* if (conferenceName.Contains("RealTime") || conferenceName.Contains("Multi") || conferenceName.Contains("All"))
                         {
                             SSH.ExecuteCommand($"fs_cli -x'conference {conferenceName} hup all'");
                         }*/
                    }
                }
            });
        }



        /// <summary>
        /// 客户端第一次打开时：开始定时器，判断软件的注册时间
        /// </summary>
        private void LoadSoftRegTime()
        {
            new Timer(AuthTimer, null, 0, 24 * 60 * 60 * 1000); //  24 小时 校验一次
            /*System.Timers.Timer timer = new System.Timers.Timer(100 * 1000);
            timer.Enabled = true;
            timer.Elapsed += ((sources, e) =>
            {
                AuthTimer(null);
            });
            timer.Start();*/
        }
        private void AuthTimer(object obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (LoginDB.AuthTime())
                {
                    DMMessageBox.Show("警告", "授权到期!!", DMMessageType.MESSAGE_FAIL);
                    // 关闭所有程序
                    Application.Current.Shutdown();
                }
            });
        }



        /// <summary>
        /// 客户端第一次打开时：加载软电话的用户，进行注册
        /// </summary>
        private void LoadSoftPhoneSipUser()
        {
            SipUtil.VideoinItialize();
        }
    }
}
