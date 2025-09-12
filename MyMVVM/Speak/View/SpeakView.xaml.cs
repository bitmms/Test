using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Speak.Model;
using MyMVVM.Speak.ViewModel;

namespace MyMVVM.Speak.View
{
    /// <summary>
    /// SpeakView.xaml 的交互逻辑
    /// </summary>
    public partial class SpeakView : UserControl
    {
        // 音频播放器实例
        private MediaPlayer _audioPlayer;
        private ListView _SpeakGroupListView;
        private ListView _MessageRecordListView;
        private TextBlock _SpeakMessageRecordListHeader;

        public SpeakView()
        {
            InitializeComponent();

            // 1. 控件
            _SpeakGroupListView = SpeakGroupListView;
            _SpeakMessageRecordListHeader = SpeakMessageRecordListHeader;
            _MessageRecordListView = MessageRecordListView;
            _audioPlayer = new MediaPlayer(); // 初始化播放器

            // 2. 定义数据源
            List<SpeakGroupVO> speakGroupVOs = SpeakGroupDB.GetMusicList();

            // 3. 绑定数据源
            _SpeakGroupListView.ItemsSource = speakGroupVOs;

            // 4. 选中某个分组的事件
            _SpeakGroupListView.SelectionChanged += SpeakGroupListView_SelectionChanged;
        }

        /// <summary>
        /// 点击切换当前被选中的分组
        /// </summary>
        private void SpeakGroupListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpeakGroupVO selectedItem = SpeakGroupListView.SelectedItem as SpeakGroupVO;
            if (selectedItem != null)
            {
                _SpeakMessageRecordListHeader.Text = selectedItem.groupOwnerNameAndCount;
                List<MessageVO> messages = SpeakGroupDB.GetMessageListByGroupId(selectedItem.groupId);
                _MessageRecordListView.ItemsSource = messages;

                // 滚动到最后
                {
                    if (_MessageRecordListView.Items.Count == 0) return;
                    // 获取最后一项
                    var lastItem = MessageRecordListView.Items[MessageRecordListView.Items.Count - 1];
                    // 滚动到最后一项（UI 线程安全）
                    MessageRecordListView.Dispatcher.Invoke(() =>
                    {
                        MessageRecordListView.ScrollIntoView(lastItem);
                        // 强制滚动到底部（解决某些情况下 ScrollIntoView 不生效的问题）
                        if (MessageRecordListView.ItemContainerGenerator.ContainerFromItem(lastItem) is ListViewItem item)
                        {
                            item.BringIntoView();
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 点击播放语音消息
        /// </summary>
        private void CLickToPlayAudioMessage(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // 1. 停止当前播放（如果有）并释放资源（实现“随时打断”）
                StopAndReleaseAudio();

                // 2. 获取当前音频URL（从数据对象中获取，替换硬编码示例）
                Grid grid = sender as Grid;
                MessageVO currentItem = grid?.DataContext as MessageVO;
                if (currentItem == null || string.IsNullOrEmpty(currentItem.messagePath))
                {
                    DMMessageBox.ShowInfo("异常");
                    return;
                }
                // string audioUrl = $"http://{DMVariable.SSHIP}:90{currentItem.messagePath}";

                // 3. 配置新的播放任务
                _audioPlayer = new MediaPlayer(); // 重新实例化（避免旧状态影响）
                _audioPlayer.MediaEnded += AudioPlayer_MediaEnded; // 监听播放结束事件
                _audioPlayer.Open(new Uri(currentItem.messagePath)); // 打开音频文件
                _audioPlayer.Play(); // 开始播放
            }
            catch (Exception ex)
            {
                DMMessageBox.ShowInfo("异常");
                StopAndReleaseAudio();
            }
            finally
            {
                // 标记事件已处理，防止冒泡到父控件
                e.Handled = true;
            }
        }

        /// <summary>
        /// 点击播放视频消息
        /// </summary>
        private void ClickToPlayOrPauseVideoMessage(object sender, RoutedEventArgs e)
        {
            try
            {
                Button border = sender as Button;
                MessageVO currentItem = border?.DataContext as MessageVO;
                if (currentItem == null || string.IsNullOrEmpty(currentItem.messagePath))
                {
                    DMMessageBox.ShowInfo("异常");
                    return;
                }
                new SpeakPlayVideoView(currentItem.messagePath.Replace("jpg", "mp4")).ShowDialog();

            }
            catch (Exception ex)
            {
                DMMessageBox.ShowInfo("异常");
            }
            finally
            {
                // 标记事件已处理，防止冒泡到父控件
                e.Handled = true;
            }
        }

        /// <summary>
        /// 播放结束时自动释放资源
        /// </summary>
        private void AudioPlayer_MediaEnded(object sender, EventArgs e)
        {
            Console.WriteLine("音频播放完毕，自动释放资源");
            StopAndReleaseAudio();
        }

        /// <summary>
        /// 停止播放并释放资源（统一处理逻辑）
        /// </summary>
        private void StopAndReleaseAudio()
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Stop(); // 停止播放
                _audioPlayer.MediaEnded -= AudioPlayer_MediaEnded; // 移除事件监听（避免内存泄漏）
                _audioPlayer.Close(); // 关闭流
                _audioPlayer = null; // 置空，方便下次重新实例化
            }
        }
    }
}
