using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Common
{
    public class LogDB
    {
        /// <summary>
        /// 用户的操作记录
        /// </summary>
        public static void InsertOperationRecord(string content, int type)
        {
            string sql = "insert into dm_operation_record_client(content, create_time, type) values (@content, @time, @type)";
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@content", content),
                new NpgsqlParameter("@time", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")),
                new NpgsqlParameter("@type", type),
            };
            DB.ExecuteNonQuery(sql, parameters);
        }
    }
}
