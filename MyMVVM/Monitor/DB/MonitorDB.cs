using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using MyMVVM.Broadcast;
using MyMVVM.Common;
using MyMVVM.Monitor.Model;

namespace MyMVVM.Monitor
{
    public class MonitorDB
    {
        /// <summary>
        /// 查询总数
        /// </summary>
        public static int GetCount()
        {
            string query = $"select count(*) from dm_monitor;";
            return DB.ExecuteCountQuery(query);
        }

        public static int GetPageSize()
        {
            return 64;
        }

        public static int GetTotalPages()
        {
            int total = GetCount();
            int TotalPages = total / GetPageSize();
            TotalPages = TotalPages + ((total % GetPageSize() > 0) ? 1 : 0);
            return TotalPages;
        }

        /// <summary>
        /// 分页查询监控
        /// </summary>
        public static List<MonitorModel> GetAllMonitorByPage(int currentPage, int pageSize)
        {
            string query = $"select id, is_talk, type, name, ip, port, username, password from dm_monitor order by id limit {pageSize} offset {(currentPage - 1) * pageSize};";
            DataTable dt = DB.ExecuteQuery(query);
            List<MonitorModel> list = new List<MonitorModel>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new MonitorModel
                {
                    Id = int.Parse(dr["id"].ToString()),
                    Name = dr["name"].ToString(),
                    IP = dr["ip"].ToString(),
                    Port = int.Parse(dr["port"].ToString()),
                    Username = dr["username"].ToString(),
                    Password = dr["password"].ToString(),
                    IsShow = true,
                    Type = dr["type"].ToString(),
                    IsShowTalk = int.Parse(dr["is_talk"].ToString()) == 1,
                    IsNotShowTalk = int.Parse(dr["is_talk"].ToString()) == 0,
                });
            }
            return list;
        }
    }
}
