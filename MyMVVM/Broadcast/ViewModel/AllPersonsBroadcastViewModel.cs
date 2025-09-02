using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyMVVM.Broadcast.Utils;
using MyMVVM.Common;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;

namespace MyMVVM.Broadcast.ViewModel
{
    public class AllPersonsBroadcastViewModel : ViewModelsBase
    {
        private BroadCastModel broadCastModel;

        private List<string> _TTSList;
        public List<string> TTSList { get => _TTSList; set => SetProperty(ref _TTSList, value); }

        private string _NowSeletedTTSItem;
        public string NowSeletedTTSItem { get => _NowSeletedTTSItem; set => SetProperty(ref _NowSeletedTTSItem, value); }


        private bool _isShowMusic;
        public bool IsShowMusic { get => _isShowMusic; set => SetProperty(ref _isShowMusic, value); }

        private bool _isShowTTS;
        public bool IsShowTTS { get => _isShowTTS; set => SetProperty(ref _isShowTTS, value); }
        private bool _IsManual;
        public bool IsManual { get => _IsManual; set => SetProperty(ref _IsManual, value); }


        private ObservableCollection<string> SelectedUserList;

        private List<string> _broadcastTypeList;
        public List<string> BroadcastTypeList { get => _broadcastTypeList; set => SetProperty(ref _broadcastTypeList, value); }

        private string _nowSelectedBroadCastType;
        public string NowSelectedBroadCastType
        {
            get => _nowSelectedBroadCastType;
            set
            {
                if (_nowSelectedBroadCastType != value)
                {
                    _nowSelectedBroadCastType = value;
                    OnPropertyChanged(nameof(NowSelectedBroadCastType));
                }

                if (_nowSelectedBroadCastType == "音乐广播")
                {
                    _isShowMusic = true;
                    _isShowTTS = false;
                    _IsManual = false;
                    OnPropertyChanged(nameof(IsShowMusic));
                    OnPropertyChanged(nameof(IsShowTTS));
                    OnPropertyChanged(nameof(IsManual));
                }
                else if (_nowSelectedBroadCastType == "TTS广播")
                {
                    _isShowMusic = false;
                    _isShowTTS = true;
                    _IsManual = false;
                    OnPropertyChanged(nameof(IsShowMusic));
                    OnPropertyChanged(nameof(IsShowTTS));
                    OnPropertyChanged(nameof(IsManual));
                }
                else if (_nowSelectedBroadCastType == "人工广播")
                {
                    _isShowMusic = false;
                    _isShowTTS = false;
                    _IsManual = true;
                    OnPropertyChanged(nameof(IsShowMusic));
                    OnPropertyChanged(nameof(IsShowTTS));
                    OnPropertyChanged(nameof(IsManual));
                }
            }
        }


        private IList<string> _musicList;
        public IList<string> MusicList { get => _musicList; set => SetProperty(ref _musicList, value); }

        public IList _SelectedMusicList;
        public IList SelectedMusicList { get => _SelectedMusicList; set => SetProperty(ref _SelectedMusicList, value); }

        private List<int> _PlayCountList;
        public List<int> PlayCountList { get => _PlayCountList; set => SetProperty(ref _PlayCountList, value); }

        private int _NowSeletedPlayCount;
        public int NowSeletedPlayCount { get => _NowSeletedPlayCount; set => SetProperty(ref _NowSeletedPlayCount, value); }


        public AllPersonsBroadcastViewModel(BroadCastModel broadCastModelTemp)
        {
            TTSList = new List<string>();
            PlayCountList = new List<int>()
            {
                1,2,3,4,5,6,7,8,9,10
            };
            NowSeletedPlayCount = PlayCountList[0];

            broadCastModel = broadCastModelTemp;

            BroadcastTypeList = new List<string>()
            {
                "人工广播",
                "TTS广播",
                "音乐广播",
            };
            NowSelectedBroadCastType = BroadcastTypeList[0];
            MusicList = new List<string>();
            List<MusicModel> list = MusicDB.GetMusicList();
            if (list.Count > 0)
            {
                foreach (MusicModel music in list)
                {
                    MusicList.Add(music.Name);
                }
            }
            SelectedMusicList = new List<string>();

            List<TTSModel> ttslist = TTSDB.GetAllTTSList();
            if (ttslist.Count > 0)
            {
                foreach (TTSModel tts in ttslist)
                {
                    TTSList.Add(tts.Name);
                }
            }
            NowSeletedTTSItem = "";
        }


