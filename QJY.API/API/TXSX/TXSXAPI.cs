using System;
using System.Collections.Generic;
using System.Web;
using QJY.Data;
using Microsoft.Practices.Unity;
using QJY.API;
using Newtonsoft.Json;
using System.Linq;
using Senparc.NeuChar.Entities;

namespace TXSX
{
    public class TXSXAPI
    {
        public static void AddALERT(SZHL_TXSX tx)
        {

            tx.Type = "3";
            tx.TXType = "1";

            tx.CFType = "2";
            tx.CFCount = 1;
            tx.Status = "0";

            if (!string.IsNullOrEmpty(tx.Date))
            {
                DateTime SendTime = DateTime.Parse(tx.Date);
                tx.Date = SendTime.ToString("yyyy-MM-dd");
                tx.Hour = SendTime.ToString("HH");
                tx.Minute = SendTime.ToString("mm");
            }

            if (tx.CRDate == null)
            {
                tx.CRDate = DateTime.Now;
            }
            tx.ZXCount = 0;
            new SZHL_TXSXB().Insert(tx);

        }
        private static object islock = new object();
        public static void AUTOALERT()
        {


            lock (islock)
            {
               
                var txLst = new SZHL_TXSXB().GetEntities(p => p.Status == "0");

                foreach (var model in txLst)
                {
                    try
                    {
                        bool canclose = false; //是否结束
                        bool cansend = false;  //是否发送提醒
                        bool upcount = false;  //是否更新次数
                        switch (model.TXType)
                        {
                            case "0":  //立即发送
                                {
                                    cansend = true;
                                    canclose = true;
                                    upcount = true;
                                }
                                break;
                            case "1":  //仅一次
                                {
                                    DateTime sd = DateTime.Parse(model.Date + " " + model.Hour + ":" + model.Minute + ":00");
                                    if (DateTime.Now >= sd)
                                    {
                                        cansend = true;
                                        canclose = true;
                                        upcount = true;
                                    }
                                }
                                break;
                            case "2":  //每个工作日
                                {
                                    DateTime sd = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00");

                                    if ((DateTime.Now.DayOfWeek == DayOfWeek.Monday
                                            || DateTime.Now.DayOfWeek == DayOfWeek.Tuesday
                                            || DateTime.Now.DayOfWeek == DayOfWeek.Wednesday
                                            || DateTime.Now.DayOfWeek == DayOfWeek.Thursday
                                            || DateTime.Now.DayOfWeek == DayOfWeek.Friday
                                            ) && DateTime.Now > sd && (model.LstSendTime == null || model.LstSendTime.Value.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd")))
                                    {
                                        upcount = true;
                                    }

                                }
                                break;
                            case "3":  //每天
                                {
                                    DateTime sd = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00");

                                    if ((DateTime.Now > sd && (model.LstSendTime == null || model.LstSendTime.Value.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))))
                                    {
                                        upcount = true;
                                    }
                                }
                                break;
                            case "5":  //每月
                                {
                                    DateTime sd = DateTime.Parse(DateTime.Now.ToString("yyyy-MM") + "-" + model.Days + " " + model.Hour + ":" + model.Minute + ":00");

                                    if ((DateTime.Now > sd && (model.LstSendTime == null || model.LstSendTime.Value.ToString("yyyy-MM") != DateTime.Now.ToString("yyyy-MM"))))
                                    {
                                        upcount = true;
                                    }
                                }
                                break;
                            case "4":  //自定义
                                {
                                    string Days = model.Days;
                                    foreach (var d in Days.Split(','))
                                    {
                                        if (getWkDays(d) == DateTime.Now.DayOfWeek)
                                        {
                                            DateTime sd = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + model.Hour + ":" + model.Minute + ":00");
                                            if ((DateTime.Now > sd && (model.LstSendTime == null || model.LstSendTime.Value.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))))
                                            {
                                                upcount = true;
                                            }
                                        }



                                    }

                                }
                                break;
                        }
                        if (upcount)
                        {
                            if (model.CFType == "1")
                            {
                                cansend = true;
                            }
                            else if (model.CFType == "2" && model.CFCount.Value > model.ZXCount) //次数
                            {
                                cansend = true;
                                if (model.CFCount.Value == model.ZXCount.Value + 1) //到次数，可以结束
                                {
                                    canclose = true;
                                }
                            }
                            else if (model.CFType == "3" && DateTime.Now < model.CFJZDate.Value)
                            {
                                cansend = true;
                            }

                            new SZHL_TXSXB().ExsSql("update SZHL_TXSX set ZXCount=isnull(ZXCount,0)+1,LstSendTime=getdate() where ID=" + model.ID);

                        }
                        if (cansend)
                        {
                            bool smsg = false;
                            bool swx = false;
                            bool swxapi = false;
                            if (model.Type == "0") //短信和微信
                            {
                                smsg = true;
                                swx = true;
                            }
                            else if (model.Type == "1")  //短信
                            {
                                smsg = true;
                            }
                            else if (model.Type == "2") //微信
                            {
                                swx = true;
                            }
                            else if (model.Type == "3")  //调接口
                            {
                                swxapi = true;
                            }

                            if (smsg)  //发短信
                            {
                                foreach (var m in model.TXUser.Split(','))
                                {
                                    object u = new SZHL_TXSXB().ExsSclarSql("select mobphone from JH_Auth_User where UserName='" + m + "' and comid='" + model.ComId + "'");
                                    if (u != null)
                                    {
                                        new SZHL_DXGLB().SendSMS(u.ToString(), model.CRUserRealName + "给您添加了提醒\n" + model.TXContent, model.ComId.Value);
                                    }
                                }

                            }
                            if (swx) //发微信
                            {
                                Article a = new Article();
                                a.Title = "日程提醒";

                                a.Description = model.CRUserRealName + "给您添加了提醒\n" + model.TXContent;
                                a.Url = model.ID.ToString();

                                List<Article> al = new List<Article>();
                                al.Add(a);
                                JH_Auth_UserB.UserInfo UserInfo = new JH_Auth_UserB().GetUserInfo(model.ComId.Value, model.CRUser);

                                try
                                {
                                    new JH_Auth_User_CenterB().SendMsg(UserInfo, model.TXMode, model.TXContent, model.ID.ToString(), model.TXUser, "A", 0, model.ISCS);
                                }
                                catch (Exception)
                                {
                                }

                                WXHelp wx = new WXHelp(UserInfo.QYinfo);
                                wx.SendTH(al, model.TXMode, model.WXLink, model.TXUser);
                            }
                            if (swxapi)  //调接口
                            {
                                try
                                {
                                    Msg_Result Model = new Msg_Result() { Action = model.FunName, ErrorMsg = "" };
                                    var container = ServiceContainerV.Current().Resolve<IWsService>(model.APIName.ToUpper());
                                    container.ProcessRequest(HttpContext.Current, ref Model, JsonConvert.SerializeObject(model), "", null);
                                }
                                catch (Exception ex)
                                {
                                    canclose = true;
                                    new JH_Auth_LogB().Insert(new JH_Auth_Log()
                                    {
                                        LogType = "TXSX",
                                        LogContent = ex.ToString(),
                                        CRDate = DateTime.Now
                                    });
                                }
                            }
                        }
                        if (canclose)
                        {
                            new SZHL_TXSXB().ExsSql("update SZHL_TXSX set Status='1' where ID=" + model.ID);
                        }
                    }
                    catch (Exception ex)
                    {
                        string ss = ex.Message;
                    }
                }
            }
        }


