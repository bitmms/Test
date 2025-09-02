using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using MyMVVM.Broadcast.Utils;
using MyMVVM.Common;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;

namespace MyMVVM.Broadcast.ViewModel
{
    public class BroadcastListViewModel : ViewModelsBase
    {
        private ObservableCollection<BroadCastListModel> _broadCastList { get; set; }
        public ObservableCollection<BroadCastListModel> BroadCastList { get => _broadCastList; set { _broadCastList = value; OnPropertyChanged(nameof(BroadCastList)); } }

        private BroadCastListModel _selectedItem;
        public BroadCastListModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        private int _IsHideFirstButton;
        public int IsHideFirstButton
        {
            get => _IsHideFirstButton;
            set
            {
                _IsHideFirstButton = value;
                OnPropertyChanged(nameof(IsHideFirstButton));
            }
        }

        private int _IsHideNextButton;
        public int IsHideNextButton
        {
            get => _IsHideNextButton;
            set
            {
                _IsHideNextButton = value;
                OnPropertyChanged(nameof(IsHideNextButton));
            }
        }


        /// <summary>
        /// 记录当前所有正在播放的广播id，用于给后面加载任务广播、定时广播时充当排除条件
        /// </summary>
        List<int> TempBroadcastIdList;
        /// <summary>
        /// 当前所有正在播放的广播
        /// </summary>
        List<BroadCastListModel> TempManualBroadcastList;
        /// <summary>
        /// 当前所有任务广播
        /// </summary>
        List<BroadCastListModel> TempTaskBroadcastList;
        /// <summary>
        /// 当前所有定时广播
        /// </summary>
        List<BroadCastListModel> TempScheduledBroadcastList;



        #region 把广播的枚举封装到 List 中


        List<BroadcastTypeEnum> AllPersonsList = new List<BroadcastTypeEnum>
        {
            BroadcastTypeEnum.AllManualBroadcast,
            BroadcastTypeEnum.AllMusicBroadcast,
            BroadcastTypeEnum.AllTTSBroadcast,
        };



        List<BroadcastTypeEnum> SelectGroupList = new List<BroadcastTypeEnum>
        {
            BroadcastTypeEnum.RealTimeManualBroadcastOfGroupAll,
            BroadcastTypeEnum.RealTimeMusicBroadcastOfGroupAll,
            BroadcastTypeEnum.RealTimeTTSBroadcastOfGroupAll,
            BroadcastTypeEnum.TaskMusicBroadcastOfGroupAll,
            BroadcastTypeEnum.TaskTTSBroadcastOfGroupAll,
            BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupAll,
            BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupAll,
        };

        /// <summary>
        /// 把任务广播的枚举封装到 List 中
        /// </summary>
        List<BroadcastTypeEnum> TaskBroadcastTypeList = new List<BroadcastTypeEnum>
        {
            BroadcastTypeEnum.TaskMusicBroadcastOfGroupAll,
            BroadcastTypeEnum.TaskMusicBroadcastOfGroupSelected,
            BroadcastTypeEnum.TaskTTSBroadcastOfGroupAll,
            BroadcastTypeEnum.TaskTTSBroadcastOfGroupSelected,
        };


        /// <summary>
        /// 把定时广播的枚举封装到 List 中
        /// </summary>
        List<BroadcastTypeEnum> ScheduledBroadcastTypeList = new List<BroadcastTypeEnum>
        {
            BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupAll,
            BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupSelected,
            BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupAll,
            BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupSelected,
        };


        /// <summary>
        /// 把人工广播的枚举封装到 List 中
        /// </summary>
        List<BroadcastTypeEnum> ManualBroadcastTypeList = new List<BroadcastTypeEnum>
        {
            BroadcastTypeEnum.RealTimeManualBroadcastOfGroupAll,
            BroadcastTypeEnum.RealTimeManualBroadcastOfGroupSelected,
            BroadcastTypeEnum.MultiGroupManualBroadcast,
            BroadcastTypeEnum.AllManualBroadcast,
        };

        /// <summary>
        /// 把音乐广播的枚举封装到 List 中
        /// </summary>
        List<BroadcastTypeEnum> MusicBroadcastTypeList = new List<BroadcastTypeEnum>
        {
            BroadcastTypeEnum.RealTimeMusicBroadcastOfGroupAll,
            BroadcastTypeEnum.RealTimeMusicBroadcastOfGroupSelected,

            BroadcastTypeEnum.MultiGroupMusicBroadcast,

            BroadcastTypeEnum.AllMusicBroadcast,

            BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupAll,
            BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupSelected,

            BroadcastTypeEnum.TaskMusicBroadcastOfGroupAll,
            BroadcastTypeEnum.TaskMusicBroadcastOfGroupSelected,
        };

