using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;

namespace MyMVVM.Broadcast.ViewModel
{
    public class MusicListViewModel : ViewModelsBase
    {

        public bool IsEditor = false;

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

        public ObservableCollection<MusicModel> _MusicList;
        public ObservableCollection<MusicModel> MusicList { get => _MusicList; set { _MusicList = value; OnPropertyChanged(nameof(MusicList)); } }


        public MusicListViewModel()
        {
            CurrentPage = 1;
            PageSize = 10;

            LoadMusicInfoByPage();
        }


        public void LoadMusicInfoByPage()
        {
            int total = MusicDB.GetMusicCount();
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

            List<MusicModel> list = MusicDB.GetMusicList(CurrentPage, PageSize);
            list.ForEach(music =>
            {
                music.DeleteMusicCommand = new ViewModelCommand(ParamArrayAttribute =>
                {
                    if (!DMMessageBox.Show("删除音乐", "确定删除该音乐文件？", DMMessageType.MESSAGE_WARING, DMMessageButton.YesNo))
                    {
                        return;
                    }

                    IsEditor = true;

                    // 1. 删除数据库
                    MusicDB.DeleteMusicById(music.Id);

                    Task.Run(() =>
                    {
                        // 2. 删除本地
                        File.Delete(music.UploadLocalPath);
                        File.Delete(music.UploadLocalPath.Replace(".wav", ".mp3"));

                        // 3. 删除服务器
                        SSH.ExecuteCommand("rm -rf " + music.UploadRemotePath);
                        SSH.ExecuteCommand("rm -rf " + music.UploadRemotePath.Replace(".wav", ".mp3"));
                    });

                    LoadMusicInfoByPage();
                });
            });
            ObservableCollection<MusicModel> musicModels = new ObservableCollection<MusicModel>(list);
            MusicList = musicModels;
        }

        public ICommand GoToFirstPageCommand => new ViewModelCommand(param =>
        {
            CurrentPage = 1;
            LoadMusicInfoByPage();
        });

        public ICommand PreviousPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage <= 1)
            {
                return;
            }
            CurrentPage--;
            LoadMusicInfoByPage();
        });

        public ICommand NextPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage >= TotalPages)
            {
                return;
            }
            CurrentPage++;
            LoadMusicInfoByPage();
        });

    }
}
