using MyMVVM.Broadcast.ViewModel;
using MyMVVM.Common;
using MyMVVM.Conference.ViewModel;
using MyMVVM.Dispatch.ViewModel;
using MyMVVM.Phone.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using MyMVVM.Common.ViewModel;
using MyMVVM.Monitor.ViewModel;
using MyMVVM.WIFI.ViewModel;
using MyMVVM.Map.ViewModel;
using MyMVVM.Battery.ViewModel;

namespace MyMVVM.Setting.ViewModel
{
    public class ThemeSettingViewModel : ViewModelsBase
    {
        private bool _radioButton1;
        private bool _radioButton2;
        private bool _radioButton3;
        public bool RadioButton1 { get { return _radioButton1; } set { SetProperty(ref _radioButton1, value); } }
        public bool RadioButton2 { get { return _radioButton2; } set { SetProperty(ref _radioButton2, value); } }
        public bool RadioButton3 { get { return _radioButton3; } set { SetProperty(ref _radioButton3, value); } }

        public ThemeSettingViewModel()
        {
            // 收集原来已有的资源字典
            List<ResourceDictionary> list = new List<ResourceDictionary>();
            foreach (ResourceDictionary resourceDictionary in Application.Current.Resources.MergedDictionaries)
            {
                list.Add(resourceDictionary);
            }

            // 找到要修改的资源字典【本项目把动态主题资源放到了资源字典的第一位】
            ResourceDictionary nowStyle = list[0];
            if (nowStyle.Source.ToString() == ThemeEnum.WarmTheme)
            {
                RadioButton1 = true;
                RadioButton2 = false;
                RadioButton3 = false;
            }
            else if (nowStyle.Source.ToString() == ThemeEnum.CoolTheme)
            {
                RadioButton1 = false;
                RadioButton2 = true;
                RadioButton3 = false;
            }
            else
            {
                RadioButton1 = false;
                RadioButton2 = false;
                RadioButton3 = true;
            }
        }

        // 暖
        public ICommand RadioButton_Click_1 => new ViewModelCommand(param =>
        {
            UpdateTheme(ThemeEnum.WarmTheme, "WarmTheme");
        });

        // 冷
        public ICommand RadioButton_Click_2 => new ViewModelCommand(param =>
        {
            UpdateTheme(ThemeEnum.CoolTheme, "CoolTheme");
        });

        //黑
        public ICommand RadioButton_Click_3 => new ViewModelCommand(param =>
        {
            UpdateTheme(ThemeEnum.BlackTheme, "BlackTheme");
        });

        public void UpdateTheme(string themeEnum, string themeName)
        {
            // 收集原来已有的资源字典
            List<ResourceDictionary> list = new List<ResourceDictionary>();
            foreach (ResourceDictionary resourceDictionary in Application.Current.Resources.MergedDictionaries)
            {
                list.Add(resourceDictionary);
            }

            // 找到要修改的资源字典【本项目把动态主题资源放到了资源字典的第一位】
            ResourceDictionary nowStyle = list[0];

            // 设置主题
            list[0] = new ResourceDictionary()
            {
                Source = new Uri(themeEnum),
            };

            // 清空资源字典【确保资源字典会重新加载】
            Application.Current.Resources.MergedDictionaries.Clear();

            // 更新资源，目的是更新主题到页面
            list.ForEach(item => Application.Current.Resources.MergedDictionaries.Add(item));

            // 保存本次修改
            Properties.Settings.Default.ThemeColors = themeName;
            Properties.Settings.Default.Save();
        }
    }
}