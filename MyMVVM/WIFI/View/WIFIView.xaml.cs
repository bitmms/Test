using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Runtime.InteropServices;
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
using MyMVVM.WIFI.ViewModel;

namespace MyMVVM.WIFI.View
{
    /// <summary>
    /// WIFIView.xaml 的交互逻辑
    /// </summary>
    public partial class WIFIView : UserControl
    {

        public class NetworkDevice
        {
            public string IPAddress { get; set; }
            public string MacAddress { get; set; }
        }

        public WIFIView()
        {
            InitializeComponent();
            this.DataContext = new WIFIViewModel();
        }

        private void LoadNetworkDevices()
        {
            // Get all devices on network
            Dictionary<IPAddress, PhysicalAddress> allDevices = GetAllDevicesOnLAN();
            List<NetworkDevice> devices = new List<NetworkDevice>();

            foreach (var kvp in allDevices)
            {
                var ipAddress = kvp.Key.ToString();
                var macAddress = kvp.Value.ToString();

                // Add to the list to display in DataGrid
                devices.Add(new NetworkDevice { IPAddress = ipAddress, MacAddress = macAddress });

                // Prepare SQL query for inserting into the wifi table
                //string sqlQuery = $"INSERT INTO dm_wifi (ip_address, mac_address) VALUES ('{ipAddress}', '{macAddress}')";

                //// Execute the query using the existing ExecuteNonQuery method
                //int result = DB.ExecuteNonQuery(sqlQuery);

                //// Optionally handle the result if necessary (e.g., check if insertion was successful)
                //if (result > 0)
                //{
                //	Console.WriteLine($"Successfully inserted {ipAddress} and {macAddress} into the database.");
                //}
                //else
                //{
                //	Console.WriteLine($"Failed to insert {ipAddress} and {macAddress} into the database.");
                //}
            }

            //DevicesDataGrid.ItemsSource = devices;
        }


        /// <summary>
        /// 获取局域网内所有已知设备的IP和MAC地址
        /// </summary>
        /// <remarks>
        /// 1. 该表不经常更新 - 它可能需要一些人性化的时间注意到设备已断开网络或新设备 已连接
        /// 2. 如果发现非本地设备，则会将其丢弃 - 这些是多播的并且可以按IP地址范围丢弃
        /// </remarks>
        /// <returns></returns>
        private static Dictionary<IPAddress, PhysicalAddress> GetAllDevicesOnLAN()
        {

            Dictionary<IPAddress, PhysicalAddress> all = new Dictionary<IPAddress, PhysicalAddress>();

            // 添加本地 ip、mac 到 list
            all.Add(GetIPAddress(), GetMacAddress());

            int spaceForNetTable = 0;

            // 获取需要的空间
            // 我们通过请求表格来做到这一点，但根本不提供任何空间。
            // 返回值将告诉我们实际需要多少。
            GetIpNetTable(IntPtr.Zero, ref spaceForNetTable, false);


            // 分配空间
            // 我们使用 try-finally 块来确保释放。
            IntPtr rawTable = IntPtr.Zero;

            try
            {
                rawTable = Marshal.AllocCoTaskMem(spaceForNetTable);

                // 获取实际数据
                int errorCode = GetIpNetTable(rawTable, ref spaceForNetTable, false);
                if (errorCode != 0)
                {
                    throw new Exception($"Unable to retrieve network table. Error code {errorCode}");
                }

                int rowsCount = Marshal.ReadInt32(rawTable);
                IntPtr currentBuffer = new IntPtr(rawTable.ToInt64() + Marshal.SizeOf(typeof(Int32)));


                // 将原始表转换为单独的条目
                MIB_IPNETROW[] rows = new MIB_IPNETROW[rowsCount];

                for (int index = 0; index < rowsCount; index++)
                {
                    rows[index] = (MIB_IPNETROW)Marshal.PtrToStructure(
                        new IntPtr(currentBuffer.ToInt64() + (index * Marshal.SizeOf(typeof(MIB_IPNETROW)))),
                        typeof(MIB_IPNETROW));
                }

                // 定义虚拟条目列表（我们可以丢弃这些）
                PhysicalAddress virtualMAC = new PhysicalAddress(new byte[] { 0, 0, 0, 0, 0, 0 });
                PhysicalAddress broadcastMAC = new PhysicalAddress(new byte[] { 255, 255, 255, 255, 255, 255 });

                foreach (MIB_IPNETROW row in rows)
                {
                    IPAddress ip = new IPAddress(BitConverter.GetBytes(row.dwAddr));
                    byte[] rawMAC = new byte[] { row.mac0, row.mac1, row.mac2, row.mac3, row.mac4, row.mac5 };
                    PhysicalAddress pa = new PhysicalAddress(rawMAC);

                    if (!pa.Equals(virtualMAC) && !pa.Equals(broadcastMAC) && !IsMulticast(ip))
                    {
                        if (!all.ContainsKey(ip))
                        {
                            all.Add(ip, pa);
                        }
                    }
                }
            }
            finally
            {
                //释放内存
                Marshal.FreeCoTaskMem(rawTable);
            }
            return all;
        }


        /// <summary>
        /// 获取当前PC的IP地址
        /// </summary>
        /// <returns></returns>
        private static IPAddress GetIPAddress()
        {
            String strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            foreach (IPAddress ip in addr)
            {
                if (!ip.IsIPv6LinkLocal)
                {
                    return ip;
                }
            }
            return addr.Length > 0 ? addr[0] : null;
        }


        /// <summary>
        /// 获取当前PC的MAC地址
        /// </summary>
        /// <returns></returns>
        private static PhysicalAddress GetMacAddress()
        {
            foreach (NetworkInterface network in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (network.NetworkInterfaceType == NetworkInterfaceType.Ethernet && network.OperationalStatus == OperationalStatus.Up)
                {
                    return network.GetPhysicalAddress();
                }
            }
            return null;
        }


        /// <summary>
        /// 如果指定的 IP 地址是多播地址,则返回 true
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private static bool IsMulticast(IPAddress ip)
        {
            if (!ip.IsIPv6Multicast)
            {
                byte highIP = ip.GetAddressBytes()[0];
                return highIP >= 224 && highIP <= 239;
            }
            return true;
        }


        /// <summary>
        /// GetIpNetTable 返回的 MIB_IPNETROW 结构
        /// 不要修改此结构。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_IPNETROW
        {
            public int dwIndex;
            public int dwPhysAddrLen;
            public byte mac0;
            public byte mac1;
            public byte mac2;
            public byte mac3;
            public byte mac4;
            public byte mac5;
            public byte mac6;
            public byte mac7;
            public int dwAddr;
            public int dwType;
        }


        /// <summary>
        /// GetIpNetTable 外部方法
        /// </summary>
        [DllImport("IpHlpApi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int GetIpNetTable(IntPtr pIpNetTable, ref int pdwSize, bool bOrder);
        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            //string url = "http://192.168.11.66/";
            //try
            //{
            //	Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            //}
            //catch (Exception ex)
            //{
            //	// 处理可能的异常
            //	MessageBox.Show($"无法打开网址: {ex.Message}");
            //}

            //string url = "http://192.168.11.66/";
            //try
            //{
            //	webBrowser.Navigate(new Uri(url));
            //}
            //catch (Exception ex)
            //{
            //	MessageBox.Show($"无法打开网址: {ex.Message}");
            //}

        }
    }
}

