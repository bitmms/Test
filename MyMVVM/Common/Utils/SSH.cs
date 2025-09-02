using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.Common.View;
using Renci.SshNet;

namespace MyMVVM.Common.Utils
{
    public class SSH
    {

        public static string ExecuteCommand(string command)
        {
            string host = DMVariable.SSHIP;
            string username = DMVariable.SSHUsername;
            string password = DMVariable.SSHPassword;

            using (var sshClient = new SshClient(host, username, password))
            {
                try
                {
                    sshClient.Connect();
                    if (!sshClient.IsConnected)
                    {
                        throw new InvalidOperationException("SSH client could not connect.");
                    }

                    string result1;
                    using (var cmd1 = sshClient.CreateCommand(command))
                    {
                        result1 = cmd1.Execute();
                    }

                    //string result2;
                    //using(var cmd2 = sshClient.CreateCommand(command2))
                    //{
                    //	result2 = cmd2.Execute();
                    //}
                    return result1;
                }
                catch (Exception ex)
                {
                    throw new Exception($"SSH command execution failed: {ex.Message}", ex);
                }
                finally
                {
                    if (sshClient.IsConnected)
                    {
                        sshClient.Disconnect();
                    }
                }
            }
        }


        public static void UploadFile(string localFilePath, string remoteFilePath)
        {
            string host = DMVariable.SSHIP;
            string username = DMVariable.SSHUsername;
            string password = DMVariable.SSHPassword;

            using (var sftp = new SftpClient(host, username, password))
            {
                try
                {
                    Console.WriteLine($"开始上传文件【{localFilePath}】-->【{remoteFilePath}】");
                    sftp.Connect();
                    if (!sftp.IsConnected)
                    {
                        throw new InvalidOperationException("SFTP client could not connect.");
                    }

                    string utf8RemoteFilePath = ToUtf8(remoteFilePath);

                    using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                    {
                        sftp.UploadFile(fileStream, utf8RemoteFilePath);
                    }

                    Console.WriteLine($"文件上传成功【{localFilePath}】-->【{remoteFilePath}】");
                }
                catch (Exception ex)
                {
                    DMMessageBox.ShowInfo($"文件上传失败: {ex.Message}");
                }
                finally
                {
                    if (sftp.IsConnected)
                    {
                        sftp.Disconnect();
                    }
                }
            }
        }


        public static void DownLoadFile(string remoteFilePath, string localFilePath)
        {
            string host = DMVariable.SSHIP;
            string username = DMVariable.SSHUsername;
            string password = DMVariable.SSHPassword;

            using (var sftp = new SftpClient(host, username, password))
            {
                try
                {
                    Console.WriteLine($"开始从服务器下载文件【{remoteFilePath}】");
                    sftp.Connect();
                    var bytes = sftp.ReadAllBytes(ToUtf8(remoteFilePath));
                    System.IO.File.WriteAllBytes(localFilePath, bytes);
                    Console.WriteLine($"从服务器下载文件成功【{remoteFilePath}】");
                }
                catch (Exception ex)
                {
                    throw new Exception($"文件同步出现异常: {ex.Message}", ex);
                }
                finally
                {
                    if (sftp.IsConnected)
                    {
                        sftp.Disconnect();
                    }
                }
            }
        }


        private static string ToUtf8(string path)
        {
            // 确保路径是使用 UTF-8 编码
            return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(path));
        }

    }
}
