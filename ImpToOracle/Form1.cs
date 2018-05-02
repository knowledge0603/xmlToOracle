using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Xml;
using System.Windows.Forms;

namespace ImpToOracle
{
    public partial class MyForm : Form
    {
        private string fileName = string.Empty;

        public MyForm()
        {
            InitializeComponent();
            this.CenterToScreen();
        }

        private void search_btn_Click(object sender, EventArgs e)
        {
            //openFileDialog.Filter = ".xml文件(*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog.SelectedPath;
                text_Box.Text = fileName.Trim();
            }
        }

        private void cancel_btn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void imp_btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(text_Box.Text))
            {
                MessageBox.Show("请选择文件!");
                return;
            }
            if (!Directory.Exists(text_Box.Text))
            {
                MessageBox.Show("指定的文件路径不存在!!");
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(text_Box.Text);

            FileInfo[] infos = dir.GetFiles();

            StringDictionary typeDict = new StringDictionary();

            typeDict.Add("T_CJGS:CJGS_GUID", "System.String");
            typeDict.Add("T_CJGS:GSBT", "System.String");
            typeDict.Add("T_CJGS:JZRQ", "System.DateTime");
            typeDict.Add("T_CJGS:GSNR", "clob");
            typeDict.Add("T_CJGS:XZQ_DM", "System.String");
            typeDict.Add("T_CJGS:SH_ZT", "System.String");
            typeDict.Add("T_CJGS:FB_SJ", "System.DateTime");
            typeDict.Add("T_CJGS:GY_FS", "System.String");
            typeDict.Add("T_CJGS:SF_TG", "System.Decimal");
            typeDict.Add("T_CJGS:SH_SJ", "System.DateTime");
            typeDict.Add("T_CJGS:OLD_CJGS_GUID", "System.String");
            typeDict.Add("T_CJGS:SHR", "System.String");
            typeDict.Add("T_CJGS:XGYY", "System.String");
            typeDict.Add("T_CJGS:GS_DW", "System.String");
            typeDict.Add("T_CJGS:GS_SJ_S", "System.DateTime");
            typeDict.Add("T_CJGS:GS_SJ_E", "System.DateTime");
            typeDict.Add("T_CJGS:ZD_SL", "System.Decimal");
            typeDict.Add("T_CJGS:ZPG_SJ_S", "System.DateTime");
            typeDict.Add("T_CJGS:ZPG_SJ_E", "System.DateTime");
            typeDict.Add("T_CJGS:ZPG_LX", "System.Decimal");
            typeDict.Add("T_CJGS:GS_LX", "System.Decimal");
            typeDict.Add("T_CJGS:LX_DW", "System.String");
            typeDict.Add("T_CJGS:DW_DZ", "System.String");
            typeDict.Add("T_CJGS:YZ_BM", "System.String");
            typeDict.Add("T_CJGS:LX_DH", "System.String");
            typeDict.Add("T_CJGS:LXR", "System.String");
            typeDict.Add("T_CJGS:EMAIL", "System.String");
            typeDict.Add("T_CJGS:SB_SJ", "System.DateTime");
            typeDict.Add("T_CJGS:CX_YY", "System.String");
            typeDict.Add("T_CJGS:BG_NR", "System.String");
            typeDict.Add("T_CJGS:ISNEW", "System.String");
            typeDict.Add("T_CJGS:SBR", "System.String");
            typeDict.Add("T_CJGS:XM_CJ", "System.Decimal");
            typeDict.Add("T_CJGS:XM_ZT", "System.String");
            typeDict.Add("T_CJGS:CREATE_DATE", "System.DateTime");
            typeDict.Add("T_CJGS:CREATE_USER", "System.String");
            typeDict.Add("T_CJGS:DELETE_DATE", "System.DateTime");
            typeDict.Add("T_CJGS:CH_SJ", "System.DateTime");
            typeDict.Add("T_CJGS:MODIFY_DATE", "System.DateTime");
            typeDict.Add("T_CJGS:WL_BZ", "System.String");

            foreach (FileInfo info in infos)
            {
                if (!LoadText(info.FullName, ref typeDict))
                {
                    continue;
                    //break;
                }
            }
            MessageBox.Show("选择文件夹中的文件读取完毕,请重新选择文件夹!");
        }

        private bool LoadText(string str, ref StringDictionary typeDict)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(str);

                if (xmlDoc != null)
                {
                    /*XmlNodeList nodeList_pri = xmlDoc.SelectNodes("//PRI_TABLE_DATA");
                    if (!parseXmlListp(nodeList_pri))
                    {
                      //  MessageBox.Show("未能全部正确解析主记录!");
                        Tool.WriteLogToFile("未能全部正确解析主记录!");

                        return false;
                    }*/
                     XmlNodeList nodeList_pri = xmlDoc.SelectNodes("//PRI_TABLE_DATA");
                    Tool.WriteLogToFile("PRI_TABLE_DATA解析");
                    Tool.WriteLogToFile("file name:" + xmlDoc.BaseURI);
                    if (!parseXmlListInsert(nodeList_pri, ref typeDict, str))
                     {
                         //ZTDialog.ztMessageBox.Messagebox("未能全部正确解析附记录!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // MessageBox.Show("未能全部正确解析主记录!");
                        Tool.WriteLogToFile("未能全部正确解析主记录!");
                        return false;
                     }
                    
                    XmlNodeList nodeList_sub1 = xmlDoc.SelectNodes("//SUB_TABLE_DATA");
                    Tool.WriteLogToFile("SUB_TABLE_DATA解析");
                    Tool.WriteLogToFile("file name:" + xmlDoc.BaseURI);
                    if (!parseXmlListInsert(nodeList_sub1, ref typeDict, str))
                    {
                        //ZTDialog.ztMessageBox.Messagebox("未能全部正确解析附记录!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                       // MessageBox.Show("未能全部正确解析sub记录!");
                        Tool.WriteLogToFile("未能全部正确解析sub记录!");
                        return false;
                    }
                    //MessageBox.Show("数据插入成功!");
                    
                    //ZTDialog.ztMessageBox.Messagebox("数据入库成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                //MessageBox.Show("解析xml数据文件的过程中发生异常!");
                Toolp.WriteLogToFile("解析xml数据文件的过程中发生异常!");
                Toolp.WriteLogToFile("详细错误信息LoadText：" + ex.Message);
                return false;
            }
            MessageBox.Show("执行完毕!");
            return true;
        }

        private bool parseXmlListp(XmlNodeList nodeList)
        {
            try
            {
                if (nodeList != null && nodeList.Count != 0)
                {
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        string table_Name = string.Empty;
                        if (nodeList[i].Attributes["tableName"] != null)
                        {
                            table_Name = nodeList[i].Attributes["tableName"].Value.Trim();
                        }

                        string query_Fields = string.Empty;
                        if (nodeList[i].Attributes["queryFields"] != null)
                        {
                            query_Fields = nodeList[i].Attributes["queryFields"].Value.Trim();
                        }

                        //if (string.IsNullOrEmpty(table_Name) || string.IsNullOrEmpty(query_Fields))
                        //{
                        //    Tool.WriteLogToFile("表名或字段系列为空");
                        //    return false;
                        //}
                        //query_Fields = "PHQYGZ_GUID,XZQ_DM,YGC_HJ,YGC_CSPHQ,YGC_CZCPHQ,XM_ZT,SH_ZT,CREATE_DATE,CREATE_USER,SB_SJ,PHQGZ_GUID,MKPHQ_Y,KQPHQ_Y,GKPHQ_Y,LQPHQ_Y";
                        string[] fields = query_Fields.Split(',');
                        if (fields.Length == 0)
                        {
                            Toolp.WriteLogToFile("无字段系列");
                            return false;
                        }

                        if (nodeList[i].HasChildNodes)
                        {
                            XmlNodeList list = nodeList[i].SelectNodes("RECORD");

                            if (list != null && list.Count != 0)
                            {
                                for (int j = 0; j < list.Count; j++)
                                {
                                    List<string> sList = new List<string>();
                                    foreach (string f in fields)
                                    {
                                        sList.Add(list[j].Attributes[f].Value.Trim());
                                    }

                                    if (!Toolp.UpDateToOracle(query_Fields,sList,table_Name))
                                    {
                                        Toolp.WriteLogToFile("更新数据库过程出错");
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                Toolp.WriteLogToFile("无record子节点");
                                return false;
                            }
                        }
                        else
                        {
                            Toolp.WriteLogToFile("无子节点");
                            return false;
                        }
                    }

                    return true;
                }
                else
                {
                    Toolp.WriteLogToFile("无主记录或附记录");
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Toolp.WriteLogToFile("详细错误信息parseXmlListp：" + ex.Message);
                return false;
            }
        }


        private bool parseXmlListInsert(XmlNodeList nodeList, ref StringDictionary typeDict, string str)
        {
            try
            {
                if (nodeList != null && nodeList.Count != 0)
                {
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        string table_Name = string.Empty;
                        if (nodeList[i].Attributes["tableName"] != null)
                        {
                            table_Name = nodeList[i].Attributes["tableName"].Value.Trim();
                        }

                        string query_Fields = string.Empty;
                        if (nodeList[i].Attributes["queryFields"] != null)
                        {
                            query_Fields = nodeList[i].Attributes["queryFields"].Value.Trim();
                        }
                        //if (query_Fields == "*")
                        // {
                        //  query_Fields = "XZ_GUID,GD_GUID,XZ_YY,XZ_LX,SF_CF,CF_SJ,XZ_MJ,SJ_TZE,TG_RQ,XZ_RDR,XZ_RD_SJ,CZ_FS,XZ_CZ_YJ,JL_XZF,YJ_SF_PZ,PZ_RQ,PZ_WH,CZ_JG,CZ_SJ,XZSH_BZ,IS_BG_DG_SJ,IS_BG_JG_SJ,BG_DG_SJ,BG_JG_SJ,CZ_ZT,BF_SH,BF_SH_MJ,CZ_SBSJ,CZR";
                        // }
                        
                        Tool.WriteLogToFile("表名:" + table_Name);

                        if (query_Fields == "*" && table_Name == "T_GYGG_ZD")
                        {
                            query_Fields = "GYGG_ZD_GUID,GYGG_GUID,ZD_BH,MJ,TD_YT,LP_ZT,ZD_ZL,MIN_RJL,MIN_RJL_TAG,MAX_RJL,MAX_RJL_TAG,MIN_LHL,MIN_LHL_TAG,MAX_LHL,MAX_LHL_TAG,MIN_JZ_MD,MIN_JZ_MD_TAG,MAX_JZ_MD,MAX_JZ_MD_TAG,TZ_QD,CR_NX,CR_BZJ,QSJ,JJ_FD,TD_XZ,GP_SJ_S,GP_SJ_E,MIN_JZ_XG,MIN_JZ_XG_TAG,MAX_JZ_XG,JC_SS,QD_PZ,XZ_TD_TJ,RJL_GX,LHL_GX,JZ_MD_GX,JZ_XG_GX,QSJ_DW,JJFD_DW,BZ,ZD_ZT,CZ_YY,CZR,CZ_SJ";
                        }
                        if (query_Fields == "*" && table_Name == "YJ_DUB")
                        {
                            query_Fields = "DUB_ID,DUB_MC,DUB_NR,DUB_USER,DUB_DATE,DUB_FS_ZT,DUB_FS_SJ,DUB_JS_ZT,DUB_JS_YJ,DUB_JS_USER,DUB_JS_SJ,XZ_GUID";
                        }
                        if (query_Fields == "*" && table_Name == "YJ_YDKGWY")
                        {
                            query_Fields = "DGYJ_ID,GD_GUID,XZQ_DM,XM_MC,BH,DZ_BA_BH,ZD_BH,GD_ZMJ,GY_FS,TD_YT,PZ_WH,GY_MJ,JE,JD_SJ,DG_SJ,QD_RQ,XM_ZT,CREATE_USER,WL_BZ,YJ_CREATE_DATE,YJ_CZ_DATE,YJ_CZ_USER,YJ_CZ_FS,YJ_CZ_YJ,YJ_ZT,YJ_DUB_CS,YJ_WWF_YY,WWF_YY_SM,MODIFY_DATE";
                        }
                        if (query_Fields == "*" && table_Name == "YJ_YDJGWY")
                        {
                            query_Fields = "JGYJ_ID,GD_GUID,XZQ_DM,XM_MC,DZ_BA_BH,ZD_BH,GD_ZMJ,GY_FS,TD_YT,PZ_WH,GY_MJ,JE,DG_SJ,QD_RQ,XM_ZT,CREATE_USER,WL_BZ,YJ_CREATE_DATE,YJ_CZ_DATE,YJ_CZ_USER,YJ_CZ_FS,YJ_CZ_YJ,YJ_ZT,YJ_DUB_CS,YJ_WWF_YY,WWF_YY_SM,MODIFY_DATE";
                        }
                        if (query_Fields == "*" && table_Name == "YJ_YSXZ")
                        {
                            query_Fields = "YSXZ_ID,GD_GUID,XZQ_DM,XM_MC,BH,DZ_BA_BH,ZD_BH,GD_ZMJ,GY_FS,TD_YT,PZ_WH,GY_MJ,JE,QD_RQ,XM_ZT,BG_DG_SJ,SJ_DG_SJ,DG_SJ,CREATE_USER,HT_LX,WL_BZ,YJ_CREATE_DATE,YJ_CZ_DATE,YJ_CZ_USER,YJ_CZ_FS,YJ_CZ_YJ,YJ_ZT,YJ_DUB_CS,WWF_YY_SM,YJ_WWF_YY,MODIFY_DATE";
                        }
                        if (query_Fields == "*" && table_Name == "YJ_XZDCZ")
                        {
                            query_Fields = "XZDCZ_ID,GD_GUID,XZ_GUID,XZQ_DM,XM_MC,BH,DZ_BA_BH,ZD_BH,GD_ZMJ,GY_FS,TD_YT,PZ_WH,GY_MJ,JE,QD_RQ,XZ_RDR,XZ_RD_SJ,DG_SJ,SJ_DG_SJ,BG_DG_SJ,XM_ZT,CREATE_USER,WL_BZ,HT_LX,YJ_CREATE_DATE,YJ_CZ_DATE,YJ_CZ_USER,YJ_CZ_FS,YJ_CZ_YJ,YJ_ZT,YJ_DUB_CS,WWF_YY_SM,YJ_WWF_YY,MODIFY_DATE";
                        }
                        if (query_Fields == "*" && table_Name == "T_GDJC_XZ")
                        {
                            query_Fields = "XZ_GUID,GD_GUID,XZ_YY,XZ_LX,SF_CF,CF_SJ,XZ_MJ,SJ_TZE,TG_RQ,XZ_RDR,XZ_RD_SJ,CZ_FS,XZ_CZ_YJ,JL_XZF,YJ_SF_PZ, PZ_RQ,PZ_WH,CZ_JG,CZ_SJ, XZSH_BZ,IS_BG_DG_SJ,IS_BG_JG_SJ, BG_DG_SJ, BG_JG_SJ, CZ_ZT, BF_SH, BF_SH_MJ, CZ_SBSJ, CZR, MODIFY_DATE";
                        }
                        if (query_Fields == "*" && table_Name == "T_CRJ_SJZF")
                        {
                            query_Fields = "SJZF_GUID,GD_GUID,SJZF_QH,SJZF_JE,ZN_JE,LX_JE,SJZF_SJ,BZ,XH,MODIFY_DATE,RCXC_GUID";
                        }
                        if (query_Fields == "*" && table_Name == "YJ_ZZGGRJL")
                        {
                            query_Fields = "ZZGGRJL_ID,GYGG_GUID,XZQ_DM,GG_BT,GG_BH,FB_SJ,GG_LX,ZD_BH,GYGG_ZD_GUID,MJ,TD_YT,XM_ZT,CREATE_USER,WL_BZ,MIN_RJL,MIN_RJL_TAG,MAX_RJL,MAX_RJL_TAG,YJ_CREATE_DATE,YJ_CZ_DATE,YJ_CZ_USER,YJ_CZ_FS,YJ_CZ_YJ,YJ_ZT,YJ_DUB_CS,WWF_YY_SM,YJ_WWF_YY,ZD_ZL,CR_NX,MODIFY_DATE";
                        }
                        if (query_Fields == "*"&& table_Name== "YJ_GGCGM")
                        {
                            query_Fields = "CGMGG_ID,GYGG_GUID,XZQ_DM,GG_BT,CSGM,GG_BH,FB_SJ,GG_LX,ZD_BH,GYGG_ZD_GUID,MJ,ZD_ZL,CR_NX,TD_YT,XM_ZT,CREATE_USER,WL_BZ,YJ_CREATE_DATE,YJ_CZ_DATE,YJ_CZ_USER,YJ_CZ_FS,YJ_CZ_YJ,YJ_ZT,YJ_DUB_CS,YJ_WWF_YY,WWF_YY_SM,MODIFY_DATE";
                        }
                        if (query_Fields == "*" && table_Name == "YJ_ZZHTRJL")
                        {
                            query_Fields = "ZZHTRJL_ID,GD_GUID,XZQ_DM,XM_MC,BH,DZ_BA_BH,ZD_BH,GD_ZMJ,GY_FS,TD_YT,PZ_WH,GY_MJ,JE,QD_RQ,XM_ZT,CREATE_USER,WL_BZ,MIN_RJL,MIN_RJL_TAG,MAX_RJL,MAX_RJL_TAG,YJ_CREATE_DATE,YJ_CZ_DATE,YJ_CZ_USER,YJ_CZ_FS,YJ_CZ_YJ,YJ_ZT,YJ_DUB_CS,WWF_YY_SM,YJ_WWF_YY,MODIFY_DATE";
                        }
                        if (query_Fields == "*" && table_Name == "YJ_HTCGM")
                        {
                            query_Fields = "CGMHT_ID,GD_GUID,XZQ_DM,CSGM,XM_MC,BH,DZ_BA_BH,ZD_BH,GD_ZMJ,GY_FS,TD_YT,PZ_WH,GY_MJ,JE,QD_RQ,XM_ZT,CREATE_USER,WL_BZ,YJ_CREATE_DATE,YJ_CZ_DATE,YJ_CZ_USER,YJ_CZ_FS,YJ_CZ_YJ,YJ_ZT,YJ_DUB_CS,YJ_WWF_YY,WWF_YY_SM,MODIFY_DATE";
                        }
                        if (query_Fields == "*" && table_Name == "YJ_YDJN")
                        {
                            query_Fields = " YDJN_ID,GD_GUID,XZQ_DM,XM_MC,BH,DZ_BA_BH,ZD_BH,GD_ZMJ,GY_FS,TD_YT,PZ_WH,GY_MJ,JE,YDZFJE_M,YDZFJE_Y,QD_RQ,XM_ZT,CREATE_USER,WL_BZ,YJ_CREATE_DATE,YJ_CZ_DATE,YJ_CZ_USER,YJ_CZ_FS,YJ_CZ_YJ,YJ_ZT,YJ_DUB_CS,WWF_YY_SM,YJ_WWF_YY,MODIFY_DATE";
                        }
                        /*if ( table_Name == "T_CRJ_ZFYD")
                        {
                            query_Fields = " CRJ_GUID,GD_GUID,ZF_QH,ZF_SJ,ZF_JE,BZ ";
                        }*/
                        if (table_Name == "T_HBGY_KZ")
                        {
                            query_Fields = " HBKZ_GUID,GD_GUID,SYQR,ZD_ZXY_XM,ZF_JZ_MJ,MIN_ZZ_TS,MIN_50_ZZ_TS,DT_JZ_MJ,DT_JZ_TS,QF_JG,QF_SJ,PT_XM,BC_TK,PM_JZ,SX_SJX,SX_XJX,GC,CJGS_ZD_GUID,CJGS_ZD_BH,WGSZD_SM ";
                        }
                        if (table_Name == "T_SQBH")
                        {
                            Tool.WriteLogToFile("接口文档中没有该表（T_SQBH）");
                            return false;
                        }
                        if (table_Name == "T_BHYJ")
                        {
                            Tool.WriteLogToFile("接口文档中没有该表（T_BHYJ）");
                            return false;
                        }
                        if (table_Name == "T_CBDLY" )
                        {
                            Tool.WriteLogToFile("接口文档中没有该表（T_CBDLY）");
                            return false;
                        }
                        if (string.IsNullOrEmpty(table_Name) || string.IsNullOrEmpty(query_Fields))
                        {
                            Tool.WriteLogToFile("表名或字段系列为空");
                            return false;
                        }
                        string[] fields = query_Fields.Split(',');
                        if (fields.Length == 0)
                        {
                            Tool.WriteLogToFile("无字段系列");
                            return false;
                        }
                        if (nodeList[i].HasChildNodes)
                        {
                            XmlNodeList list = nodeList[i].SelectNodes("RECORD");

                            if (list != null && list.Count != 0)
                            {
                                for (int j = 0; j < list.Count; j++)
                                {
                                    List<string> sList = new List<string>();
                                    foreach (string f in fields)
                                    {
                                        if (list[j].Attributes[f] == null)
                                        {
                                            Tool.WriteLogToFile("---------------");
                                            Tool.WriteLogToFile("详细错误信息parseXmlListInsert1:" + f);
                                            Tool.WriteLogToFile("---------------");
                                            //list[j].Attributes[f] = "";
                                            //continue;
                                            //
                                            sList.Add("");
                                            //return false;
                                        }
                                        
                                        if (f == "GD_GUID" &&list[j].Attributes[f] != null&&string.IsNullOrEmpty(list[j].Attributes[f].ToString()))
                                        {
                                            Tool.WriteLogToFile("---------------");
                                            Tool.WriteLogToFile("主键Gd_Guid为空！");
                                            Tool.WriteLogToFile("---------------");
                                            //MessageBox.Show("主键Gd_Guid为空！");
                                            
                                            sList.Add("");
                                            //return false;
                                        }
                                        if (!(list[j].Attributes[f] == null)&& !(f == "GD_GUID" && list[j].Attributes[f] != null && string.IsNullOrEmpty(list[j].Attributes[f].ToString())))
                                        {
                                            sList.Add(list[j].Attributes[f].Value.Trim());
                                        }
                                    }

                                    if (!Tool.InsertToOracle(query_Fields, sList, table_Name, ref typeDict))
                                    {
                                        Tool.WriteLogToFile("--------------");
                                        Tool.WriteLogToFile("插入数据库过程出错");
                                        Tool.WriteLogToFile("--------------");
                                        //  MessageBox.Show("插入数据库过程出错!");
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                Tool.WriteLogToFile("无record子节点");
                                return false;
                            }
                        }
                        else
                        {
                            Tool.WriteLogToFile("无子节点");
                            return false;
                        }
                    }

                    return true;
                }
                else
                {
                    Tool.WriteLogToFile("无主记录或附记录");
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Tool.WriteLogToFile("详细错误信息  parseXmlListInsert 2：" + ex.Message);
                return false;
            }
        }
    }
}
