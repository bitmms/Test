using MyMVVM.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Broadcast.ViewModel
{
    public class BroadcastDetailViewModel : ViewModelsBase
    {
        private BroadCastListModel _NowBroadCastListModel;
        public BroadCastListModel NowBroadCastListModel { get => _NowBroadCastListModel; set { _NowBroadCastListModel = value; OnPropertyChanged(nameof(NowBroadCastListModel)); } }


        private int ID;
        public int _ID { get => _ID; set { _ID = value; OnPropertyChanged(nameof(ID)); } }

        private string _DisplayObject;
        public string DisplayObject { get => _DisplayObject; set { _DisplayObject = value; OnPropertyChanged(nameof(DisplayObject)); } }

        private string _DisplayType;
        public string DisplayType { get => _DisplayType; set { _DisplayType = value; OnPropertyChanged(nameof(DisplayType)); } }

        private string _DisplayContent;
        public string DisplayContent { get => _DisplayContent; set { _DisplayContent = value; OnPropertyChanged(nameof(DisplayContent)); } }

        private string _DisplayTime;
        public string DisplayTime { get => _DisplayTime; set { _DisplayTime = value; OnPropertyChanged(nameof(DisplayTime)); } }

        private string _DisplayPlayStatus;
        public string DisplayPlayStatus { get => _DisplayPlayStatus; set { _DisplayPlayStatus = value; OnPropertyChanged(nameof(DisplayPlayStatus)); } }

        private string _PlayCount;
        public string PlayCount { get => _PlayCount; set { _PlayCount = value; OnPropertyChanged(nameof(PlayCount)); } }

        private bool _IsShowPlayCount;
        public bool IsShowPlayCount { get => _IsShowPlayCount; set { _IsShowPlayCount = value; OnPropertyChanged(nameof(IsShowPlayCount)); } }


        public BroadcastDetailViewModel(BroadCastListModel broadCastListModel)
        {
            ID = broadCastListModel.Id;
            DisplayObject = broadCastListModel.RealObject;
            PlayCount = broadCastListModel.PlayCount;
            DisplayType = broadCastListModel.DisplayType;
            DisplayContent = broadCastListModel.RealContent;
            DisplayTime = broadCastListModel.DisplayTime;
            DisplayPlayStatus = broadCastListModel.DisplayPlayStatus;
            IsShowPlayCount = false;
            if (DisplayType.Contains("音乐") || DisplayType.Contains("TTS"))
            {
                IsShowPlayCount = true;
                StringBuilder sb = new StringBuilder();
                string[] tempMusicList = DisplayContent.Split('!');
                foreach (var item in tempMusicList)
                {
                    sb.Append(item).Append("，");
                }
                sb.Length--;
                DisplayContent = sb.ToString();
            }
        }
    }
}
