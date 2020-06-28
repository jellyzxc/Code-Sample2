using eDRCNet.EF.DAL;
using eDRCNet.SYS.Model;
using eDRCNet.SYS.Model.WF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace eDRCNet.SYS.BLL.WF
{
    public abstract class WorkFlowBase
    {
        /// <summary>
        /// 发起
        /// </summary>
        /// <param name="RefID">业务单据ID</param>
        /// <param name="RefName">状态</param>
        /// <param name="ProcessModalName">流程模板名称</param>
        public abstract string Create(Int64 RefID, string RefName, string ProcessModelName);

        /// <summary>
        /// 审批通过
        /// 通过当前步骤
        /// </summary>
        /// <param name="RefID">业务单据ID</param>
        public abstract bool Approve(long RefID, int Status, int userID, string userName, int nextUserID, string nextUserName, string Content = "", int Conclusion = 0, int RefIDType = 0);

        /// <summary>
        /// 归档
        /// </summary>
        /// <param name="RefID"></param>
        public abstract bool SaveData(long RefID);

        public abstract bool Nullify(long RefID, int Status, int userID, string userName, string Content = "", int Conclusion = 2, int RefIDType = 0);

        /// <summary>
        /// 初始化审批实例
        /// </summary>
        /// <param name="RefID">业务单据ID</param>
        /// <param name="RefName">业务单据名称</param>
        /// <param name="ProcessModelName">流程模板名称</param>
        /// <returns>流程实例ID</returns>
        public string Initialize(Int64 RefID, string RefName, string RefType)
        {
            object obj = SqlMapDAL.CreateNameQuery("ProcessInit")
                                 .SetParameter("RefID", RefID)
                                 .SetParameter("RefName", RefName)
                                 .SetParameter("Name", RefType)
                                 .ExecuteScalar();
            return obj.ToString();
        }

        /// <summary>
        /// 获取流程实例当前步骤
        /// </summary>
        /// <param name="RefID">业务单据ID</param>
        /// <returns></returns>
        public ProcessStepEntityInfo GetCurrentStepByRefID(Int64 RefID, string RefType)
        {
            try
            {
                ProcessStepEntityInfo entity = SqlMapDAL.CreateNameQuery("GetCurrentStepByRefID").SetParameter("RefID", RefID).SetParameter("RefType", RefType).Entity<ProcessStepEntityInfo>();
                if (entity != null && entity.ProcessStepModelID != Guid.Empty)
                {
                    entity.Relatives = GetRelatives(entity.ProcessStepModelID);
                }
                return entity;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取步骤执行人列表
        /// </summary>
        /// <param name="stepModelID">步骤模板id</param>
        /// <returns></returns>
        public List<UserBasicInfo> GetRelatives(Guid stepModelID)
        {
            return SqlMapDAL.CreateNameQuery("GetRelatives").SetParameter("ProcessStepModelID", stepModelID).ListEntity<UserBasicInfo>();
        }

        /// <summary>
        /// 获取上一步
        /// </summary>
        /// <param name="EntityID">流程实例ID</param>
        /// <param name="num">步骤序号</param>
        /// <returns></returns>
        public ProcessStepEntityInfo GetPreviousStep(Guid EntityID, int num)
        {
            return SqlMapDAL.CreateNameQuery("GetPreviousStep").SetParameter("ProcessEntityID", EntityID).SetParameter("StepNum", num).Entity<ProcessStepEntityInfo>();
        }

        /// <summary>
        /// 获取下一步
        /// </summary>
        /// <param name="EntityID">流程实例ID</param>
        /// <param name="num">步骤序号</param>
        /// <returns></returns>
        public ProcessStepEntityInfo GetNextStep(Guid EntityID, int num)
        {
            return SqlMapDAL.CreateNameQuery("GetNextStep").SetParameter("ProcessEntityID", EntityID).SetParameter("StepNum", num).Entity<ProcessStepEntityInfo>();
        }

        /// <summary>
        /// 获取流程指定步骤
        /// </summary>
        /// <param name="EntityID">流程实例ID</param>
        /// <param name="StepNum">流程步骤</param>
        /// <returns></returns>
        public ProcessStepEntityInfo GetProcessStep(Guid EntityID, int StepNum)
        {
            return SqlMapDAL.CreateNameQuery("GetProcessStep").SetParameter("ProcessEntityID", EntityID).SetParameter("StepNum", StepNum).Entity<ProcessStepEntityInfo>();
        }
        /// <summary>
        /// 获取流程指定步骤
        /// </summary>
        /// <param name="RefID">业务单据ID</param>
        /// <param name="RefType">业务单据类别</param>
        /// <param name="StepNum">步骤序号</param>
        /// <returns></returns>
        public ProcessStepEntityInfo GetProcessStep(long RefID, string RefType, int StepNum)
        {
            return SqlMapDAL.CreateNameQuery("GetProcessStepByRef")
                .SetParameter("RefID", RefID)
                .SetParameter("RefType", RefType)
                .SetParameter("StepNum", StepNum)
                .Entity<ProcessStepEntityInfo>();
        }

        /// <summary>
        /// 保存通过日志
        /// </summary>
        /// <param name="currentStep"></param>
        /// <param name="RefID"></param>
        /// <param name="userID"></param>
        /// <param name="userName"></param>
        /// <param name="Content"></param>
        /// <param name="Conclusion"></param>
        /// <param name="RefIDType"></param>
        /// <returns></returns>
        private bool SaveApproveLog(ProcessStepEntityInfo currentStep, ProcessStepEntityInfo preStep, ProcessStepEntityInfo nextStep, long RefID, int userID, string userName, int nextUserID, string nextUserName, string Content = "", int RefIDType = 0)
        {
            //------流程流转日志---------
            if (preStep == null)//上一步骤为空 写本次日志
            {
                ProcessStepLogInfo logInfoCurrent = new ProcessStepLogInfo(currentStep, RefID, currentStep.Status, userID, userName, DateTime.Now, userID, userName, DateTime.Now, Content, 1, RefIDType);
                SaveLog(logInfoCurrent);//记录本次操作的日志，提交人、执行人都为当前用户
            }
            else //上一步骤非空   update本次日志
            {
                ProcessStepLogInfo logInfoCurrent = GetStepLogByRefID(currentStep.ProcessEntityID);
                logInfoCurrent.ExecutiveBy = userName;
                logInfoCurrent.ExecutiveByID = userID;
                logInfoCurrent.ExecutiveTime = DateTime.Now;
                logInfoCurrent.Content = Content;
                logInfoCurrent.Conclusion = 1;
                logInfoCurrent.RefIDType = RefIDType;
                logInfoCurrent.Status = currentStep.Status;
                UpdateLog(logInfoCurrent);
            }

            if (nextStep != null)    //下一步骤非空， insert下一条日志的提交人
            {
                ProcessStepLogInfo logInfoNext = new ProcessStepLogInfo(nextStep, RefID, userID, userName, DateTime.Now);
                logInfoNext.ExecutiveByID = nextUserID;
                logInfoNext.ExecutiveBy = nextUserName;
                SaveLog(logInfoNext);
            }

            return true;
        }

        /// <summary>
        /// 保存打回日志
        /// </summary>
        /// <param name="currentStep"></param>
        /// <param name="RefID"></param>
        /// <param name="userID"></param>
        /// <param name="userName"></param>
        /// <param name="Content"></param>
        /// <param name="Conclusion"></param>
        /// <param name="RefIDType"></param>
        /// <returns></returns>
        private bool SaveFailLog(ProcessStepEntityInfo currentStep, ProcessStepEntityInfo preStep, long RefID, int userID, string userName, string Content = "", int RefIDType = 0)
        {
            //------日志---------
            ProcessStepLogInfo logInfoCurrent = GetStepLogByRefID(currentStep.ProcessEntityID);//该表单待更新的日志
            logInfoCurrent.ExecutiveBy = userName;
            logInfoCurrent.ExecutiveByID = userID;
            logInfoCurrent.ExecutiveTime = DateTime.Now;
            logInfoCurrent.Content = Content;
            logInfoCurrent.Conclusion = 0;
            logInfoCurrent.RefIDType = RefIDType;
            logInfoCurrent.Status = preStep.Status;
            UpdateLog(logInfoCurrent);
            if (preStep != null)    //上一步骤非空， insert 下一条日志的提交人
            {
                ProcessStepLogInfo logInfoNext = new ProcessStepLogInfo(preStep, RefID, userID, userName, DateTime.Now);
                SaveLog(logInfoNext);
            }

            return true;
        }

        /// <summary>
        /// 作废业务单据时要终止流程，此时记日志，审批结果为“作废”
        /// </summary>
        /// <param name="currentStep"></param>
        /// <param name="RefID"></param>
        /// <param name="userID"></param>
        /// <param name="userName"></param>
        /// <param name="Content"></param>
        /// <param name="RefIDType"></param>
        /// <returns></returns>
        public bool SaveNullifyLog(ProcessStepEntityInfo currentStep, long RefID, int userID, string userName, string Content, int RefIDType = 0)
        {
            ProcessStepLogInfo logInfoCurrent = GetStepLogByRefID(currentStep.ProcessEntityID);
            logInfoCurrent.ExecutiveBy = userName;
            logInfoCurrent.ExecutiveByID = userID;
            logInfoCurrent.ExecutiveTime = DateTime.Now;
            logInfoCurrent.Content = Content;
            logInfoCurrent.Conclusion = 2;
            logInfoCurrent.RefIDType = RefIDType;
            logInfoCurrent.Status = "已作废";
            UpdateLog(logInfoCurrent);

            return true;
        }

        /// <summary>
        /// 获取最后一条待更新的日志
        /// </summary>
        /// <param name="RefID"></param>
        /// <returns></returns>
        public ProcessStepLogInfo GetStepLogByRefID(Guid ProcessEntityID)
        {
            return SqlMapDAL.CreateNameQuery("GetStepLogByRefID").SetParameter("ProcessEntityID", ProcessEntityID).Entity<ProcessStepLogInfo>();
        }

        /// <summary>
        /// 添加一条流程操作日志
        /// </summary>
        /// <param name="logInfo"></param>
        public void SaveLog(ProcessStepLogInfo logInfo)
        {
            SqlMapDAL.CreateNameQuery("SaveLog")
                    .SetParameter("ProcessEntityID", logInfo.ProcessEntityID)
                    .SetParameter("RefID", logInfo.RefID)
                    .SetParameter("ExecutiveByID", logInfo.ExecutiveByID)
                    .SetParameter("ExecutiveBy", logInfo.ExecutiveBy)
                    .SetParameter("ExecutiveTime", logInfo.ExecutiveTime)
                    .SetParameter("SubmitterID", logInfo.SubmitterID)
                    .SetParameter("SubmitBy", logInfo.SubmitBy)
                    .SetParameter("SubmitTime", logInfo.SubmitTime)
                    .SetParameter("Content", logInfo.Content)
                    .SetParameter("Conclusion", logInfo.Conclusion)
                    .SetParameter("RefIDType", logInfo.RefIDType)
                    .SetParameter("ProcessStepEntityID", logInfo.ProcessStepEntityID)
                    .SetParameter("StepName", logInfo.StepName)
                    .SetParameter("StepNum", logInfo.StepNum)
                    .SetParameter("Status", logInfo.Status)
                    .ExecuteNonQuery();
        }

        /// <summary>
        /// 更新流程操作日志
        /// </summary>
        /// <param name="logInfo"></param>
        public void UpdateLog(ProcessStepLogInfo logInfo)
        {
            SqlMapDAL.CreateNameQuery("UpdateLog")
                .SetParameter("ExecutiveByID", logInfo.ExecutiveByID)
                .SetParameter("ExecutiveBy", logInfo.ExecutiveBy)
                .SetParameter("ExecutiveTime", logInfo.ExecutiveTime)
                .SetParameter("Content", logInfo.Content)
                .SetParameter("Conclusion", logInfo.Conclusion)
                .SetParameter("RefIDType", logInfo.RefIDType)
                .SetParameter("StepName", logInfo.StepName)
                .SetParameter("StepNum", logInfo.StepNum)
                .SetParameter("Status", logInfo.Status)
                .SetParameter("logID", logInfo.logID)
                .ExecuteNonQuery();
        }

        /// <summary>
        /// 执行审核操作
        /// </summary>
        /// <param name="currentStep"></param>
        /// <param name="RefID"></param>
        /// <param name="userID"></param>
        /// <param name="userName"></param>
        /// <param name="Content"></param>
        /// <param name="Conclusion"></param>
        /// <param name="RefIDType"></param>
        /// <returns></returns>
        public bool SetApprove(ProcessStepEntityInfo currentStep, long RefID, int userID, string userName, int nextUserID, string nextUserName, string Content = "", int Conclusion = 0, int RefIDType = 0)
        {
            //上一步骤,下一步骤
            ProcessStepEntityInfo preStep = GetPreviousStep(currentStep.ProcessEntityID, currentStep.StepNum);
            ProcessStepEntityInfo nextStep = GetNextStep(currentStep.ProcessEntityID, currentStep.StepNum);

            if (Conclusion == 1)
            {
                // 通过
                SetApproveStatusApprove(currentStep.ProcessEntityID, currentStep.Status, userID, userName, nextUserID, nextUserName);
                SaveApproveLog(currentStep, preStep, nextStep, RefID, userID, userName, nextUserID, nextUserName, Content, RefIDType);
            }
            else
            {
                // 打回
                SetApproveStatusFail(currentStep.ProcessEntityID, preStep.Status, userID, userName);
                SaveFailLog(currentStep, preStep, RefID, userID, userName, Content, RefIDType);
            }
            return true;
        }

        /// <summary>
        /// 通过，进入下一步
        /// </summary>
        /// <param name="stepInfo"></param>
        public void SetApproveStatusApprove(Guid ProcessEntityID, string Status, int ExecutiveByID, string ExecutiveBy, int nextUserID, string nextUserName)
        {
            SqlMapDAL.CreateNameQuery("SetApproveStatusApprove")
                .SetParameter("ProcessEntityID", ProcessEntityID)
                .SetParameter("Status", Status)
                .SetParameter("ExecutiveByID", ExecutiveByID)
                .SetParameter("ExecutiveBy", ExecutiveBy)
                .SetParameter("NextUserID", nextUserID)
                .SetParameter("NextUserName", nextUserName)
                .ExecuteNonQuery();
        }

        /// <summary>
        /// 打回，回到上一步
        /// </summary>
        /// <param name="ProcessEntityID">流程实例ID</param>
        /// <param name="Status">状态(打回后流程实例的状态)</param>
        /// <param name="ExecutiveByID"></param>
        /// <param name="ExecutiveBy"></param>
        public void SetApproveStatusFail(Guid ProcessEntityID, string Status, int ExecutiveByID, string ExecutiveBy)
        {
            SqlMapDAL.CreateNameQuery("SetApproveStatusFail")
                .SetParameter("ProcessEntityID", ProcessEntityID)
                .SetParameter("Status", Status)
                .SetParameter("ExecutiveByID", ExecutiveByID)
                .SetParameter("ExecutiveBy", ExecutiveBy)
                .ExecuteNonQuery();
        }

        /// <summary>
        /// 结束流程
        /// </summary>
        /// <param name="ProcessEntityID"></param>
        public void SetProcessFinish(Guid ProcessEntityID)
        {
            SqlMapDAL.CreateNameQuery("SetProcessFinish").SetParameter("ProcessEntityID", ProcessEntityID).ExecuteNonQuery();
        }
    }
}
