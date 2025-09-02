using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyMVVM.Broadcast.Utils;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;

namespace MyMVVM.Broadcast.ViewModel
{
    public class ScheduledBroadcastViewModel : ViewModelsBase
    {
        private List<string> _TTSList;
        public List<string> TTSList { get => _TTSList; set => SetProperty(ref _TTSList, value); }

        private string _NowSeletedTTSItem;
        public string NowSeletedTTSItem { get => _NowSeletedTTSItem; set => SetProperty(ref _NowSeletedTTSItem, value); }

        private BroadCastModel broadCastModel;


        private int _nowSeletedBroadcastDuration;
        public int NowSeletedBroadcastDuration { get => _nowSeletedBroadcastDuration; set => SetProperty(ref _nowSeletedBroadcastDuration, value); }

        private List<int> _broadcastDurationList;
        public List<int> BroadcastDurationList { get => _broadcastDurationList; set => SetProperty(ref _broadcastDurationList, value); }
        private bool _isShowMusic;
        public bool IsShowMusic { get => _isShowMusic; set => SetProperty(ref _isShowMusic, value); }

        private bool _isShowTTS;
        public bool IsShowTTS { get => _isShowTTS; set => SetProperty(ref _isShowTTS, value); }
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
                    OnPropertyChanged(nameof(IsShowMusic));
                    OnPropertyChanged(nameof(IsShowTTS));
                }
                else if (_nowSelectedBroadCastType == "TTS广播")
                {
                    _isShowMusic = false;
                    _isShowTTS = true;
                    OnPropertyChanged(nameof(IsShowMusic));
                    OnPropertyChanged(nameof(IsShowTTS));
                }
            }
        }

        private IList<string> _musicList;
        public IList<string> MusicList { get => _musicList; set => SetProperty(ref _musicList, value); }

        public IList _SelectedMusicList;
        public IList SelectedMusicList { get => _SelectedMusicList; set => SetProperty(ref _SelectedMusicList, value); }


        private string _dMScheduldTime;
        public string DMScheduldTimeString { get => _dMScheduldTime; set => SetProperty(ref _dMScheduldTime, value); }

        private int NowSeletedPlayCount;

        public ScheduledBroadcastViewModel(BroadCastModel broadCastModelTemp)
        {
            TTSList = new List<string>();
            NowSeletedPlayCount = -1;

            broadCastModel = broadCastModelTemp;

            BroadcastDurationList = new List<int>()
            {
                5, 10,15, 20, 25,30,60,90,120
            };
            NowSeletedBroadcastDuration = BroadcastDurationList[0];

            IsShowTTS = true; // 默认是TTS
            IsShowMusic = false;

            BroadcastTypeList = new List<string>()
            {
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

            DateTime d = DateTime.Now;
            DMScheduldTimeString = DMUtil.GetDateTimeString(new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0));

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






        // 处理定时广播的操作
        private bool HandleScheduledBroadcast()
        {
            if (DMScheduldTimeString == null || DMScheduldTimeString == "")
            {
                MessageBox.Show("定时时间为空");
                return false;
            }

            DMScheduldTimeString = DMUtil.TransformTimeString(DMScheduldTimeString, "M/d/yyyy h:mm:ss tt", "hh:mm:ss");

            // DMScheduldTimeString = DateTime.ParseExact(DMScheduldTimeString, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture).ToString("hh:mm:ss", CultureInfo.InvariantCulture);

            if (IsShowMusic)
            {

                if (MusicList.Count == 0)
                {
                    DMMessageBox.ShowInfo("当前音乐列表为空，请先上传音乐文件");
                    return false;
                }

                if (SelectedMusicList.Count == 0)
                {
                    DMMessageBox.ShowInfo("请选择音乐");
                    return false;
                }


                MusicBroadcast();
            }
            if (IsShowTTS)
            {
                if (NowSeletedTTSItem == null || NowSeletedTTSItem == "")
                {
                    DMMessageBox.ShowInfo("请选择TTS广播的内容");
                    return false;
                }
                TTSBroadcast();
            }

            return true;
        }


        // 定时 TTS 广播
        private void TTSBroadcast()
        {
            // 构建 Model
            broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
            broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);

            broadCastModel.BroadCastBeginTime = DMScheduldTimeString;
            broadCastModel.BroadcastDuration = NowSeletedBroadcastDuration;
            broadCastModel.PlayStatus = BroadCastPlayStatusEnum.ScheduledBroadcastActive;
            broadCastModel.TTSText = TTSDB.GeTextByName(NowSeletedTTSItem);
            broadCastModel.MusicPath = TTSDB.getPathByName(NowSeletedTTSItem);
            broadCastModel.PlayCount = NowSeletedPlayCount;
            broadCastModel.Type = (broadCastModel.Users.Count > 0) ? BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupSelected : BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupAll;

            // 处理广播
            Task.Run(() =>
            {
                BroadCastDB.InsertBroadCast(broadCastModel);
                BroadCastUtil.BroadCastTypeFunction.HandlerBroadcast(broadCastModel);
            });
        }


        // 定时 音乐广播
        private void MusicBroadcast()
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < SelectedMusicList.Count; j++)
            {
                sb.Append((string)SelectedMusicList[j]).Append("!");
            }
            sb.Length--;

            // 构建 Model
            broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
            broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);

            broadCastModel.BroadCastBeginTime = DMScheduldTimeString;
            broadCastModel.BroadcastDuration = NowSeletedBroadcastDuration;
            broadCastModel.PlayStatus = BroadCastPlayStatusEnum.ScheduledBroadcastActive;
            // broadCastModel.MusicPath = MusicDB.GetUploadRemotePath() + NowSelectedMusic + ".wav";
            broadCastModel.MusicPath = sb.ToString();
            broadCastModel.PlayCount = NowSeletedPlayCount;
            broadCastModel.Type = (broadCastModel.Users.Count > 0) ? BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupSelected : BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupAll;

            // 处理
            Task.Run(() =>
            {
                BroadCastDB.InsertBroadCast(broadCastModel);
                BroadCastUtil.BroadCastTypeFunction.HandlerBroadcast(broadCastModel);
            });
        }



        // 确定按钮
        public ICommand ConfirmButtonCommand => new ViewModelCommand(param =>
        {
            // 获得窗口
            Window window = (Window)param;

            // 根据BroadcastType调用不同的处理方法  
            bool isCloseWindow = HandleScheduledBroadcast();

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

