using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MyMVVM.Common.Utils;
using MyMVVM.Common.View;
using Npgsql;

namespace MyMVVM.Common
{
    public class DB
    {

        public static NpgsqlConnection SqlConn111 = null;

        public static NpgsqlConnection GetConnection()
        {
            if (SqlConn111 == null)
            {
                SqlConn111 = new NpgsqlConnection(DMVariable.DB_STRING);
            }
            return SqlConn111;
        }

        /// <summary>
        /// 使用DataAdapter查询，返回DataSet
        /// </summary>
        /// <param name="sqrstr"></param>
        /// <returns></returns>
        public static DataSet ExecuteQuery_dt(string sqrstr)
        {

            NpgsqlConnection SqlConn = new NpgsqlConnection(DMVariable.DB_STRING);
            DataSet ds = new DataSet();
            SqlConn.Open();
            try
            {
                using (NpgsqlDataAdapter sqldap = new NpgsqlDataAdapter(sqrstr, SqlConn))
                {
                    sqldap.Fill(ds);
                }
                return ds;
            }
            catch (Exception ex)
            {
                //DMMessageBox.Show("错误", "网络连接失败!!请联系管理员", DMMessageType.MESSAGE_FAIL);
                throw;
            }
            finally { SqlConn.Close(); }
        }


        /// <summary>
        /// 增删改操作
        /// </summary>
        public static int ExecuteNonQuery(string sqrstr)
        {
            NpgsqlConnection sqlConn = new NpgsqlConnection(DMVariable.DB_STRING);
            try
            {
                sqlConn.Open();
                using (NpgsqlCommand SqlCommand = new NpgsqlCommand(sqrstr, sqlConn))
                {
                    int r = SqlCommand.ExecuteNonQuery();  //执行查询并返回受影响的行数
                    return r; //r如果是>0操作成功！ 
                }
            }
            catch (System.Exception ex)
            {
                //DMMessageBox.Show("错误", "网络连接失败!!请联系管理员", DMMessageType.MESSAGE_FAIL);
                throw;
            }
            finally { sqlConn.Close(); }

        }


        /// <summary>
        /// 增删改查(含参)
        /// </summary>
        /// <param name="sqrstr"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sqrstr, NpgsqlParameter[] parameters)
        {
            NpgsqlConnection sqlConn = new NpgsqlConnection(DMVariable.DB_STRING);
            try
            {
                sqlConn.Open();
                using (NpgsqlCommand SqlCommand = new NpgsqlCommand(sqrstr, sqlConn))
                {
                    if (parameters != null)
                    {
                        SqlCommand.Parameters.AddRange(parameters);
                    }
                    int r = SqlCommand.ExecuteNonQuery();  //执行查询并返回受影响的行数
                    return r; //r如果是>0操作成功！ 
                }
            }
            catch (System.Exception ex)
            {
                //DMMessageBox.Show("错误", "网络连接失败!!请联系管理员", DMMessageType.MESSAGE_FAIL);
                throw;
            }
            finally { sqlConn.Close(); }

        }

        /// <summary>
        /// 返回首行首列数据
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlstr)
        {
            NpgsqlConnection sqlConn = new NpgsqlConnection(DMVariable.DB_STRING);
            try
            {
                sqlConn.Open();
                using (NpgsqlCommand SqlCommand = new NpgsqlCommand(sqlstr, sqlConn))
                {
                    object d = SqlCommand.ExecuteScalar();
                    return d;
                }
            }
            catch (Exception ex)
            {
                //DMMessageBox.Show("错误", "网络连接失败!!请联系管理员", DMMessageType.MESSAGE_FAIL);
                throw;
            }
            finally
            {
                sqlConn.Close();
            }
        }


