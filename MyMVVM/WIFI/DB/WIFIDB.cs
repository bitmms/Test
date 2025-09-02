using MyMVVM.Common;
using MyMVVM.WIFI.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.WIFI
{
    public class WIFIDB
    {

        /// <summary>
        /// 条件查询数量
        /// </summary>
        public static int GetMusicCountBySearch(string searchText)
        {
            string query = $"" +
                $"select count(id) " +
                $"from dm_wifi " +
                $"where name like '%{searchText}%' or ip_address::text like '%{searchText}%' or mac_address like '%{searchText}%'";
            return DB.ExecuteCountQuery(query);
        }

        /// <summary>
        /// 根据名称搜索数据库中的基站信息
        /// </summary>
        public static ObservableCollection<WIFIModel> SearchWifiList(string searchText, int currentPage, int pageSize)
        {
            List<WIFIModel> tempList = new List<WIFIModel>();
            string query = $"" +
                $"select id, ip_address, mac_address, name " +
                $"from dm_wifi " +
                $"where name like '%{searchText}%' or ip_address::text like '%{searchText}%' or mac_address like '%{searchText}%' " +
                $"order by ip_address " +
                $"limit {pageSize} offset {(currentPage - 1) * pageSize}";
            DataTable dt = DB.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                tempList.Add(new WIFIModel
                {
                    Id = (int)row["id"],
                    WIFIName = row["name"].ToString(),
                    WIFIIP = row["ip_address"].ToString(),
                    WIFIMAC = row["mac_address"].ToString(),
                });
            }
            return new ObservableCollection<WIFIModel>(tempList);
        }






        /// <summary>
        /// 查询数量
        /// </summary>
        public static int GetMusicCount()
        {
            string query = $"select count(*) from dm_wifi;";
            return DB.ExecuteCountQuery(query);
        }



        /// <summary>
        /// 插入一条基站信息到数据库
        /// </summary>
        public static void AddWIFIModel(WIFIModel model)
        {
            string sql = $"INSERT INTO dm_wifi(ip_address, mac_address, name) VALUES ('{model.WIFIIP}', '{model.WIFIMAC}', '{model.WIFIName}')";
            DB.ExecuteNonQuery(sql);
        }


        /// <summary>
        /// 修改一条基站记录的基站名称字段
        /// </summary>
        public static void UpdateWIFIModelName(WIFIModel model)
        {
            string sql = $"update dm_wifi set name = '{model.WIFIName}' where id='{model.Id}'";
            DB.ExecuteNonQuery(sql);
        }


        /// <summary>
        /// 删除一条基站记录
        /// </summary>
        public static void DeleteWIFIModelName(WIFIModel model)
        {
            string sql = $"delete from dm_wifi where id='{model.Id}'";
            DB.ExecuteNonQuery(sql);
        }


        /// <summary>
        /// 分页查
        /// </summary>
        public static ObservableCollection<WIFIModel> GetWifiList(int currentPage, int pageSize)
        {
            List<WIFIModel> tempList = new List<WIFIModel>();
            string query = $"" +
                $"select id,ip_address, mac_address,name " +
                $"from dm_wifi " +
                $"order by ip_address " +
                $"limit {pageSize} offset {(currentPage - 1) * pageSize};";
            DataTable dt = DB.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                tempList.Add(new WIFIModel
                {
                    Id = (int)row["id"],
                    WIFIName = row["name"].ToString(),
                    WIFIIP = row["ip_address"].ToString(),
                    WIFIMAC = row["mac_address"].ToString(),
                });
            }
            return new ObservableCollection<WIFIModel>(tempList);
        }

        /// <summary>
        /// 查全部
        /// </summary>
        public static ObservableCollection<WIFIModel> GetWifiList()
        {
            List<WIFIModel> tempList = new List<WIFIModel>();
            string query = $"select id,ip_address, mac_address,name from dm_wifi order by ip_address";
            DataTable dt = DB.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                tempList.Add(new WIFIModel
                {
                    Id = (int)row["id"],
                    WIFIName = row["name"].ToString(),
                    WIFIIP = row["ip_address"].ToString(),
                    WIFIMAC = row["mac_address"].ToString(),
                });
            }
            return new ObservableCollection<WIFIModel>(tempList);
        }


        /// <summary>
        /// 查询基站过滤规则
        /// </summary>
        public static string GetWIFIFilterRule()
        {
            string query = $"select wifi_filter_rule from dm_client_config where id = 1";
            DataTable dt = DB.ExecuteQuery(query);

            return dt.Rows[0]["wifi_filter_rule"].ToString();
        }


        /// <summary>
        /// 更新基站过滤规则
        /// </summary>
        public static void UpdateWIFIRule(string rule)
        {
            string query = $"update dm_client_config set wifi_filter_rule='{rule}' where id = 1";
            DB.ExecuteNonQuery(query);
        }

    }
}