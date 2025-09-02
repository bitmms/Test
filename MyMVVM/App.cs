using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MyMVVM.Common;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Login.View;
using MyMVVM.Login.ViewModel;
using MyMVVM.MainWindow.View;
using MyMVVM.SoftReg.View;
using Npgsql;

namespace MyMVVM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected void ApplicationStart(object sender, StartupEventArgs e)
        {
            // 获取当前运行进程的主模块的名称（即执行文件的名称，比如myapp.exe）
            string MName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
            // 从模块名称中移除扩展名（如.exe），得到不包含扩展名的文件名
            string PName = System.IO.Path.GetFileNameWithoutExtension(MName);
            // 获取所有与当前应用程序的文件名匹配的进程
            System.Diagnostics.Process[] myProcess = System.Diagnostics.Process.GetProcessesByName(PName);
            // 判断是否重复启动
            if (myProcess.Length > 1)
            {
                DMMessageBox.ShowInfo("软件已经启动成功");
                Application.Current.Shutdown();
                return;
            }
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            if (IsRegistered() == false)
            {
                SoftRegView reg = new SoftRegView();
                reg.Show();
            }
            else
            {
                LoginView loginView = new LoginView();
                loginView.Show();
                loginView.IsVisibleChanged += (s, ev) =>
                {
                    if (loginView.IsVisible == false && loginView.IsLoaded)
                    {
                        MainFormView mainView = new MainFormView();
                        mainView.Show();
                        loginView.Close();
                    }
                };
            }
        }

        //全局异常捕获
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            DMMessageBox.Show("错误", "系统出现异常！请重试！", DMMessageType.MESSAGE_FAIL, DMMessageButton.Confirm);
            Console.WriteLine(e);
            e.Handled = true;
        }

        private bool IsRegistered()
        {
            // 检查验证状态
            object value = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mySoftWare\\Register.INI", "UserName", false);
            return value != null && value.Equals("Rsoft");
        }
    }
}
