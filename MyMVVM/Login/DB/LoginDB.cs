using MyMVVM.Common;
using MyMVVM.Common.Utils;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyMVVM.Login
{
    public class LoginDB
    {

        public static int AuthLoginInfo(string username)
        {
            string sql = $"select count(*) from dm_login where login_user = '{username}'";
            DataTable dt = DB.ExecuteQuery2(sql);
            if (dt == null)
            {
                return -1;
            }
            DataRowCollection rows = dt.Rows;
            return int.Parse(rows[0]["count"].ToString());
        }


        public static int AuthLoginInfo(string username, string password)
        {
            string sql = $"select count(*) from dm_login where login_user = '{username}' and login_pwd = '{password}'";
            DataTable dt = DB.ExecuteQuery2(sql);
            if (dt == null)
            {
                return -1;
            }
            DataRowCollection rows = dt.Rows;
            return int.Parse(rows[0]["count"].ToString());
        }


        public static int AuthenticateByUsername(string username)
        {
            string sql = "select count(*) from dm_login where login_user = @username ";
            NpgsqlParameter param = new NpgsqlParameter("@username", username);
            return DB.ExecuteCountQuery(sql, param);
        }


        public static int AuthenticateByUsernameAndPassword(string username, string password)
        {
            string sql = "select count(*) from dm_login where login_user = @username and login_pwd = @password  ";
            NpgsqlParameter usernameParam = new NpgsqlParameter("username", username);
            NpgsqlParameter passwordParam = new NpgsqlParameter("password", password);
            return DB.ExecuteCountQuery(sql, usernameParam, passwordParam);
        }


        public static int AuthenticateByUsernamePasswordAndIp(string username, string password, string ip)
        {
            string sql = "select count(*) from dm_login where login_user = @username and login_pwd = @password and login_ip = @ip  ";
            NpgsqlParameter usernameParam = new NpgsqlParameter("username", username);
            NpgsqlParameter passwordParam = new NpgsqlParameter("password", password);
            NpgsqlParameter ipParam = new NpgsqlParameter("ip", ip);
            return DB.ExecuteCountQuery(sql, usernameParam, passwordParam, ipParam);
        }




        /// <summary>
        /// 验证到期时间，软件到期时返回true，可是正常使用返回false
        /// </summary>
        /// <returns></returns>
		public static bool AuthTime()
        {
            string sql = "select time from dm_time";
            DataTable dt = DB.ExecuteQuery(sql);
            string time = dt.Rows[0]["time"].ToString();
            DateTime date1 = DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string command = "timedatectl";
            string result = SSH.ExecuteCommand(command);
            Regex regex = new Regex(@"Local time: (\w{3} \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) CST");
            Match match = regex.Match(result);
            DateTime date2;
            if (match.Success)
            {
                string dateTimeString = match.Groups[1].Value;
                date2 = DateTime.ParseExact(dateTimeString, "ddd yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                if (date2 >= date1)
                {
                    return true;
                }
            }
            return false;
        }


    }
}

