using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.Common;
using MyMVVM.MainWindow.Model;

namespace MyMVVM.MainWindow
{
    public class SideButtonDB
    {
        /// <summary>
        /// 查询所有的侧边栏按钮
        /// </summary>
        public static List<SideButtonModel> GetAllSideButtons()
        {
            List<SideButtonModel> tempList = new List<SideButtonModel>();
            string query = $"select id,is_show, name, icon, is_ok from dm_side_buttons order by sort_id";
            DataTable dt = DB.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                tempList.Add(new SideButtonModel
                {
                    Id = (int)row["id"],
                    Icon = row["icon"].ToString(),
                    Name = row["name"].ToString(),
                    IsOk = (int)row["is_ok"],
                    IsShow = (int)row["is_show"],
                });
            }
            return tempList;
        }


    }
}