        // 发起人工广播
        public bool ManualBroadcast()
        {
            // 0. 各种状态的判断
            Dictionary<string, string> dictNumber = CommonDB.GetDispatchNum();
            string LeftNumber = dictNumber["left"];
            string RightNumber = dictNumber["right"];
            string NowDispatchNum = broadCastModel.DispatchNum;
            bool LeftIsCalling = CommonDB.IsCallingByNumber(LeftNumber);
            bool RightIsCalling = CommonDB.IsCallingByNumber(RightNumber);
            bool LeftIsAuth = CommonDB.IsAuthingOfNumber(LeftNumber);
            bool RightIsAuth = CommonDB.IsAuthingOfNumber(RightNumber);

            // 1. 左调度、右调度均处于忙碌状态
            if (LeftIsCalling && RightIsCalling)
            {
                DMMessageBox.Show("错误", "左调度、右调度均处于忙碌状态!!!", DMMessageType.MESSAGE_FAIL);
                return true;
            }

            // 2. 左右调度均未鉴权，仅考虑调度优先打出【该状态下，左右至少有一个处于空闲状态】
            if (!LeftIsAuth && !RightIsAuth)
            {
                // 【左优先】左空闲 以左直接打出
                // 【右优先】右空闲 以右直接打出
                if ((NowDispatchNum == LeftNumber && !LeftIsCalling) || (NowDispatchNum == RightNumber && !RightIsCalling))
                {
                    // 构建 Model
                    broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
                    broadCastModel.BroadCastBeginTime = broadCastModel.CreateTime;
                    broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);

                    broadCastModel.Type = BroadcastTypeEnum.AllManualBroadcast;
                    broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;

                    // 处理广播
                    Task.Run(() =>
                    {
                        BroadCastDB.InsertBroadCast(broadCastModel);
                        BroadCastUtil.BroadCastTypeFunction.HandlerBroadcast(broadCastModel);
                    });

                    // 打开监控视频
                    // 获取全部广播对象
                    List<string> list = BroadCastDB.GetAllBroadcastUser();
                    // 摄像头
                    BroadCastUtil.CreateBroadcastCameraVideo(list);
                }

                // 【左优先】左忙碌 右空闲 提示右未鉴权
                // 【右优先】右忙碌 左空闲 提示左未鉴权
                else
                {
                    string ret = (NowDispatchNum != LeftNumber) ? "左" : "右";
                    DMMessageBox.Show("警告", ret + "调度未鉴权!!!", DMMessageType.MESSAGE_FAIL);
                }

                return true;
            }

            // 3. 左右都鉴权，以调度优先为主【该状态下，说明左右均处于空闲状态】
            if (LeftIsAuth && RightIsAuth)
            {
                // 左右都鉴权，说明左右都空闲
                Dictionary<string, string> dict = (NowDispatchNum == LeftNumber) ? CommonDB.GetUUIDByAuthing(LeftNumber) : CommonDB.GetUUIDByAuthing(RightNumber);

                // 谁优先使用谁
                if (dict != null)
                {
                    // 构建 Model
                    broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
                    broadCastModel.BroadCastBeginTime = broadCastModel.CreateTime;
                    broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);

                    broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
                    broadCastModel.Type = BroadcastTypeEnum.AllManualBroadcast;
                    // 存数据库，并生成 id
                    BroadCastDB.InsertBroadCast(broadCastModel);

                    // 发起广播
                    // 在数据库保存：会议ID
                    BroadCastDB.AddSelectedUsersAndBroadcastName("conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp, BroadCastDB.GetCallIdByGroupId(broadCastModel.GroupId));

                    // WXL_TODO 直接打到脚本的 #33CallID 方法执行全体人工广播
                    SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {dict["uuid"]} #33{BroadCastDB.GetCallIdByGroupId(broadCastModel.GroupId)} '");

                    // 打开监控视频
                    // 获取全部广播对象
                    List<string> list = BroadCastDB.GetAllBroadcastUser();
                    // 摄像头
                    BroadCastUtil.CreateBroadcastCameraVideo(list);
                }

                return true;
            }