        /// <summary>
        /// 把TTS广播的枚举封装到 List 中
        /// </summary>
        List<BroadcastTypeEnum> TTSBroadcastTypeList = new List<BroadcastTypeEnum>
        {
            BroadcastTypeEnum.RealTimeTTSBroadcastOfGroupAll,
            BroadcastTypeEnum.RealTimeTTSBroadcastOfGroupSelected,

            BroadcastTypeEnum.MultiGroupTTSBroadcast,

            BroadcastTypeEnum.AllTTSBroadcast,

            BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupAll,
            BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupSelected,

            BroadcastTypeEnum.TaskTTSBroadcastOfGroupAll,
            BroadcastTypeEnum.TaskTTSBroadcastOfGroupSelected,
        };


        #endregion



        /// <summary>
        /// 构造器
        /// </summary>
        public BroadcastListViewModel()
        {
            CurrentPage = 1;
            PageSize = 10;
            PageNumberOfGoto = "";

            // 初始化
            TempBroadcastIdList = new List<int>();
            TempManualBroadcastList = new List<BroadCastListModel>();
            TempTaskBroadcastList = new List<BroadCastListModel>();
            TempScheduledBroadcastList = new List<BroadCastListModel>();
            BroadCastList = new ObservableCollection<BroadCastListModel>();

            // 定时加载广播列表
            WhileUpdateBroadCastList(null, null);

            Timer myTimer = new Timer();
            myTimer.Elapsed += new System.Timers.ElapsedEventHandler(WhileUpdateBroadCastList);  //到达时间的时候执行事件；
            myTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)； 
            myTimer.Interval = 1000;
            myTimer.Start();
            TimerPool.AddTimer("broadcastList", myTimer);
        }




        /// <summary>
        /// 定时加载广播列表5
        /// </summary>
        public void WhileUpdateBroadCastList(object source, System.Timers.ElapsedEventArgs e)
        {
            // 清空临时集合
            TempBroadcastIdList.Clear();

            // 查询并收集广播列表数据
            List<BroadCastModel> list = new List<BroadCastModel>();

            int[] playState = new int[1024];
            int index = 0;

            // 播放中的广播
            foreach (var broadcast in BroadCastDB.GetBroadCastOfPlaying())
            {
                TempBroadcastIdList.Add(broadcast.Id);
                list.Add(broadcast);
                playState[index++] = 1;
            }

            // 等待播放中的广播
            foreach (var broadcast in BroadCastDB.GetScheduledBroadCastOfAction())
            {
                if (TempBroadcastIdList.Contains(broadcast.Id))
                {
                    continue;
                }
                list.Add(broadcast);
                playState[index++] = 0;
            }
            foreach (var broadcast in BroadCastDB.GetTaskBroadCastOfAction())
            {
                if (TempBroadcastIdList.Contains(broadcast.Id))
                {
                    continue;
                }
                list.Add(broadcast);
                playState[index++] = 0;
            }

            playState[index++] = -1;

            TotalPages = (list.Count / PageSize);
            TotalPages += (list.Count % PageSize > 0) ? 1 : 0;

            if (CurrentPage < TotalPages)
            {
                IsHideNextButton = 1;
            }
            else
            {
                IsHideNextButton = 0;
            }

            // 处理显示的属性内容
            BroadCastList = SetBroadcastByType(list, playState);
        }





        /// <summary>
        /// 处理广播列表
        /// </summary>
        private ObservableCollection<BroadCastListModel> SetBroadcastByType(List<BroadCastModel> broadcastModelList, int[] playStateArr)
        {
            int index = 0;
            ObservableCollection<BroadCastListModel> list = new ObservableCollection<BroadCastListModel>();
            foreach (BroadCastModel broadcastModel in broadcastModelList)
            {
                BroadCastListModel broadCastListModel = new BroadCastListModel();
                // 1. ID
                broadCastListModel.Id = broadcastModel.Id;
                // 2. 广播类型
                broadCastListModel.DisplayType = EnumExtension.GetDescription(broadcastModel.Type);
                // 3. 广播内容
                broadCastListModel.DisplayContent = GetBroadcastContent(broadCastListModel, broadcastModel);
                // 4. 广播对象
                broadCastListModel.DisplayObject = GetBroadcastObject(broadCastListModel, broadcastModel);
                // 5. 广播次数
                SolveBroadcastPlayCount(broadCastListModel, broadcastModel);
                // 6. 时间
                broadCastListModel.DisplayTime = GetBroadcastTime(broadcastModel);
                // 7. 状态
                broadCastListModel.DisplayPlayStatus = GetBroadcastPlayState(playStateArr[index]);
                // 8. 操作
                SetBroadcastOperate(broadcastModel, broadCastListModel, playStateArr[index]);
                index++;
                list.Add(broadCastListModel);
            }

            // 分页查询
            ObservableCollection<BroadCastListModel> pageList = new ObservableCollection<BroadCastListModel>();

            int start = (CurrentPage - 1) * PageSize;
            int end = start + PageSize - 1;
            if (end >= list.Count)
            {
                end = list.Count - 1;
            }


            for (int startIndex = start; startIndex <= end; startIndex++)
            {
                pageList.Add(list[startIndex]);
            }
            return pageList;

            // return list;
        }



