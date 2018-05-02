using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.IO;
using System.Data.OracleClient;
using System.Reflection;


namespace ImpToOracle
{
    class Toolp
    {
        private static string strCnn = @"Data Source=ORCL;User ID=tdly;Password=123456";

        public static bool UpDateToOracle(string fields, List<string> sList, string tablename)
        {
            try
            {
               // tablename =   tablename;
                string[] fArr = fields.Split(',');

                string sql = "update " + tablename + " set ";
                for (int i = 1; i < fArr.Length; i++)
                {
                    if (string.IsNullOrEmpty(sList[i]))
                    {
                        continue;
                    }

                    sql += getLink(fArr[i],sList[i]) + ",";
                }
                if (sql == "update " + tablename + " set ")
                {
                    return true;
                }

                sql = sql.Remove(sql.Length - 1);

                //if (sql == "update " + tablename + " set ")
                //{
                //    return true;
                //}

                sql += " where " + fArr[0] + "='" + sList[0] + "'";

                int st = ExecuteNonQuery(sql);
                if (st < 0)
                {
                    //WriteLogToFile("");
                    //return false;
                }
                else
                {
                    WriteLogToFile("更新成功");
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        private static string getLink(string str,string value)
        {
            if(!string.IsNullOrEmpty(value))
            {
                return str + "=to_date('" + value + "','YYYY-MM-DD HH24:MI:SS')";
            }
            return "";
        }

        
        private static DataTable ExeReturnDataTable(string strSql, string cnstr)
        {
            DataTable dt = new DataTable();
            OracleConnection cnn = new OracleConnection(cnstr);
            try
            {
                cnn.Open();
                OracleDataAdapter da = new OracleDataAdapter(strSql, cnn);
                da.Fill(dt);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                cnn.Close();
                cnn.Dispose();
            }
            return dt;
        }
        /// <summary>
        /// 执行非查询操作
        /// </summary>
        /// <param name="strSql">查询语句</param>
        /// <returns>返回影响行数</returns>
        private static int ExecuteNonQuery(string strSql)
        {
            int i = -1;
            OracleConnection cnn = new OracleConnection(strCnn);
            cnn.Open();
            
            try
            {
                OracleCommand cmd = new OracleCommand(strSql, cnn);
                i = cmd.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                WriteLogToFile(ex.Message);
            }
            finally
            {
                cnn.Close();
                cnn.Dispose();
            }
            return i;
        }
        public static void WriteLogToFile(string s)
        {
            FileStream fs = null;
            try { fs = new FileStream("D:\\TDJG_log.txt", FileMode.Open); }
            catch { fs = null; }
            try { if (fs == null) fs = new FileStream("D:\\TDJG_log.txt", FileMode.Create); }
            catch { fs = null; }
            if (fs == null) return;
            fs.Seek(0, SeekOrigin.End);
            StreamWriter sw = new StreamWriter(fs);
            System.DateTime currentTime = System.DateTime.Now;
            sw.Write("[");
            sw.Write(currentTime.ToString());
            sw.Write("]");
            sw.Write(s);
            sw.Write("\r\n");
            sw.Flush();
            sw.Close();
            fs.Close();
        }


    }
}
