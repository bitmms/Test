using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyMVVM.Common;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;

namespace MyMVVM.Login.ViewModel
{
    public class LoginViewModel : ViewModelsBase
    {
        private string _username;
        private string _password;
        private string _ip;
        private bool _isViewVisible = true;
        private bool _rememberMe;

        public string Username { get => _username; set { _username = value; OnPropertyChanged(nameof(Username)); } }

        public string Password { get => _password; set { _password = value; OnPropertyChanged(nameof(Password)); } }

        public string IP { get => _ip; set { _ip = value; OnPropertyChanged(nameof(IP)); } }

        public bool IsViewVisible { get => _isViewVisible; set { _isViewVisible = value; OnPropertyChanged(nameof(IsViewVisible)); } }

        public bool RememberMe { get => _rememberMe; set { _rememberMe = value; OnPropertyChanged(nameof(RememberMe)); } }


        public ICommand LoginCommand { get; }
        public ICommand VersionCommand { get; }
        public ICommand RecoverPasswordCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new ViewModelCommand(p => ExecuteRecoverPassCommand("", ""));
            LoadLoginInfo();
            DefaultTheme();
        }

        private bool CanExecuteLoginCommand(object obj)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(IP);
        }

        private void ExecuteLoginCommand(object obj)
        {
            if (Username == null || Username == "")
            {
                DMMessageBox.Show("错误", "用户名错误", DMMessageType.MESSAGE_FAIL);
                return;
            }
            if (Password == null || Password == "")
            {
                DMMessageBox.Show("错误", "密码错误", DMMessageType.MESSAGE_FAIL);
                return;
            }
            if (!DMUtil.IsIP(IP))
            {
                DMMessageBox.Show("错误", "IP 地址错误", DMMessageType.MESSAGE_FAIL);
                return;
            }

            DMVariable.SSHIP = IP;
            DMVariable.POSTGRESQL_IP = IP;
            DMVariable.DB_STRING = $"Host={IP}; Port={DMVariable.POSTGRESQL_PORT}; User Id={DMVariable.POSTGRESQL_USERNAME}; Password={DMVariable.POSTGRESQL_PASSWORD}; Database={DMVariable.POSTGRESQL_DATABASE}";

            int ret1 = LoginDB.AuthLoginInfo(Username);
            int ret2 = LoginDB.AuthLoginInfo(Username, Password);

            if (ret1 < 0 || ret2 < 0)
            {
                DMMessageBox.Show("错误", "IP 地址错误", DMMessageType.MESSAGE_FAIL);
                return;
            }
            if (ret1 == 0)
            {
                DMMessageBox.Show("错误", "用户名错误", DMMessageType.MESSAGE_FAIL);
                return;
            }
            if (ret2 == 0)
            {
                DMMessageBox.Show("错误", "密码错误", DMMessageType.MESSAGE_FAIL);
                return;
            }

            // 登录成功
            DMVariable.NowLoginUserName = Username;
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(Username), null);
            IsViewVisible = false;
            if (RememberMe)
            {
                SaveLoginInfo();
            }
            else
            {
                ClearLoginInfo();
            }
        }

        private void ExecuteRecoverPassCommand(string username, string email)
        {
            throw new NotImplementedException();
        }

        private void SaveLoginInfo()
        {
            Properties.Settings.Default.Username = Username;
            Properties.Settings.Default.Password = Password;
            Properties.Settings.Default.IP = IP;
            Properties.Settings.Default.RememberMe = RememberMe;
            Properties.Settings.Default.Save();
        }

        private void LoadLoginInfo()
        {
            if (Properties.Settings.Default.RememberMe)
            {
                Username = Properties.Settings.Default.Username;
                Password = Properties.Settings.Default.Password;
                IP = Properties.Settings.Default.IP;
                RememberMe = Properties.Settings.Default.RememberMe;
            }
        }

        private void ClearLoginInfo()
        {
            Properties.Settings.Default.Username = string.Empty;
            Properties.Settings.Default.Password = string.Empty;
            Properties.Settings.Default.RememberMe = false;
            Properties.Settings.Default.Save();
        }

        //加载默认主题
        public void DefaultTheme()
        {
            List<ResourceDictionary> list = new List<ResourceDictionary>();
            foreach (ResourceDictionary resourceDictionary in Application.Current.Resources.MergedDictionaries)
            {
                list.Add(resourceDictionary);
            }
            if (!String.IsNullOrEmpty(Properties.Settings.Default.ThemeColors))
            {
                ResourceDictionary nowStyle = list[0];
                list[0] = new ResourceDictionary()
                {
                    Source = new Uri("pack://application:,,,/MyMVVM;component/Common/Styles/" + Properties.Settings.Default.ThemeColors + ".xaml"),
                };
            }
            else
            {
                list[0] = new ResourceDictionary()
                {
                    Source = new Uri(ThemeEnum.CoolTheme),
                };
            }
            Application.Current.Resources.MergedDictionaries.Clear();
            list.ForEach(x => Application.Current.Resources.MergedDictionaries.Add(x));
        }
    }
}

