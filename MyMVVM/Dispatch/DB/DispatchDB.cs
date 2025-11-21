using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyMVVM.Common;
using MyMVVM.Common.Model;
using MyMVVM.Common.Utils;
using MyMVVM.Dispatch.Model;
using MyMVVM.MainWindow.Model;
using Npgsql;

namespace MyMVVM.Dispatch
{
    public class DispatchDB
    {


        /// <summary>
        /// 通话记录
        /// </summary>
        public static List<GatewayAlarmRecordModel> GetGatewayAlarmRecorList()
        {
            List<GatewayAlarmRecordModel> list = new List<GatewayAlarmRecordModel>();
            string query = "select id, telno, line_state, line_length, termination_type from dm_gateway_line_diagnosis;";
            DataTable dt = DB.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                var item = new GatewayAlarmRecordModel
                {
                    id = int.Parse(row["id"].ToString()),
                    telno = row["telno"].ToString(),
                    lineState = row["line_state"].ToString(),
                    lineLength = row["line_length"].ToString(),
                    terminationType = row["termination_type"].ToString(),
                };
                if (item.lineLength == null || item.lineLength == "") item.lineLength = "";
                if ("0 m".Equals(item.lineLength) || item.lineLength == "") item.lineLength = "\\";





                if (item.lineState.Contains("AB all break off or no phone connceted")) item.lineState = "未接设备";
                else if (item.lineState.Contains("A fault to ground")) item.lineState = "接地故障";
                else
                {
                    item.lineState = "未知异常";
                }

                list.Add(item);
            }
            list.Sort((p1, p2) =>
            {
                return p1.telno.CompareTo(p2.telno);
            });
            return list;
        }

        /// <summary>
        /// 通话记录
        /// </summary>
        public static List<DefaultUserModel> GetCallingUser(string queryNowNumber, string queryNowTime, string queryNowMissCall)
        {
            var queryMappings = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(queryNowNumber))
            {
                queryMappings[queryNowNumber] = "查询当前号码中";
            }

            if (!string.IsNullOrEmpty(queryNowTime))
            {
                queryMappings[queryNowTime] = "查询当前时间中";
            }

            if (!string.IsNullOrEmpty(queryNowMissCall))
            {
                queryMappings[queryNowMissCall] = "查询未接来电中";
            }

            string query = "select cid_num, CASE WHEN dest = '#' THEN '调度' ELSE dest END AS dest  " +
                " from detailed_calls" +
                " where callstate = 'ACTIVE' AND NOT (dest LIKE '#1%' AND char_length(dest) = 7) " +
                " AND dest <> (SELECT auth FROM config WHERE id = 1)";
            DataTable dt = DB.ExecuteQuery(query);
            List<DefaultUserModel> defaultUserModel = new List<DefaultUserModel>();
            foreach (DataRow row in dt.Rows)
            {
                string destValue = row["dest"].ToString();

                if (queryMappings.TryGetValue(destValue, out string mappedValue))
                {
                    destValue = mappedValue;
                }

                defaultUserModel.Add(new DefaultUserModel
                {
                    Cid_num = row["cid_num"].ToString(),
                    Dest = destValue
                });
            }

