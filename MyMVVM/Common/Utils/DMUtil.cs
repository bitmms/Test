using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using MyMVVM.Common.Model;
using MyMVVM.Dispatch.Model;

namespace MyMVVM.Common.Utils
{
    public class DMUtil
    {
        /// <summary>
        /// 更新用户通话中时显示的文本内容
        /// </summary>
        public static string SetOneUserButtonDisplayText(string fromNumber, string toNumber)
        {
            if (toNumber == null || toNumber == "") return fromNumber;

            Dictionary<string, string> dict = CommonDB.GetFunctionNumber();
            if (toNumber.Equals(dict["number"]))
            {
                return "查询本机号码";
            }
            else if (toNumber.Equals(dict["date"]))
            {
                return "查询当前时间";
            }
            else if (toNumber.Equals(dict["ring"]))
            {
                return "自振铃中";
            }
            else if (toNumber.Equals(dict["misscall"]))
            {
                return "查询未接来电";
            }
            else if (toNumber.Equals(dict["lastcall"]))
            {
                return "查询最后通话";
            }
            else if (toNumber.Contains("000000"))
            {
                return "通话中";
            }
            return $"{fromNumber} -> {toNumber}";
        }






        // 判断一个号码是否存在 List<GatewayAlarmRecordModel> 列表中
        public static bool IsFailDevice(List<GatewayAlarmRecordModel> list, string number)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].telno == number) return true;
            }
            return false;
        }


        public static bool PingNetworkDevice(string IP)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            PingReply reply = pingSender.Send(IP, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// 判断字符串是不是点分十进制的IP地址
        /// </summary>
        public static bool IsIP(string str)
        {
            if (str == null || str == "")
            {
                return false;
            }

            Regex rx = new Regex(@"^((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}$");

            if (rx.IsMatch(str))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 复制一个列表的元素到另一个列表，保证两个列表的元素相同，但并不是一个对象
        /// </summary>
        public static ObservableCollection<string> CopyObservableCollection(ObservableCollection<string> source)
        {
            if (source == null)
            {
                return new ObservableCollection<string>();
            }
            ObservableCollection<string> list = new ObservableCollection<string>();
            foreach (var item in source)
            {
                list.Add(item);
            }
            return list;
        }



        /// <summary>
        /// 获取当前时间的字符串
        ///     格式为 yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string GetNowDateTimeString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }



        /// <summary>
        /// 获取当前时间的时间戳
        /// </summary>
        public static string GetNowTimeStamp()
        {
            DateTime nowDateTime = DateTime.Now;
            return new DateTimeOffset(new DateTime(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day, nowDateTime.Hour, nowDateTime.Hour, nowDateTime.Second, DateTimeKind.Utc)).ToUnixTimeSeconds().ToString();
        }



        /// <summary>
        /// 获取指定时间的字符串
        ///     - 格式为 yyyy-MM-dd HH:mm:ss
        ///     - 如果传值为 null，返回当前时间的字符串
        /// </summary>
        public static string GetDateTimeString(DateTime dateTime)
        {
            if (dateTime == null)
            {
                return GetNowDateTimeString();
            }
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }



        /// <summary>
        /// 获取指定时间的时间戳
        /// </summary>
        public static string GetTimeStamp(DateTime nowDateTime)
        {
            return new DateTimeOffset(new DateTime(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day, nowDateTime.Hour, nowDateTime.Hour, nowDateTime.Second, DateTimeKind.Utc)).ToUnixTimeSeconds().ToString();
        }

        /// <summary>
        /// 获取指定格式的字符串类型的时间的时间戳
        /// </summary>
        public static string GetTimeStamp(string str, string parse = "yyyy-MM-dd HH:mm:ss")
        {
            DateTime nowDateTime = DateTime.ParseExact(str, parse, System.Globalization.CultureInfo.CurrentCulture);
            return new DateTimeOffset(new DateTime(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day, nowDateTime.Hour, nowDateTime.Hour, nowDateTime.Second, DateTimeKind.Utc)).ToUnixTimeSeconds().ToString();
        }


        /// <summary>
        /// 将指定格式的字符串转换成 DateTime
        /// </summary>
        public static DateTime GetDateTimeByString(string dateString, string parse)
        {
            return DateTime.ParseExact(dateString, parse, System.Globalization.CultureInfo.CurrentCulture);
        }



        /// <summary>
        /// 获取两个时间直接的间隔
        ///     - 传入的时间字符串格式为 "yyyy-MM-dd HH:mm:ss"
        ///     - 返回 XX小时YY分钟
        /// </summary>
        public static string GetTimeSpan(string _begin, string _end)
        {
            DateTime end = GetDateTimeByString(_end, "yyyy-MM-dd HH:mm:ss");
            DateTime beg = GetDateTimeByString(_begin, "yyyy-MM-dd HH:mm:ss");
            string second = (end - beg).TotalSeconds.ToString();
            int totalSeconds = int.Parse(second);
            int hours = totalSeconds / (60 * 60);
            int minutes = (totalSeconds % (60 * 60)) / 60;
            return $"{hours}小时{minutes}分钟";
        }





        /// <summary>
        /// 判断一个字符串是不是整数
        /// </summary>
        public static bool IsNumber(string a)
        {
            Regex regex = new Regex(@"^[0-9]\d*$");
            return regex.IsMatch(a);
        }


        /// <summary>
        /// 判断一个字符串是不是MAC地址，或者是不是mac地址的部分内容
        /// </summary>
        public static bool IsMac(string mac)
        {
            for (int i = 0; i < mac.Length; i++)
            {
                char c = mac[i];

                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || c == ':'))
                {
                    return false;
                }
            }
            return true;
        }






        /// <summary>
        /// 将日期时间字符串 转换为 yyyy-MM-dd HH:mm:ss 格式的字符串
        /// </summary>
        public static string TransformDateTimeString(string time, string p1, string p2)
        {
            // 原本就格式为：yyyy-MM-dd HH:mm:ss
            if (time.Contains("-"))
            {
                return time;
            }
            // 原本格式为：M/d/yyyy h:mm:ss tt
            DateTime datetime1 = DateTime.ParseExact(time, p1, CultureInfo.InvariantCulture);
            string datetime2 = datetime1.ToString(p2, CultureInfo.InvariantCulture);
            return datetime2;
        }


        /// <summary>
        /// 将日期时间字符串 转换为 HH:mm:ss 格式的字符串
        /// </summary>
        public static string TransformTimeString(string time, string p1, string p2)
        {
            // 原本格式为：M/d/yyyy h:mm:ss tt
            if (time.Contains("/"))
            {
                DateTime datetime1 = DateTime.ParseExact(time, p1, CultureInfo.InvariantCulture);
                string datetime2 = datetime1.ToString(p2, CultureInfo.InvariantCulture);
                return datetime2;
            }
            // 原本格式为：yyyy-MM-dd HH:mm:ss
            else
            {
                DateTime datetime1 = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                string datetime2 = datetime1.ToString(p2, CultureInfo.InvariantCulture);
                return datetime2;
            }
        }

        /// <summary>
        /// 颜色转换
        /// </summary>
        public static string ColorToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

    }
}

