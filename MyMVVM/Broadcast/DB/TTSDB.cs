using MyMVVM.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Broadcast
{
    public class TTSDB
    {
        /// <summary>
        /// TTS名称，获取TTS时长
        /// </summary>
        public static string GeTimeByPath(string path)
        {
            string sql = $"select time from dm_tts where path = '{path}'";
            object result = DB.ExecuteScalar(sql);
            return (string)result;
        }


        /// <summary>
        /// 获取文本
        /// </summary>
        public static string GeTextByName(string name)
        {
            string sql = $"select text from dm_tts where name = '{name}'";
            object result = DB.ExecuteScalar(sql);
            return (string)result;
        }


        /// <summary>
        /// 根据名称查询路径
        /// </summary>
        public static string getPathByName(string name)
        {
            string query = $"select path from dm_tts where name = '{name}'";
            DataTable dt = DB.ExecuteQuery(query);
            return dt.Rows[0]["path"].ToString();
        }


        /// <summary>
        /// 从数据库中删除一条记录
        /// </summary>
        public static void DeleteTTSById(int id)
        {
            string sql = "delete from dm_tts where id = " + id;
            DB.ExecuteNonQuery(sql);
        }


        /// <summary>
        /// 查询总数
        /// </summary>
        public static int GetTTSCount()
        {
            string query = $"select count(*) from dm_tts;";
            return DB.ExecuteCountQuery(query);
        }


        /// <summary>
        /// 分页查询
        /// </summary>
        public static List<TTSModel> GetTTSList(int currentPage, int pageSize)
        {
            string query = $"select id, name, text, time, path from dm_tts limit {pageSize} offset {(currentPage - 1) * pageSize};";
            DataTable dt = DB.ExecuteQuery(query);
            List<TTSModel> files = new List<TTSModel>();
            foreach (DataRow dr in dt.Rows)
            {
                files.Add(new TTSModel
                {
                    Id = int.Parse(dr["id"].ToString()),
                    Name = dr["name"].ToString(),
                    Text = dr["text"].ToString(),
                    Time = dr["time"].ToString(),
                    Path = dr["path"].ToString(),
                });
            }
            return files;
        }


        /// <summary>
        /// 查询全部
        /// </summary>
        public static List<TTSModel> GetAllTTSList()
        {
            string query = $"select id, name, text, time, path from dm_tts;";
            DataTable dt = DB.ExecuteQuery(query);
            List<TTSModel> files = new List<TTSModel>();
            foreach (DataRow dr in dt.Rows)
            {
                files.Add(new TTSModel
                {
                    Id = int.Parse(dr["id"].ToString()),
                    Name = dr["name"].ToString(),
                    Text = dr["text"].ToString(),
                    Time = dr["time"].ToString(),
                    Path = dr["path"].ToString(),
                });
            }
            return files;
        }


        /// <summary>
        /// 判断是否存在相同名称的TTS文件
        /// </summary>
        public static bool IsExist(string name)
        {
            string sql = $"select exists(select * from dm_tts where name = '{name}')";
            object ret = DB.ExecuteScalar(sql);
            return (bool)ret;
        }


        /// <summary>
        /// 增加TTS文件
        /// </summary>
        public static void AddTTSText(string name, string text, string time, string path)
        {
            string sql = $"insert into dm_tts(name, text, time, path) values ('{name}', '{text}', '{time}', '{path}')";
            DB.ExecuteNonQuery(sql);
        }


        public static string GetTTSPath()
        {
            return "/home/freeswitch-record/tts-audio/";
        }

    }
}