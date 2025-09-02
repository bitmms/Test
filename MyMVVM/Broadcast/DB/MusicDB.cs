using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMVVM.Common;
using MyMVVM.Common.Utils;
using Npgsql;

namespace MyMVVM.Broadcast
{
    public class MusicDB
    {
        /// <summary>
        /// 音乐音乐名称，获取音乐时长
        /// </summary>
        public static string GeTimeBytMusicName(string name)
        {
            string sql = $"select dm_music.music_time from dm_music where music_name = '{name}.wav'";
            object result = DB.ExecuteScalar(sql);
            return (string)result;
        }


        /// <summary>
        /// 音乐上传到远程的路径
        /// </summary>
        public static string GetUploadRemotePath()
        {
            // 这里写死路径
            return "/home/freeswitch-record/music-audio/";
        }

        /// <summary>
        /// 音乐上传到本地的路径
        /// </summary>
        public static string GetUploadLocalPath()
        {
            string path = @"D:\DMKJ";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// 增加一条音乐记录到数据库
        /// </summary>
        public static void AddMusic(MusicModel musicModel)
        {
            bool a = false;

            // 特殊的处理：mp3 to wav
            if (musicModel.Name.Contains(".mp3"))
            {
                string mp3ToWavStr = $"export LD_LIBRARY_PATH=/usr/local/lib:$LD_LIBRARY_PATH; ffmpeg -i {MusicDB.GetUploadRemotePath()}{musicModel.Name} {MusicDB.GetUploadRemotePath()}{musicModel.Name.Replace("mp3", "wav")}";
                string x2 = SSH.ExecuteCommand(mp3ToWavStr);

                musicModel.UploadLocalPath = musicModel.UploadLocalPath.Replace("mp3", "wav");
                musicModel.UploadRemotePath = musicModel.UploadRemotePath.Replace("mp3", "wav");
                musicModel.Name = musicModel.Name.Replace("mp3", "wav");
            }

            string sql = $"insert into dm_music(local_path, remote_path, music_name, music_time) VALUES (@local, @remote, @name, @time)";
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@local", musicModel.UploadLocalPath),
                new NpgsqlParameter("@remote", musicModel.UploadRemotePath),
                new NpgsqlParameter("@name", musicModel.Name),
                new NpgsqlParameter("@time", musicModel.Time)
            };

            DB.ExecuteNonQuery(sql, parameters);
        }


        /// <summary>
        /// 从数据库中删除一条音乐文件记录
        /// </summary>
        public static void DeleteMusicById(int id)
        {
            string sql = "delete from dm_music where id = " + id;
            DB.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 分页查询音乐信息
        /// </summary>
        public static List<MusicModel> GetMusicList(int currentPage, int pageSize)
        {
            string query = $"select id, local_path, remote_path, music_name, music_time from dm_music limit {pageSize} offset {(currentPage - 1) * pageSize};";



            DataTable dt = DB.ExecuteQuery(query);
            List<MusicModel> files = new List<MusicModel>();
            foreach (DataRow dr in dt.Rows)
            {
                files.Add(new MusicModel
                {
                    Id = int.Parse(dr["id"].ToString()),
                    Name = dr["music_name"].ToString().Substring(0, dr["music_name"].ToString().Length - 4), // 不展示 .wav 
                    Time = dr["music_time"].ToString(),
                    UploadLocalPath = dr["local_path"].ToString(),
                    UploadRemotePath = dr["remote_path"].ToString(),
                });
            }
            return files;
        }

        /// <summary>
        /// 查询音乐总数
        /// </summary>
        public static int GetMusicCount()
        {
            string query = $"select count(id) from dm_music;";
            return DB.ExecuteCountQuery(query);
        }

        /// <summary>
        /// 查询全部音乐信息
        /// </summary>
        public static List<MusicModel> GetMusicList()
        {
            string query = $"select id, local_path, remote_path, music_name, music_time from dm_music;";
            DataTable dt = DB.ExecuteQuery(query);
            List<MusicModel> files = new List<MusicModel>();
            foreach (DataRow dr in dt.Rows)
            {
                files.Add(new MusicModel
                {
                    Id = int.Parse(dr["id"].ToString()),
                    Name = dr["music_name"].ToString().Substring(0, dr["music_name"].ToString().Length - 4), // 不展示 .wav 
                    Time = dr["music_time"].ToString(),
                    UploadLocalPath = dr["local_path"].ToString(),
                    UploadRemotePath = dr["remote_path"].ToString(),
                });
            }
            return files;
        }

        public static bool IsExistMusic(string name)
        {
            string query = $"select local_path, remote_path, music_name, music_time from dm_music where music_name='{name}';";
            DataTable dt = DB.ExecuteQuery(query);
            List<MusicModel> files = new List<MusicModel>();
            return dt.Rows.Count > 0;
        }
    }
}

