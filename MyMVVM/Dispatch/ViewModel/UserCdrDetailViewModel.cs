using MyMVVM.Common.Model;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.Dispatch;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace MyMVVM.Dispatch.ViewModel
{
    public class UserCdrDetailViewModel : ViewModelsBase
    {

        public UserCdrDetailViewModel()
        {
            LoadDispatchCdr();
            NextPageCommand = new ViewModelCommand(NextPage, CanGoToNextPage);
            PreviousPageCommand = new ViewModelCommand(PreviousPage, CanGoToPreviousPage);
            GoToPageCommand = new ViewModelCommand(GoToPage, CanGoToPage);
            GoToFirstPageCommand = new ViewModelCommand(GoToFirstPage, CanGoToFirstPage);

        }



        public ObservableCollection<DefaultUserModel> _dispatchCdr;
        public ObservableCollection<DefaultUserModel> DispatchCdr { get => _dispatchCdr; set { _dispatchCdr = value; OnPropertyChanged(nameof(DispatchCdr)); } }


        private const int PageSize = 14;
        private int _currentPage = 1;
        private int _totalPages;

        public ICollectionView PagedCDR { get; set; }

        public ICommand NextPageCommand { get; set; }
        public ICommand PreviousPageCommand { get; set; }

        public ICommand GoToPageCommand { get; set; }

        public ICommand GoToFirstPageCommand { get; set; }

        public int CurrentPage { get => _currentPage; set { if (_currentPage != value) { _currentPage = value; OnPropertyChanged(nameof(CurrentPage)); PagedCDR.Refresh(); } } }

        public int TotalPages { get => _totalPages; set { if (_totalPages != value) { _totalPages = value; OnPropertyChanged(nameof(TotalPages)); } } }


        private string _pageNumberInput;
        public string PageNumberInput { get => _pageNumberInput; set { _pageNumberInput = value; OnPropertyChanged(nameof(PageNumberInput)); } }


        public DefaultUserModel _selectedItem;
        public DefaultUserModel SelectedItem
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
                        DataTable dataTable = DispatchDB.DispatchNum();
                        string left = dataTable.Rows[0]["left_dispatch"].ToString();
                        string right = dataTable.Rows[0]["right_dispatch"].ToString();

                        if (_selectedItem.CallNum1 == left || _selectedItem.CallNum1 == right)
                        {
                            ExecuteSshCommand(_selectedItem.CallNum2);
                        }

                        if (_selectedItem.CallNum2 == left || _selectedItem.CallNum2 == right)
                        {
                            ExecuteSshCommand(_selectedItem.CallNum1);
                        }


                    }
                }
            }
        }


        private void LoadDispatchCdr()
        {
            DataTable dataTable = DispatchDB.DispatchNum();
            string left = dataTable.Rows[0]["left_dispatch"].ToString();
            string right = dataTable.Rows[0]["right_dispatch"].ToString();

            DataTable dt = DispatchDB.DispatchCdr(left, right);
            ObservableCollection<DefaultUserModel> _dispatchCdr = new ObservableCollection<DefaultUserModel>();
            foreach (DataRow row in dt.Rows)
            {
                _dispatchCdr.Add(new DefaultUserModel
                {
                    CallerNum = $"{row["caller_name"]} ({row["caller_id_number"]})",
                    CalleeNum = $"{row["callee_name"]} ({row["destination_number"]})",
                    Duration = Convert.ToDateTime(row["start_stamp"]).ToString("HH:mm:ss"),
                    CallNum1 = row["caller_id_number"].ToString(),
                    CallNum2 = row["destination_number"].ToString()
                }); ;
            }

            DispatchCdr = _dispatchCdr;

            PagedCDR = CollectionViewSource.GetDefaultView(DispatchCdr);
            PagedCDR.Filter = Paginate;
            _totalPages = (int)Math.Ceiling((double)DispatchCdr.Count / PageSize);
            PagedCDR.Refresh();
        }



        /// <summary>
        /// 从调度记录呼叫
        /// </summary>
        /// <param name="value"></param>
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
                    ? $"fs_cli -x 'bgapi originate user/{right} {value}'"
                    : $"fs_cli -x 'bgapi originate user/{left} {value}'";

                await Task.Run(() => SSH.ExecuteCommand(command));
            }
        }

        private bool Paginate(object item)
        {
            int index = DispatchCdr.IndexOf(item as DefaultUserModel);
            return index >= (_currentPage - 1) * PageSize && index < _currentPage * PageSize;
        }

        public void NextPage(object parameter)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                PagedCDR.Refresh();
            }
        }

        public void PreviousPage(object parameter)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                PagedCDR.Refresh();
            }
        }

        private bool CanGoToNextPage(object parameter)
        {
            return _currentPage < _totalPages;
        }

        private bool CanGoToPreviousPage(object parameter)
        {
            return _currentPage > 1;
        }


        public void GoToPage(object parameter)
        {
            if (parameter is string pageStr && int.TryParse(pageStr, out int pageNumber))
            {
                if (pageNumber >= 1 && pageNumber <= TotalPages)
                {
                    CurrentPage = pageNumber;
                }
            }
        }

        private bool CanGoToPage(object parameter)
        {
            if (parameter is string pageStr && int.TryParse(pageStr, out int pageNumber))
            {
                return pageNumber >= 1 && pageNumber <= TotalPages;
            }
            return false;
        }


        public void GoToFirstPage(object parameter)
        {
            CurrentPage = 1;
        }


        private bool CanGoToFirstPage(object parameter)
        {
            return CurrentPage > 1;

        }
    }
}
