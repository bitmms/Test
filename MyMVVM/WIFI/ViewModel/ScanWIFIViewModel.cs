using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Common.ViewModel;
using MyMVVM.WIFI.Model;
using MyMVVM.WIFI.Utils;

namespace MyMVVM.WIFI.ViewModel
{
    public class ScanWIFIViewModel : ViewModelsBase
    {
        private string _ruleDes;
        public bool IsAdd = false;
        private string _wifiFilterRule;
        private List<string> MacList = new List<string>();
        private ObservableCollection<WIFIModel> _wifiDateList;
        public string RuleDes { get => _ruleDes; set => SetProperty(ref _ruleDes, value); }
        public string WIFIFilterRule { get => _wifiFilterRule; set => SetProperty(ref _wifiFilterRule, value); }
        public ObservableCollection<WIFIModel> WIFIDateList { get => _wifiDateList; set => SetProperty(ref _wifiDateList, value); }

        /// <summary>
        /// 构造器，主窗口点击"扫描基站"按钮时，创建 ScanWIFIViewModel 对象，并通过构造器传递 "已存在的基站列表"
        /// </summary>
        public ScanWIFIViewModel(ObservableCollection<WIFIModel> aWIFIModelList)
        {
            WIFIFilterRule = WIFIDB.GetWIFIFilterRule();
            RuleDes = "格式为：XX:XX:XX:XX:XX:XX（可模糊匹配）";

            // 获取已经存在的 mac 地址，格式为 XX:XX:XX:XX:XX:XX
            foreach (WIFIModel wifiModel in aWIFIModelList)
            {
                MacList.Add(wifiModel.WIFIMAC);
            }

            // 开始扫描基站信息
            List<WIFIModel> ScanList = WIFIUtil.GetAllDevicesOnLAN();

            // 过滤已存在的基站
            ObservableCollection<WIFIModel> FilterList = new ObservableCollection<WIFIModel>();
            ScanList.ForEach(wifiModel =>
            {
                wifiModel.WIFIName = wifiModel.WIFIMAC.Substring(12, 2) + wifiModel.WIFIMAC.Substring(15, 2);
                if (!MacList.Contains(wifiModel.WIFIMAC))
                {
                    FilterList.Add(wifiModel);
                }
            });
            WIFIDateList = FilterList;
        }

        /// <summary>
        /// 确定按钮
        /// </summary>
        public ICommand ConfirmButtonCommand => new ViewModelCommand(param =>
        {
            // 插入数据库
            foreach (WIFIModel item in WIFIDateList)
            {
                WIFIDB.AddWIFIModel(item);
            }
            if (WIFIDateList.Count > 0)
            {
                IsAdd = true;
            }
            Window window = (Window)param;
            window.Close();
        });

        /// <summary>
        /// 取消按钮
        /// </summary>
        public ICommand CancelButtonCommand => new ViewModelCommand(param =>
        {
            Window window = (Window)param;
            window.Close();
        });

        /// <summary>
        /// 点击过滤基站
        /// </summary>
        public ICommand FilterWIFICommand => new ViewModelCommand(param =>
        {
            WIFIFilterRule = WIFIFilterRule.Trim().ToUpper().Replace("：", ":");
            if (!(DMUtil.IsMac(WIFIFilterRule) || WIFIFilterRule == ""))
            {
                DMMessageBox.ShowInfo("格式错误");
                return;
            }

            // 用户输入的规则为空字符串，此时不进行任何的筛选
            if (WIFIFilterRule == "")
            {
                // 1. 扫描全部的基站
                List<WIFIModel> ScanWIFIList = WIFIUtil.GetAllDevicesOnLAN();
                // 2. 排除已经存在的基站
                List<WIFIModel> tempList = new List<WIFIModel>();
                tempList = new List<WIFIModel>();
                foreach (WIFIModel wifiModel in ScanWIFIList)
                {
                    if (!MacList.Contains(wifiModel.WIFIMAC))
                    {
                        tempList.Add(wifiModel);
                    }
                }
                // 3. 初始化基站名称
                for (int i = tempList.Count - 1; i >= 0; i--)
                {
                    tempList[i].WIFIName = tempList[i].WIFIMAC.Substring(12, 2) + tempList[i].WIFIMAC.Substring(15, 2);
                }
                // 4. 规则筛选，并更新【用户输入为空字符串，此时不进行仅筛选出不存在的基站】
                WIFIDateList = new ObservableCollection<WIFIModel>(tempList);
                WIFIDB.UpdateWIFIRule(WIFIFilterRule.Trim().ToUpper());
            }

            // 用户输入的扫描规则
            else
            {
                // 1. 扫描全部的基站
                List<WIFIModel> ScanWIFIList = WIFIUtil.GetAllDevicesOnLAN();
                // 2. 排除已经存在的基站
                List<WIFIModel> tempList = new List<WIFIModel>();
                tempList = new List<WIFIModel>();
                foreach (WIFIModel wifiModel in ScanWIFIList)
                {
                    if (!MacList.Contains(wifiModel.WIFIMAC))
                    {
                        tempList.Add(wifiModel);
                    }
                }
                // 3. 初始化基站名称
                for (int i = tempList.Count - 1; i >= 0; i--)
                {
                    tempList[i].WIFIName = tempList[i].WIFIMAC.Substring(12, 2) + tempList[i].WIFIMAC.Substring(15, 2);
                }
                // 4. 规则扫描
                for (int i = tempList.Count - 1; i >= 0; i--)
                {
                    bool f = false;
                    foreach (var item in WIFIFilterRule.Split(','))
                    {
                        if (tempList[i].WIFIMAC.Contains(item))
                        {
                            f = true;
                            break;
                        }
                    }
                    if (!f)
                    {
                        tempList.RemoveAt(i);
                    }
                }

                // 5. 更新
                WIFIDateList = new ObservableCollection<WIFIModel>(tempList);
                WIFIDB.UpdateWIFIRule(WIFIFilterRule.Trim().ToUpper());
            }
        });
    }
}