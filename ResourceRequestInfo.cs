using eDRCNet.EF.Model;
using eDRCNet.SYS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace eDRCNet.RS.Model
{
    [DataContract]
    [Serializable]
    public class ResourceRequestInfo : Entitybase
    {
        [DataMember]
        public int RequestID { get { return GetPropertyValue<int>("RequestID"); } set { SetPropertyValue("RequestID", value); } }
        [DataMember]
        public int RequestByID { get { return GetPropertyValue<int>("RequestByID"); } set { SetPropertyValue("RequestByID", value); } }
        [DataMember]
        public string RequestBy { get { return GetPropertyValue<string>("RequestBy"); } set { SetPropertyValue("RequestBy", value); } }
        [DataMember]
        public DateTime RequestDate { get { return GetPropertyValue<DateTime>("RequestDate"); } set { SetPropertyValue("RequestDate", value); } }
        [DataMember]
        public string Title { get { return GetPropertyValue<string>("Title"); } set { SetPropertyValue("Title", value); } }
        [DataMember]
        public string Description { get { return GetPropertyValue<string>("Description"); } set { SetPropertyValue("Description", value); } }
        [DataMember]
        public DateTime EndDate { get { return GetPropertyValue<DateTime>("EndDate"); } set { SetPropertyValue("EndDate", value); } }
        [DataMember]
        public int Status { get { return GetPropertyValue<int>("Status"); } set { SetPropertyValue("Status", value); } }
        [DataMember]
        public DateTime LastUpdateDate { get { return GetPropertyValue<DateTime>("LastUpdateDate"); } set { SetPropertyValue("LastUpdateDate", value); } }
        [DataMember]
        public long FileID { get { return GetPropertyValue<long>("FileID"); } set { SetPropertyValue("FileID", value); } }
        [DataMember]
        public string FileName { get { return GetPropertyValue<string>("FileName"); } set { SetPropertyValue("FileName", value); } }
        [DataMember]
        public int CurrentExecutiveByID { get { return GetPropertyValue<int>("CurrentExecutiveByID"); } set { SetPropertyValue("CurrentExecutiveByID", value); } }
        [DataMember]
        public List<UserBasicInfo> ExecutiverList { get { return GetPropertyValue<List<UserBasicInfo>>("ExecutiverList"); } set { SetPropertyValue("ExecutiverList", value); } }
        [DataMember]
        public int UserType { get { return GetPropertyValue<int>("UserType"); } set { SetPropertyValue("UserType", value); } }
    }
}
