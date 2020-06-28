using eDRCNet.SYS.Model.WF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eDRCNet.Framework;

namespace eDRCNet.SYS.BLL.WF
{
    public class WorkFlowInstance : WorkFlowBase
    {
        /// <summary>
        /// 发起流程，生成流程实例和流程步骤实例
        /// </summary>
        /// <param name="RefID"></param>
        /// <param name="RefName"></param>
        /// <param name="ProcessModelName"></param>
        public override string Create(long RefID, string RefName, string ProcessModelName)
        {
            // 创建流程实例，返回流程实例ID
            string EntityID = base.Initialize(RefID, RefName, ProcessModelName);
            return EntityID;
        }
        //向下执行--通过
        public override bool Approve(long RefID, int Status, int userID, string userName, int nextUserID, string nextUserName, string Content = "", int Conclusion = 0, int RefIDType = 0)
        {
            throw new NotImplementedException();
        }

        public override bool SaveData(long RefID)
        {
            throw new NotImplementedException();
        }

        public override bool Nullify(long RefID, int Status, int userID, string userName, string Content = "", int Conclusion = 2, int RefIDType = 0)
        {
            throw new NotImplementedException();
        }
        
    }
}
