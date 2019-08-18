using Log2Net.Models;
using LogORM.ComUtil;
using LogORM.Models;
using LogORM.SqlExtensions;
using LogORMWeb.Models;
using LogORMWeb_DNC.Dals;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace LogORMWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            var dic = Log2Net.LogApi.GetLogWebApplicationsName();

            var curDal = new Log_OperateTraceAdoDal();
            DBOperUser dbUser = new DBOperUser() { UserId = "CNNO2", UserName = "李大大" };


            #region 测试获取数据
            var dbData = curDal.GetAll(new LogORM.Models.PageSerach<Log_OperateTrace>()
            {
                Filter = a => a.Id > 0,
                OrderBy = a => a.OrderByDescending(m => m.LogTime),
                PageSize = 2
            });
            var dbModels = DtModelConvert<List<Log_OperateTrace>>.DeepClone(dbData.ExeModel);
            var selectModels = dbModels.ConvertAll(a => (object)a);
            #endregion 测试获取数据

            #region 测试添加更新删除
            LogORM.Models.AddDBPara<Log_OperateTrace> addDBPara = new LogORM.Models.AddDBPara<Log_OperateTrace>()
            {
                Model = new Log_OperateTrace()
                {
                    ClientHost = "江南可采莲",
                    Detail = "鱼戏莲叶间",
                    ClientIP = "鱼戏莲叶东西南北中间",
                    LogTime = System.DateTime.Now,
                    LogType = LogType.添加,
                    ServerHost = "鱼戏莲叶下上左右后前",
                    ServerIP = "鱼莲玩嗨乐翻天",
                    SystemID = SysCategory.SysA_02,
                    TabOrModu = "莲叶变黄了_DNC",
                    UserID = "鱼还没戏够",
                    UserName = "下周继续",
                }
            };
            var resAdd = curDal.Add(addDBPara, new DBOperUser() { UserId = "CN666", UserName = "fanbingbing" });

            var resUpdate = curDal.Update(new Dictionary<string, object>() { { "id", 2 } }, new Dictionary<string, object>() { { "Detail", "后事如何，下回分解" } });

            var delRes = curDal.Delete(new Dictionary<string, object>() { { "id", 2 } }, dbUser);
            var delRes2 = curDal.Delete(3, dbUser);

            #endregion 测试添加更新删除

            #region 测试批量添加和更新
            Log_OperateTrace curAddLog = new Log_OperateTrace()
            {
                ClientHost = "江南可采莲",
                Detail = "鱼戏莲叶间",
                ClientIP = "鱼戏莲叶东西南北中间",
                LogTime = System.DateTime.Now,
                LogType = LogType.添加,
                ServerHost = "鱼戏莲叶下上左右后前",
                ServerIP = "鱼莲玩嗨乐翻天",
                SystemID = SysCategory.SysA_02,
                TabOrModu = "莲叶变黄了_DNC",
                UserID = "鱼还没戏够",
                UserName = "下周继续",
            };

            List<AddUpdateDelEdm> AddUpdateDelEdms = new List<AddUpdateDelEdm>();
            AddUpdateDelEdms.Add(new AddUpdateDelEdm() { TableName = "Log_OperateTrace", Datas = new List<object>() { curAddLog } });
            var resBtAdd = curDal.AddUpdateDelete(AddUpdateDelEdms, new DBOperUser() { UserId = "CN1234", UserName = "韩梅梅" });

            //以下为更新
            AddUpdateDelEdms.Add(new AddUpdateDelEdm() { TableName = "Log_OperateTrace", Datas = selectModels });
            AddUpdateDelEdms[0].Datas = (selectModels);
            AddUpdateDelEdms[0].UpdateFD = new List<Dictionary<string, string>> { new Dictionary<string, string> { { "ServerIP", "1.1.1.1" } } };
            var resBtUpdate = curDal.AddUpdateDelete(AddUpdateDelEdms, new DBOperUser() { UserId = "CN12348", UserName = "Lucy" });
            #endregion 测试批量添加和更新

            #region 测试存储过程
            DbParameter[] spParameters = new SqlParameter[] { new SqlParameter("@userid", "CN4096"), new SqlParameter("@bok", System.Data.SqlDbType.Int) };
            spParameters[1].Direction = System.Data.ParameterDirection.Output;
            spParameters[1].Value = 0;
            var spRes = curDal.ExecuteStoredProcedure("getInsertLog", new DBOperUser() { UserId = "CN8192", UserName = "张三丰" }, spParameters);
            #endregion 测试存储过程

            #region 测试日志记录
            LogTraceVM model = new LogTraceVM()
            {
                Detail = "所有的程序员都是天才编剧，所有的计算机都是烂演员",
                LogType = LogType.业务记录,
                Remark = "文学奖评选",
                TabOrModu = "计算机编程",
            };
            var logRes = new ComClass().WriteLog(LogLevel.Info, model);
            #endregion 测试日志记录

            //测试获取Sql
            var sql = curDal.CurSqlProvider.Select("username", "realname", "age").From("sys_user").Where<KeyValue>(a => a.Name == "username1").SqlString;

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            LogTraceVM logModel = new LogTraceVM() { Detail = "进入了关于页面" };
            new ComClass().WriteLog(LogLevel.Info, logModel);
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        class KeyValue
        {
            public string Name { get; set; }

            public string Value { get; set; }
        }
    }
}
