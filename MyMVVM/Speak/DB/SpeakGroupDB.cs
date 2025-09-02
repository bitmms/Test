using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.Broadcast;
using MyMVVM.Common;
using MyMVVM.Common.Utils;
using MyMVVM.Speak.Model;
using Rubyer;

namespace MyMVVM.Speak
{
    public class SpeakGroupDB
    {
        /// <summary>
        /// 查询系统中全部的群组
        /// </summary>
        public static List<SpeakGroupVO> GetMusicList()
        {
            string query = $"" +
                $"select g.group_id, g.group_owner_number, g.group_name, u.username, (select count(*) from dm_speak_group_and_member gm where gm.group_id = g.group_id) as count " +
                $"from dm_speak_group g " +
                $"left join dm_user u on g.group_owner_number = u.usernum ";
            DataTable dt = DB.ExecuteQuery(query);
            List<SpeakGroupVO> list = new List<SpeakGroupVO>();
            foreach (DataRow dr in dt.Rows)
            {
                SpeakGroupVO speakGroupVO = new SpeakGroupVO();

                speakGroupVO.groupId = dr["group_id"].ToString();
                speakGroupVO.groupName = dr["group_name"].ToString();
                speakGroupVO.groupOwnerNumber = dr["group_owner_number"].ToString();
                speakGroupVO.groupOwnerName = dr["username"].ToString();
                speakGroupVO.groupMemberCount = int.Parse(dr["count"].ToString());
                // ===========================================
                speakGroupVO.groupOwnerNameAndNumber = $"{speakGroupVO.groupOwnerName}（{speakGroupVO.groupOwnerNumber}）";
                speakGroupVO.groupOwnerNameAndCount = $"{speakGroupVO.groupName}（{speakGroupVO.groupMemberCount}）";

                list.Add(speakGroupVO);
            }
            return list;
        }

        /// <summary>
        /// 查询指定群组的聊天记录
        /// </summary>
        public static List<MessageVO> GetMessageListByGroupId(string groupId)
        {
            string query = $"" +
                $"select type, from_number, t_u.username as from_name, from_time, from_group_id, text, path " +
                $"from dm_speak_message t_m " +
                $"left join dm_user t_u on t_m.from_number = t_u.usernum " +
                $"where t_m.from_group_id = '{groupId}' " +
                $"order by t_m.from_time";
            DataTable dt = DB.ExecuteQuery(query);
            List<MessageVO> list = new List<MessageVO>();
            foreach (DataRow dr in dt.Rows)
            {
                MessageVO messageVO = new MessageVO();

                messageVO.messageType = dr["type"].ToString();
                messageVO.messageFromNumber = dr["from_number"].ToString();
                messageVO.messageFromName = dr["from_name"].ToString();
                messageVO.messageFromTime = dr["from_time"].ToString();
                messageVO.messageFromGroupId = dr["from_group_id"].ToString();
                messageVO.messageText = dr["text"].ToString();
                messageVO.messagePath = dr["path"].ToString();

                // ===========================================

                messageVO.messageFromNameAndFromNumber = $"{messageVO.messageFromName}（{messageVO.messageFromNumber}）";
                messageVO.messageAudioTimeInfo = $"{messageVO.messageText}\"";
                messageVO.messageIsText = messageVO.messageType == "0";
                messageVO.messageIsAudio = messageVO.messageType == "1";
                messageVO.messageIsImage = messageVO.messageType == "2";
                messageVO.messageSendTimeInfo = $"{messageVO.messageFromTime} 发送";
                if (messageVO.messagePath.Length > 10)
                {
                    messageVO.messagePath = $"http://{DMVariable.SSHIP}:{90}{messageVO.messagePath}";
                }

                list.Add(messageVO);
            }
            return list;
        }

    }
}
