using MyMVVM.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Phone
{
    public class PhoneDB
    {
        /// <summary>
        /// 查询视频通话用户的号码和密码
        /// </summary>
        public static Dictionary<string, string> GetNumberOfVideoUser()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string query = $"select softphone, softphone_pass from dm_unify_config where id = 1";
            DataTable dt = DB.ExecuteQuery(query);
            if (dt.Rows.Count == 0)
            {
                dic.Add("softphone", "");
                dic.Add("softphone_pass", "");
                return dic;
            }
            else
            {
                dic.Add("softphone", dt.Rows[0]["softphone"].ToString());
                dic.Add("softphone_pass", dt.Rows[0]["softphone_pass"].ToString());
                return dic;
            }
        }

    }
}
