using eDRCNet.EF.Common;
using eDRCNet.EF.DAL;
using eDRCNet.RS.Model;
using eDRCNet.SYS.Model;
using eDRCNet.SYS.Model.WF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eDRCNet.SYS.BLL.WF
{
    public class ResourceRequestWF : WorkFlowBase
    {
        /// <summary>
        /// 发起
        /// </summary>
        /// <param name="RefID"></param>
        /// <param name="status"></param>
        /// <param name="ProcessModelName"></param>
        /// <returns></returns>
        public override string Create(long RefID, string RefName, string ProcessModelName)
        {
            return base.Initialize(RefID, RefName, ProcessModelName); ;
        }

        /// <summary>
        /// 审核（通过、打回都在此处理）
        /// </summary>
        /// <param name="RefID"></param>
        /// <param name="userID"></param>
        /// <param name="userName"></param>
        /// <param name="Content"></param>
        /// <param name="Conclusion"></param>
        /// <param name="RefIDType"></param>
        /// <returns></returns>
        public override bool Approve(long RefID, int Status, int userID, string userName, int nextUserID, string nextUserName, string Content = "", int Conclusion = 0, int RefIDType = 0)
        {
            ResourceRequestInfo resource = SqlMapDAL.CreateNameQuery("GetMyResourceInfo").SetParameter("RequestID", RefID).Entity<ResourceRequestInfo>();
            if (resource == null || resource.Status != Status)
            {
                return false;
            }

            ProcessStepEntityInfo currentStep = base.GetCurrentStepByRefID(RefID, "资源申请业务流程");
            if (currentStep != null)
            {
                try
                {
                    int RequestUserType = GetRequestUserType(resource.RequestByID);////审核申请，当前用户不一定是申请人，要用任务的申请人

                    //上一步骤,下一步骤
                    ProcessStepEntityInfo prevStep = GetProcessStep(currentStep.ProcessEntityID, currentStep.StepNum - 1);
                    ProcessStepEntityInfo nextStep = GetProcessStep(currentStep.ProcessEntityID, currentStep.StepNum + 1);

                    int nextStatus = resource.Status + (Conclusion == 1 ? 1 : -1);
                    switch (RequestUserType)
                    {
                        case 1:
                            if (currentStep.StepNum == 1)
                            {
                                // 所内用户提交请求，自动进行1，2，3步骤
                                base.SetApprove(currentStep, RefID, userID, userName, 0, "", Content, Conclusion, RefIDType);
                                base.SetApprove(nextStep, RefID, userID, userName, 0, "", "所内用户申请，自动通过审批", Conclusion, RefIDType);
                                nextStep = GetProcessStep(currentStep.ProcessEntityID, currentStep.StepNum + 2);
                                base.SetApprove(nextStep, RefID, userID, userName, nextUserID, nextUserName, "所内用户申请，自动通过审批", Conclusion, RefIDType);
                                nextStatus = 3;
                            }
                            else if (currentStep.StepNum == 4 && Conclusion == 0)
                            {
                                // 所内用户请求中心主任分派步骤打回，直接打回至请求用户(即作废请求)
                                base.SetApprove(currentStep, RefID, userID, userName, 0, "", Content, Conclusion, RefIDType);
                                prevStep = null;
                                nextStatus = 0;
                            }
                            else if (currentStep.StepNum == 6 && Conclusion == 1)
                            {
                                // 所内用户请求中心主任审核步骤通过，直接通过至请求人
                                base.SetApprove(currentStep, RefID, userID, userName, 0, "", Content, Conclusion, RefIDType);
                                base.SetApprove(nextStep, RefID, userID, userName, 0, "", "所内用户申请，自动通过审批", Conclusion, RefIDType);
                                nextStep = GetProcessStep(currentStep.ProcessEntityID, currentStep.StepNum + 2);
                                base.SetApprove(nextStep, RefID, userID, userName, nextUserID, nextUserName, "所内用户申请，自动通过审批", Conclusion, RefIDType);
                                nextStep = null;
                                nextStatus = 8;
                            }
                            else
                            {
                                base.SetApprove(currentStep, RefID, userID, userName, nextUserID, nextUserName, Content, Conclusion, RefIDType);
                            }
                            break;
                        case 2:
                            if (currentStep.StepNum == 1)
                            {
                                // 科技厅系统内用户，自动进行1，2步骤
                                base.SetApprove(currentStep, RefID, userID, userName, 0, "", Content, Conclusion, RefIDType);
                                base.SetApprove(nextStep, RefID, userID, userName, nextUserID, nextUserName, "厅系统内部用户申请，自动通过审批", Conclusion, RefIDType);
                                nextStatus = 2;
                            }
                            else if (currentStep.StepNum == 3 && Conclusion == 0)
                            {
                                // 科技厅系统内用户请求所领导分发步骤打回，直接打回至请求用户
                                base.SetApprove(currentStep, RefID, userID, userName, 0, "", Content, Conclusion, RefIDType);
                                prevStep = null;
                                nextStatus = 0;
                            }
                            else if (currentStep.StepNum == 7 && Conclusion == 1)
                            {
                                // 科技厅系统内用户请求所领导审核步骤通过，直接通过至请求人
                                base.SetApprove(currentStep, RefID, userID, userName, 0, "", Content, Conclusion, RefIDType);
                                base.SetApprove(prevStep, RefID, userID, userName, nextUserID, nextUserName, "厅系统内部用户申请，自动通过审批", Conclusion, RefIDType);
                                nextStep = null;
                                nextStatus = 8;
                            }
                            else
                            {
                                base.SetApprove(currentStep, RefID, userID, userName, nextUserID, nextUserName, Content, Conclusion, RefIDType);
                            }
                            break;
                        case 3:
                            // 普通外部用户
                            base.SetApprove(currentStep, RefID, userID, userName, nextUserID, nextUserName, Content, Conclusion, RefIDType);
                            break;
                        default:
                            // 普通外部用户
                            base.SetApprove(currentStep, RefID, userID, userName, nextUserID, nextUserName, Content, Conclusion, RefIDType);
                            break;
                    }
                    // 更新单据状态
                    UpdateRequestStatus(RefID, nextStatus);

                    #region 发送通知
                    int reciever;
                    string message;
                    if (Conclusion == 1)
                    {
                        if (nextStep != null)
                        {
                            // 通过，通知下一步执行人
                            reciever = nextUserID;
                            message = "有新的资源请求【" + resource.Title + "】，请及时处理。";
                            MessageBLL.SendTask(userID, reciever.ToString(), message, "/RS/Resource/ResourceTaskList");
                        }
                        else
                        {
                            // 归档，通知资源申请人
                            reciever = resource.RequestByID;
                            message = "您的资源请求【" + resource.Title + "】已完成，请下载查看。";
                            MessageBLL.SendMessage(userID, reciever.ToString(), message, "/RS/Resource/ResourceList");
                        }
                    }
                    else
                    {
                        if (prevStep != null)
                        {
                            // 打回，通知上一步执行人
                            reciever = prevStep.ExecutiveByID;
                            message = "资源请求【" + resource.Title + "】被打回，请及时处理";
                            MessageBLL.SendTask(userID, reciever.ToString(), message, "/RS/Resource/ResourceTaskList");
                        }
                        else
                        {
                            // 打回，通知资源申请人
                            reciever = resource.RequestByID;
                            message = "您的资源请求【" + resource.Title + "】被驳回，请知悉。";
                            MessageBLL.SendMessage(userID, reciever.ToString(), message, "/RS/Resource/ResourceTaskList");
                        }
                    }
                    #endregion

                    return true;
                }
                catch (Exception e)
                {
                    LogUtility.WriteLog("审核操作失败：" + e.Message);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 归档
        /// </summary>
        /// <param name="RefID"></param>
        /// <returns></returns>
        public override bool SaveData(long RefID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 作废
        /// </summary>
        /// <param name="RefID"></param>
        /// <param name="userID"></param>
        /// <param name="userName"></param>
        /// <param name="Content"></param>
        /// <param name="Conclusion"></param>
        /// <param name="RefIDType"></param>
        /// <returns></returns>
        public override bool Nullify(long RefID, int Status, int userID, string userName, string Content = "", int Conclusion = 2, int RefIDType = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取执行人列表
        /// </summary>
        /// <param name="RefID"></param>
        /// <param name="stepNum"></param>
        /// <returns></returns>
        public List<UserBasicInfo> GetRelatives(int RefID, int stepNum)
        {
            List<UserBasicInfo> list = new List<UserBasicInfo>();
            ProcessStepEntityInfo step;
            UserBasicInfo user;
            string sql;
            switch (stepNum)
            {
                case 2:
                case 3:
                case 4:
                case 8:
                    step = GetProcessStep(RefID, "资源申请业务流程", stepNum);
                    list = base.GetRelatives(step.ProcessStepModelID);
                    break;
                case 5:
                    // 第四步执行人的下属
                    step = GetProcessStep(RefID, "资源申请业务流程", 4);
                    sql = @"SELECT  U.*
                            FROM    dbo.SYS_R_User_Department UD
		                            INNER JOIN dbo.SYS_R_User_Department UD1 ON UD1.depID = UD.depID
                                    INNER JOIN dbo.SYS_User U ON U.userID = UD1.UserID
                            WHERE	UD.UserID = @UserID AND UD.IsSupervisor = 1";
                    list = SqlMapDAL.CreateNameQuery("QueryDb").ReplaceText("{0}", sql).SetParameter("UserID", step.ExecutiveByID).ListEntity<UserBasicInfo>();
                    break;
                case 6:
                    // 第四步执行人
                    step = GetProcessStep(RefID, "资源申请业务流程", 4);
                    user = new UserBasicInfo();
                    user.UserID = step.ExecutiveByID;
                    user.UserName = step.ExecutiveBy;
                    list.Add(user);
                    break;
                case 7:
                    // 第三步执行人
                    step = GetProcessStep(RefID, "资源申请业务流程", 3);
                    user = new UserBasicInfo();
                    user.UserID = step.ExecutiveByID;
                    user.UserName = step.ExecutiveBy;
                    list.Add(user);
                    break;
            }
            return list;
        }

        /// <summary>
        /// 更新单据状态
        /// </summary>
        /// <param name="RequestID"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        private bool UpdateRequestStatus(long RequestID, int Status)
        {
            string sql = @"UPDATE dbo.RS_Request SET Status =  @Status, LastUpdateDate = GETDATE() WHERE RequestID = @RequestID";
            SqlMapDAL.CreateNameQuery("QueryDb")
                .ReplaceText("{0}", sql)
                .SetParameter("RequestID", RequestID)
                .SetParameter("Status", Status)
                .ExecuteNonQuery();
            return true;
        }

        //1:所内用户  2：科技厅用户   3：纯外部用户
        public int GetRequestUserType(int UserID)
        {
            UserBasicInfo currentUser = GetBasicUserInfoByUserID(UserID);
            if (currentUser.UserTypeID == 1)
            {
                return 1;
            }
            else
            {
                OuterUserInfo OuterUser = GetOuterUserInfoByUserID(UserID);
                bool var1 = OuterUser.Roles.Exists((RoleInfo r) => r.RoleId == 4);//科技厅领导角色是4
                bool var2 = false;
                if (OuterUser.InstitueName != null)
                {
                    var2 = OuterUser.InstitueName.Contains("科技厅");//机构名称中有科技厅
                }
                if (var1 || var2) return 2;
                else return 3;
            }
        }

        //避免调用ISYSServiceProvider,自此重写一下
        private UserBasicInfo GetBasicUserInfoByUserID(int userId)
        {
            return SqlMapDAL.CreateNameQuery("GetBasicUserInfoByUserID").SetParameter("userID", userId).ListEntity<UserBasicInfo>().FirstOrDefault();
        }
        private OuterUserInfo GetOuterUserInfoByUserID(int userId)
        {
            OuterUserInfo info = new OuterUserInfo();
            info = SqlMapDAL.CreateNameQuery("GetOuterUserInfoByUserID").SetParameter("userId", userId).ListEntity<OuterUserInfo>().FirstOrDefault();
            if (info != null)
            {
                info.Roles = SqlMapDAL.CreateNameQuery("GetRolesByUserID").SetParameter("userId", userId).ListEntity<RoleInfo>();
            }
            return info;

        }
    }
}