        /// <summary>
        /// 生日提醒
        /// </summary>
        public static void AUTOALERTSR()
        {
            try
            {

                List<JH_Auth_QY> QyModel = new JH_Auth_QYB().GetEntities(d => d.ComId != 0).ToList();
                WXHelp wx = new WXHelp(QyModel[0]);

                var qdata = new JH_Auth_UserB().GetEntities(d => d.Birthday != null);
                foreach (var item in qdata)
                {
                    if (item.Birthday.Value.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))
                    {
                        Article ar0 = new Article();
                        ar0.Title = "生日提醒";
                        ar0.Description = "";
                        ar0.Url = "http://www.baidu.com";
                        ar0.PicUrl = "";
                        List<Article> al = new List<Article>();
                        al.Add(ar0);
                        wx.SendTPMSG("XXFB", al, item.UserName);
                    }


                }
            }
            catch (Exception)
            {

            }
        }

        private static DayOfWeek getWkDays(string wk)
        {
            DayOfWeek dw = new DayOfWeek();
            switch (wk)
            {
                case "周一":
                    dw = DayOfWeek.Monday;
                    break;
                case "周二":
                    dw = DayOfWeek.Tuesday;
                    break;
                case "周三":
                    dw = DayOfWeek.Wednesday;
                    break;
                case "周四":
                    dw = DayOfWeek.Thursday;
                    break;
                case "周五":
                    dw = DayOfWeek.Friday;
                    break;
                case "周六":
                    dw = DayOfWeek.Saturday;
                    break;
                case "周日":
                    dw = DayOfWeek.Sunday;
                    break;
            }

            return dw;
        }
    }
}