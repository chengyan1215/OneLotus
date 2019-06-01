using Newtonsoft.Json.Linq;
using QJY.BusinessData;
using QJY.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QJY.API
{

    


    #region 同事社区
    public class SZHL_TSSQB : BaseEFDao<SZHL_TSSQ>
    {

    }
    #endregion

    #region 人力资源

    #region 薪资管理
    public class SZHL_XZ_JLB : BaseEFDao<SZHL_XZ_JL> { }
    public class SZHL_XZ_GZDB : BaseEFDao<SZHL_XZ_GZD> { }

    public class SZHL_GZGLB : BaseEFDao<SZHL_GZGL> { }
    public class SZHL_GZGL_JCSZB : BaseEFDao<SZHL_GZGL_JCSZ> { }
    public class SZHL_GZGL_FLB : BaseEFDao<SZHL_GZGL_FL> { }
    public class SZHL_GZGL_WXYJB : BaseEFDao<SZHL_GZGL_WXYJ> { }
    #endregion


    #endregion

}