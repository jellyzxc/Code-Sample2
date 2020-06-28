using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using eDRCNet.EF.WCFService;
using eDRCNet.EF.DAL;
using Newtonsoft.Json;
using System.IO;
using eDRCNet.RS.Model;
using eDRCNet.SYS.BLL.WF;
using eDRCNet.SYS.Model;

namespace eDRCNet.RS.WCFService
{
    
    public class IRSServiceProvider : IEFWCFServiceProvider, IRSService
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

      
        #region 资源请求

        /// <summary>
        /// 我的资源列表分页数据
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="sort"></param>
        /// <param name="sortOrder"></param>
        /// <param name="UserID"></param>
        /// <param name="Title"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public string GetMyResourceListWithPage(int limit, int offset, string sort, string order, int UserID, string Title, int Status)
        {
            string where = "1=1 AND RequestByID = @RequestByID";
            if (!string.IsNullOrEmpty(Title))
            {
                where += " AND Title LIKE '%" + Title.Replace("'", "''") + "%'";
            }
            if (Status != -1)
            {
                where += " AND Status = @Status";
            }

            string orderby = " ORDER BY " + sort + " " + order;
            int total;
            List<ResourceRequestInfo> list = SqlMapDAL.CreateNameQuery("GetMyResourceList")
                .ReplaceText("1=1", where)
                .SetParameter("RequestByID", UserID)
                .SetParameter("Status", Status)
                .AppendText(orderby)
                .ListEntityByPage<ResourceRequestInfo>(offset + 1, limit, out total);
            return "{\"total\":" + total + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
        }

        /// <summary>
        /// 资源请求处理列表分页数据
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="sort"></param>
        /// <param name="sortOrder"></param>
        /// <param name="UserID"></param>
        /// <param name="Title"></param>
        /// <param name="Begin"></param>
        /// <param name="End"></param>
        /// <returns></returns>
        public string GetMyResourceTaskListWithPage(int limit, int offset, string sort, string order, int UserID, string Title, string From, string To)
        {
            string orderby = " ORDER BY " + sort + " " + order;
            string where = "SL.ExecutiveByID = @ExecutiveByID";
            if (!string.IsNullOrEmpty(Title))
            {
                where += " AND R.Title LIKE '%" + Title.Replace("'", "''") + "%'";
            }
            DateTime dtFrom, dtTo;
            if (DateTime.TryParse(From, out dtFrom))
            {
                where += " AND DATEDIFF(DAY, '" + dtFrom.ToString("yyyy-MM-dd") + "', R.EndDate) >= 0";
            }
            if (DateTime.TryParse(To, out dtTo))
            {
                where += " AND DATEDIFF(DAY, '" + dtTo.ToString("yyyy-MM-dd") + "', R.EndDate) <= 0";
            }

            int total;
            List<ResourceRequestInfo> list = SqlMapDAL.CreateNameQuery("GetMyResourceTaskList")
                .ReplaceText("1=1", where)
                .SetParameter("ExecutiveByID", UserID)
                .AppendText(orderby)
                .ListEntityByPage<ResourceRequestInfo>(offset + 1, limit, out total);
            return "{\"total\":" + total + ",\"rows\":" + JsonConvert.SerializeObject(list) + "}";
        }

        /// <summary>
        /// 获取资源请求详细信息
        /// </summary>
        /// <param name="RequestID"></param>
        /// <returns></returns>
        public ResourceRequestInfo GetMyResourceInfo(int RequestID)
        {
            return SqlMapDAL.CreateNameQuery("GetMyResourceInfo").SetParameter("RequestID", RequestID).Entity<ResourceRequestInfo>();
        }

        /// <summary>
        /// 获取步骤执行人列表
        /// </summary>
        /// <param name="RefID"></param>
        /// <param name="StepNum"></param>
        /// <returns></returns>
        public List<UserBasicInfo> GetResourceRequestRelatives(int RefID, int StepNum)
        {
            ResourceRequestWF workflow = new ResourceRequestWF();
            return workflow.GetRelatives(RefID, StepNum);
        }

        public List<UserBasicInfo> GetResourceRequestRelativesByProcessModelIDandStepNum(Guid ProcessModelID, int StepNum)
        {
            return SqlMapDAL.CreateNameQuery("GetResourceRequestRelativesByProcessModelIDandStepNum").SetParameter("ProcessModelID", ProcessModelID).SetParameter("StepNum", StepNum).ListEntity<UserBasicInfo>();
        }

        /// <summary>
        /// 提交资源请求
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Description"></param>
        /// <param name="EndDate"></param>
        /// <param name="UserID"></param>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public string ResourceRequest(string Title, string Description, DateTime EndDate, int UserID, string UserName, int UserType, int NextUserID, string NextUser)
        {
            string data = "{\"result\":\"success\",\"message\":\"请求已成功提交\"}";
            try
            {

                var pk = SqlMapDAL.CreateNameQuery("SaveResourceRequest")
                                .SetParameter("Title", Title)
                                .SetParameter("Description", Description)
                                .SetParameter("EndDate", EndDate)
                                .SetParameter("UserID", UserID)
                                .SetParameter("UserName", UserName)
                                .SetParameter("UserType", UserType)
                                .ExecuteScalar();
                int RequestID = int.Parse(pk.ToString());
                ResourceRequestWF workflow = new ResourceRequestWF();
                // 初始化流程
                string ProcessID = workflow.Create(RequestID, Title, "资源申请业务流程");
                workflow.Approve(RequestID, 0, UserID, UserName, NextUserID, NextUser, "提交资源请求", 1);
 
            }
            catch (Exception ex)
            {
                var obj = new { result = "fail", message = ex.Message };
                data = JsonConvert.SerializeObject(obj);
            }
            return data;
        }

        /// <summary>
        /// 更新资源请求FileID
        /// </summary>
        /// <param name="RequestID"></param>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public bool UpdateResourceRequestFileID(int RequestID, int FileID)
        {
            SqlMapDAL.CreateNameQuery("UpdateResourceRequestFileID").SetParameter("RequestID", RequestID).SetParameter("FileID", FileID).ExecuteNonQuery();
            return true;
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="RequestID"></param>
        /// <param name="Conclusion"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        public string ResourceRequestApprove(int RequestID, int Status, int Conclusion, string Content, int UserID, string UserName, int NextUserID, string NextUserName)
        {
            try
            {
                ResourceRequestWF workflow = new ResourceRequestWF();
                workflow.Approve(RequestID, Status, UserID, UserName, NextUserID, NextUserName, Content, Conclusion);
                ResourceRequestInfo resource = SqlMapDAL.CreateNameQuery("GetMyResourceInfo").SetParameter("RequestID", RequestID).Entity<ResourceRequestInfo>();

                var obj = new { result = "success", message = "操作成功", Title = resource.Title };
                return JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                var obj = new { result = "fail", message = ex.Message };
                return JsonConvert.SerializeObject(obj);
            }
        }

        #endregion
    }
}
