using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Common.Utils
{
    public class FSSocket
    {
        private static Socket socket;
        private static string ip = DMVariable.SSHIP;
        private static int port = 8021;
        private static string password = "ClueCon";

        // 使用 static 修饰构造器，实现一个静态加载的代码块，全局保持一个 Socket 对象
        static FSSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ip, port);
            ReciveResponseData();
            SendCommand($"auth {password}");
        }

        /// <summary>
        /// 接收响应体的数据
        /// </summary>
        private static string ReciveResponseData()
        {
            string responseHead = "";
            string responseBody = "";

            // 接收响应头
            while (!responseHead.EndsWith("\n\n")) // 使用一个循环来不断从套接字接收数据，直到累积的数据以两个换行符("\n\n")结尾  
            {
                byte[] buffer = new byte[100000]; // 为每次接收操作分配一个新的字节数组缓冲区
                int bytesReceived = socket.Receive(buffer); // 从套接字接收数据到缓冲区中，并返回实际接收到的字节数  
                string data = Encoding.UTF8.GetString(buffer, 0, bytesReceived); // 使用UTF8编码将接收到的字节数据转换为字符串  
                responseHead += data; // 将转换后的字符串追加到dataBuffer中  
            }

            // 判断是否有响应体
            int contentLenght = -1;
            if (responseHead.Contains("Content-Length:"))
            {
                foreach (string line in responseHead.Split('\n')) // 遍历每一行
                {
                    if (line.StartsWith($"Content-Length:")) // 检查当前行是否以指定的键名开头
                    {
                        string value = line.Substring($"Content-Length:".Length).Trim(); // 如果是，则截取键名之后的部分作为值，并去除值前后的空白字符
                        contentLenght = int.Parse(value);
                        break;
                    }
                }
            }

            // 接收响应体
            if (contentLenght >= 0)
            {
                while (responseBody.Length < contentLenght)
                {
                    byte[] buffer = new byte[10000];
                    int bytesReceived = socket.Receive(buffer);
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    responseBody += data;
                }
            }

            return responseHead + responseBody;
        }

        /// <summary>
        /// 发送 FS 命令
        /// </summary>
        public static string SendCommand(string command)
        {
            byte[] msg = Encoding.UTF8.GetBytes(command + "\n\n");
            socket.Send(msg);
            return ReciveResponseData();
        }
    }
}
