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
    public class MusicBroadcastViewModel : ViewModelsBase
    {
        private BroadCastModel broadCastModel;

        private IList<string> _musicList;
        public IList<string> MusicList { get => _musicList; set => SetProperty(ref _musicList, value); }

        public IList _SelectedMusicList;
        public IList SelectedMusicList { get => _SelectedMusicList; set => SetProperty(ref _SelectedMusicList, value); }


        private List<int> _PlayCountList;
        public List<int> PlayCountList { get => _PlayCountList; set => SetProperty(ref _PlayCountList, value); }

        private int _NowSeletedPlayCount;
        public int NowSeletedPlayCount { get => _NowSeletedPlayCount; set => SetProperty(ref _NowSeletedPlayCount, value); }



        public MusicBroadcastViewModel(BroadCastModel broadCastModelTemp)
        {
            PlayCountList = new List<int>()
            {
                1,2,3,4,5,6,7,8,9,10
            };
            NowSeletedPlayCount = PlayCountList[0];

            broadCastModel = broadCastModelTemp;
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
        }

        private bool HandleTaskBroadcast()
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

            MusicBroadcast();

            return true;
        }

        // 发送音乐广播
        public void MusicBroadcast()
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < SelectedMusicList.Count; j++)
            {
                sb.Append((string)SelectedMusicList[j]).Append("!");
            }
            sb.Length--;

            // 构建 Model
            broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
            broadCastModel.BroadCastBeginTime = broadCastModel.CreateTime;
            broadCastModel.BroadCastEndTime = broadCastModel.CreateTime;
            broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);

            broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
            // broadCastModel.MusicPath = $"{MusicDB.GetUploadRemotePath()}{NowSelectedMusic}.wav";
            broadCastModel.MusicPath = sb.ToString();
            broadCastModel.PlayCount = NowSeletedPlayCount;
            broadCastModel.Type = (broadCastModel.Users.Count > 0) ? BroadcastTypeEnum.RealTimeMusicBroadcastOfGroupSelected : broadCastModel.Type = BroadcastTypeEnum.RealTimeMusicBroadcastOfGroupAll;

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