using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.MainWindow.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyMVVM.Dispatch.ViewModel
{
    public class MissCallViewModel : ViewModelsBase
    {
        private int _currentPage;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    if (_currentPage == 1)
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

        private int _isHideFirstButton;
        public int IsHideFirstButton
        {
            get => _isHideFirstButton;
            set
            {
                _isHideFirstButton = value;
                OnPropertyChanged(nameof(IsHideFirstButton));
            }
        }

        private int _isHideNextButton;
        public int IsHideNextButton
        {
            get => _isHideNextButton;
            set
            {
                _isHideNextButton = value;
                OnPropertyChanged(nameof(IsHideNextButton));
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
            }
        }

        private int _pageSize;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
            }
        }

        private string _pageNumberInput;
        public string PageNumberInput
        {
            get => _pageNumberInput;
            set
            {
                _pageNumberInput = value;
                OnPropertyChanged(nameof(PageNumberInput));
            }
        }

        private MissCallModel _selectedItem;
        public MissCallModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    if (_selectedItem != null)
                    {
                        ExecuteSshCommand(SelectedItem.MissNum);
                    }
                }
            }
        }

        private ObservableCollection<MissCallModel> _misscallModel;
        public ObservableCollection<MissCallModel> MissCallModels
        {
            get => _misscallModel;
            set
            {
                _misscallModel = value;
                OnPropertyChanged(nameof(MissCallModels));
            }
        }


        public MissCallViewModel()
        {
            CurrentPage = 1;
            PageSize = 12;
            LoadMissCall();
        }

        private void LoadMissCall()
        {
            int total = DispatchDB.GetMissCallCount();
            TotalPages = total / PageSize;
            TotalPages = (int)Math.Ceiling((double)total / PageSize);

            if (CurrentPage < TotalPages)
            {
                IsHideFirstButton = 1;
            }
            else
            {
                IsHideFirstButton = 0;
            }

            List<MissCallModel> listModel = DispatchDB.MissCall(CurrentPage, PageSize);

            ObservableCollection<MissCallModel> missCallModels = new ObservableCollection<MissCallModel>(listModel);

            MissCallModels = missCallModels;
        }

        private async void ExecuteSshCommand(string value)
        {
            bool result = DMMessageBox.Show("呼叫选择", $"是否呼叫: {value}?", DMMessageType.MESSAGE_INFO, DMMessageButton.YesNo);
            if (result)
            {
                DataTable dt = await Task.Run(() => DispatchDB.DispatchNum());
                string left = dt.Rows[0]["left_dispatch"].ToString();
                string right = dt.Rows[0]["right_dispatch"].ToString();

                bool isLeftAvailable = await Task.Run(() => DispatchDB.DispatchStatus(left, left));

                string command = isLeftAvailable
                    ? $"fs_cli -x 'originate user/{right} {value}'"
                    : $"fs_cli -x 'originate user/{left} {value}'";

                await Task.Run(() => SSH.ExecuteCommand(command));
            }
        }

        public ICommand GoToFirstPageCommand => new ViewModelCommand(p =>
        {
            CurrentPage = 1;
            LoadMissCall();
        });

        public ICommand PreviousPageCommand => new ViewModelCommand(p =>
        {
            if (CurrentPage <= 1)
            {
                return;
            }
            CurrentPage--;
            LoadMissCall();
        });

        public ICommand NextPageCommand => new ViewModelCommand(p =>
        {
            if (CurrentPage >= TotalPages)
            {
                return;
            }
            CurrentPage++;
            LoadMissCall();
        });

        public ICommand GoToPageCommand => new ViewModelCommand(p =>
        {
            if (!int.TryParse(PageNumberInput, out int pageNumber))
            {
                return;
            }

            if (pageNumber < 1 || pageNumber > TotalPages)
            {

                return;
            }


            CurrentPage = pageNumber;
            LoadMissCall();
        });
    }
}
