using INCHEQS.Areas.COMMON.Models.AccountProfile;
using INCHEQS.Areas.COMMON.Models.BankCharges;
using INCHEQS.Areas.COMMON.Models.BankChargesType;
using INCHEQS.Areas.COMMON.Models.BankCode;
using INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ;
using INCHEQS.Areas.COMMON.Models.BankZone;
using INCHEQS.Areas.COMMON.Models.BranchCode;
using INCHEQS.Areas.COMMON.Models.Holiday;
using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Areas.COMMON.Models.InternalBranchKBZ;
using INCHEQS.Areas.COMMON.Models.Message;
using INCHEQS.Areas.ICS.Models.ICSRetentionPeriod;
using INCHEQS.Areas.ICS.Models.LargeAmount;
using INCHEQS.Areas.OCS.Models.OCSRetentionPeriod;
using INCHEQS.Areas.OCS.Models.ScannerWorkStation;
using INCHEQS.ConfigVerification.VerificationLimit;
using INCHEQS.Areas.ICS.Models.NonConformanceFlag;
using INCHEQS.Areas.ICS.Models.TransCode;
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Areas.COMMON.Models.StateCode;
using INCHEQS.Areas.ICS.Models.HighRiskAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance
{
    public interface IMaintenanceAuditLogDao
    {
        //Bank Code/Profile New
        string BankProfile_AddTemplate(string bankCode, string TableHeader, string SystemProfile, string bankType);
        string BankProfile_EditTemplate(string bankCode, BankCodeModel before, BankCodeModel after, string TableHeader, string bankType);
        string BankProfile_DeleteTemplate(string bankCode, string TableHeader);
        string BankProfileChecker_AddTemplate(string bankCode, string TableHeader, string Action);
        string BankProfileChecker_EditTemplate(string bankCode, BankCodeModel before, BankCodeModel after, string TableHeader);
        string BankProfileChecker_DeleteTemplate(string bankCode, string TableHeader);

        BankCodeModel GetBankCodeDataTemp(string bankID);

        //Branch Profile New
        string BranchProfile_AddTemplate(string branchId, string TableHeader, string SystemProfile);
        string BranchProfile_EditTemplate(string branchId, BranchCodeModel before, BranchCodeModel after, string TableHeader);
        string BranchProfile_DeleteTemplate(string branchId, string TableHeader);
        string BranchProfileChecker_AddTemplate(string branchId, string TableHeader, string Action);
        string BranchProfileChecker_EditTemplate(string branchId, BranchCodeModel before, BranchCodeModel after, string TableHeader);
        string BranchProfileChecker_DeleteTemplate(string branchId, string TableHeader);

        BranchCodeModel GetBranchCodeDataById(string branchId, string action);

        //Internal Branch KBZ Profile
        string InternalBranchKBZ_AddTemplate(string branchId, string TableHeader, string SystemProfile);
        string InternalBranchKBZ_EditTemplate(string branchId, InternalBranchKBZModel before, InternalBranchKBZModel after, string TableHeader);
        string InternalBranchKBZ_DeleteTemplate(string branchId, string TableHeader);
        string InternalBranchKBZChecker_AddTemplate(string branchId, string TableHeader, string Action);
        string InternalBranchKBZChecker_EditTemplate(string branchId, InternalBranchKBZModel before, InternalBranchKBZModel after, string TableHeader);
        string InternalBranchKBZChecker_DeleteTemplate(string branchId, string TableHeader);

        InternalBranchKBZModel GetInternalBranchData(string id);
        InternalBranchKBZModel GetInternalBranchDataTemp(string id);

        //Internal Branch Profile
        string InternalBranch_AddTemplate(string branchId, string TableHeader, string SystemProfile);
        string InternalBranch_EditTemplate(string branchId, InternalBranchModel before, InternalBranchModel after, string TableHeader);
        string InternalBranch_DeleteTemplate(string branchId, string TableHeader);
        string InternalBranchChecker_AddTemplate(string branchId, string TableHeader, string Action);
        string InternalBranchChecker_EditTemplate(string branchId, InternalBranchModel before, InternalBranchModel after, string TableHeader);
        string InternalBranchChecker_DeleteTemplate(string branchId, string TableHeader);

        InternalBranchModel GetInternalBranchDataMAB(string id);
        InternalBranchModel GetInternalBranchDataTempMAB(string id);

        ////Return Code
        //string ReturnCode_AddTemplate(string rejectCode, string TableHeader, string SystemProfile);
        //string ReturnCode_EditTemplate(string rejectCode, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after, string TableHeader);
        //string ReturnCode_DeleteTemplate(string rejectCode, string TableHeader);
        //string ReturnCodeChecker_AddTemplate(string rejectCode, string TableHeader, string Action);
        //string ReturnCodeChecker_EditTemplate(string rejectCode, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after, string TableHeader);
        //string ReturnCodeChecker_DeleteTemplate(string rejectCode, string TableHeader);

        //Verification Threshold
        string VerificationThreshold_AddTemplate(string bankCode, string type, string sequence, string TableHeader, string SystemProfile);
        string VerificationThreshold_EditTemplate(string bankCode, string type, string sequence, ThresholdModel before, ThresholdModel after, string TableHeader);
        string VerificationThreshold_DeleteTemplate(string bankCode, string type, string sequence, string TableHeader);
        string VerificationThresholdChecker_AddTemplate(string bankCode, string type, string sequence, string TableHeader, string Action);
        string VerificationThresholdChecker_EditTemplate(string bankCode, string type, string sequence, ThresholdModel before, ThresholdModel after, string TableHeader, string Action);
        string VerificationThresholdChecker_DeleteTemplate(string bankCode, string type, string sequence, string TableHeader);

        ThresholdModel GetThresholdData(string type, string sequence, string bankCode);
        ThresholdModel GetThresholdDataTemp(string type, string sequence, string bankCode);

        //Account Profile
        string AccountProfile_AddTemplate(string accountNumber, string TableHeader, string SystemProfile);
        string AccountProfile_EditTemplate(string accountNumber, AccountProfileModel before, AccountProfileModel after, string TableHeader);
        string AccountProfile_DeleteTemplate(string accountNumber, string TableHeader);
        string AccountProfileChecker_AddTemplate(string accountNumber, string TableHeader, string Action);
        string AccountProfileChecker_EditTemplate(string accountNumber, AccountProfileModel before, AccountProfileModel after, string TableHeader);
        string AccountProfileChecker_DeleteTemplate(string accountNumber, string TableHeader);

        //Verification Limit
        string VerificationLimit_AddTemplate(string classId, string TableHeader, string SystemProfile);
        string VerificationLimit_EditTemplate(string classId, VerificationLimitModel before, VerificationLimitModel after, string TableHeader);
        string VerificationLimit_DeleteTemplate(string classId, string TableHeader);
        string VerificationLimitChecker_AddTemplate(string classId, string TableHeader, string Action);
        string VerificationLimitChecker_EditTemplate(string classId, VerificationLimitModel before, VerificationLimitModel after, string TableHeader);
        string VerificationLimitChecker_DeleteTemplate(string classId, string TableHeader);

        VerificationLimitModel GetVerificationLimitData(string classId);
        VerificationLimitModel GetVerificationLimitDataTemp(string classId);

        //Large Amount Limit
        string LargeAmountLimit_EditTemplate(string bankCode, LargeAmountModel before, LargeAmountModel after, string TableHeader);
        string LargeAmountLimitChecker_EditTemplate(string bankCode, LargeAmountModel before, LargeAmountModel after, string TableHeader);

        LargeAmountModel GetLargeAmountTemp(string bankCode);

        //Bank Host Status KBZ
        string BankHostStatusKBZ_AddTemplate(string bankhostCode, string TableHeader, string SystemProfile);
        string BankHostStatusKBZ_EditTemplate(string bankhostCode, BankHostStatusKBZModel before, BankHostStatusKBZModel after, string TableHeader);
        string BankHostStatusKBZ_DeleteTemplate(string bankhostCode, string TableHeader);
        string BankHostStatusKBZChecker_AddTemplate(string bankhostCode, string TableHeader, string Action);
        string BankHostStatusKBZChecker_EditTemplate(string bankhostCode, BankHostStatusKBZModel before, BankHostStatusKBZModel after, string TableHeader);
        string BankHostStatusKBZChecker_DeleteTemplate(string bankhostCode, string TableHeader);

        BankHostStatusKBZModel GetStatus(string bankhostCode);

        //Bank Zone
        string BankZone_AddTemplate(string bankzonecode, string TableHeader, string SystemProfile);
        string BankZone_EditTemplate(string bankzonecode, BankZoneModel before, BankZoneModel after, string TableHeader);
        string BankZone_DeleteTemplate(string bankzonecode, string TableHeader);
        string BankZoneChecker_AddTemplate(string bankzonecode, string TableHeader, string Action);
        string BankZoneChecker_EditTemplate(string bankzonecode, BankZoneModel before, BankZoneModel after, string TableHeader);
        string BankZoneChecker_DeleteTemplate(string bankzonecode, string TableHeader);

        //Bank Charges Type
        string BankChargesType_AddTemplate(string bankchargestype, string TableHeader, string SystemProfile);
        string BankChargesType_EditTemplate(string bankchargestype, BankChargesTypeModel before, BankChargesTypeModel after, string TableHeader);
        string BankChargesType_DeleteTemplate(string bankchargestype, string TableHeader);
        string BankChargesTypeChecker_AddTemplate(string bankchargestype, string TableHeader, string Action);
        string BankChargesTypeChecker_EditTemplate(string bankchargestype, BankChargesTypeModel before, BankChargesTypeModel after, string TableHeader);
        string BankChargesTypeChecker_DeleteTemplate(string bankchargestype, string TableHeader);

        //Bank Charges
        string BankCharges_AddTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, string TableHeader, string SystemProfile);
        string BankCharges_EditTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, BankChargesModel before, BankChargesModel after, string TableHeader);
        string BankCharges_DeleteTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, string TableHeader);
        string BankChargesChecker_AddTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, string TableHeader, string Action);
        string BankChargesChecker_EditTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, BankChargesModel before, BankChargesModel after, string TableHeader);
        string BankChargesChecker_DeleteTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, string TableHeader);

        BankChargesModel GetBankChargesbyView(string bankCode, string productCode, string bankChargesType, string minAmount, string maxAmount, string tblname);

        //Terminal Scanner
        string TerminalScanner_AddTemplate(string scannerId, string TableHeader, string SystemProfile);
        string TerminalScanner_EditTemplate(string scannerId, ScannerWorkStationModel before, ScannerWorkStationModel after, string TableHeader);
        string TerminalScanner_DeleteTemplate(string scannerId, string TableHeader);
        string TerminalScannerChecker_AddTemplate(string scannerId, string TableHeader, string Action);
        string TerminalScannerChecker_EditTemplate(string scannerId, ScannerWorkStationModel before, ScannerWorkStationModel after, string TableHeader);
        string TerminalScannerChecker_DeleteTemplate(string scannerId, string TableHeader);

        ScannerWorkStationModel GetScannerTempbyId(string scannerId);

        //Holiday Calendar
        string HolidayCalendar_AddTemplate(FormCollection col, string getDate, string TableHeader);
        string HolidayCalendar_EditTemplate(string holidayId, HolidayModel before, HolidayModel after, string tblname, string TableHeader);
        string HolidayCalendar_DeleteTemplate(string holidayId, HolidayModel sHoliday, string tblname, string TableHeader);
        string HolidayCalendarChecker_AddTemplate(string holidayId, HolidayModel sHoliday, string tblname, string TableHeader);
        string HolidayCalendarChecker_EditTemplate(string holidayId, HolidayModel before, HolidayModel after, string tblname, string TableHeader);
        string HolidayCalendarChecker_DeleteTemplate(string holidayId, HolidayModel sHoliday, string tblname, string TableHeader);

        HolidayModel GetHolidayDatabyId(string id);
        HolidayModel GetHolidayDataTempbyId(string id);
        string OCSRetentionPeriod_EditTemplate(OCSRetentionPeriodModel before, OCSRetentionPeriodModel after, string TableHeader);
        string OCSRetentionPeriodChecker_EditTemplate(OCSRetentionPeriodModel before, OCSRetentionPeriodModel after, string TableHeader);
        string ICSRetentionPeriod_EditTemplate(ICSRetentionPeriodModel before, ICSRetentionPeriodModel after, string TableHeader);
        string ICSRetentionPeriodChecker_EditTemplate(ICSRetentionPeriodModel before, ICSRetentionPeriodModel after, string TableHeader);

        //Message Maintenance
        string Message_AddTemplate(string Message, string TableHeader, string SystemProfile);
        string Message_EditTemplate(string MessageId, MessageModel before, MessageModel after, string TableHeader);
        string Message_DeleteTemplate(string MessageId, string TableHeader);
        string MessageChecker_AddTemplate(string MessageId, string TableHeader, string Action);
        string MessageChecker_EditTemplate(string MessageId, MessageModel before, MessageModel after, string TableHeader);
        string MessageChecker_DeleteTemplate(string MessageId, string TableHeader);

        //Non-Conformance Flag
        string NCF_AddTemplate(string NCFCode, string TableHeader, string SystemProfile);
        string NCF_EditTemplate(string NCFCode, NonConformanceFlagModel before, NonConformanceFlagModel after, string TableHeader);
        string NCF_DeleteTemplate(string NCFCode, string TableHeader);
        string NCFChecker_AddTemplate(string NCFCode, string TableHeader, string Action);
        string NCFChecker_EditTemplate(string NCFCode, NonConformanceFlagModel before, NonConformanceFlagModel after, string TableHeader);
        string NCFChecker_DeleteTemplate(string NCFCode, string TableHeader);

        NonConformanceFlagModel GetNCFCode(string ncfCode);
        NonConformanceFlagModel GetNCFCodeTemp(string ncfCode);

        //Transaction Code
        string TransCode_AddTemplate(string TransCode, string TableHeader, string SystemProfile);
        string TransCode_EditTemplate(string TransCode, TransCodeModel before, TransCodeModel after, string TableHeader);
        string TransCode_DeleteTemplate(string TransCode, string TableHeader);
        string TransCodeChecker_AddTemplate(string TransCode, string TableHeader, string Action);
        string TransCodeChecker_EditTemplate(string TransCode, TransCodeModel before, TransCodeModel after, string TableHeader);
        string TransCodeChecker_DeleteTemplate(string TransCode, string TableHeader);

        TransCodeModel GetTransCode(string transCode);
        TransCodeModel GetTransCodeTemp(string transCode);

        //new Return Code
        string RetCode_AddTemplate(string ReturnCode, string TableHeader, string SystemProfile);
        string RetCode_EditTemplate(string ReturnCode, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after, string TableHeader);
        string RetCode_DeleteTemplate(string ReturnCode, string TableHeader);
        string RetCodeChecker_AddTemplate(string ReturnCode, string TableHeader, string Action);
        string RetCodeChecker_EditTemplate(string ReturnCode, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after, string TableHeader);
        string RetCodeChecker_DeleteTemplate(string ReturnCode, string TableHeader);

        //State Code
        string StateCode_AddTemplate(string StateCode, string TableHeader, string SystemProfile);
        string StateCode_EditTemplate(string StateCode, StateCodeModel before, StateCodeModel after, string TableHeader);
        string StateCode_DeleteTemplate(string StateCode, string TableHeader);
        string StateCodeChecker_AddTemplate(string StateCode, string TableHeader, string Action);
        string StateCodeChecker_EditTemplate(string StateCode, StateCodeModel before, StateCodeModel after, string TableHeader);
        string StateCodeChecker_DeleteTemplate(string StateCode, string TableHeader);

        //Internal Branch Profile New
        string InternalBranchProfile_AddTemplate(string IbranchId, string TableHeader, string SystemProfile);
        string InternalBranchProfile_EditTemplate(string IbranchId, InternalBranchModel before, InternalBranchModel after, string TableHeader);
        string InternalBranchProfile_DeleteTemplate(string IbranchId, string TableHeader);
        string InternalBranchProfileChecker_AddTemplate(string IbranchId, string TableHeader, string Action);
        string InternalBranchProfileChecker_EditTemplate(string IbranchId, InternalBranchModel before, InternalBranchModel after, string TableHeader);
        string InternalBranchProfileChecker_DeleteTemplate(string IbranchId, string TableHeader);

        InternalBranchModel GetInternalBranchDataById(string IbranchId, string action);

        //High Risk Account
        string HighRiskAccount_AddTemplate(string IBranchCode, string TableHeader, string SystemProfile);
        string HighRiskAccount_EditTemplate(string IBranchCode, HighRiskAccountModel before, HighRiskAccountModel after, string TableHeader);
        string HighRiskAccount_DeleteTemplate(string IBranchCode, string TableHeader);
        string HighRiskAccountChecker_AddTemplate(string IBranchCode, string TableHeader, string Action);
        string HighRiskAccountChecker_EditTemplate(string IBranchCode, HighRiskAccountModel before, HighRiskAccountModel after, string TableHeader);
        string HighRiskAccountChecker_DeleteTemplate(string IBranchCode, string TableHeader);

        HighRiskAccountModel GetHighRiskAccount(string IBranchCode, string action);
    }

}
