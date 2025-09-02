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
    public class TaskBroadcastViewModel : ViewModelsBase
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

        private string _dMBeginDateTime;
        public string DMBeginDateTimeString { get => _dMBeginDateTime; set => SetProperty(ref _dMBeginDateTime, value); }
        private string _dMEndDateTime;
        public string DMEndDateTimeString { get => _dMEndDateTime; set => SetProperty(ref _dMEndDateTime, value); }



        private IList<string> _musicList;
        public IList<string> MusicList { get => _musicList; set => SetProperty(ref _musicList, value); }

        public IList _SelectedMusicList;
        public IList SelectedMusicList { get => _SelectedMusicList; set => SetProperty(ref _SelectedMusicList, value); }

        private int NowSeletedPlayCount;


        public TaskBroadcastViewModel(BroadCastModel broadCastModelTemp)
        {
            TTSList = new List<string>();
            NowSeletedPlayCount = -1;

            broadCastModel = broadCastModelTemp;

            IsShowTTS = true; // 默认是TTS
            IsShowMusic = false;

            DateTime d = DateTime.Now;
            DateTime d1 = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0);
            DateTime d2 = d1.AddMinutes(5);
            DateTime d3 = d1.AddMinutes(65);
            DMBeginDateTimeString = DMUtil.GetDateTimeString(d2);
            DMEndDateTimeString = DMUtil.GetDateTimeString(d3);

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




        private bool HandleTaskBroadcast()
        {
            if (DMBeginDateTimeString == null || DMBeginDateTimeString == "")
            {
                MessageBox.Show("任务开始时间为为空");
                return false;
            }

            if (DMEndDateTimeString == null || DMEndDateTimeString == "")
            {
                MessageBox.Show("任务结束时间为为空");
                return false;
            }

            if (IsShowMusic && MusicList.Count == 0)
            {
                DMMessageBox.ShowInfo("当前音乐列表为空，请先上传音乐文件");
                return false;
            }

            if (IsShowMusic && SelectedMusicList.Count == 0)
            {
                DMMessageBox.ShowInfo("请选择音乐");
                return false;
            }

            if (IsShowTTS && (NowSeletedTTSItem == null || NowSeletedTTSItem == ""))
            {
                DMMessageBox.ShowInfo("请选择TTS广播的内容");
                return false;
            }

            string begin = DMUtil.TransformDateTimeString(DMBeginDateTimeString, "M/d/yyyy h:mm:ss tt", "yyyy-MM-dd HH:mm:ss");
            string end = DMUtil.TransformDateTimeString(DMEndDateTimeString, "M/d/yyyy h:mm:ss tt", "yyyy-MM-dd HH:mm:ss");

            if (DateTime.Now >= DateTime.Parse(begin))
            {
                MessageBox.Show("请不要将任务的开始时间设置的太晚");
                return false;
            }

            if (DateTime.Parse(end) <= DateTime.Parse(begin))
            {
                MessageBox.Show("请不要将任务的结束时间设置的太早");
                return false;
            }

            DMBeginDateTimeString = begin;
            DMEndDateTimeString = end;

            if (IsShowMusic)
            {
                MusicBroadcast();
            }

            if (IsShowTTS)
            {
                TTSBroadcast();
            }

            return true;
        }



        // 任务 TTS 广播
        private bool TTSBroadcast()
        {
            // 构建 Model
            broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
            broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);
            broadCastModel.BroadCastBeginTime = DMBeginDateTimeString;
            broadCastModel.BroadCastEndTime = DMEndDateTimeString;
            broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
            broadCastModel.TTSText = TTSDB.GeTextByName(NowSeletedTTSItem);
            broadCastModel.MusicPath = TTSDB.getPathByName(NowSeletedTTSItem);
            broadCastModel.PlayCount = NowSeletedPlayCount;
            broadCastModel.Type = (broadCastModel.Users.Count > 0) ? BroadcastTypeEnum.TaskTTSBroadcastOfGroupSelected : BroadcastTypeEnum.TaskTTSBroadcastOfGroupAll;

            // 处理广播
            Task.Run(() =>
            {
                BroadCastDB.InsertBroadCast(broadCastModel);
                BroadCastUtil.BroadCastTypeFunction.HandlerBroadcast(broadCastModel);
            });

            return true;
        }



        // 任务 音乐广播
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
            broadCastModel.BroadCastBeginTime = DMBeginDateTimeString;
            broadCastModel.BroadCastEndTime = DMEndDateTimeString;
            broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
            // broadCastModel.MusicPath = MusicDB.GetUploadRemotePath() + NowSelectedMusic + ".wav";
            broadCastModel.MusicPath = sb.ToString();
            broadCastModel.PlayCount = NowSeletedPlayCount;
            broadCastModel.Type = (broadCastModel.Users.Count > 0) ? BroadcastTypeEnum.TaskMusicBroadcastOfGroupSelected : BroadcastTypeEnum.TaskMusicBroadcastOfGroupAll;

            // 处理广播
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
            bool isCloseWindow = HandleTaskBroadcast();

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