        private int _PageSize;
        public int PageSize
        {
            get => _PageSize;
            set
            {
                if (_PageSize != value)
                {
                    _PageSize = value;
                    OnPropertyChanged(nameof(PageSize));
                }
            }
        }

        private int _CurrentPage;
        public int CurrentPage
        {
            get => _CurrentPage;
            set
            {
                if (_CurrentPage != value)
                {
                    _CurrentPage = value;
                    if (_CurrentPage == 1)
                    {
                        IsHideFirstButton = 0;
                    }
                    else
                    {
                        IsHideFirstButton = 1;
                    }
                    OnPropertyChanged(nameof(CurrentPage));
                }
            }
        }

        private int _TotalPages;
        public int TotalPages
        {
            get => _TotalPages;
            set
            {
                if (_TotalPages != value)
                {
                    _TotalPages = value;
                    OnPropertyChanged(nameof(TotalPages));
                }
            }
        }


        private string _PageNumberOfGoto;
        public string PageNumberOfGoto
        {
            get => _PageNumberOfGoto;
            set
            {
                if (_PageNumberOfGoto != value)
                {
                    _PageNumberOfGoto = value;
                    OnPropertyChanged(nameof(PageNumberOfGoto));
                }
            }
        }



        public ICommand PreviousPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage <= 1)
            {
                return;
            }

            CurrentPage -= 1;

            WhileUpdateBroadCastList(null, null);
        });

        public ICommand NextPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage >= TotalPages)
            {
                return;
            }

            CurrentPage += 1;
            WhileUpdateBroadCastList(null, null);
        });


        public ICommand GoToFirstPageCommand => new ViewModelCommand(param =>
        {
            CurrentPage = 1;
            WhileUpdateBroadCastList(null, null);
        });

        public ICommand GoToPageCommand => new ViewModelCommand(param =>
        {
            if (!DMUtil.IsNumber(PageNumberOfGoto))
            {
                DMMessageBox.ShowInfo("输入的页码无效");
                PageNumberOfGoto = "";
                return;
            }

            int number = int.Parse(PageNumberOfGoto);


            if (number < 1)
            {
                number = 1;
            }
            else if (number > TotalPages)
            {
                number = TotalPages;
            }

            PageNumberOfGoto = number.ToString();

            CurrentPage = number;
            WhileUpdateBroadCastList(null, null);
        });


























        /// <summary>
        /// 解析出广播内容
        /// </summary>
        private string GetBroadcastContent(BroadCastListModel broadCastListModel, BroadCastModel broadcastModel)
        {
            int len = 10;
            string ret = "";
            if (ManualBroadcastTypeList.Contains(broadcastModel.Type))
            {
                ret = "人工语音";
                broadCastListModel.RealContent = ret;
            }
            else if (MusicBroadcastTypeList.Contains(broadcastModel.Type))
            {
                broadCastListModel.RealContent = broadcastModel.MusicPath;

                StringBuilder sb = new StringBuilder();
                string[] tempMusicList = broadcastModel.MusicPath.Split('!');
                foreach (string tempMusic in tempMusicList)
                {
                    sb.Append(tempMusic).Append("，"); ;
                }
                sb.Length--;
                string temp = sb.ToString();

                if (temp.Length > len)
                {
                    ret = temp.Substring(0, len) + "...";
                }
                else
                {
                    ret = temp;
                }
            }
            else
            {
                broadCastListModel.RealContent = broadcastModel.TTSText;
                string temp = broadcastModel.TTSText;
                if (temp.Length > len)
                {
                    ret = temp.Substring(0, len) + "...";
                }
                else
                {
                    ret = temp;
                }
            }
            return ret;
        }



        /// <summary>
        /// 解析出广播对象
        /// </summary>
        private string GetBroadcastObject(BroadCastListModel broadCastListModel, BroadCastModel broadcastModel)
        {
            string obj = "";
            if (AllPersonsList.Contains(broadcastModel.Type))
            {
                obj = "全体广播用户";
                broadCastListModel.RealObject = obj;
            }
            else if (SelectGroupList.Contains(broadcastModel.Type))
            {
                obj = BroadCastDB.GetGroupNameById(broadcastModel.GroupId);
                broadCastListModel.RealObject = obj;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in broadcastModel.Users)
                {
                    sb.Append(item).Append("，");
                }
                sb.Length--; // 利用 StringBuilder 删除字符串最后一位
                broadCastListModel.RealObject = sb.ToString();

                int len = 10;
                if (sb.Length > len)
                {
                    obj = sb.ToString().Substring(0, len) + "...";
                }
                else
                {
                    obj = sb.ToString();
                }
            }
            return obj;
        }


        // 处理广播的播放次数
        private void SolveBroadcastPlayCount(BroadCastListModel broadCastListModel, BroadCastModel broadCastModel)
        {
            // 音乐、TTS
            if (MusicBroadcastTypeList.Contains(broadCastModel.Type) || TTSBroadcastTypeList.Contains(broadCastModel.Type))
            {
                broadCastListModel.PlayCount = $"{broadCastModel.PlayCount}";
            }
            else
            {
                broadCastListModel.PlayCount = "-";
            }

            if (broadCastModel.PlayCount == -1 || broadCastModel.PlayCount == 86400)
            {
                broadCastListModel.PlayCount = "-";
            }
        }


        /// <summary>
        /// 解析出广播时间
        /// </summary>
        private string GetBroadcastTime(BroadCastModel broadcastModel)
        {
            string beginTime = "";
            if (TaskBroadcastTypeList.Contains(broadcastModel.Type))
            {
                //Console.WriteLine(broadcastModel.BroadCastBeginTime + " ➔ " + broadcastModel.BroadCastEndTime);
                //Console.WriteLine($"于 {broadcastModel.BroadCastBeginTime} 开始广播，持续时间 {DMUtil.GetTimeSpan(broadcastModel.BroadCastBeginTime, broadcastModel.BroadCastEndTime)}");
                // beginTime = broadcastModel.BroadCastBeginTime + " ➔ " + broadcastModel.BroadCastEndTime;
                beginTime = $"{broadcastModel.BroadCastBeginTime} 开始广播，共 {DMUtil.GetTimeSpan(broadcastModel.BroadCastBeginTime, broadcastModel.BroadCastEndTime)}";
            }
            else if (ScheduledBroadcastTypeList.Contains(broadcastModel.Type))
            {
                beginTime = $"每天 {broadcastModel.BroadCastBeginTime} 开始广播，共 {broadcastModel.BroadcastDuration} 分钟";
            }
            else
            {
                beginTime = $"{broadcastModel.BroadCastBeginTime} 开始广播";
            }
            return beginTime;
        }


        /// <summary>
        /// 解析出广播播放状态
        /// </summary>
        private string GetBroadcastPlayState(int playStateArr)
        {
            return (playStateArr == 1) ? "播放中" : "等待中";
        }


        /// <summary>
        /// 设置广播的操作按钮
        /// </summary>
        private void SetBroadcastOperate(BroadCastModel broadcastModel, BroadCastListModel broadCastListModel, int playState)
        {
            if (playState == 1)
            {
                broadCastListModel.IsShowCancelButton = false;
                broadCastListModel.IsShowStopButton = true;
                broadCastListModel.StopCommand = new ViewModelCommand(param =>
                {
                    if (!DMMessageBox.Show("停止广播", "是否停止广播", DMMessageType.MESSAGE_WARING, DMMessageButton.YesNo))
                    {
                        return;
                    }

                    // 发送停止广播命令
                    SSH.ExecuteCommand($"fs_cli -x 'conference conference-{broadcastModel.Type + "-" + broadcastModel.Id + "-" + broadcastModel.TimeStamp} hup all'");

                    // 释放定时器
                    if (!ScheduledBroadcastTypeList.Contains(broadcastModel.Type))
                    {
                        TimerPool.StopAndRemoveTimer("conference-" + broadcastModel.Type + "-" + broadcastModel.Id + "-" + broadcastModel.TimeStamp);
                    }

                    // 修改数据库【不修改定时广播】
                    if (!ScheduledBroadcastTypeList.Contains(broadcastModel.Type))
                    {
                        BroadCastDB.UpdateBroadCastIsPlayed(broadcastModel.Id, (int)BroadCastPlayStatusEnum.StopOneBroadcastClicked);
                    }
                });
            }

            else
            {
                broadCastListModel.IsShowCancelButton = true;
                broadCastListModel.IsShowStopButton = false;
                broadCastListModel.CancelCommand = new ViewModelCommand(param =>
                {
                    if (!DMMessageBox.Show("取消广播", "是否取消广播", DMMessageType.MESSAGE_WARING, DMMessageButton.YesNo))
                    {
                        return;
                    }

                    // 释放定时器
                    TimerPool.StopAndRemoveTimer("conference-" + broadcastModel.Type + "-" + broadcastModel.Id + "-" + broadcastModel.TimeStamp);

                    // 修改数据库
                    BroadCastDB.UpdateBroadCastIsPlayed(broadcastModel.Id, (int)BroadCastPlayStatusEnum.BroadcastCancelled);
                });

            }
        }
    }
}

