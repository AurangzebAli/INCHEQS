using INCHEQS.Models;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Helpers;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using System.Text.RegularExpressions;
using INCHEQS.Security;

namespace INCHEQS.Areas.COMMON.Models.Message
{

    public class MessageDao : IMessageDao
    {
        private readonly ApplicationDbContext dbContext;
        //private readonly ISystemProfileDao systemProfileDao;
        //private readonly ISecurityProfileDao securityProfileDao;
        public MessageDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            //this.systemProfileDao = systemProfileDao;
            //this.securityProfileDao = securityProfileDao;
        }

        public MessageModel GetMessage(string Message)
        {

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            MessageModel message = new MessageModel();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessage", Message));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgMessage", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                message.fldBroadcastMessageId = row["fldBroadcastMessageId"].ToString();
                message.fldBroadcastMessage = row["fldBroadcastMessage"].ToString();
                message.fldCreateTimeStamp = row["fldCreateTimeStamp"].ToString();
            }
            else
            {
                message = null;
            }
            return message;
        }

        public MessageModel GetMessageTemp(string Message)
        {

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            MessageModel message = new MessageModel();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessage", Message));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgMessageTemp", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                message.fldBroadcastMessageId = row["fldBroadcastMessageId"].ToString();
                message.fldBroadcastMessage = row["fldBroadcastMessage"].ToString();
                message.fldApproveStatus = row["fldApproveStatus"].ToString();
            }
            else
            {
                message = null;
            }
            return message;
        }

        public MessageModel GetMessagebyId(string MessageId)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            MessageModel message = new MessageModel();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", MessageId));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgMessagebyId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                message.fldBroadcastMessageId = row["fldBroadcastMessageId"].ToString();
                message.fldBroadcastMessage = row["fldBroadcastMessage"].ToString();
                message.fldCreateTimeStamp = row["fldCreateTimeStamp"].ToString();
                message.fldCreateUserId = row["fldCreateUserId"].ToString();
            }
            else
            {
                message = null;
            }
            return message;
        }

        public MessageModel GetMessageTempbyId(string MessageId)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            MessageModel message = new MessageModel();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", MessageId));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBroadcastMessageMasterTempById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                message.fldBroadcastMessageId = row["fldBroadcastMessageId"].ToString();
                message.fldBroadcastMessage = row["fldBroadcastMessage"].ToString();
                message.fldCreateTimeStamp = row["fldCreateTimeStamp"].ToString();
                message.fldCreateUserId = row["fldCreateUserId"].ToString();
            }
            else
            {
                message = null;
            }
            return message;
        }


        public List<string> ValidateMessage(FormCollection col, string action)
        {
            List<string> err = new List<string>();

            if ((action.Equals("Update")))
            {
                if ((col["fldBroadcastMessage"].Equals("")))
                {
                    err.Add(Locale.MessageDescCannotBeBlank);
                }
            }
            else if ((action.Equals("Create")))
            {
                MessageModel CheckUserExist = GetMessage(col["fldBroadcastMessage"]);
                if ((CheckUserExist != null))
                {
                    err.Add(Locale.MessageDescExist);
                }
                if ((col["fldBroadcastMessage"].Equals("")))
                {
                    err.Add(Locale.MessageDescCannotBeBlank);
                }
            }
            return err;

        }

        public bool UpdateBroadcastMessageMaster(FormCollection col)
        {

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", col["fldBroadcastMessageId"]));
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessage", col["fldBroadcastMessage"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBroadcastMessageMaster", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }

        public void CreateBroadcastMessageMasterTemp(FormCollection col, string crtUser, string messageId, string Action)
        {
            List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
            string fldApproveStatus = "";
            string fldCreateUserId = "";
            string fldCreateTimeStamp = "";

            if (Action == "Update")
            {
                Action = "Update";
                fldApproveStatus = "U";

                MessageModel message = GetMessagebyId(col["fldBroadcastMessageId"]);
                fldCreateTimeStamp = message.fldCreateTimeStamp;
                fldCreateUserId = message.fldCreateUserId;
            }
            else if (Action == "Create")
            {
                Action = "Create";
                fldApproveStatus = "A";
                fldCreateUserId = crtUser;
                fldCreateTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
            }
            else {
                Action = "Delete";
                fldApproveStatus = "D";

                MessageModel message = GetMessagebyId(messageId);
                fldCreateTimeStamp = message.fldCreateTimeStamp;
                fldCreateUserId = message.fldCreateUserId;
                col["fldBroadcastMessage"] = message.fldBroadcastMessage;
            }

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@TableName", "tblBroadcastMessageMaster"));
            sqlParameterNext.Add(new SqlParameter("@NextNo", 0));
            sqlParameterNext[1].Direction = ParameterDirection.Output;
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcgCTCSNextSeqNo", sqlParameterNext.ToArray());
            long iNextNo = (long)sqlParameterNext[1].Value;

            string daf = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
            long iNext = iNextNo;

            if (Action == "Update")
            {
                sqlParameterNext1.Add(new SqlParameter("@fldBroadcastMessageId", col["fldBroadcastMessageId"]));
            }
            else if (Action == "Delete")
            {
                sqlParameterNext1.Add(new SqlParameter("@fldBroadcastMessageId", messageId));
            }
            else
            {
                sqlParameterNext1.Add(new SqlParameter("@fldBroadcastMessageId", iNext));
            }
            
            sqlParameterNext1.Add(new SqlParameter("@fldBroadcastMessage", col["fldBroadcastMessage"].ToString()));
            sqlParameterNext1.Add(new SqlParameter("@fldApproveStatus", fldApproveStatus));
            sqlParameterNext1.Add(new SqlParameter("@fldCreateUserId", fldCreateUserId));
            sqlParameterNext1.Add(new SqlParameter("@fldCreateTimeStamp", fldCreateTimeStamp));
            sqlParameterNext1.Add(new SqlParameter("@fldUpdateUserId", crtUser));
            sqlParameterNext1.Add(new SqlParameter("@fldUpdateTimeStamp", daf));
            sqlParameterNext1.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBroadcastMessageMasterTemp", sqlParameterNext1.ToArray());

        }

        public bool DeleteBroadcastMessageMaster(string messageId)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", messageId));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBroadcastMessageMaster", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }

        public bool CheckBroadcastMessageMasterTempById(string messageId)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", messageId));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBroadcastMessageMasterTempById", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CreateBroadcastMessageMaster(FormCollection col)
        {

            int intRowAffected;
            bool blnResult = false;
            

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            // XX START 20210602
            //sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", col["fldBroadcastMessageId"]));
            // XX END 20210602
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessage", col["fldBroadcastMessage"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBroadcastMessageMaster", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }

        public void MoveToBroadcastMessageMasterFromTemp(string messageId, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", messageId));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBroadcastMessageMasterFromTemp", sqlParameterNext.ToArray());
        }

        public bool DeleteBroadcastMessageMasterTemp(string messageId)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", messageId));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBroadcastMessageMasterTemp", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }

    }
}