            // 4. 仅一个鉴权，以鉴权为主【该状态下，哪个处于鉴权状态必定处于空闲状态，一定使用该调度号码发出点呼】
            //      - 同等状态下，鉴权的优先级 大于 左右优先
            //      - 比如：左鉴权，右优先且空闲，此时点呼从鉴权话机打出
            if ((LeftIsAuth && !RightIsAuth) || (!LeftIsAuth && RightIsAuth))
            {
                // 鉴权，代表一定处于空闲状态
                Dictionary<string, string> dict = LeftIsAuth ? CommonDB.GetUUIDByAuthing(LeftNumber) : CommonDB.GetUUIDByAuthing(RightNumber);

                if (dict != null)
                {
                    // 构建 Model
                    broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
                    broadCastModel.BroadCastBeginTime = broadCastModel.CreateTime;
                    broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);

                    broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
                    broadCastModel.Type = BroadcastTypeEnum.AllManualBroadcast;
                    // 存数据库，并生成 id
                    BroadCastDB.InsertBroadCast(broadCastModel);

                    // 发起广播
                    // 在数据库保存：会议ID
                    BroadCastDB.AddSelectedUsersAndBroadcastName("conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp, BroadCastDB.GetCallIdByGroupId(broadCastModel.GroupId));

                    // WXL_TODO 直接打到脚本的 #33CallID 方法执行全体人工广播
                    SSH.ExecuteCommand($"fs_cli -x 'uuid_transfer {dict["uuid"]} #33{BroadCastDB.GetCallIdByGroupId(broadCastModel.GroupId)} '");

                    // 打开监控视频
                    // 获取全部广播对象
                    List<string> list = BroadCastDB.GetAllBroadcastUser();
                    // 摄像头
                    BroadCastUtil.CreateBroadcastCameraVideo(list);
                }

                return true;
            }

            return true;
        }


        // 发起TTS广播
        public bool TTSBroadcast()
        {
            if (NowSeletedTTSItem == null || NowSeletedTTSItem == "")
            {
                DMMessageBox.ShowInfo("请选择TTS广播的内容");
                return false;
            }

            // 构建 Model
            broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
            broadCastModel.BroadCastBeginTime = broadCastModel.CreateTime;
            broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);

            broadCastModel.Type = BroadcastTypeEnum.AllTTSBroadcast;
            broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
            broadCastModel.TTSText = TTSDB.GeTextByName(NowSeletedTTSItem);
            broadCastModel.MusicPath = TTSDB.getPathByName(NowSeletedTTSItem);
            broadCastModel.PlayCount = NowSeletedPlayCount;

            // 处理广播
            Task.Run(() =>
            {
                BroadCastDB.InsertBroadCast(broadCastModel);
                BroadCastUtil.BroadCastTypeFunction.HandlerBroadcast(broadCastModel);
            });

            return true;
        }


        // 发送音乐广播
        public bool MusicBroadcast()
        {
            if (MusicList.Count == 0)
            {
                DMMessageBox.ShowInfo("当前音乐列表为空，请先上传音乐文件");
                return false;
            }

            if (SelectedMusicList == null || SelectedMusicList.Count <= 0)
            {
                DMMessageBox.ShowInfo("请选择音乐");
                return false;
            }


            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < SelectedMusicList.Count; j++)
            {
                sb.Append((string)SelectedMusicList[j]).Append("!");
            }
            sb.Length--;


            // 构建 Model
            broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
            broadCastModel.BroadCastBeginTime = broadCastModel.CreateTime;
            broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);

            broadCastModel.Type = BroadcastTypeEnum.AllMusicBroadcast;
            broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
            broadCastModel.MusicPath = sb.ToString();
            broadCastModel.PlayCount = NowSeletedPlayCount;

            // 处理广播
            Task.Run(() =>
            {
                BroadCastDB.InsertBroadCast(broadCastModel);
                BroadCastUtil.BroadCastTypeFunction.HandlerBroadcast(broadCastModel);
            });

            return true;
        }

        // 确定按钮
        public ICommand ConfirmButtonCommand => new ViewModelCommand(param =>
        {

            // 获得窗口
            Window window = (Window)param;

            // 根据BroadcastType调用不同的处理方法  
            bool isCloseWindow = false;
            switch (NowSelectedBroadCastType)
            {
                case "人工广播":
                    isCloseWindow = ManualBroadcast();
                    break;
                case "TTS广播":
                    isCloseWindow = TTSBroadcast();
                    break;
                case "音乐广播":
                    isCloseWindow = MusicBroadcast();
                    break;
                default:
                    MessageBox.Show("未知的广播类型");
                    break;
            }

            // 关闭窗口
            if (isCloseWindow)
            {
                window.Close();
            }
        });

        // 取消按钮
        public ICommand CancelButtonCommand => new ViewModelCommand(param =>
        {
            Window window = (Window)param;
            window.Close();
        });
    }
}
