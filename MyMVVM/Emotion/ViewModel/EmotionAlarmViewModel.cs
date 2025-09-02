using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyMVVM.Broadcast;
using MyMVVM.Broadcast.View;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.Emotion.Model;
using MyMVVM.Emotion.View;
using Newtonsoft.Json;

namespace MyMVVM.Emotion.ViewModel
{
    internal class EmotionAlarmViewModel : ViewModelsBase
    {
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

        private int _TotalPages;
        public int TotalPages { get => _TotalPages; set { _TotalPages = value; OnPropertyChanged(nameof(TotalPages)); } }

        private int _PageSize;
        public int PageSize { get => _PageSize; set { _PageSize = value; OnPropertyChanged(nameof(PageSize)); } }

        private EmotionModel _selectedItem;
        public EmotionModel SelectedItem
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

        public ObservableCollection<EmotionModel> _EmotionList;
        public ObservableCollection<EmotionModel> EmotionList { get => _EmotionList; set { _EmotionList = value; OnPropertyChanged(nameof(EmotionList)); } }


        public String _KeyWordData;
        public String KeyWordData { get => _KeyWordData; set { _KeyWordData = value; OnPropertyChanged(nameof(KeyWordData)); } }

        private string _KeyWord;

        public EmotionAlarmViewModel()
        {
            CurrentPage = 1;
            PageSize = 10;

            LoadEmotionfoByPage();

            List<string> JsonsList = JsonConvert.DeserializeObject<List<string>>(EmotionAlarmDB.getKeyWord());
            StringBuilder sb = new StringBuilder();
            foreach (string json in JsonsList)
            {
                sb.Append(json).Append(",");
            }
            sb.Length--;
            KeyWordData = "当前关键字：" + sb.ToString();
        }


        public void LoadEmotionfoByPage()
        {
            int total = EmotionAlarmDB.getCount();
            TotalPages = total / PageSize;
            TotalPages = TotalPages + ((total % PageSize > 0) ? 1 : 0);

            if (CurrentPage < TotalPages)
            {
                IsHideNextButton = 1;
            }
            else
            {
                IsHideNextButton = 0;
            }
            List<EmotionModel> list = EmotionAlarmDB.getList(CurrentPage, PageSize);
            list.ForEach(emotionItem =>
            {
                emotionItem.CancelCommand = new ViewModelCommand(pp =>
                {
                    EmotionAlarmDB.ConfirmEmotion(emotionItem.Id);
                    LoadEmotionfoByPage();
                });

                emotionItem.ShowMoreInfo = new ViewModelCommand(pp =>
                {
                    new PlayAudioView(emotionItem).ShowDialog();
                });

                // CallText 表示匹配到的关键字
                List<string> keywords = JsonConvert.DeserializeObject<List<string>>(EmotionAlarmDB.getKeyWord());
                StringBuilder sb = new StringBuilder();
                foreach (var keyword in keywords)
                {
                    if (emotionItem.CallText.Contains(keyword))
                    {
                        sb.Append(keyword).Append(",").Append(" ");
                    }
                }
                if (sb.Length > 0)
                {
                    sb.Length--;
                    sb.Length--;
                }
                emotionItem.CallText = sb.ToString();
            });
            ObservableCollection<EmotionModel> musicModels = new ObservableCollection<EmotionModel>(list);
            EmotionList = musicModels;
        }

        public ICommand GoToFirstPageCommand => new ViewModelCommand(param =>
        {
            CurrentPage = 1;
            LoadEmotionfoByPage();
        });

        public ICommand PreviousPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage <= 1)
            {
                return;
            }
            CurrentPage--;
            LoadEmotionfoByPage();
        });

        public ICommand NextPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage >= TotalPages)
            {
                return;
            }
            CurrentPage++;
            LoadEmotionfoByPage();
        });


        public ICommand UpdateEmotionKeyWord => new ViewModelCommand(param =>
        {
            var view = new UpdateEmotionKeyWordView();
            view.Closed += (sender, args) =>
            {
                List<string> JsonsList = JsonConvert.DeserializeObject<List<string>>(EmotionAlarmDB.getKeyWord());
                StringBuilder sb = new StringBuilder();
                foreach (string json in JsonsList)
                {
                    sb.Append(json).Append(",");
                }
                sb.Length--;
                KeyWordData = "当前关键字：" + sb.ToString();
                LoadEmotionfoByPage();
            };
            view.ShowDialog();
        });

    }
}