using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using QJY.Data;
using QJY.Common;

namespace QJY.API
{
    public class ServiceContainerV
    {
        public static IUnityContainer Current()
        {

            IUnityContainer container = new UnityContainer();



            //免注册接口类
            container.RegisterType<IWsService, Commanage>("Commanage".ToUpper());//

            #region 基础模块接口

            //基础接口
            container.RegisterType<IWsService, AuthManage>("XTGL".ToUpper());//
            container.RegisterType<IWsService, INITManage>("INIT".ToUpper());//系统配置相关API
            container.RegisterType<IWsService, XXFBManage>("XXFB");// 信息发布
            container.RegisterType<IWsService, FORMBIManage>("FORMBI".ToUpper());//流程审批
            container.RegisterType<IWsService, JSAPI>("JSSDK".ToUpper());            // JSAPI
            container.RegisterType<IWsService, DXGLManage>("DXGL".ToUpper());//短信管理 
            container.RegisterType<IWsService, TXLManage>("QYTX".ToUpper());//通讯录 
            container.RegisterType<IWsService, TXSXManage>("TXSX".ToUpper());//提醒事项 
            container.RegisterType<IWsService, QYWDManage>("QYWD".ToUpper());//企业文档 
            container.RegisterType<IWsService, NOTEManage>("NOTE".ToUpper());//记事本管理 
            container.RegisterType<IWsService, DBGLManage>("DBGL".ToUpper());//数据库管理
            container.RegisterType<IWsService, DataSourceManage>("BIDS".ToUpper());//数据源
            container.RegisterType<IWsService, DataSetManage>("BIDSET".ToUpper());//数据集
            container.RegisterType<IWsService, YBPManage>("BIYBP".ToUpper());//仪表盘
            container.RegisterType<IWsService, FORMBIManage>("FORMBI".ToUpper());//表单BI

            #endregion




            #region 人力资源
            container.RegisterType<IWsService, XZGLManage>("XZGL".ToUpper());//薪资管理 

            #endregion


            container.RegisterType<IWsService, TSSQManage>("TSSQ".ToUpper());//同事社区 









            return container;
        }

    }
}
