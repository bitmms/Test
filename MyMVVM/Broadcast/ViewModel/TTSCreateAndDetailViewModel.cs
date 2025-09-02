using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MyMVVM.Common.ViewModel;
using System.Windows;
using MyMVVM.Common.View;
using MyMVVM.Common.Utils;
using Newtonsoft.Json;

namespace MyMVVM.Broadcast.ViewModel
{
    public class TTSCreateAndDetailViewModel : ViewModelsBase
    {
        private string _ttsText;

        public string TTSText
        {
            get => _ttsText;
            set => SetProperty(ref _ttsText, value);
        }

        private string _ttsName;

        public string TTSName
        {
            get => _ttsName;
            set => SetProperty(ref _ttsName, value);
        }

        public TTSCreateAndDetailViewModel()
        {
            TTSText = "";
            TTSName = "";
        }

        private async void Handler()
        {
            // 老版本劣质 TTS
            await Task.Run(async () =>
            {
                // 1. 参数
                var name = TTSName;
                var text = TTSText;

                // 2. TTS，文本转语音
                var timestamp = DMUtil.GetNowTimeStamp();
                var path = TTSDB.GetTTSPath() + timestamp + ".wav";
                SSH.ExecuteCommand($"ekho \"{text}\" -o {path}");
                var url = $"http://{DMVariable.SSHIP}:90" + TTSDB.GetTTSPath() + timestamp + ".wav";

                // 3. 判断是否生成，一直到生成成功，才之后下面的步骤
                while (true)
                {
                    WebRequest request = WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (Convert.ToInt32(response.StatusCode) == 200 && response.StatusCode.ToString() == "OK") // 生成成功
                        break;
                }

                // 4. 获取TTS语音时间长度
                var time = await MusicDuration(url);
                time = (time == "00:00") ? "00:01" : time;

                // 5. 生成数据表
                TTSDB.AddTTSText(name, text, time, path);
            });

            // 百度 TTS
            /*
            await Task.Run(async () =>
            {
                // 1. 参数
                var name = TTSName;
                var text = TTSText;
                var time = "0";
                var path = "";
                var filename = DMUtil.GetNowTimeStamp();

                // 2. 数据
                var dict = new Dictionary<string, string>
                {
                    { "content", text },
                    { "filename", filename }
                };

                // 3. 发 post 请求
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, $"http://{DMVariable.SSHIP}:{5000}/tts/v1");
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Content = new StringContent(JsonConvert.SerializeObject(dict), Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);

                // 4. 收响应
                path = await response.Content.ReadAsStringAsync();

                // 5. 获取TTS语音时间长度
                time = await MusicDuration($"http://{DMVariable.SSHIP}:90/usr/local/ttsVoice/" + filename + ".wav");
                time = (time == "00:00") ? "00:01" : time;

                // 6. 生成数据表
                TTSDB.AddTTSText(name, text, time, path);
            });
            */
        }

        //计算音乐时长
        private async Task<string> MusicDuration(string url)
        {
            string x = "";
            string tempFilePath = System.IO.Path.GetTempFileName(); // 创建一个临时文件来保存下载的音频  
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();
            // 检查响应的内容类型是否为音频文件（可选）  
            using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = System.IO.File.Create(tempFilePath))
            {
                await contentStream.CopyToAsync(fileStream);
            }

            // 现在使用 WaveFileReader 读取本地文件并计算时长  
            using (var waveFileReader = new WaveFileReader(tempFilePath))
            {
                TimeSpan duration = waveFileReader.TotalTime;
                x = $"{duration.Minutes:D2}:{duration.Seconds:D2}";
            }

            // 清理临时文件  
            System.IO.File.Delete(tempFilePath);

            response.Dispose();
            client.Dispose();

            return x;
        }

        // 确定按钮
        public ICommand ConfirmButtonCommand => new ViewModelCommand(param =>
        {
            if (TTSName == null || TTSName == "")
            {
                DMMessageBox.ShowInfo("请为该TTS文本语音设置一个名称");
                return;
            }

            if (TTSText == null || TTSText == "")
            {
                DMMessageBox.ShowInfo("请输入TTS文本");
                return;
            }

            if (TTSDB.IsExist(TTSName))
            {
                DMMessageBox.ShowInfo($"已经存在相同名称的TTS文件，请修改名称");
                return;
            }

            if (TTSText.Length >= 2000)
            {
                DMMessageBox.ShowInfo($"TTS文本最长为2000个字符，请修改文本");
                return;
            }

            // 处理特殊的空白字符串
            StringBuilder sb = new StringBuilder();
            foreach (char c in TTSText)
            {
                if (!char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }

            TTSText = sb.ToString();

            Handler();

            Window window = (Window)param;
            window.Close();
        });

        // 取消按钮
        public ICommand CancelButtonCommand => new ViewModelCommand(param =>
        {
            Window window = (Window)param;
            window.Close();
        });
    }
}