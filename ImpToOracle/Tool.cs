using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.IO;
using System.Collections.Specialized;
using System.Data.OracleClient;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ImpToOracle
{
    class Tool
    {
        private static string strCnn = @"Data Source=ORCL;User ID=tdly;Password=123456";

        public static bool InsertToOracle(string fields, List<string> sList, string tablename, ref StringDictionary typeDict)
        {
            try
            {
                //tablename = "NMG_" + tablename;
                string typeStr = getTypeStr(tablename, fields,ref typeDict);

               /* if (string.IsNullOrEmpty(typeStr))
                {
                    WriteLogToFile("类型系列为空");
                    return false;
                }*/

                string sqlStr = string.Empty;
                string valueStr = string.Empty;

                string[] typeArr = typeStr.Split(',');
                string[] fArr = fields.Split(',');

                if (typeArr.Length != sList.Count)
                {
                    WriteLogToFile("类型系列和值系列长度不等");
                    return false;
                }

                int m = typeArr.Length;
                for (int k = 0; k < fArr.Length - 1; k++)
                {
                    if (fArr[k] == fArr[fArr.Length - 1] && fArr[k] == "OLD_GYGG_GUID" && tablename == "NMG_T_GYGG")
                    {
                        fields = fields.Remove(fields.LastIndexOf(','));
                        m -= 1;
                        break;
                    }
                }

                for (int i = 0; i <m; i++)
                {
                    valueStr += getValueStr(typeArr[i], sList[i]) + ",";
                }
                valueStr = valueStr.Remove(valueStr.Length-1);

                if (string.IsNullOrEmpty(valueStr))
                {
                    WriteLogToFile("字符串为空值");
                    return false;
                }
                sqlStr += "INSERT INTO " + tablename + "(" +fields +") VALUES("+valueStr +")";
                int st = ExecuteNonQuery(sqlStr);
                if (st < 1)
                {
                    WriteLogToFile("执行SQL语句出错");
                    return false;
                }

                if (tablename == "NMG_T_GYJH" && fields.Contains("NEIRONG"))
                {
                    int r = getIndexByName(fArr, "NEIRONG");
                    if (r > -1)
                    {
                        int erow1 = UpdateClogData(sList[r], "NEIRONG", "NMG_T_GYJH", fArr[0], sList[0]);
                        if (erow1 != 1)
                        {
                            return false;
                        }
                    }
                }
                if (tablename == "NMG_T_GYGG" && fields.Contains("GYGG_NR"))
                {
                    int l = getIndexByName(fArr, "GYGG_NR");
                    if (l > -1)
                    {
                        int erow2=UpdateClogData(sList[l], "GYGG_NR", "NMG_T_GYGG", fArr[0], sList[0]);
                        if (erow2 != 1)
                        {
                            return false;
                        }
                    }
                }
                if (tablename == "NMG_T_CJGS" && fields.Contains("GSNR"))
                {
                    int w = getIndexByName(fArr, "GSNR");
                    if (w > -1)
                    {
                        int erow3 = UpdateClogData(sList[w], "GSNR", "NMG_T_CJGS", fArr[0], sList[0]);
                        if (erow3 != 1)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        private static int getIndexByName(string[] strArr,string str)
        {
            for (int i = 0; i < strArr.Length; i++)
            {
                if (strArr[i].Trim().ToLower() == str.Trim().ToLower())
                {
                    return i;
                }
            }

            return -1;
        }
        public static string getTypeStr(string tablename, string queryFields, ref StringDictionary typeDict)
        {
            try
            {
                if (typeDict.ContainsKey(tablename + ":" + queryFields))
                {
                    return typeDict[tablename + ":" + queryFields].ToString() ;
                }

                string str = "select " + queryFields + " from " + tablename + " WHERE ROWNUM <=1";
                DataTable dt = ExeReturnDataTable(str, strCnn);
                string typeStr = string.Empty;

                if (dt != null)
                {
                    if (dt.Columns.Count > 0)
                    {
                        foreach (DataColumn col in dt.Columns)
                        {
                            if ((tablename == "T_CJGS" && col.Caption == "GSNR")||
                                (tablename == "T_GYJH" && col.Caption == "NEIRONG")||
                                (tablename == "T_GYGG" && col.Caption == "GYGG_NR") 
                                )
                            {
                                typeStr += "clob" + ",";
                            } 
                            else
                            {
                                typeStr += col.DataType.ToString() + ",";
                            }
                        }
                        typeStr = typeStr.Remove(typeStr.Length - 1);
                    }
                }

                if (!string.IsNullOrEmpty(typeStr))
                {
                    typeDict.Add(tablename + ":" + queryFields, typeStr);
                }

                return typeStr;
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                WriteLogToFile("取得数据库表字段异常");
                WriteLogToFile(e.Message);
                return string.Empty;
            }
        }
        static string  valueStrClob = string.Empty;
        private static string getValueStr(string myType,string myValue)
        {
            string valueStr = string.Empty;
           
            string result = string.Empty;
            if (myType=="System.Decimal")
            {
                if (!string.IsNullOrEmpty(myValue))
                {
                    valueStr = Decimal.Parse(myValue).ToString();
                }
                else
                {
                    valueStr = "null";
                }
            }
            else if(myType=="System.String")
            {
                myValue = myValue.Replace("'", " ").Replace("\"", " ").Replace("&nbsp"," " ).Replace("&#"," ") ;
                valueStr = "'" + myValue + "'";
            }
            else if (myType =="System.DateTime")
            {
                if (!string.IsNullOrEmpty(myValue))
                {
                    valueStr = "to_date('" + myValue + "','YYYY-MM-DD HH24:MI:SS')";
                }
                else
                {
                    valueStr = "null";
                }
            }
            else if (myType == "clob")
            {
                valueStrClob = string.Empty;
                if (!string.IsNullOrEmpty(myValue))
                {
                    valueStr = ":clob";
                    valueStrClob = myValue;
                }
                else
                {
                    valueStr = ":clob";
                    valueStrClob = "null";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(myValue))
                {
                    valueStr = myValue;
                }
                else
                {
                    valueStr = "null";
                }
            }

            return valueStr;
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
       /* private static int ExecuteNonQuery(string strSql)
        {
            int i = -1;
            OracleConnection cnn = null;
            try
            {
                cnn = new OracleConnection(strCnn);
                cnn.Open();

                OracleCommand cmd = new OracleCommand(strSql, cnn);
                i = cmd.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                WriteLogToFile(ex.Message);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cnn.Close();
                cnn.Dispose();
            }
            return i;
        }*/


        private static int ExecuteNonQuery(string strSql)
        {
            int i = -1;
            OracleConnection cnn = null;
            try
            {
                //将字符串类型数据转化为CLOB类型,存入数据库
                cnn = new OracleConnection(strCnn);
                cnn.Open();
                OracleCommand cmd = new OracleCommand(strSql, cnn);
                if (!valueStrClob.Equals("")) {
                    OracleParameter op = new OracleParameter("clob", OracleType.Clob);
                    op.Value = valueStrClob;
                    cmd.Parameters.Add(op);
                }
                i = cmd.ExecuteNonQuery();
            }
            catch (System.Exception ex)
            {
                WriteLogToFile("--------------");
                WriteLogToFile(ex.Message);
                WriteLogToFile("--------------");
                // MessageBox.Show(ex.Message);
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

        #region
        //public static void UpdateClogData(string newStr,string fname,string tablename,string f_guid,string f_value)
        //{
        //    // 操作对象
        //    OracleLob lob;
        //    OracleTransaction txn = null;
        //    OracleConnection xiaoayolife_con = null;
        //    OracleCommand xiaoyaolife_command = null;
        //    OracleDataReader xiaoyaolife_datareader = null;

        //    string strSql = string.Empty;
        //    string content = string.Empty;

        //    try
        //    {
        //        xiaoayolife_con = new OracleConnection(strCnn);
        //        xiaoayolife_con.Open();
        //        txn = xiaoayolife_con.BeginTransaction();
        //        xiaoyaolife_command = new OracleCommand(strSql, xiaoayolife_con, txn);

        //        // 注意这里的 FOR UPDATE 进行记录锁定

        //        xiaoyaolife_command.CommandText = "SELECT " + fname + " FROM " + tablename + " FOR UPDATE";
        //        //xiaoyaolife_command.CommandText = "SELECT " + fname + " FROM " + tablename + " WHERE " + f_guid + "='" + f_value + "' FOR UPDATE";
        //        //xiaoyaolife_command.CommandText = "SELECT " + fname + " FROM " + tablename + " WHERE " + f_guid + "='" + f_value + "'";
        //        xiaoyaolife_datareader = xiaoyaolife_command.ExecuteReader();
        //        xiaoyaolife_datareader.Read();


        //        while (xiaoyaolife_datareader.Read())
        //        {
        //            lob = xiaoyaolife_datareader.GetOracleLob(0);
        //            if (lob != OracleLob.Null)
        //            {
        //                //content = lob.Value.ToString();
        //                // 进行修改操作
        //                content = newStr;
        //                // 将新的数据值转换成byte[]
        //                byte[] buffer = System.Text.Encoding.Unicode.GetBytes(content);
        //                // 写回lob对象
        //                lob.Write(buffer, 0, buffer.Length);
        //            }
        //        }
        //        // 提交操作
        //        txn.Commit();

        //        Console.WriteLine("===============Success================");

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error: {0}", ex.ToString());
        //    }
        //    finally
        //    {
        //        xiaoyaolife_datareader.Close();
        //        xiaoayolife_con.Close();
        //        xiaoyaolife_command.Dispose();
        //    }

        //}
        #endregion


        public static int UpdateClogData(string newStr, string fname, string tablename, string f_guid, string f_value)
        {
            if (string.IsNullOrEmpty(newStr))
            {
                return 1;
            }

            OracleConnection oraconn = null;
            OracleCommand cmd = null;

            string sql = string.Empty;
            int i = -1;

            try
            {
                sql = "update " + tablename + " set " + fname + "=:clobstr where " + f_guid + "='" + f_value + "'";
                oraconn = new OracleConnection(strCnn);
                oraconn.Open();
                cmd = new OracleCommand(sql, oraconn);
                cmd.Parameters.Add(":clobstr", OracleType.Clob);
                cmd.Parameters[":clobstr"].Value = newStr;

                i = cmd.ExecuteNonQuery();
            }
            catch
            {

            }
            finally
            {
                oraconn.Close();
                oraconn.Dispose();
                cmd.Dispose();
            }

            return i;
        }


    }
}
