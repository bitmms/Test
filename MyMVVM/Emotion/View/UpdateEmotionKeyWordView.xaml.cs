using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace MyMVVM.Emotion.View
{
    /// <summary>
    /// UpdateEmotionKeyWordView.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateEmotionKeyWordView : Window
    {
        public UpdateEmotionKeyWordView()
        {
            InitializeComponent();

            List<string> JsonsList = JsonConvert.DeserializeObject<List<string>>(EmotionAlarmDB.getKeyWord());
            StringBuilder sb = new StringBuilder();
            foreach (string json in JsonsList)
            {
                sb.Append(json).Append(",");
            }
            sb.Length--;
            keyword_text.Text = sb.ToString();
        }

        // 取消
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // 确定
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();
            string[] words = keyword_text.Text.Replace('，', ',').Replace(' ', ',').Split(',');
            foreach (var item in words)
            {
                if (item != "") list.Add(item);
            }
            string jsonStr = JsonConvert.SerializeObject(list);
            EmotionAlarmDB.UpdateKeyWord(jsonStr);
            this.Close();
        }
    }
}
