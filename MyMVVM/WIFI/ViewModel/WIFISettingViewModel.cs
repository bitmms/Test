using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.WIFI.Model;
using MyMVVM.WIFI.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyMVVM.WIFI.ViewModel
{
    public class WIFISettingViewModel : ViewModelsBase
    {
        private ObservableCollection<WIFIModel> _wifiDateList;
        public ObservableCollection<WIFIModel> WIFIDateList { get => _wifiDateList; set => SetProperty(ref _wifiDateList, value); }

        private string _searchText;
        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

        private bool IsSearchLoad;

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



        public WIFISettingViewModel()
        {
            CurrentPage = 1;
            PageSize = 7;
            IsSearchLoad = false;
            LoadWIFIModelList();
        }


        public void LoadWIFIModelList()
        {
            int total = WIFIDB.GetMusicCount();
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

            WIFIDateList = new ObservableCollection<WIFIModel>();
            foreach (var item in WIFIDB.GetWifiList(CurrentPage, PageSize))
            {
                item.SaveEditorCommand = new ViewModelCommand(param =>
                {
                    EditorWIFIView editorWIFIView = new EditorWIFIView(item);
                    editorWIFIView.ShowDialog();

                    if (IsSearchLoad)
                    {
                        SearchTextFun();
                    }
                    else
                    {
                        LoadWIFIModelList();
                    }
                });
                item.DeleteCommand = new ViewModelCommand(param =>
                {
                    if (DMMessageBox.Show("删除数据", $"确定删除 {item.WIFIName} 基站?", DMMessageType.MESSAGE_WARING, DMMessageButton.YesNo))
                    {
                        WIFIDB.DeleteWIFIModelName(item);

                        if (IsSearchLoad)
                        {
                            int total11 = WIFIDB.GetMusicCountBySearch(SearchText);
                            if (total11 <= 0)
                            {
                                WIFIDateList = new ObservableCollection<WIFIModel>();
                                return;
                            }
                            TotalPages = total11 / PageSize;
                            TotalPages = TotalPages + ((total11 % PageSize > 0) ? 1 : 0);

                            if (CurrentPage > TotalPages)
                            {
                                CurrentPage = TotalPages;
                            }
                            SearchTextFun();
                        }
                        else
                        {
                            int total11 = WIFIDB.GetMusicCount();
                            if (total11 <= 0)
                            {
                                WIFIDateList = new ObservableCollection<WIFIModel>();
                                return;
                            }
                            TotalPages = total11 / PageSize;
                            TotalPages = TotalPages + ((total11 % PageSize > 0) ? 1 : 0);

                            if (CurrentPage > TotalPages)
                            {
                                CurrentPage = TotalPages;
                            }
                            LoadWIFIModelList();
                        }
                    }
                });
                WIFIDateList.Add(item);
            }
        }



        /// <summary>
        /// 搜索按钮
        /// </summary>
        public ICommand SearchCommand => new ViewModelCommand(ele =>
        {
            if (SearchText == null || SearchText == "")
            {
                LoadWIFIModelList();
            }
            else
            {
                int total = WIFIDB.GetMusicCountBySearch(SearchText);
                TotalPages = total / PageSize;
                TotalPages = TotalPages + ((total % PageSize > 0) ? 1 : 0);
                CurrentPage = 1;
                IsSearchLoad = true;
                SearchTextFun();
            }
        });


        public void SearchTextFun()
        {
            WIFIDateList = new ObservableCollection<WIFIModel>();
            foreach (var item in WIFIDB.SearchWifiList(SearchText, CurrentPage, PageSize))
            {
                item.SaveEditorCommand = new ViewModelCommand(param =>
                {
                    EditorWIFIView editorWIFIView = new EditorWIFIView(item);
                    editorWIFIView.ShowDialog();

                    if (IsSearchLoad)
                    {
                        SearchTextFun();
                    }
                    else
                    {
                        LoadWIFIModelList();
                    }
                });
                item.DeleteCommand = new ViewModelCommand(param =>
                {
                    if (DMMessageBox.Show("删除数据", $"确定删除 {item.WIFIName} 基站?", DMMessageType.MESSAGE_WARING, DMMessageButton.YesNo))
                    {
                        WIFIDB.DeleteWIFIModelName(item);

                        if (IsSearchLoad)
                        {
                            int total11 = WIFIDB.GetMusicCountBySearch(SearchText);
                            if (total11 <= 0)
                            {
                                WIFIDateList = new ObservableCollection<WIFIModel>();
                                return;
                            }
                            TotalPages = total11 / PageSize;
                            TotalPages = TotalPages + ((total11 % PageSize > 0) ? 1 : 0);

                            if (CurrentPage > TotalPages)
                            {
                                CurrentPage = TotalPages;
                            }
                            SearchTextFun();
                        }
                        else
                        {
                            int total11 = WIFIDB.GetMusicCount();
                            if (total11 <= 0)
                            {
                                WIFIDateList = new ObservableCollection<WIFIModel>();
                                return;
                            }
                            TotalPages = total11 / PageSize;
                            TotalPages = TotalPages + ((total11 % PageSize > 0) ? 1 : 0);

                            if (CurrentPage > TotalPages)
                            {
                                CurrentPage = TotalPages;
                            }
                            LoadWIFIModelList();
                        }
                    }
                });
                WIFIDateList.Add(item);
            }
        }


        public ICommand GoToFirstPageCommand => new ViewModelCommand(param =>
        {
            CurrentPage = 1;
            if (IsSearchLoad)
            {
                SearchTextFun();
            }
            else
            {
                LoadWIFIModelList();
            }
        });

        public ICommand PreviousPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage <= 1)
            {
                return;
            }

            if (IsSearchLoad)
            {
                CurrentPage--;
                SearchTextFun();
            }
            else
            {
                CurrentPage--;
                LoadWIFIModelList();
            }
        });

        public ICommand NextPageCommand => new ViewModelCommand(param =>
        {
            if (IsSearchLoad)
            {
                if (CurrentPage >= TotalPages)
                {
                    return;
                }
                CurrentPage++;
                SearchTextFun();
            }
            else
            {
                if (CurrentPage >= TotalPages)
                {
                    return;
                }
                CurrentPage++;
                LoadWIFIModelList();
            }

        });





    }
}