            return defaultUserModel;
        }

        /// <summary>
        /// 获取UUID
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static DataTable GetUUid(string num)
        {
            string sql = "select uuid from channels where cid_num=@num or dest=@num";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@num", num) };
            DataTable dt = DB.ExecuteQuery(sql, parameters);
            return dt;
        }

        /// <summary>
        /// 调度状态(用于自动应答排队电话)
        /// </summary>
        /// <param name="dispatch_num"></param>
        /// <returns></returns>
        public static bool DispatchStatus(string dispatch_num, string dispatch_num2)
        {
            string sql = "select exists(select 1 from channels where cid_num=@dispatch_num and dest !=(select auth from config where id = 1) or dest =@dispatch_num2)";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@dispatch_num", dispatch_num), new NpgsqlParameter("dispatch_num2", dispatch_num2) };
            object result = DB.ExecuteScalar(sql, parameters);
            return (bool)result;
        }



        /// <summary>
        ///  获取当前鉴权号码
        /// </summary>
        /// <returns></returns>
        public static DataTable GetNum()
        {
            string query = "select auth from config where id = 1";
            DataTable dataTable = DB.ExecuteQuery(query);
            string auth = dataTable.Rows[0]["auth"].ToString();
            string sql = $"select cid_num, uuid from channels where dest='{auth}'";
            DataTable dt = DB.ExecuteQuery(sql);
            return dt;
        }


        /// <summary>
        /// 调度记录
        /// </summary>
        /// <param name="leftNum"></param>
        /// <param name="rightNum"></param>
        public static DataTable DispatchCdr(string leftNum, string rightNum)
        {
            string auth = CommonDB.GetAuthNum(); // 鉴权号码动态获取
            string query = "SELECT a.caller_id_number, a.destination_number, a.id, a.start_stamp, u1.username AS caller_name, u2.username AS callee_name " +
                            "FROM dm_cdr a " +
                            "LEFT JOIN dm_user u1 ON a.caller_id_number = u1.usernum " +
                            "LEFT JOIN  dm_user u2  ON a.destination_number = u2.usernum " +
                            $"WHERE (a.caller_id_number IN (@leftNum, @rightNum) OR a.destination_number IN (@leftNum, @rightNum)) AND a.destination_number not like '{auth}' AND a.destination_number not like '#%' AND a.destination_number not like '00000000%' AND a.destination_number NOT LIKE '*%' AND a.caller_id_number not like '00000000%' AND a.start_stamp >= @startOfDay AND a.start_stamp < @endOfDay ORDER BY a.id DESC; ";

            DateTime today = DateTime.Today;
            DateTime startOfDay = today.Date;
            DateTime endOfDay = startOfDay.AddDays(1);

            NpgsqlParameter[] parameter = {
                new NpgsqlParameter("@leftNum", leftNum),
                new NpgsqlParameter("@rightNum", rightNum),
                new NpgsqlParameter("@startOfDay", startOfDay),
                 new NpgsqlParameter("@endOfDay", endOfDay),
            };
            DataTable dt = DB.ExecuteQuery(query, parameter);
            return dt;
        }

        /// <summary>
        /// 用户通话记录
        /// </summary>
        /// <returns></returns>
        public static List<UserCdrModel> UsrCdr(int currentPage, int pageSize)
        {
            string query = "SELECT " +
                            "a.caller_id_number,a.destination_number,a.id,a.start_stamp, a.end_stamp," +
                            "u1.username AS caller_name,u2.username AS callee_name FROM dm_cdr a " +
                            "LEFT JOIN dm_user u1 ON a.caller_id_number = u1.usernum " +
                            "LEFT JOIN  dm_user u2 ON a.destination_number = u2.usernum " +
                            $"WHERE (LENGTH(a.destination_number) = 4 AND a.destination_number NOT LIKE '*%' AND LENGTH(a.caller_id_number) = 4)   ORDER BY  a.id DESC LIMIT {pageSize} offset {(currentPage - 1) * pageSize}  ";
            DataTable dt = DB.ExecuteQuery(query);
            List<UserCdrModel> model = new List<UserCdrModel>();
            foreach (DataRow row in dt.Rows)
            {
                model.Add(new UserCdrModel
                {
                    CallerNum = $"{row["caller_name"]} ({row["caller_id_number"]})",
                    CalleeNum = $"{row["callee_name"]} ({row["destination_number"]})",
                    StartTime = row["start_stamp"].ToString(),
                    EndTime = row["end_stamp"].ToString(),
                });
            }
            return model;
        }

        /// <summary>
        /// 查询通话记录总数
        /// </summary>
        /// <returns></returns>
        public static int GetUserCdrCount()
        {
            string query = "SELECT COUNT(*) " +
               "FROM dm_cdr a " +
               "LEFT JOIN dm_user u1 ON a.caller_id_number = u1.usernum " +
               "LEFT JOIN dm_user u2 ON a.destination_number = u2.usernum " +
               "WHERE (LENGTH(a.destination_number) = 4 " +
               "AND a.destination_number NOT LIKE '*%' " +
               "AND LENGTH(a.caller_id_number) = 4)";

            return DB.ExecuteCountQuery(query);
        }

        /// <summary>
        /// 未接来电信息
        /// </summary>
        /// <param name="leftNum"></param>
        /// <param name="rightNum"></param>
        /// <returns></returns>
        public static List<MissCallModel> MissCall(int currentPage, int pageSize)
        {
            string query = "select  u1.caller_number, u1.time,u2.username from dm_misscall u1" +
                " left join dm_user u2 on u1.caller_number =u2.usernum WHERE  u1.time NOT IN (" +
                "  SELECT cdr.start_stamp   " +
                " FROM dm_cdr cdr " +
                "   WHERE cdr.destination_number = (SELECT dispatcher FROM config WHERE id = 1) and cdr.hangup_cause = 'NORMAL_CLEARING'" +
                " ) " +
                $" ORDER BY u1.time desc LIMIT {pageSize} offset {(currentPage - 1) * pageSize}";
            DataTable dt = DB.ExecuteQuery(query);
            List<MissCallModel> model = new List<MissCallModel>();
            foreach (DataRow row in dt.Rows)
            {
                model.Add(new MissCallModel
                {
                    MissName = row["username"].ToString(),
                    MissNum = row["caller_number"].ToString(),
                    MissTime = row["time"].ToString(),
                });
            }
            return model;
        }

        /// <summary>
        /// 未接来电总数
        /// </summary>
        /// <returns></returns>
        public static int GetMissCallCount()
        {
            string query = "SELECT COUNT(*) " +
               "FROM dm_misscall u1 " +
               "LEFT JOIN dm_user u2 ON u1.caller_number = u2.usernum " +
               "WHERE u1.time NOT IN ( " +
               "  SELECT cdr.start_stamp " +
               "  FROM dm_cdr cdr " +
               "  WHERE cdr.destination_number = (SELECT dispatcher FROM config WHERE id = 1) " +
               "    AND cdr.hangup_cause = 'NORMAL_CLEARING' " +
               ")";

            return DB.ExecuteCountQuery(query);
        }

        /// <summary>
        /// 获取调度号码
        /// </summary>
        public static DataTable DispatchNum()
        {
            string query = "select left_dispatch,right_dispatch from dm_dispatch_number where login_user = '" + DMVariable.NowLoginUserName + "'";
            DataTable dt = DB.ExecuteQuery(query);
            return dt;
        }

        /// <summary>
        /// 来电等待用户
        /// </summary>
        public static DataTable WaitUser()
        {
            string query = "select u1.uuid,u1.created,u1.cid_num,u2.username from channels u1 join dm_user u2 ON u2.usernum = u1.cid_num where  u1.application ='fifo' ORDER BY created";
            DataTable dt = DB.ExecuteQuery(query);
            return dt;
        }

        public static bool TransferNum(string number)
        {
            string query = $"update config set transfernum = '{number}' where id = 2";
            int result = DB.ExecuteNonQuery(query);
            if (result > 0) return true;
            else return false;
        }

        /// <summary>
        /// 获取UUID
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static DataTable GetUUid_Transfer(string num)
        {
            string sql = "select uuid from channels where  dest=@num and direction ='outbound'";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@num", num) };
            DataTable dt = DB.ExecuteQuery(sql, parameters);
            return dt;
        }

        /// <summary>
        /// 获取UUID
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static DataTable GetUUid_Transfer1(string num)
        {
            string sql = "select uuid from channels where  (dest = @num OR dest = (SELECT dispatcher FROM config WHERE id = 1)) and direction ='inbound'";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@num", num) };
            DataTable dt = DB.ExecuteQuery(sql, parameters);
            return dt;
        }
    }
}

