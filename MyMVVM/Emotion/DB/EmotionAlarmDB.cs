using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.Broadcast;
using MyMVVM.Common;
using MyMVVM.Emotion.Model;
using Newtonsoft.Json;
using Npgsql;

namespace MyMVVM.Emotion
{
    public class EmotionAlarmDB
    {
        // 查询报警的数量
        public static int getCount()
        {
            // String query = "select count(*) from dm_record_ai where action_statu = 1";



            List<string> JsonsList = JsonConvert.DeserializeObject<List<string>>(EmotionAlarmDB.getKeyWord());
            StringBuilder sb = new StringBuilder();
            if (JsonsList.Count > 0)
            {
                sb.Append(" and (");
                foreach (var item in JsonsList)
                {
                    sb.Append(" text LIKE '%" + item + "%' OR");
                }
                sb.Length--;
                sb.Length--;
                sb.Append(" )");
            }
            string query = $"select  count(*) from dm_record_ai where action_statu = 1 " + sb.ToString();
            return DB.ExecuteCountQuery(query);
        }

        public static int ConfirmEmotion(int id)
        {
            // 0 用户点击忽略
            // 1 报警状态
            // 2 无需处理状态
            String query = "update dm_record_ai set action_statu = 0 where id = " + id;
            return DB.ExecuteCountQuery(query);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public static List<EmotionModel> getList(int currentPage, int pageSize)
        {
            List<string> JsonsList = JsonConvert.DeserializeObject<List<string>>(EmotionAlarmDB.getKeyWord());
            StringBuilder sb = new StringBuilder();
            if (JsonsList.Count > 0)
            {
                sb.Append(" and (");
                foreach (var item in JsonsList)
                {
                    sb.Append(" text LIKE '%" + item + "%' OR");
                }
                sb.Length--;
                sb.Length--;
                sb.Append(" )");
            }
            string query = $"select id, from_number, to_number, file_name, text, emotion, action_statu from dm_record_ai where action_statu = 1 " + sb.ToString() + " limit " + pageSize + " offset " + ((currentPage - 1) * pageSize);


            DataTable dt = DB.ExecuteQuery(query);
            List<EmotionModel> list = new List<EmotionModel>();
            foreach (DataRow dr in dt.Rows)
            {
                var item = new EmotionModel
                {
                    Id = int.Parse(dr["id"].ToString()),
                    FromNumber = dr["from_number"].ToString(),
                    ToNumber = dr["to_number"].ToString(),
                    CallTime = dr["file_name"].ToString(),
                    FileName = dr["file_name"].ToString(),
                    CallText = dr["text"].ToString(),
                    ActionStatu = int.Parse(dr["action_statu"].ToString()),
                };
                DateTime dateTime;
                item.CallTime = item.CallTime.Split('_')[0];
                if (DateTime.TryParseExact(item.CallTime, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                {
                    item.CallTime = dateTime.ToString("yyyy年MM月dd日 HH时mm分");
                }
                list.Add(item);
            }
            return list;
        }

        // 修改关键字
        public static int UpdateKeyWord(String word)
        {
            String query = "update dm_record_ai_keyword set keyword = @keyword where id = 1";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@keyword", word) };
            return DB.ExecuteNonQuery(query, parameters);
        }

        // 查询关键字
        public static string getKeyWord()
        {
            string sql = "select keyword from dm_record_ai_keyword where id = 1";
            DataTable dt = DB.ExecuteQuery(sql);
            return dt.Rows[0]["keyword"].ToString();
        }
    }
}
