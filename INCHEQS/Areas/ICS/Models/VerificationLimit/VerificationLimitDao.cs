using INCHEQS.ConfigVerification.VerificationLimit;
using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
//using INCHEQS.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.ConfigVerification.Resource;

namespace INCHEQS.Areas.ICS.Models.VerificationLimit
{
    public class VerificationLimitDao : IVerificationLimitDao
    {

        private readonly ApplicationDbContext dbContext;
        public VerificationLimitDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DataTable ListVerificationLimit()
        {
            string stmt = "SELECT * FROM tblverificationbatchsizelimit";
            return dbContext.GetRecordsAsDataTable(stmt); ;
        }
        public DataTable Find(string classId)
        {

            string stmt = "SELECT fldClass,fld1stAmt,fld2ndAmt,fld1stType,fld2ndType,fldConcatenate,fldLimitDesc, fldUpdateTimeStamp FROM tblVerificationBatchSizeLimit WHERE fldClass = @fldClass";

            return dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldClass", classId) });
        }
        public void Update(FormCollection collection, string currentUserId)
        {

            string stmt = "update tblVerificationBatchSizeLimit set fld1stAmt=@fld1stAmt,fld2ndAmt=@fld2ndAmt,fld1stType=@fld1stType,fld2ndType=@fld2ndType,fldConcatenate=@fldConcatenate,fldLimitDesc=@fldLimitDesc,fldUpdateUserId=@fldUpdateUserId,fldUpdateTimeStamp=@fldUpdateTimeStamp where fldClass=@fldClass";

            string secondType = collection["secondType"];
            if (secondType == null)
            { //because dropdown is disabled
                secondType = "";
            }

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldClass", collection["txtclass"]),
                new SqlParameter("@fld1stAmt", collection["firstAmount"].ToString()),
                new SqlParameter("@fld2ndAmt", collection["secondAmount"].ToString()),
                new SqlParameter("@fld1stType", collection["firstType"]),
                new SqlParameter("@fld2ndType", secondType),
                new SqlParameter("@fldConcatenate", collection["concat"]),
                new SqlParameter("@fldLimitDesc", collection["VerifyLimitDesc"]),
                new SqlParameter("@fldUpdateUserId", currentUserId),
                new SqlParameter("@fldUpdateTimeStamp", DateTime.Now)
            });
        }

        public void CreateVerificationBatchSizeLimitTemp(FormCollection collection, string currentUserId)
        {

            string stmt = "insert into tblVerificationBatchSizeLimitTemp (fldClass,fld1stAmt,fld2ndAmt,fld1stType,fld2ndType,fldConcatenate,fldLimitDesc,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp,fldApproveStatus)values(@fldClass, @fld1stAmt, @fld2ndAmt, @fld1stType, @fld2ndType, @fldConcatenate, @fldLimitDesc, @fldCreateUserId, @fldCreateTimeStamp, @fldUpdateUserId, @fldUpdateTimeStamp,@fldApproveStatus)";

            string secondType = collection["secondType"];
            if (secondType == null)
            { //because dropdown is disabled
                secondType = "";
            }

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldClass", collection["class"]),
                new SqlParameter("@fld1stAmt", collection["firstAmount"].ToString()),
                new SqlParameter("@fld2ndAmt", collection["secondAmount"].ToString()),
                new SqlParameter("@fld1stType", collection["firstType"]),
                new SqlParameter("@fld2ndType", secondType),
                new SqlParameter("@fldConcatenate", collection["concat"]),
                new SqlParameter("@fldLimitDesc", collection["VerifyLimitDesc"]),
                new SqlParameter("@fldUpdateUserId", currentUserId),
                new SqlParameter("@fldUpdateTimeStamp", DateTime.Now),
                new SqlParameter("@fldCreateUserId", currentUserId),
                new SqlParameter("@fldCreateTimeStamp", DateTime.Now),
                new SqlParameter("@fldApproveStatus", "A")
            });
        }

        public void DeleteInVerificationBatchSizeLimit(string delete)
        {

            string[] aryText = delete.Split(',');
            if ((aryText.Length > 0))
            {
                string stmt = "delete from tblVerificationBatchSizeLimit where fldClass in (" + DatabaseUtils.getParameterizedStatementFromArray(aryText) + ")";

                dbContext.ExecuteNonQuery(stmt, DatabaseUtils.getSqlParametersFromArray(aryText).ToArray());
            }
        }
        public VerificationLimitModel GetVerifyLimit(string classId)
        {

            string stmt = "Select * from tblVerificationBatchSizeLimit where fldClass=@fldclass";

            VerificationLimitModel verifyLimit = new VerificationLimitModel();
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldclass", classId) });

            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                verifyLimit.verifyLimitClass = row["fldClass"].ToString();
                verifyLimit.verifyLimitDesc = row["fldLimitDesc"].ToString();
                verifyLimit.fldConcatenate = row["fldConcatenate"].ToString();
                verifyLimit.fld1stType = row["fld1stType"].ToString();
                verifyLimit.fld2ndType = row["fld2ndType"].ToString();
                verifyLimit.fld1stAmt = Convert.ToSingle(row["fld1stAmt"].ToString());
                if (string.IsNullOrEmpty(row["fld2ndAmt"].ToString()))
                {
                    verifyLimit.fld2ndAmt = 0;
                }
                else
                {
                    verifyLimit.fld2ndAmt = Convert.ToSingle(row["fld2ndAmt"].ToString());
                }

            }
            return verifyLimit;
        }
        public List<string> Validate(FormCollection collection)
        {
            List<string> error = new List<string>();

            if (checkClassExist(collection["class"]))
            {
                //error.Add(Locale.Classalreadyexist);
            }
            return error;
        }
        public bool checkClassExist(string fldClass)
        {
            string stmt = "select 1 from tblVerificationBatchSizeLimit where fldClass=@fldclass";
            return dbContext.CheckExist(stmt, new[] { new SqlParameter("@fldclass", fldClass) });
        }

        public void CreateInVerificationBatchSizeLimit(string classId)
        {

            string stmt = "insert into tblVerificationBatchSizeLimit (fldClass,fld1stAmt,fld2ndAmt,fld1stType,fld2ndType,fldConcatenate,fldLimitDesc,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) " +
                " select fldClass,fld1stAmt,fld2ndAmt,fld1stType,fld2ndType,fldConcatenate,fldLimitDesc,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp from tblVerificationBatchSizeLimitTemp where fldClass = @fldClass ";

            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldClass", classId) });
        }

        public void DeleteInVerificationBatchSizeLimitTemp(string clasId)
        {

            string stmt = "delete from tblVerificationBatchSizeLimitTemp where @fldClass=fldClass";

            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldClass", clasId) });
        }

        public void AddToVerificationBatchSizeLimitTempToDelete(string clasId)
        {

            string stmt = "insert into tblVerificationBatchSizeLimitTemp (fldClass,fldLimitDesc) " +
                " select fldClass,fldLimitDesc from tblVerificationBatchSizeLimit where fldClass=@fldClass" +
                " update tblVerificationBatchSizeLimitTemp set fldApproveStatus=@fldApproveStatus where fldClass=@fldClass";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldApproveStatus", "D"), new SqlParameter("@fldClass", clasId) });
        }


    }
}