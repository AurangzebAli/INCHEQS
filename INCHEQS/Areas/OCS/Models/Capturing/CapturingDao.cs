using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
//using INCHEQS.DataAccessLayer.OCS;
using System.Management;
using System.Text.RegularExpressions;
using INCHEQS.Resources;
using INCHEQS.Common;
using System.Data.Entity;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security.Account;
using INCHEQS.Security;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.Capturing
{
    public class CapturingDao : ICapturingDao
    {
        private readonly ApplicationDbContext dbContext;

        public CapturingDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public CapturingModel GetCaptureData()
        {
            CapturingModel capturingModel = new CapturingModel();
            capturingModel.CapturingModeDataTable = this.GetCapturingModeDataTable();
            capturingModel.CapturingTypeDataTable = this.GetCapturingTypeDataTable("");
            capturingModel.CapturingRelationDataTable = this.GetCapturingRelationshipDataTable();

            return capturingModel;
        }

        public DataTable GetCapturingModeDataTable()
        {
            DataTable dtCapturingMode = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            dtCapturingMode = dbContext.GetRecordsAsDataTableSP("spcgcapturemode", SqlParameterNext.ToArray());

            return dtCapturingMode;
        }

        public DataTable GetCapturingTypeDataTable(string strModeId)
        {
            //strModeId = "CP";
            DataTable dtCapturingType = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@p_modeid", strModeId));
            dtCapturingType = dbContext.GetRecordsAsDataTableSP("spcgcapturetype", SqlParameterNext.ToArray());

            return dtCapturingType;
        }

        public DataTable GetCapturingInfo(string strUserId)
        {
            DataTable dtCapturingInfo = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@userid", CurrentUser.Account.UserId));
            dtCapturingInfo = dbContext.GetRecordsAsDataTableSP("spcgcaptureinfo", sqlParameterNext.ToArray());
            return dtCapturingInfo;
        }
        public DataTable GetPostingModeInfo()
        {
            DataTable dtPostingModeInfo = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dtPostingModeInfo = dbContext.GetRecordsAsDataTableSP("spcgPostingMode", sqlParameterNext.ToArray());
            return dtPostingModeInfo;
        }

        public DataTable GetMacAddressClient(string listMacAddress)
        {
            DataTable dtCapturePageInfo = new DataTable();
            List<string> macAddress = new List<string>();
            macAddress = listMacAddress.Split(',').ToList();
            foreach (string mac in macAddress)
            {
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@macaddress", mac));
                dtCapturePageInfo = dbContext.GetRecordsAsDataTableSP("spcgCapturePageInfo", sqlParameterNext.ToArray());
            }
            return dtCapturePageInfo;
        }

        public DataTable GetCapturingRelationshipDataTable()
        {
            DataTable dtCapturingRelationship = new DataTable();
            dtCapturingRelationship = dbContext.GetRecordsAsDataTableSP("spcgcapturerelationship");
            return dtCapturingRelationship;
        }

        public DataTable GetCapturingModeDetailsDataTable(string strModeId)
        {
            DataTable dtCapturingModeDesc = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldModeId", strModeId));
            dtCapturingModeDesc = dbContext.GetRecordsAsDataTableSP("spcgCaptureModeDesc", sqlParameterNext.ToArray());
            return dtCapturingModeDesc;
        }
        public  DataTable GetSelfClearingBranchId(string BranchId)
        {
            DataTable dtClearingBranchId = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldbranchid", BranchId));
            dtClearingBranchId = dbContext.GetRecordsAsDataTableSP("spcgSelfClearingId", sqlParameterNext.ToArray());
            return dtClearingBranchId;
        }

        public DataTable GetPostingModeDetailsDataTable(string strModeId)
        {
            DataTable dtCapturingModeDesc = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldModeId", strModeId));
            dtCapturingModeDesc = dbContext.GetRecordsAsDataTableSP("spcgPostingModeDesc", sqlParameterNext.ToArray());
            return dtCapturingModeDesc;
        }

        public DataTable GetCheckTypeDetailsDataTable(string strTypeId)
        {
            DataTable dtCapturingTypeDesc = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTypeId", strTypeId));
            dtCapturingTypeDesc = dbContext.GetRecordsAsDataTableSP("spcgCaptureTypeDesc", sqlParameterNext.ToArray());
            return dtCapturingTypeDesc;

        }

        public DataTable GetWorkstationScannerDataTable(string strMACAddress, string strUserId)
        {
            DataTable dtScannerWorkstationInfo = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@UserId", strUserId));
            sqlParameterNext.Add(new SqlParameter("@fldMACAddress", strMACAddress));
            dtScannerWorkstationInfo = dbContext.GetRecordsAsDataTableSP("spcgScannerWorkstationInfo", sqlParameterNext.ToArray());
            return dtScannerWorkstationInfo;
        }

        public List<string> GetMacAddress(AccountModel currentUser)
        {
            List<string> lstMacAddress = new List<string>();
            List<string> lstMacAddress2 = new List<string>();
            lstMacAddress = currentUser.macAddress.Split(',').ToList();
            foreach (string macAddress in lstMacAddress)
            {
                if (macAddress != null && macAddress != "")
                {
                    lstMacAddress2.Add(macAddress);
                }
            }
            return lstMacAddress2;
        }
        public DataTable getCapturePageInfo(AccountModel currentUser)
        {
            DataTable dtCapturePageInfo = new DataTable();
            List<string> macAddress = new List<string>();
            if (currentUser.macAddress != null)
            {
                macAddress = currentUser.macAddress.Split(',').ToList();
                foreach (string mac in macAddress)
                {
                    if (mac != null && mac != "")
                    {
                        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                        sqlParameterNext.Add(new SqlParameter("@macaddress", mac));
                        sqlParameterNext.Add(new SqlParameter("@userid", currentUser.UserId));
                        dtCapturePageInfo = dbContext.GetRecordsAsDataTableSP("spcgCapturePageInfo", sqlParameterNext.ToArray());

                        if (dtCapturePageInfo.Rows.Count > 0)
                        {
                            break;
                        }
                    }
                }
            } 
            return dtCapturePageInfo;
        }
        public DataTable GetCaptureDate()
        {
            DataTable dsCaptureDate = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dsCaptureDate = dbContext.GetRecordsAsDataTableSP("spcgcapturedate", sqlParameterNext.ToArray());
            return dsCaptureDate;
        }

        public List<String> ValidateMacAdress(List<string> lstMacAddress, FormCollection col)
        {
            List<String> err = new List<String>();
            string format = "^([0-9a-fA-F][0-9a-fA-F]:){5}([0-9a-fA-F][0-9a-fA-F])$";
            foreach (string macAddress in lstMacAddress)
            {
                if (!(Regex.IsMatch(macAddress, format)))
                {
                    err.Add("Invalid MAC Address : " + macAddress);//Pending Locale Registration
                }
            }
            if (col["fldCapturingBranchCode"] == "")
            {
                err.Add("Capturing Branch Cannot be Empty.");
            }
            return err;
        }

        public DataTable GetScannerErrorDataTable(string strScannerTypeId)
        {
            DataTable dtScannerErrorInfo = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@ScannerTypeId", strScannerTypeId));
            dtScannerErrorInfo = dbContext.GetRecordsAsDataTableSP("spcgScannerErrorInfo", sqlParameterNext.ToArray());
            return dtScannerErrorInfo;
        }

        public string GetProcessDate()
        {
            string strProcessDate = "";
            DataTable dtProcessDate = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dtProcessDate = dbContext.GetRecordsAsDataTableSP("spcgProcessDate", sqlParameterNext.ToArray());
            strProcessDate = dtProcessDate.Rows[0]["fldProcessDate"].ToString();
            return strProcessDate;
        }

        public DataTable GetCurrencyDataTable(string strCurrencyId)
        {
            DataTable dtCurrencyInfo = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldCurrencyId", strCurrencyId));
            dtCurrencyInfo = dbContext.GetRecordsAsDataTableSP("spcgCurrencyInfo", sqlParameterNext.ToArray());
            return dtCurrencyInfo;
        }

        public DataTable GetCompleteSeqNoDataTable(string profileCode)
        {
            profileCode = "";
            DataTable dtSystemProfile = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@profileCode", profileCode));
            dtSystemProfile = dbContext.GetRecordsAsDataTableSP("spcgSystemProfile", sqlParameterNext.ToArray());
            return dtSystemProfile;
        }

        public DataTable GetUICInfoDataTable(string strScannerId, string strBranchId)
        {
            DataTable dtUICInfo = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldScannerId", strScannerId));
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", strBranchId));
            dtUICInfo = dbContext.GetRecordsAsDataTableSP("spcgUICInfo", sqlParameterNext.ToArray());
            return dtUICInfo;
        }

        public DataTable GetScannerTuningDataTable(string strScannerTypeId)
        {
            DataTable dtScannerTuningInfo = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldScannerTypeID", strScannerTypeId));
            dtScannerTuningInfo = dbContext.GetRecordsAsDataTableSP("spcgScannerTuningInfo", sqlParameterNext.ToArray());
            return dtScannerTuningInfo;
        }

        public DataTable GetBranchEndOfDayDataTable(string strProcessDate, string strBranchId, string strBankCode)
        {
            DataTable dtBranchEndOfDayInfo = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldActive", "Y"));
            sqlParameterNext.Add(new SqlParameter("@fldProcessDate", strProcessDate));
            sqlParameterNext.Add(new SqlParameter("@fldBranchID", strBranchId));
            sqlParameterNext.Add(new SqlParameter("@fldEODStatus", "Y"));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", CurrentUser.Account.UserId));
            dtBranchEndOfDayInfo = dbContext.GetRecordsAsDataTableSP("spcgBranchEndOfDayInfo", sqlParameterNext.ToArray());
            return dtBranchEndOfDayInfo;
        }

        public DataTable GetCenterEndOfDayDataTable(string strProcessDate, string strBankCode)
        {
            DataTable dtCenterEndOfDayInfo = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldActive", "Y"));
            sqlParameterNext.Add(new SqlParameter("@fldProcessDate", strProcessDate));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));
            dtCenterEndOfDayInfo = dbContext.GetRecordsAsDataTableSP("spcgdtCenterEndOfDayInfoInfo", sqlParameterNext.ToArray());
            return dtCenterEndOfDayInfo;
        }

        public DataTable GetGroupCodeByUserIdDataTable(string strUserId)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@UserId", strUserId));
            SqlParameterNext.Add(new SqlParameter("@AllowLogin", "INTEGER"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgGroupProfileByUserId", SqlParameterNext.ToArray());
            return dt;
        }

        public string FormatProcessDate(int intYear, int intMonth, int intDay)
        {
            string[] arrMonth = new string[12];
            arrMonth[0] = "Jan";
            arrMonth[1] = "Feb";
            arrMonth[2] = "Mar";
            arrMonth[3] = "Apr";
            arrMonth[4] = "May";
            arrMonth[5] = "Jun";
            arrMonth[6] = "Jul";
            arrMonth[7] = "Aug";
            arrMonth[8] = "Sep";
            arrMonth[9] = "Oct";
            arrMonth[10] = "Nov";
            arrMonth[11] = "Dec";
            return intDay + " " + arrMonth[intMonth - 1] + " " + intYear;
        }

        public bool UpdateUICInfo(string strScannerId, string strBatchNo, string strSeqNo, string strUserId, string strClearingBranch)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldScannerId", strScannerId));
            sqlParameterNext.Add(new SqlParameter("@fldBatchNo", strBatchNo));
            sqlParameterNext.Add(new SqlParameter("@fldSeqNo", strSeqNo));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUserId));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime()));
            sqlParameterNext.Add(new SqlParameter("@strClearingBranch", strClearingBranch));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuUICInfo", sqlParameterNext.ToArray()); ;
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;

            #region BeforeConvertToSP

            //string strQueryUpdate = "UPDATE tblscannerworkstation SET fldbatchno=@fldBatchNo,fldseqno=@fldSeqNo,fldupdateuserid=@fldUpdateUserId,fldupdatetimestamp=@fldUpdateTimeStamp WHERE fldscannerid=@fldScannerId and fldbranchid = @strClearingBranch";
            //try
            //{
            //    this.dbContext.ExecuteNonQuery(strQueryUpdate, new SqlParameter[] {
            //        new SqlParameter("@fldScannerId", strScannerId),
            //        new SqlParameter("@fldBatchNo", strBatchNo),
            //        new SqlParameter("@fldSeqNo", strSeqNo),
            //        new SqlParameter("@fldUpdateUserId", strUserId),
            //        new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime()),
            //        new SqlParameter("@strClearingBranch", strClearingBranch)
            //    });
            //    return true;
            //}
            //catch (Exception exception)
            //{
            //    throw exception;
            //} 
            #endregion
        }

        //public bool UpdateUICInfoIncSequence(string strScannerId, string strBranchId, int intBatchNo, int intSeqNo, string strUserId)
        //{
        //    string strQueryUpdate = "UPDATE tblscannerworkstation SET fldbatchno=@fldBatchNo,fldseqno=@fldSeqNo,fldupdateuserid=@fldUpdateUserId,fldupdatetimestamp=@fldUpdateTimeStamp WHERE fldscannerid=@fldScannerId";
        //    try
        //    {
        //        this.dbContext.ExecuteNonQuery(strQueryUpdate, new SqlParameter[] {
        //            new SqlParameter("@fldScannerId", strScannerId),
        //            new SqlParameter("@fldBatchNo", intBatchNo),
        //            new SqlParameter("@fldSeqNo", intSeqNo),
        //            new SqlParameter("@fldUpdateUserId", strUserId),
        //            new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime())
        //        });
        //        return true;
        //    }
        //    catch (Exception exception)
        //    {
        //        throw exception;
        //    }
        //}

        public bool CheckBank(string bankCode)
        {
            bool result = false;
            DataTable dt = new DataTable();
            if (bankCode.Trim() == CurrentUser.Account.BankCode)
            {
                result = true;
            }
            else
            {
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode.Trim()));
                dt = dbContext.GetRecordsAsDataTableSP("spcgDisableBankCode", SqlParameterNext.ToArray());
                if (dt.Rows.Count > 0)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }
        public List<CapturingModel> GetCapturingBranchesInfo(string UserID)
        {
            DataTable resultTable = new DataTable();
            List<CapturingModel> branchList = new List<CapturingModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@userid", UserID));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgcapturingBranchinfo", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    CapturingModel objCapturing = new CapturingModel();
                    objCapturing.BranchID = row["fldbranchid"].ToString();
                    objCapturing.BranchCode = row["fldbranchCode"].ToString();
                    objCapturing.BranchDesc = row["fldbranchDesc"].ToString();
                    branchList.Add(objCapturing);
                }
            }
            return branchList;
        }
    }
}