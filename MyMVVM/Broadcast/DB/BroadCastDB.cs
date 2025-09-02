using MyMVVM.Common;
using MyMVVM.Common.Utils;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace MyMVVM.Broadcast
{
    public class BroadCastDB
    {


        /// <summary>
        /// 根据用户信息查询与之对于的摄像头信息
        /// </summary>
        public static Dictionary<string, string> GetCameraInfoByUsernum(string usernum)
        {
            string sql = $"select camera_ip, username, camera_password, camera_account, camera_port from dm_user where usernum = '{usernum}'";
            DataTable dt = DB.ExecuteQuery(sql);
            return new Dictionary<string, string>()
                    {
                        {"camera_ip", dt.Rows[0]["camera_ip"].ToString() },
                        {"camera_port", dt.Rows[0]["camera_port"].ToString() },
                        {"camera_account", dt.Rows[0]["camera_account"].ToString() },
                        {"camera_password", dt.Rows[0]["camera_password"].ToString()  },
                        {"username", dt.Rows[0]["username"].ToString()  },
                        {"usernum", usernum  },
                    };
        }



        /// <summary>
        /// 根据 CallId 将 已选择的用户 和 广播的id标志 插入到数据表
        /// </summary>
        public static void AddSelectedUsersAndBroadcastName(string str, string groupCallId)
        {
            string sql = $"update dm_group set group_members = '{str}' where call_id = '{groupCallId}'";
            DB.ExecuteNonQuery(sql);
        }




        /// <summary>
        /// 根据 GroupId 查询 CallID
        /// </summary>
        public static string GetCallIdByGroupId(int id)
        {
            string sql = "select call_id from dm_group where id = " + id;
            DataTable dt = DB.ExecuteQuery(sql);
            return dt.Rows[0]["call_id"].ToString();
        }


        /// <summary>
        /// 查询全部的广播用户
        /// </summary>
        public static List<string> GetAllBroadcastUser()
        {
            List<string> list = new List<string>();
            string query = "SELECT usernum from dm_user where partition='广播'";
            DataTable dt = DB.ExecuteQuery(query);
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(dr["usernum"].ToString());
            }
            return list;
        }


        /// <summary>
        /// 根据组id查询组的名称
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static string GetGroupNameById(int groupId)
        {
            List<string> list = new List<string>();
            string query = "SELECT group_name from dm_group where id=" + groupId;
            DataTable dt = DB.ExecuteQuery(query);
            return dt.Rows[0]["group_name"].ToString();
        }


        /// <summary>
        /// 增加一条广播记录，并将自增键的 id 值赋值给为 model 的 Id 属性
        /// </summary>
        public static void InsertBroadCast(BroadCastModel model)
        {
            string sql = "insert into dm_broadcast(group_id, play_count, users, tts_text, type, play_status, create_time, begin_time, dispatch_num, timestamp, music_path, end_time, duration) " +
                         "values (@groupId, @playCount, @users, @ttsText, @type, @playStatus, @createTime, @broadcastBeginTime, @dispatchNum, @timestamp, @musicPath, @broadcastEndTime, @broadcastDuration)";
            NpgsqlParameter[] parameters = new NpgsqlParameter[]{
                // 特殊
                new NpgsqlParameter("@users", model.Users == null ? "" : JsonConvert.SerializeObject(model.Users)), // 选人时可以正常，不选人时为 null

                // int
                new NpgsqlParameter("@playStatus", (int)model.PlayStatus), // 不设置时默认为0
                new NpgsqlParameter("@playCount", (int)model.PlayCount), // 不设置时默认为0
                new NpgsqlParameter("@broadcastDuration", model.BroadcastDuration),

                // varchar
                new NpgsqlParameter("@ttsText", model.TTSText ?? ""),
                new NpgsqlParameter("@musicPath", model.MusicPath ?? ""),
                new NpgsqlParameter("@broadcastEndTime", model.BroadCastEndTime ?? ""),

                // 必须设置的值
                new NpgsqlParameter("@groupId",model.GroupId),
                new NpgsqlParameter("@type", (int)model.Type),
                new NpgsqlParameter("@timestamp", model.TimeStamp),
                new NpgsqlParameter("@createTime", model.CreateTime),
                new NpgsqlParameter("@dispatchNum", model.DispatchNum),
                new NpgsqlParameter("@broadcastBeginTime", model.BroadCastBeginTime),

            };
            // 
            // 数据库中设置为 integer，最大值为 21 亿左右，不考虑超出范围的情况
            model.Id = DB.ExecuteInsert(sql, parameters);
        }



        /// <summary>
        /// 修改任务广播的 playStatus
        /// </summary>
        public static void UpdateBroadCastIsPlayed(int id, int playStatus)
        {
            string sql = "update dm_broadcast set play_status = @playStatus where id=@id";
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@playStatus", playStatus),
                new NpgsqlParameter("@id", id),
            };
            DB.ExecuteNonQuery(sql, parameters);
        }


        /// <summary>
        /// 查询所有正在播放的广播【优化方向：这里传入一个 List，避免每次都创建新的 List】
        /// </summary>
        public static List<BroadCastModel> GetBroadCastOfPlaying()
        {
            List<int> tempBroadCastIdList = new List<int>();
            List<BroadCastModel> tempBroadCastModelList = new List<BroadCastModel>();

            // "conference-" + broadCastModel.Type + "-" + broadCastModel.Id + "-" + broadCastModel.TimeStamp;
            string ret = SSH.ExecuteCommand("fs_cli -x 'conference list'");
            MatchCollection matches = new Regex(@"\+OK Conference ([a-zA-Z0-9-]+) \(").Matches(ret);
            foreach (Match match in matches)
            {
                string conferenceName = match.Groups[1].Value;
                // 利用 '-' 字符出现三次来筛选出广播会议的ID
                int times = 0;
                for (int i = 0; i < conferenceName.Length; i++)
                {
                    if (conferenceName[i] == '-')
                    {
                        times++;
                    }
                }
                if (times == 3)
                {
                    tempBroadCastIdList.Add(int.Parse(match.Groups[1].Value.Split('-')[2]));
                }
            }
            foreach (int broadcastId in tempBroadCastIdList)
            {
                tempBroadCastModelList.Add(BroadCastDB.GetBroadCastById(broadcastId));
            }
            return tempBroadCastModelList;
        }



        /// <summary>
        /// 查询广播
        ///     - 1. 未播放的任务广播
        ///     - 2. 激活中的定时广播
        /// </summary>
        public static List<BroadCastModel> GetBroadCastOfAction()
        {
            // 未播放的任务广播
            int type1 = (int)BroadcastTypeEnum.TaskMusicBroadcastOfGroupAll;
            int type2 = (int)BroadcastTypeEnum.TaskTTSBroadcastOfGroupAll;
            int type3 = (int)BroadcastTypeEnum.TaskMusicBroadcastOfGroupSelected;
            int type4 = (int)BroadcastTypeEnum.TaskTTSBroadcastOfGroupSelected;
            int playStatus1 = (int)BroadCastPlayStatusEnum.Unplayed;

            // 激活中的定时广播
            int type5 = (int)BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupAll;
            int type6 = (int)BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupAll;
            int type7 = (int)BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupSelected;
            int type8 = (int)BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupSelected;
            int playStatus2 = (int)BroadCastPlayStatusEnum.ScheduledBroadcastActive;

            List<BroadCastModel> broadcastList = new List<BroadCastModel>(16);

            string sql =
                $"select id, group_id, play_count, users, tts_text, type, timestamp, play_status, create_time, begin_time, dispatch_num, music_path , end_time, duration " +
                $"from dm_broadcast " +
                $"where (type in ({type1}, {type2}, {type3}, {type4}) and play_status = {playStatus1}) or (type in ({type5}, {type6}, {type7}, {type8}) and play_status = {playStatus2})";
            DataTable dt = DB.ExecuteQuery(sql);

            foreach (DataRow dr in dt.Rows)
            {
                BroadCastModel broadCastModel = new BroadCastModel()
                {
                    Id = int.Parse(dr["id"].ToString()),
                    GroupId = int.Parse(dr["group_id"].ToString()),
                    Users = JsonConvert.DeserializeObject<ObservableCollection<string>>(dr["users"].ToString()),
                    Type = (BroadcastTypeEnum)int.Parse(dr["type"].ToString()),
                    TTSText = dr["tts_text"].ToString(),
                    PlayStatus = (BroadCastPlayStatusEnum)int.Parse(dr["play_status"].ToString()),
                    PlayCount = int.Parse(dr["play_count"].ToString()),
                    CreateTime = dr["create_time"].ToString(),
                    BroadCastBeginTime = dr["begin_time"].ToString(),
                    TimeStamp = dr["timestamp"].ToString(),
                    DispatchNum = dr["dispatch_num"].ToString(),
                    MusicPath = dr["music_path"].ToString(),
                    BroadCastEndTime = dr["end_time"].ToString(),
                    BroadcastDuration = int.Parse(dr["duration"].ToString()),
                };
                broadcastList.Add(broadCastModel);
            }
            return broadcastList;
        }



        /// <summary>
        /// 根据广播 id 查询一条记录
        /// </summary>
        public static BroadCastModel GetBroadCastById(int id)
        {
            string sql =
                $"select id, group_id, play_count, users, tts_text, type, timestamp, play_status, create_time, begin_time, dispatch_num, music_path , end_time, duration " +
                $"from dm_broadcast " +
                $"where id = " + id;
            DataTable dt = DB.ExecuteQuery(sql);

            return new BroadCastModel()
            {
                Id = int.Parse(dt.Rows[0]["id"].ToString()),
                GroupId = int.Parse(dt.Rows[0]["group_id"].ToString()),
                Users = JsonConvert.DeserializeObject<ObservableCollection<string>>(dt.Rows[0]["users"].ToString()),
                Type = (BroadcastTypeEnum)int.Parse(dt.Rows[0]["type"].ToString()),
                TTSText = dt.Rows[0]["tts_text"].ToString(),
                PlayStatus = (BroadCastPlayStatusEnum)int.Parse(dt.Rows[0]["play_status"].ToString()),
                PlayCount = int.Parse(dt.Rows[0]["play_count"].ToString()),
                CreateTime = dt.Rows[0]["create_time"].ToString(),
                BroadCastBeginTime = dt.Rows[0]["begin_time"].ToString(),
                TimeStamp = dt.Rows[0]["timestamp"].ToString(),
                DispatchNum = dt.Rows[0]["dispatch_num"].ToString(),
                MusicPath = dt.Rows[0]["music_path"].ToString(),
                BroadCastEndTime = dt.Rows[0]["end_time"].ToString(),
                BroadcastDuration = int.Parse(dt.Rows[0]["duration"].ToString()),
            };
        }



        /// <summary>
        /// 查询未播放的，且未超时的任务广播【优化方向：这里传入一个 List，避免每次都创建新的 List】
        /// </summary>
        public static List<BroadCastModel> GetTaskBroadCastOfAction()
        {
            int type1 = (int)BroadcastTypeEnum.TaskMusicBroadcastOfGroupAll;
            int type2 = (int)BroadcastTypeEnum.TaskTTSBroadcastOfGroupAll;
            int type3 = (int)BroadcastTypeEnum.TaskMusicBroadcastOfGroupSelected;
            int type4 = (int)BroadcastTypeEnum.TaskTTSBroadcastOfGroupSelected;
            int playStatus = (int)BroadCastPlayStatusEnum.Unplayed;

            List<BroadCastModel> broadcastList = new List<BroadCastModel>(16);
            string sql =
                $"select id, group_id, play_count, users, tts_text, type, timestamp, play_status, create_time, begin_time, dispatch_num, music_path , end_time, duration " +
                $"from dm_broadcast " +
                $"where type in ({type1}, {type2}, {type3}, {type4}) and play_status = {playStatus} ";
            DataTable dt = DB.ExecuteQuery(sql);

            foreach (DataRow dr in dt.Rows)
            {
                BroadCastModel broadCastModel = new BroadCastModel()
                {
                    Id = int.Parse(dr["id"].ToString()),
                    GroupId = int.Parse(dr["group_id"].ToString()),
                    Users = JsonConvert.DeserializeObject<ObservableCollection<string>>(dr["users"].ToString()),
                    Type = (BroadcastTypeEnum)int.Parse(dr["type"].ToString()),
                    TTSText = dr["tts_text"].ToString(),
                    PlayStatus = (BroadCastPlayStatusEnum)int.Parse(dr["play_status"].ToString()),
                    PlayCount = int.Parse(dr["play_count"].ToString()),
                    CreateTime = dr["create_time"].ToString(),
                    BroadCastBeginTime = dr["begin_time"].ToString(),
                    TimeStamp = dr["timestamp"].ToString(),
                    DispatchNum = dr["dispatch_num"].ToString(),
                    MusicPath = dr["music_path"].ToString(),
                    BroadCastEndTime = dr["end_time"].ToString(),
                    BroadcastDuration = int.Parse(dr["duration"].ToString()),
                };
                broadcastList.Add(broadCastModel);
            }

            return broadcastList;
        }



        /// <summary>
        /// 查询处于激活状态的定时广播【优化方向：这里传入一个 List，避免每次都创建新的 List】
        /// </summary>
        public static List<BroadCastModel> GetScheduledBroadCastOfAction()
        {
            int type1 = (int)BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupAll;
            int type2 = (int)BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupAll;
            int type3 = (int)BroadcastTypeEnum.ScheduledMusicBroadcastOfGroupSelected;
            int type4 = (int)BroadcastTypeEnum.ScheduledTTSBroadcastOfGroupSelected;
            int playStatus = (int)BroadCastPlayStatusEnum.ScheduledBroadcastActive;

            List<BroadCastModel> broadcastList = new List<BroadCastModel>(16);
            string sql =
                $"select id, group_id, play_count, users, tts_text, type, timestamp, play_status, create_time, begin_time, dispatch_num, music_path , end_time, duration " +
                $"from dm_broadcast " +
                $"where type in ({type1}, {type2}, {type3}, {type4}) and play_status = {playStatus} ";
            DataTable dt = DB.ExecuteQuery(sql);

            foreach (DataRow dr in dt.Rows)
            {
                BroadCastModel broadCastModel = new BroadCastModel()
                {
                    Id = int.Parse(dr["id"].ToString()),
                    GroupId = int.Parse(dr["group_id"].ToString()),
                    Users = JsonConvert.DeserializeObject<ObservableCollection<string>>(dr["users"].ToString()),
                    Type = (BroadcastTypeEnum)int.Parse(dr["type"].ToString()),
                    TTSText = dr["tts_text"].ToString(),
                    PlayStatus = (BroadCastPlayStatusEnum)int.Parse(dr["play_status"].ToString()),
                    PlayCount = int.Parse(dr["play_count"].ToString()),
                    CreateTime = dr["create_time"].ToString(),
                    BroadCastBeginTime = dr["begin_time"].ToString(),
                    TimeStamp = dr["timestamp"].ToString(),
                    DispatchNum = dr["dispatch_num"].ToString(),
                    MusicPath = dr["music_path"].ToString(),
                    BroadCastEndTime = dr["end_time"].ToString(),
                    BroadcastDuration = int.Parse(dr["duration"].ToString()),
                };
                broadcastList.Add(broadCastModel);
            }

            return broadcastList;
        }


    }
}