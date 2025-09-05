using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.Common.Model;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using MyMVVM.MainWindow.Model;
using MyMVVM.Monitor.Model;
using Newtonsoft.Json;
using Npgsql;


namespace MyMVVM.Common
{
    public class CommonDB
    {
        /// <summary>
        /// 查询当前处于主机还是备机，返回字符串：主机、备机
        /// </summary>
        /// <returns></returns>
        public static string GetNowHostName()
        {
            string sql = "select name from dm_host_name where id = 1";
            DataTable dt = DB.ExecuteQuery(sql);
            return dt.Rows[0]["name"].ToString();
        }

        /// <summary>
        /// 查询IP地址是什么
        /// </summary>
        public static string GetHostIPByType(string type)
        {
            string sql = $"select join_ip from dm_server_join where join_type = '{type}'";
            DataTable dt = DB.ExecuteQuery(sql);
            return dt.Rows[0]["join_ip"].ToString();
        }


        #region 左右调度

        /// <summary>
        /// 查询数据库中存储的左右调度号码
        /// </summary>
        public static Dictionary<string, string> GetDispatchNum()
        {
            string sql = "select left_dispatch, right_dispatch from dm_dispatch_number where login_user = '" + DMVariable.NowLoginUserName + "'";
            DataTable dispatch_number_table = DB.ExecuteQuery(sql);
            Dictionary<string, string> dispatchNum = new Dictionary<string, string>();
            dispatchNum.Add("left", dispatch_number_table.Rows[0]["left_dispatch"].ToString());
            dispatchNum.Add("right", dispatch_number_table.Rows[0]["right_dispatch"].ToString());
            return dispatchNum;
        }


        /// <summary>
        /// 查询工具命令的号码
        /// </summary>
        public static Dictionary<string, string> GetFunctionNumber()
        {
            string sql = "select number, data, ring, misscall from dm_function where id = 1";
            DataTable table = DB.ExecuteQuery(sql);
            Dictionary<string, string> dispatchNum = new Dictionary<string, string>();
            dispatchNum.Add("number", table.Rows[0]["number"].ToString());
            dispatchNum.Add("date", table.Rows[0]["data"].ToString());
            dispatchNum.Add("ring", table.Rows[0]["ring"].ToString());
            dispatchNum.Add("misscall", table.Rows[0]["misscall"].ToString());
            return dispatchNum;
        }


        /// <summary>
        /// 验证指定的调度号的状态【1.正在通话中 2.正在向别人呼叫中 3.鉴权成功（属于正在向别人呼叫中）】
        /// </summary>
        public static bool GetDispatchNumStatus(string NowDispatchNum)
        {
            string sql = "select count(uuid) as cnt from channels where cid_num=@dispatch_num or dest = @dispatch_num";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@dispatch_num", NowDispatchNum) };
            DataTable dt = DB.ExecuteQuery(sql, parameters);
            if (dt == null || dt.Rows[0]["cnt"].ToString() == "0")
            {
                return false;
            }
            return true;
        }
        #endregion



        #region 用户分组展示


        /// <summary>
        /// 查询所有在线用户的号码
        /// </summary>
        public static List<String> GetOnlineUserNum()
        {
            List<string> onlineUserList = new List<string>();
            string query = "select sip_user from sip_registrations;";
            DataTable dt = DB.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                onlineUserList.Add(row["sip_user"].ToString());
            }
            return onlineUserList;
        }




