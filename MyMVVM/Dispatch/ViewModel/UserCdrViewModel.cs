using MyMVVM.Common.ViewModel;
using MyMVVM.Dispatch.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyMVVM.Dispatch.ViewModel
{
    public class UserCdrViewModel : ViewModelsBase
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

        public ObservableCollection<UserCdrModel> _userModel;
        public ObservableCollection<UserCdrModel> UserModel
        {
            get => _userModel;
            set
            {
                _userModel = value;
                OnPropertyChanged(nameof(UserModel));
            }
        }

        public UserCdrViewModel()
        {
            CurrentPage = 1;
            PageSize = 17;
            LoadUserCdrInfo();
        }

        public void LoadUserCdrInfo()
        {
            int total = DispatchDB.GetUserCdrCount();
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

            List<UserCdrModel> listModel = DispatchDB.UsrCdr(CurrentPage, PageSize);

            ObservableCollection<UserCdrModel> userModel = new ObservableCollection<UserCdrModel>(listModel);

            UserModel = userModel;

        }

        public ICommand GoToFirstPageCommand => new ViewModelCommand(p =>
        {
            CurrentPage = 1;
            LoadUserCdrInfo();
        });


        public ICommand PreviousPageCommand => new ViewModelCommand(p =>
        {
            if (CurrentPage <= 1)
            {
                return;
            }
            CurrentPage--;
            LoadUserCdrInfo();
        });

        public ICommand NextPageCommand => new ViewModelCommand(p =>
        {
            if (CurrentPage >= TotalPages)
            {
                return;
            }
            CurrentPage++;
            LoadUserCdrInfo();

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
            LoadUserCdrInfo();
        });
    }
}
