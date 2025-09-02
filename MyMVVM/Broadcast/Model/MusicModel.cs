using MyMVVM.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyMVVM.Broadcast
{
    public class MusicModel : ViewModelsBase
    {

        public int Id { get; set; }

        // 音乐文件名称
        public string Name { get; set; }

        // 音乐时长
        public string Time { get; set; }

        // 当前音乐文件上传之前在本地的路径
        public string LocalPath { get; set; }

        // 上传路径：远程服务器路径
        public string UploadRemotePath { get; set; }

        // 上传路径：本地主机路径
        public string UploadLocalPath { get; set; }


        private ICommand _DeleteMusicCommand;
        public ICommand DeleteMusicCommand { get => _DeleteMusicCommand; set => SetProperty(ref _DeleteMusicCommand, value); }
    }
}