        /// <summary>
        /// 查询全部组信息【根据分区查询分组】
        /// </summary>
        public static ObservableCollection<GroupModel> GetGroupListByType(string type)
        {
            ObservableCollection<GroupModel> groups = new ObservableCollection<GroupModel>();
            string query = $"select group_name, call_id, id from dm_group where group_type='{type}' and is_show=1 order by sort_id asc";
            DataTable dt = DB.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                GroupModel groupModel = new GroupModel
                {
                    Id = row["id"].ToString(),
                    CallId = row["call_id"].ToString(),
                    GroupName = row["group_name"].ToString()
                };
                ObservableCollection<DefaultUserModel> users = new ObservableCollection<DefaultUserModel>();
                GetUserListByGroupId(groupModel.Id, users);
                if (users.Count > 0)
                    groups.Add(groupModel);
            }
            return groups;
        }



        /// <summary>
        /// 查询指定组的全部用户【根据类别查询：座机、广播、手机、外线】
        /// </summary>
        public static void GetUserListByGroupId(string groupId, ObservableCollection<DefaultUserModel> users)
        {
            try
            {
                users.Clear();
                string _sql = $"select usernum, username, camera_ip, camera_port, camera_account, camera_password from get_users_all_info_by_group_id({groupId})";
                DataTable _dt = DB.ExecuteQuery(_sql);
                foreach (DataRow row in _dt.Rows)
                {
                    users.Add(new DefaultUserModel()
                    {
                        Username = row["username"].ToString(),
                        Usernum = row["usernum"].ToString(),
                        UserDisplay = row["usernum"].ToString(),
                        CameraIP = row["camera_ip"].ToString(),
                        CameraPort = row["camera_port"].ToString(),
                        CameraAccount = row["camera_account"].ToString(),
                        CameraPassword = row["camera_password"].ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                DMMessageBox.ShowInfo(ex.Message);
            }
        }

        #endregion



        #region 用户状态的变化：离线、在线、振铃、通话中

        /// <summary>
        /// 查询 detailed_calls 表
        /// </summary>

        public static DataTable GetDetailedCalls()
        {
            string query = "SELECT cid_num, cid_name, dest, application, callee_name, callee_num, callstate FROM detailed_calls";
            DataTable dt = DB.ExecuteQuery(query);
            return dt;
        }



        public static DataTable GetOnlineUser()
        {
            string query = "SELECT sip_user FROM sip_registrations ;";
            DataTable dt = DB.ExecuteQuery(query);
            return dt;
        }

        public static bool IsBFieldEmpty()
        {
            string query = "SELECT cid_num FROM detailed_calls WHERE cid_num IS NULL LIMIT 1";
            DataSet ds = DB.ExecuteQuery_dt(query);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion



        /// <summary>
        /// 查询指定类型的功能按钮数据：1属于话机，3属于广播
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> GetAllButtonsByType(int type)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            string sql = "select id, name, type, is_ok, mapping_name,icon from dm_buttons where type = " + type + " order by sort_id";
            DataTable dt = DB.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Dictionary<string, string>()
                {
                    {"Id", row["id"].ToString()},
                    {"Name", row["name"].ToString()},
                    {"IsOk", row["is_ok"].ToString()},
                    {"type", row["type"].ToString()},
                    {"Icon", row["icon"].ToString()},
                    {"MappingName", row["mapping_name"].ToString()},
                });
            }
            return list;
        }



        /// <summary>
        /// 将 已选择的用户 插入到数据表
        /// </summary>
        public static void AddSelectedUsers(ObservableCollection<string> selectedUser, string groupCallId)
        {
            string jsonString = JsonConvert.SerializeObject(selectedUser);
            string sql = $"update dm_group set group_members = @users where call_id = '{groupCallId}'";
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@users", jsonString)
            };
            DB.ExecuteNonQuery(sql, parameters);
        }


        /// <summary>
        /// 查询鉴权所需要的鉴权号码：也即动态的查询 233 号码
        /// </summary>
        public static string GetAuthNum()
        {
            string sql = "select auth from config where id = 1";
            DataTable dt = DB.ExecuteQuery(sql);
            return dt.Rows[0]["auth"].ToString();
        }

        /// <summary>
        /// 判断指定号码是否处于鉴权状态
        /// </summary>
        public static bool IsAuthingOfNumber(string number)
        {
            string sql = $"select exists(select * from channels where (cid_num = @dispatch_num and dest = (select auth from config where id = 1)))";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@dispatch_num", number) };
            object result = DB.ExecuteScalar(sql, parameters);
            return (bool)result;
        }


        /// <summary>
        /// 已知某个号码处于鉴权状态，查询其当前鉴权所产生的 UUID
        /// </summary>
        public static Dictionary<string, string> GetUUIDByAuthing(string number)
        {
            string sql = $"select cid_num, uuid from channels where dest = (select auth from config where id = 1) and cid_num = '{number}'";
            DataTable dt = DB.ExecuteQuery(sql);
            if (dt.Rows.Count <= 0)
            {
                return null;
            }
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            keyValues["uuid"] = dt.Rows[0]["uuid"].ToString();
            keyValues["cid_num"] = dt.Rows[0]["cid_num"].ToString();
            return keyValues;
        }



        /// <summary>
        /// 判断指定号码是否处于忙碌状态【仅考虑通话中，在这里鉴权不算忙碌】
        /// </summary>
        public static bool IsCallingByNumber(string dispatch_num)
        {
            string sql = "select exists(select * from channels where ((cid_num = @dispatch_num and dest != (select auth from config where id = 1)) or (dest = @dispatch_num)) and (dest != (select auth from config where id = 1)))";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@dispatch_num", dispatch_num) };
            object result = DB.ExecuteScalar(sql, parameters);
            return (bool)result;
        }





        /// <summary>
        /// 根据用户号码，查询对应的摄像仪数据
        /// </summary>
        public static Dictionary<string, string> getMonitorDataByNumber(string number)
        {
            string sql = $"select usernum, username, camera_ip, camera_account, camera_password, camera_port from dm_user where usernum = '{number}';";
            DataTable dispatch_number_table = DB.ExecuteQuery(sql);
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"ip", dispatch_number_table.Rows[0]["camera_ip"].ToString() },
                {"account", dispatch_number_table.Rows[0]["camera_account"].ToString() },
                {"password", dispatch_number_table.Rows[0]["camera_password"].ToString() },
                {"port", dispatch_number_table.Rows[0]["camera_port"].ToString() },
                {"sipNumber", dispatch_number_table.Rows[0]["usernum"].ToString() },
                {"sipName", dispatch_number_table.Rows[0]["username"].ToString() },
                {"type", "海康" }
            };
            return dict;
        }


        /// <summary>
        /// 查询所有与调度通话的号码
        /// </summary>
        /// <param name="leftNumber"></param>
        /// <param name="rightNumber"></param>
        /// <returns></returns>
        public static List<string> QueryAllCallingNumberByDispatchNumber(string leftNumber, string rightNumber)
        {
            List<string> list = new List<string>();
            string query = $"SELECT DISTINCT cid_num, dest from channels where cid_num = '{leftNumber}' or cid_num = '{rightNumber}' or dest = '{leftNumber}' or dest = '{rightNumber}'";
            DataTable dt = DB.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                string cid_num = row["cid_num"].ToString();
                string dest = row["dest"].ToString();
                if (!list.Contains(cid_num) && cid_num != leftNumber && cid_num != rightNumber && QueryNumberIsHasMonitor(cid_num) && !cid_num.Contains("*") && !cid_num.Contains("#"))
                {
                    list.Add(cid_num);
                }
                if (!list.Contains(dest) && dest != leftNumber && dest != rightNumber && QueryNumberIsHasMonitor(dest) && !dest.Contains("*") && !dest.Contains("#"))
                {
                    list.Add(dest);
                }
            }
            return list;
        }


        public static bool QueryNumberIsHasMonitor(string number)
        {
            string sql = $"select count(*) as count from dm_user where usernum = '{number}' and camera_ip IS NOT NULL;";
            DataTable dispatch_number_table = DB.ExecuteQuery(sql);
            return dispatch_number_table.Rows[0]["count"].ToString() != "0";
        }

    }
}
