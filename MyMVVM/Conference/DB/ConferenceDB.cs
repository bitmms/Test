using MyMVVM.Common;
using MyMVVM.Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.Conference
{
    public class ConferenceDB
    {

        /// <summary>
        /// 查询会议全部分组
        /// </summary>
        public static ObservableCollection<GroupModel> GetAllConferenceGroup()
        {
            ObservableCollection<GroupModel> groups = new ObservableCollection<GroupModel>();
            string sql = "select id, sort_id, call_id, meeting_name from dm_meeting order by sort_id asc";
            DataTable dt = DB.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                groups.Add(new GroupModel
                {
                    Id = row["id"].ToString(),
                    CallId = row["call_id"].ToString(),
                    GroupName = row["meeting_name"].ToString()
                });
            }
            return groups;
        }


        /// <summary>
        /// 查询会议组的全部用户
        /// </summary>
        public static void GetUserListByGroupId(string groupId, ObservableCollection<DefaultUserModel> users)
        {
            users.Clear();
            string _sql = "select dm_user.usernum, dm_user.username from dm_user left join dm_user_meeting on dm_user.id = dm_user_meeting.user_id where dm_user_meeting.meeting_id = " + groupId;
            DataTable _dt = DB.ExecuteQuery(_sql);
            foreach (DataRow row in _dt.Rows)
            {
                users.Add(new DefaultUserModel()
                {
                    Username = row["username"].ToString(),
                    Usernum = row["usernum"].ToString(),
                    UserDisplay = row["usernum"].ToString(),
                });
            }
        }
    }
}