        /// <summary>
        /// 返回首行首列数据(含参)
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlstr, NpgsqlParameter[] parameters)
        {
            NpgsqlConnection sqlConn = new NpgsqlConnection(DMVariable.DB_STRING);

            try
            {
                sqlConn.Open();

                using (NpgsqlCommand SqlCommand = new NpgsqlCommand(sqlstr, sqlConn))
                {
                    if (parameters != null)
                    {
                        SqlCommand.Parameters.AddRange(parameters);
                    }

                    object d = SqlCommand.ExecuteScalar();
                    return d;
                }
            }
            catch (Exception ex)
            {
                //DMMessageBox.Show("错误", "网络连接失败!!请联系管理员", DMMessageType.MESSAGE_FAIL);
                throw;
            }
            finally
            {
                sqlConn.Close();
            }
        }


        /// <summary>
        /// 返回DataReader的查询
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public static DbDataReader GetReader(string cmdText)
        {
            NpgsqlConnection sqlConn = new NpgsqlConnection(DMVariable.DB_STRING);
            if (sqlConn.State != ConnectionState.Open)
                sqlConn.Open();
            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, sqlConn))
                {
                    NpgsqlDataReader sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    return sdr;
                }
            }
            catch (System.Exception ex)
            {
                //DMMessageBox.Show("错误", "网络连接失败!!请联系管理员", DMMessageType.MESSAGE_FAIL);
                throw;
            }
            finally { sqlConn.Close(); }
        }


        /// <summary>
        /// 查询 postgre 数据库，返回 DataTable 数据
        /// </summary>
        /// <param name="sqlText">sql查询语句</param>
        /// <returns></returns>
        public static DataTable ExecuteQuery(string sqlText)
        {
            return ExecuteQuery(sqlText, null);
        }

        /// <summary>
        /// 查询 postgre 数据库，返回 DataTable 数据
        /// </summary>
        public static DataTable ExecuteQuery(string sqlText, NpgsqlParameter[] param)
        {
            NpgsqlConnection npgsql = new NpgsqlConnection(DMVariable.DB_STRING);
            DataTable dt = new DataTable();
            try
            {
                if (npgsql.State != ConnectionState.Open)
                    npgsql.Open();
                using (NpgsqlDataAdapter sqldap = new NpgsqlDataAdapter(sqlText, npgsql))
                {
                    if (param != null)
                    {
                        sqldap.SelectCommand.Parameters.AddRange(param);
                    }
                    sqldap.Fill(dt);
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                npgsql.Close();
            }
        }

        public static DataTable ExecuteQuery(string sqlText, NpgsqlParameter[] param1, NpgsqlParameter[] param2)
        {
            using (NpgsqlConnection npgsql = new NpgsqlConnection(DMVariable.DB_STRING))
            {
                DataTable dt = new DataTable();
                npgsql.Open();
                try
                {
                    using (NpgsqlDataAdapter sqldap = new NpgsqlDataAdapter(sqlText, npgsql))
                    {
                        // 合并两个参数数组
                        NpgsqlParameter[] combinedParams = param1.Concat(param2).ToArray();

                        if (combinedParams != null && combinedParams.Length > 0)
                        {
                            sqldap.SelectCommand.Parameters.AddRange(combinedParams);
                        }
                        sqldap.Fill(dt);
                    }
                    return dt;
                }
                catch (Exception ex)
                {
                    //DMMessageBox.Show("错误", "网络连接失败!!请联系管理员", DMMessageType.MESSAGE_FAIL);
                    throw;
                }
            }
        }

        public static int ExecuteCountQuery(string query, params NpgsqlParameter[] parameters)
        {
            try
            {
                object result = DB.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                //DMMessageBox.Show("错误", "网络连接失败!!请联系管理员", DMMessageType.MESSAGE_FAIL);
                throw;
            }
        }






        /// <summary>
        /// 增加一条记录，并返回自增主键的值
        /// </summary>
        public static int ExecuteInsert(string insertSql, NpgsqlParameter[] parameters)
        {
            NpgsqlConnection sqlConn = new NpgsqlConnection(DMVariable.DB_STRING);

            try
            {
                sqlConn.Open();
                insertSql = insertSql + " RETURNING id";
                using (NpgsqlCommand SqlCommand = new NpgsqlCommand(insertSql, sqlConn))
                {
                    if (parameters != null)
                    {
                        SqlCommand.Parameters.AddRange(parameters);
                    }
                    object result = SqlCommand.ExecuteScalar();
                    if (result != null)
                    {
                        return (int)result;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                //DMMessageBox.Show("错误", "网络连接失败!!请联系管理员", DMMessageType.MESSAGE_FAIL);
                throw;
            }
        }


        /// <summary>
        /// Login 专用的查询
        /// </summary>
        public static DataTable ExecuteQuery2(string sqlText)
        {
            NpgsqlConnection npgsql = new NpgsqlConnection(DMVariable.DB_STRING);
            DataTable dt = new DataTable();
            try
            {
                npgsql.Open();
                using (NpgsqlDataAdapter sqldap = new NpgsqlDataAdapter(sqlText, npgsql))
                {
                    sqldap.Fill(dt);
                }
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                npgsql.Close();
            }
        }
    }
}
