using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.Common;

namespace MyMVVM.Setting
{
    public class SettingDB
    {
        public static bool IsShowWIFIMenuItem()
        {
            string sql = "select is_show, is_ok from dm_side_buttons where id = 5";
            DataTable dt = DB.ExecuteQuery(sql);
            if (dt.Rows.Count > 0 && int.Parse(dt.Rows[0]["is_show"].ToString()) == 1 && int.Parse(dt.Rows[0]["is_ok"].ToString()) == 1)
            {
                return true;
            }
            return false;
        }
    }
}
