using System;
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
    public class TTSRealTimeBroadcastViewModel : ViewModelsBase
    {
        private BroadCastModel broadCastModel;
        //private string _ttsText;
        //public string TTSText { get => _ttsText; set => SetProperty(ref _ttsText, value); }

        private List<string> _TTSList;
        public List<string> TTSList { get => _TTSList; set => SetProperty(ref _TTSList, value); }

        private string _NowSeletedTTSItem;
        public string NowSeletedTTSItem { get => _NowSeletedTTSItem; set => SetProperty(ref _NowSeletedTTSItem, value); }


        private List<int> _PlayCountList;
        public List<int> PlayCountList { get => _PlayCountList; set => SetProperty(ref _PlayCountList, value); }

        private int _NowSeletedPlayCount;
        public int NowSeletedPlayCount { get => _NowSeletedPlayCount; set => SetProperty(ref _NowSeletedPlayCount, value); }


        public TTSRealTimeBroadcastViewModel(BroadCastModel broadCastModelTemp)
        {
            TTSList = new List<string>();
            PlayCountList = new List<int>()
            {
                1,2,3,4,5,6,7,8,9,10
            };
            NowSeletedPlayCount = PlayCountList[0];

            broadCastModel = broadCastModelTemp;


            List<TTSModel> list = TTSDB.GetAllTTSList();
            if (list.Count > 0)
            {
                foreach (TTSModel tts in list)
                {
                    TTSList.Add(tts.Name);
                }
            }
            NowSeletedTTSItem = "";

        }


        // 处理实时广播操作
        private bool HandleRealTimeBroadcast()
        {
            if (NowSeletedTTSItem == null || NowSeletedTTSItem == "")
            {
                DMMessageBox.ShowInfo("请选择TTS广播的内容");
                return false;
            }

            RealTimeBroadcast();

            return true;
        }


        // 点击确认按钮执行TTS实时广播
        private void RealTimeBroadcast()
        {
            // 构建 Model
            broadCastModel.CreateTime = DMUtil.GetNowDateTimeString();
            broadCastModel.BroadCastBeginTime = broadCastModel.CreateTime;
            broadCastModel.TimeStamp = DMUtil.GetTimeStamp(broadCastModel.CreateTime);

            broadCastModel.TTSText = TTSDB.GeTextByName(NowSeletedTTSItem);

            broadCastModel.MusicPath = TTSDB.getPathByName(NowSeletedTTSItem);
            broadCastModel.PlayCount = NowSeletedPlayCount;
            broadCastModel.PlayStatus = BroadCastPlayStatusEnum.Unplayed;
            broadCastModel.Type = (broadCastModel.Users.Count > 0) ? BroadcastTypeEnum.RealTimeTTSBroadcastOfGroupSelected : BroadcastTypeEnum.RealTimeTTSBroadcastOfGroupAll;

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
            bool isCloseWindow = HandleRealTimeBroadcast();

            Window window = (Window)param;
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
