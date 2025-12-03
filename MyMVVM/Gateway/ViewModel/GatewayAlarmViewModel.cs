using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyMVVM.Common;
using MyMVVM.Common.ViewModel;
using MyMVVM.Dispatch.Model;
using MyMVVM.Emotion;
using MyMVVM.Emotion.Model;
using MyMVVM.Emotion.View;
using Newtonsoft.Json;

namespace MyMVVM.Gateway.ViewModel
{
    internal class GatewayAlarmViewModel : ViewModelsBase
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
        private ObservableCollection<GatewayAlarmRecordModel> _List;
        public ObservableCollection<GatewayAlarmRecordModel> PageList { get => _List; set { _List = value; OnPropertyChanged(nameof(PageList)); } }

        public GatewayAlarmViewModel()
        {
            CurrentPage = 1;
            PageSize = 10;
            LoadByPage();
        }

        public void LoadByPage()
        {
            int total = CommonDB.GetGatewayAlarmCount();
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
            PageList = new ObservableCollection<GatewayAlarmRecordModel>(CommonDB.GetGatewayAlarmRecorList(CurrentPage, PageSize));
        }

        public ICommand GoToFirstPageCommand => new ViewModelCommand(param =>
        {
            CurrentPage = 1;
            LoadByPage();
        });

        public ICommand PreviousPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage <= 1)
            {
                return;
            }
            CurrentPage--;
            LoadByPage();
        });

        public ICommand NextPageCommand => new ViewModelCommand(param =>
        {
            if (CurrentPage >= TotalPages)
            {
                return;
            }
            CurrentPage++;
            LoadByPage();
        });
    }
}
