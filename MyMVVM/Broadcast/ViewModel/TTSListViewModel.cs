using MyMVVM.Broadcast.View;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyMVVM.Broadcast.ViewModel
{
    public class TTSListViewModel : ViewModelsBase
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

        public ObservableCollection<TTSModel> _TTSList;
        public ObservableCollection<TTSModel> TTSList { get => _TTSList; set { _TTSList = value; OnPropertyChanged(nameof(TTSList)); } }



        public TTSListViewModel()
        {
            CurrentPage = 1;
            PageSize = 9;

            LoadTTSInfoByPage();
        }


        public void LoadTTSInfoByPage()
        {
            int total = TTSDB.GetTTSCount();
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

            List<TTSModel> list = TTSDB.GetTTSList(CurrentPage, PageSize);
            list.ForEach(tts =>
            {
                tts.DeleteTTSCommand = new ViewModelCommand(ParamArrayAttribute =>
                {
                    if (!DMMessageBox.Show("删除TTS", "确定删除该TTS文件？", DMMessageType.MESSAGE_WARING, DMMessageButton.YesNo))
                    {
                        return;
                    }

                    // 1. 删除数据库
                    TTSDB.DeleteTTSById(tts.Id);

                    // 2. 删除服务器
                    //Task.Run(() =>
                    //{
                    //    // 3. 
                    //    SSH.ExecuteCommand("rm -rf " + music.UploadRemotePath);
                    //});

                    LoadTTSInfoByPage();
                });
            });
            TTSList = new ObservableCollection<TTSModel>(list);
        }


        public ICommand GoToFirstPageCommand => new ViewModelCommand(param =>
        {
            CurrentPage = 1;
            LoadTTSInfoByPage();
        });

        public ICommand PreviousPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage <= 1)
            {
                return;
            }
            CurrentPage--;
            LoadTTSInfoByPage();
        });

        public ICommand NextPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage >= TotalPages)
            {
                return;
            }
            CurrentPage++;
            LoadTTSInfoByPage();
        });

        public ICommand ClickCreateTTSText => new ViewModelCommand(param =>
        {
            TTSCreateAndDetailView view = new TTSCreateAndDetailView();
            TTSCreateAndDetailViewModel viewModel = new TTSCreateAndDetailViewModel();
            view.DataContext = viewModel;
            view.Closed += (sender, args) =>
            {
                Task.Run(() =>
                {
                    Thread.Sleep(500);
                    LoadTTSInfoByPage();
                });
            };
            view.ShowDialog();
        });


    }
}

