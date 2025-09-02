using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using Microsoft.Win32;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.Login.View;
using MyMVVM.MainWindow.View;
using MyMVVM.SoftReg.Utils;

namespace MyMVVM.SoftReg.View
{
    /// <summary>
    /// SoftReg.xaml 的交互逻辑
    /// </summary>
    public partial class SoftRegView : Window
    {
        public SoftRegView()
        {
            InitializeComponent();
            LoadMachineCode();
        }

        SoftRegUtil _softReg = new SoftRegUtil();

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void LoadMachineCode()
        {
            machineCode.Text = _softReg.GetMNum();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {

                    Filter = "Encrypted files (*.enc)|*.enc|All files (*.*)|*.*"
                };

                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    string filePath = openFileDialog.FileName;

                    byte[] encryptedData = File.ReadAllBytes(filePath);

                    //string registerCodeFromFile = DecryptStringFromBytes_Aes(encryptedData, "password123");
                    string registerCodeFromFile = AES.Decrypt(Encoding.ASCII.GetString(encryptedData), DMConfig.KEY);

                    if (registerCodeFromFile.Trim() == _softReg.GetRNum())
                    {
                        DMMessageBox.Show("通知", "注册成功", DMMessageType.MESSAGE_SUCCESS);
                        RegistryKey retkey = Registry.CurrentUser.OpenSubKey("Software", true)
                                                        .CreateSubKey("mySoftWare")
                                                        .CreateSubKey("Register.INI")
                                                        .CreateSubKey(machineCode.Text);
                        RegistryKey retkey2 = Registry.CurrentUser.OpenSubKey("Software", true)
                                                        .CreateSubKey("mySoftWare")
                                                        .CreateSubKey("Register.INI");
                        retkey2.SetValue("UserName", "Rsoft");

                        LoginView loginView = new LoginView();
                        loginView.Show();
                        Close();
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
                    else
                    {
                        DMMessageBox.Show("警告", "注册码错误,请联系管理员", DMMessageType.MESSAGE_FAIL);
                        machineCode.SelectAll();
                    }
                }
            }
            catch (Exception ex)
            {
                DMMessageBox.Show("错误", $"发生异常: {ex.Message}", DMMessageType.MESSAGE_FAIL);
            }


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            try
            {
                string u1 = username.Text.ToString();
                string u2 = usernum.Text.ToString();
                string u3 = principal.Text.ToString();
                string u4 = applicant.Text.ToString();
                string u5 = date.Text.ToString();

                if (string.IsNullOrEmpty(u1))
                {
                    DMMessageBox.Show("警告", "用户名称不能为空", DMMessageType.MESSAGE_WARING);
                    return;
                }
                if (string.IsNullOrEmpty(u2))
                {
                    DMMessageBox.Show("警告", "负责人不能为空", DMMessageType.MESSAGE_WARING);
                    return;
                }
                if (string.IsNullOrEmpty(u3))
                {
                    DMMessageBox.Show("警告", "负责人电话不能为空", DMMessageType.MESSAGE_WARING);
                    return;
                }
                if (string.IsNullOrEmpty(u4))
                {
                    DMMessageBox.Show("警告", "申请人不能为空", DMMessageType.MESSAGE_WARING);
                    return;
                }
                if (string.IsNullOrEmpty(u5))
                {
                    DMMessageBox.Show("警告", "申请时间不能为空", DMMessageType.MESSAGE_WARING);
                    return;
                }
                else
                {
                    string content = machineCode.Text;


                    //加密内容
                    //byte[] encryptedData = EncryptStringToBytes_Aes(content, "password123");
                    string encryptedData = AES.Encrypt(content, DMConfig.KEY);

                    //获取桌面路径
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    string filename = $"{u1}_{u2}_{u3}_{u4}_{u5}.enc";

                    string filePath = System.IO.Path.Combine(desktopPath, filename);


                    //将加密后的数据写入文件
                    File.WriteAllBytes(filePath, Encoding.Default.GetBytes(encryptedData));


                    DMMessageBox.Show("提示", "机器码已成功导出!", DMMessageType.MESSAGE_SUCCESS);
                }
            }
            catch (Exception ex)
            {
                DMMessageBox.Show("错误", "导出过程中发生错误:" + ex.Message, DMMessageType.MESSAGE_FAIL);
            }

        }

        static byte[] EncryptStringToBytes_Aes(string plainText, string key)
        {
            byte[] Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            byte[] IV = Encoding.UTF8.GetBytes("1234567890123456");
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, string key)
        {
            byte[] Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            byte[] IV = Encoding.UTF8.GetBytes("1234567890123456");
            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}
