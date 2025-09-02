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

            // UI 主线程未捕获异常
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            // 非UI线程未捕获异常（后台线程）
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // Task 异步异常
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        /// UI 主线程异常
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // DMMessageBox.Show("错误", "系统出现异常！请重试！", DMMessageType.MESSAGE_FAIL, DMMessageButton.Confirm);
            Console.WriteLine("UI线程异常: " + e.Exception);
            e.Handled = true;
        }
        /// 非UI线程异常（后台线程）
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 如果需要弹窗提示，可以用 Dispatcher.Invoke 切回UI线程
            // this.Dispatcher.Invoke(() => DMMessageBox.Show("错误", "后台线程出现异常！", DMMessageType.MESSAGE_FAIL, DMMessageButton.Confirm));
            Console.WriteLine("非UI线程异常: " + e.ExceptionObject);
            
        }
        /// Task 异常
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // DMMessageBox.Show("错误", "系统出现异常！请重试！", DMMessageType.MESSAGE_FAIL, DMMessageButton.Confirm);
            Console.WriteLine("Task异常: " + e.Exception);
            e.SetObserved();
        }

        private bool IsRegistered()
        {
            // 检查验证状态
            object value = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mySoftWare\\Register.INI", "UserName", false);
            return value != null && value.Equals("Rsoft");
        }
    }
}
