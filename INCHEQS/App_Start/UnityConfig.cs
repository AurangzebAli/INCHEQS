using System.Web.Mvc;
using System.Windows;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Unity;
using Unity.Mvc5;
//using Unity.Container;
using INCHEQS.Security.UserSession;
using INCHEQS.Security.Account;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Security.Group;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.Password;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.DashboardConfig;
using INCHEQS.Models;
using INCHEQS.Models.State;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Areas.OCS.Models.InwardReturn;
using INCHEQS.Models.Sequence;
using INCHEQS.ConfigVerificationBranch.BranchActivation;
using INCHEQS.Areas.ICS.Models.AutoApproveThresholdModel;
using INCHEQS.Models.TaskMenu;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Areas.ICS.Models.ICSDataProcess;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Report;
//using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Models.Verification;
using INCHEQS.EOD.DataHouseKeep;
using INCHEQS.ConfigVerification.ThresholdSetting;
using INCHEQS.ConfigVerification.VerificationLimit;
using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.Areas.COMMON.Models.BankHostStatus;
using INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.EOD.DayEndProcess;
using INCHEQS.ConfigVerification.ReleaseLockedCheque;
using INCHEQS.ConfigVerificationBranch.ReleaseBranchLockedCheque;
using INCHEQS.TaskAssignment;
using INCHEQS.Areas.ICS.Models.DedicatedBranchDay;
using INCHEQS.Areas.ICS.Models.ChequeImage;
using INCHEQS.Areas.ICS.Models.DedicatedBranchOfficer;
using INCHEQS.Areas.ICS.Models.LateReturnedMaintenance;
using INCHEQS.Areas.ICS.Models.MICRImage;
//using INCHEQS.Calendar.Holiday;
using INCHEQS.Areas.ICS.Models.BandQueueSetting;
//using INCHEQS.InternalBranch.InternalBranch;
using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Areas.COMMON.Models.InternalBranchKBZ;
using INCHEQS.PPS.CMSAccountInfo;
using INCHEQS.Areas.ICS.Models.PullOutReason;
//using INCHEQS.Models.TransactionType;
using INCHEQS.Areas.ICS.Models.ScheduledTask;
using INCHEQS.Models.DataCorrectionDao;
using INCHEQS.Models.Report.OCS;
using INCHEQS.Models.Signature;
using INCHEQS.Models.DbJoin;
using INCHEQS.Areas.ICS.Models.OutwardReturnICL;
using INCHEQS.Areas.ICS.Models.GenerateUPI;
using INCHEQS.Areas.ICS.Models.ProgressStatus;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;
using INCHEQS.Areas.ICS.Models.ICSAPICreditPosting;
using INCHEQS.Areas.ICS.Models.ICSAPIDebitPosting;
using INCHEQS.Areas.ICS.Models.GenerateRepairedDebitFile;
using INCHEQS.Areas.ICS.Models.GenerateCreditFile;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;
using INCHEQS.Areas.ICS.Models.ICSRetentionPeriod;
using INCHEQS.Areas.ICS.Models.BranchSubmission;

// Bank Of Malaysia
//using INCHEQS.BNM.BankCode;
//using INCHEQS.BNM.StateCode;
//using INCHEQS.BNM.BranchCode;
//using INCHEQS.BNM.ReturnCode;

using INCHEQS.Models.HostFile;
using INCHEQS.Models.RejectReentry;

// Bank Of Philipine
//using INCHEQS.PCHC.BankCode;
//using INCHEQS.PCHC.StateCode;
//using INCHEQS.PCHC.ReturnCode;

//OCS
using INCHEQS.Areas.OCS.Models.TransactionType;
using INCHEQS.Areas.OCS.Models.TransactionCode;
using INCHEQS.Areas.OCS.Models.TCCode;
using INCHEQS.Areas.COMMON.Models.HubUser;
using INCHEQS.Areas.OCS.Models.SystemCutOffTime;
using INCHEQS.Areas.OCS.Models.ScannerWorkStation;
using INCHEQS.Areas.OCS.Models.ProcessDate;
using INCHEQS.Areas.OCS.Models.AIFImport;
using INCHEQS.Areas.OCS.Models.ChequeAmountEntry;
using INCHEQS.Areas.OCS.Models.ChequeDateAmountEntry;
using INCHEQS.Areas.OCS.Models.BranchClearingItem;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.Areas.OCS.Models.ChequeAccountEntry;
using INCHEQS.Areas.OCS.Models.MicrRepair;
using INCHEQS.Areas.OCS.Models.Capturing;
using INCHEQS.Areas.OCS.Models.Search;
using INCHEQS.Areas.OCS.Models.OCSProcessDate;
using INCHEQS.Areas.OCS.Models.Clearing;
using INCHEQS.Areas.OCS.Models.OCSReleaseLockedCheque;
using INCHEQS.Areas.OCS.Models.ChequeDataEntry;
using INCHEQS.Areas.OCS.Models.BranchEndOfDay;
using INCHEQS.Models.OCSAIFFileUploadDao;
using INCHEQS.Areas.OCS.Models.Balancing;
using INCHEQS.Areas.OCS.Models.BranchEndOfDaySummary;
//using INCHEQS.Areas.OCS.Models.BranchSubmission;
using INCHEQS.Areas.OCS.Models.HostFile;
using INCHEQS.Areas.OCS.Models.ClearingItem;
using INCHEQS.Areas.OCS.Models.ProgressStatus;
using INCHEQS.Models.ProgressStatus;
using INCHEQS.Areas.OCS.Models.ClearingItemsSummary;
using INCHEQS.Areas.COMMON.Models.BankCode;
using INCHEQS.Areas.OCS.Models.BankBranchesOcs;
using INCHEQS.Areas.OCS.Models.ChequeImage;
using INCHEQS.Areas.COMMON.Models.Group;
using INCHEQS.Areas.COMMON.Models.BankZone;
using INCHEQS.Areas.COMMON.Models.BankChargesType;
using INCHEQS.Areas.COMMON.Models.BankCharges;
//using INCHEQS.Areas.OCS.Models.HubBranch;

using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Areas.COMMON.Models.BranchCode;
using INCHEQS.Areas.COMMON.Models.Holiday;
using INCHEQS.Areas.ICS.Models.ICSReleaseLockedCheque;
using INCHEQS.Areas.COMMON.Models.AccountProfile;
using INCHEQS.Areas.ICS.Models.LargeAmount;
using INCHEQS.Areas.OCS.Models.OutwardClearingICL;
using INCHEQS.Areas.COMMON.Models.HubBranch;
using INCHEQS.Areas.OCS.Models.OCSAPICreditPosting;
using INCHEQS.Areas.COMMON.Models.Branch;
using INCHEQS.Areas.ICS.Models.ProgressStatus;
using INCHEQS.Areas.OCS.Models.AuditTrailOCS;
using INCHEQS.Areas.OCS.Models.InwardReturnICL;
using INCHEQS.Areas.OCS.Models.OCSAPIDebitPosting;
//using INCHEQS.Areas.ICS.Models.BranchCode;
//using INCHEQS.Areas.OCS.Models.BankBranchesOcs;
//using INCHEQS.Areas.ICS.Models.BankCode;
//using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Areas.OCS.Models.OCSRetentionPeriod;
//using INCHEQS.Areas.ICS.Models.ICSProcessDate;
using INCHEQS.Areas.ICS.Models.ICSStartOfDay;
using INCHEQS.Areas.ICS.Models.ICSStartOfDayMaker;
using INCHEQS.Areas.ICS.Models.LoadDailyFile;

//Azim Start (5 Jan 2021)
using INCHEQS.Areas.ICS.Models.NonConformanceFlag;
using INCHEQS.Areas.ICS.Models.TransCode;
using INCHEQS.Areas.COMMON.Models.Message;
using INCHEQS.Areas.COMMON.Models.EnableUserID;
using INCHEQS.Areas.COMMON.Models.StateCode;
//Azim End

//Kew start (24 Jan 2021)
using INCHEQS.Areas.ICS.Models.HighRiskAccount;
using INCHEQS.Areas.ICS.Models.RationalBranch;
using INCHEQS.Areas.ICS.Models.LoadNcf;
//Kew End

//XX start
using INCHEQS.Areas.PPS.Models.ViewPPSFile;
using INCHEQS.Areas.PPS.Models.StartBatchICR;
using INCHEQS.Areas.PPS.Models.PositivePayMatching;
using INCHEQS.Areas.ICS.Models.LoadBankHostFile;
using INCHEQS.Areas.ICS.Models.LoadBankHostFile2nd;
using INCHEQS.Areas.PPS.Models.LoadPPSFile;
//XX End
using INCHEQS.Areas.ICS.Models.Truncation;
using INCHEQS.Areas.COMMON.Models.CreditAccount;
using INCHEQS.Areas.ICS.Models.HostValidation;
using INCHEQS.Areas.ICS.Models.DayEndSettlement;

namespace INCHEQS
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterType<IUserSessionDao, UserSessionDao>();
            container.RegisterType<ILoginAccountService, LoginAccountService>();
            container.RegisterType<INCHEQS.Security.User.IUserDao, INCHEQS.Security.User.UserDao>();
            container.RegisterType<INCHEQS.Areas.COMMON.Models.Users.IUserDao, INCHEQS.Areas.COMMON.Models.Users.UserDao>();
            container.RegisterType<ISystemProfileDao, SystemProfileDao>();
            
            container.RegisterType<IGroupDao, GroupDao>();
            container.RegisterType<IAuditTrailDao, AuditTrailDao>();
            container.RegisterType<IPasswordDao, PasswordDao>();
            container.RegisterType<IPageConfigDao, PageConfigDao>();
            container.RegisterType<IDashboardConfigDao, DashboardConfigDao>();
            
            container.RegisterType<IStateDao, StateDao>();
            container.RegisterType<ICommonInwardItemDao, CommonInwardItemDao>();
            container.RegisterType<ISequenceDao, SequenceDao>();
            container.RegisterType<IBranchActivationDao, BranchActivationDao>();
            container.RegisterType<ITaskMenuDao,TaskMenuDao>();
            container.RegisterType<IDataProcessDao,DataProcessDao>();
            container.RegisterType<ICSIDataProcessDao, ICSDataProcessDao>();
            container.RegisterType<IFileManagerDao,FileManagerDao>();
            container.RegisterType<ISearchPageService,SearchPageService>();
            container.RegisterType<IReportService, ReportService>();
            container.RegisterType<IMICRImageDao,MICRImageDao>();
            container.RegisterType<IVerificationDao, VerificationDao>();
            container.RegisterType<IDataHouseKeepDao,DataHouseKeepDao>();
            container.RegisterType<IThresholdSettingDao, ThresholdSettingDao>();
            container.RegisterType<IVerificationLimitDao,VerificationLimitDao >();
            container.RegisterType<IHostReturnReasonDao,HostReturnReasonDao>();
            container.RegisterType<ILargeAmountDao,LargeAmountDao>();
            container.RegisterType<IDayEndProcessDao, DayEndProcessDao>();
            container.RegisterType<IReleaseLockedChequeDao, ReleaseLockedChequeDao>();
            container.RegisterType<IReleaseBranchLockedChequeDao, ReleaseBranchLockedChequeDao>();
            container.RegisterType<IBankCodeDao, BankCodeDao>();
            container.RegisterType<ITaskDao,TaskDao>();
            container.RegisterType<IDedicatedBranchDayDao,DedicatedBranchDayDao>();
            container.RegisterType<iDedicatedBranchOfficerDao,DedicatedBranchOfficerDao>();
            container.RegisterType<iHolidayDao,HolidayDao>();
            container.RegisterType<IBandQueueSettingDao,BandQueueSettingDao>();
            //container.RegisterType<IStateCodeDao,StateCodeDao>();
            container.RegisterType<IBranchCodeDao, BranchCodeDao>();
            container.RegisterType<IInternalBranchDao, InternalBranchDao>();
			container.RegisterType<IInternalBranchKBZDao, InternalBranchKBZDao>();
            
            container.RegisterType<ICMSAccountInfoDao,CMSAccountInfoDao>();
            container.RegisterType<IPullOutReasonDao,PullOutReasonDao>();
            container.RegisterType<ITransactionTypeDao,TransactionTypeDao>();
            container.RegisterType<IDataCorrectionDao,DataCorrectionDao>();
			container.RegisterType<IHostFileDao, HostFileDao>();
            container.RegisterType<IRejectReentryDao, RejectReentryDao>();
            container.RegisterType<ISignatureDao, SignatureDao>();
            container.RegisterType<ILateReturnedMaintenanceDao, LateReturnedMaintenanceDao>();
            container.RegisterType<IScheduledTaskDao, ScheduledTaskDao>();
            //container.RegisterType<IInvalidDateDao,InvalidDateDao>();
            container.RegisterType<IOutwardReturnICLDao, OutwardReturnICLDao>();
            container.RegisterType<IGenerateUPIDao, GenerateUPIDao>();
            container.RegisterType<IDbJoinDao, DbJoinDao>();
            container.RegisterType<ISecurityAuditLogDao, SecurityAuditLogDao>();
            container.RegisterType<IBankHostStatusKBZDao, BankHostStatusKBZDao>();
            container.RegisterType<IBankHostStatusDao, BankHostStatusDao>();
            container.RegisterType<IICSDebitPostingDao, ICSDebitPostingDao>();
            container.RegisterType<IGenerateRepairedDebitFileDao, GenerateRepairedDebitFileDao>();
            container.RegisterType<IGenerateCreditFileDao, GenerateCreditFileDao>();
            container.RegisterType<IICSCreditPostingDao, ICSCreditPostingDao>();
            container.RegisterType<ISecurityAuditLogDao, SecurityAuditLogDao>();
            container.RegisterType<IMaintenanceAuditLogDao, MaintenanceAuditLogDao>();
            container.RegisterType<IICSRetentionPeriodDao, ICSRetentionPeriodDao>();
            container.RegisterType<ITransactionCodeDao, TransactionCodeDao>();
            //OCS
            container.RegisterType<IPageConfigDaoOCS, PageConfigDaoOCS>();
            container.RegisterType<ITCCodeDao, TCCodeDao>();
			container.RegisterType<OCSIDataProcessDao, OCSDataProcessDao>();
            container.RegisterType<IHubDao, HubDao>();
            container.RegisterType<IHubBranchDao, HubBranchDao>();
            container.RegisterType<ISystemCutOffTimeDao, SystemCutOffTimeDao>();
            container.RegisterType<IScannerWorkStationDao, ScannerWorkStationDao>();
			container.RegisterType<IProcessDateDao, ProcessDateDao>();
            container.RegisterType<IBranchClearingItemDao, BranchClearingItemDao>();
            container.RegisterType<IAIFImportDao, AIFImportDao>();
			container.RegisterType<IReportServiceOCS, ReportServiceOCS>();
            container.RegisterType<ISearchDao, SearchDao>();
            container.RegisterType<IChequeAccountEntryDao, ChequeAccountEntryDao>();
			container.RegisterType<IChequeAmountEntryDao, ChequeAmountEntryDao>();
            container.RegisterType<IChequeDateAmountEntryDao, ChequeDateAmountEntryDao>();
            container.RegisterType<IMicrRepairDao, MicrRepairDao>();
            container.RegisterType<IReportServiceOCS, ReportServiceOCS>();
            container.RegisterType<IInwardReturnDao, InwardReturnDao>();
            container.RegisterType<ICommonOutwardItemDao, CommonOutwardItemDao>();
            container.RegisterType<ICapturingDao, CapturingDao>();
            container.RegisterType<IClearingDao, ClearingDao>();
			container.RegisterType<IOCSProcessDateDao, OCSProcessDateDao>();
			container.RegisterType<OCSIDataProcessDao, OCSDataProcessDao>();
            container.RegisterType<OCSIReleaseLockedChequeDao, OCSReleaseLockedChequeDao>();
            container.RegisterType<IChequeDataEntryDao, ChequeDataEntryDao>();
            container.RegisterType<IBranchEndOfDayDao, BranchEndOfDayDao>();
            container.RegisterType<IOCSAIFFileUploadDao, OCSAIFFileUploadDao>();
            container.RegisterType<ITransactionBalancingDao, TransactionBalancingDao>();
            container.RegisterType<IBranchEndOfDaySummaryDao, BranchEndOfDaySummaryDao>();
            container.RegisterType<IHostFileOCSDao, HostFileOCSDao>(); 
            container.RegisterType<IClearingItemDao, ClearingItemDao>(); 
            container.RegisterType<IOCSProgressStatusDao, OCSProgressStatusDao>(); 
            container.RegisterType<IClearingItemsSummaryDao, ClearingItemsSummaryDao>();
            container.RegisterType<IBankBranchesOcsDao, BankBranchesOcsDao>();
            container.RegisterType<IOCSChequeImageDao, OCSChequeImageDao>();
            container.RegisterType<IICSChequeImageDao, ICSChequeImageDao>();
            container.RegisterType<IGroupProfileDao, GroupProfileDao>();
            container.RegisterType<ICSIReleaseLockedChequeDao, ICSReleaseLockedChequeDao>();
            container.RegisterType<IAccountProfileDao, AccountProfileDao>();
            container.RegisterType<IPageConfigDaoICS, PageConfigDaoICS>();
            container.RegisterType<ICSILargeAmountDao, ICSLargeAmountDao>();
            container.RegisterType<IOutwardClearingICLDao, OutwardClearingICLDao>();
            container.RegisterType<IOCSDebitPostingDao, OCSDebitPostingDao>();
            container.RegisterType<IOCSCreditPostingDao, OCSCreditPostingDao>();
            container.RegisterType<IBranchDao, BranchDao>();
            container.RegisterType<IProgressStatusDao, ProgressStatusDao>();
			  container.RegisterType<IInwardReturnICLDao, InwardReturnICLDao>();
            container.RegisterType<IBankZoneDao, BankZoneDao>();
            container.RegisterType<IBankChargesTypeDao, BankChargesTypeDao>();
            container.RegisterType<IBankChargesDao, BankChargesDao>();
            container.RegisterType<IAuditTrailOCSDao, AuditTrailOCSDao>();
			container.RegisterType<IOCSRetentionPeriodDao, OCSRetentionPeriodDao>();
            //container.RegisterType<IICSProcessDateDao, ICSProcessDateDao>();
            container.RegisterType<IAutoApproveThresholdDao, AutoApproveThresholdDao>();
            //Azim Start (5 Jan 2021)
            container.RegisterType<INonConformanceFlagDao, NonConformanceFlagDao>();
            container.RegisterType<ITransCodeDao, TransCodeDao>();
            container.RegisterType<IMessageDao, MessageDao>();
            container.RegisterType<IReturnCodeDao, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeDao>();
            container.RegisterType<ISecurityProfileDao, SecurityProfileDao>();
            container.RegisterType<IEnableUserIDDao, EnableUserIDDao>();
            container.RegisterType<IStateCodeDao, INCHEQS.Areas.COMMON.Models.StateCode.StateCodeDao>();
            //Azim End

            //Aifah
            container.RegisterType<IICSStartOfDayDao, ICSStartOfDayDao>();
            container.RegisterType<IICSStartOfDayMakerDao, ICSStartOfDayMakerDao>();
            container.RegisterType<ILoadDailyFileDao, LoadDailyFileDao>();
            container.RegisterType<IBranchSubmissionDao, BranchSubmissionDao>();
            //Aifah End

            //kew Start(23 Jan 2021)
            container.RegisterType<IHighRiskAccountDao, HighRiskAccountDao>();
            container.RegisterType<IRationalBranchDao, RationalBranchDao>();
            container.RegisterType<ILoadNcfDao, LoadNcfDao>();
            //Kew End
            container.RegisterType<INCHEQS.Areas.ICS.Models.VerificationLimit.IVerificationLimitDao, INCHEQS.Areas.ICS.Models.VerificationLimit.VerificationLimitDao>();
            container.RegisterType<IViewPPSFileDao, ViewPPSFileDao>();
            container.RegisterType<IStartBatchICRDao, StartBatchICRDao>();
            container.RegisterType<ILoadPPSFileDao, LoadPPSFileDao>();
            container.RegisterType<IPositivePayMatchingDao, PositivePayMatchingDao>();
            container.RegisterType<INCHEQS.Areas.PPS.Models.Verification.IVerificationDao, INCHEQS.Areas.PPS.Models.Verification.VerificationDao>();
            container.RegisterType<ILoadBankHostFileDao, LoadBankHostFileDao>();
			container.RegisterType<ILoadBankHostFile2ndDao, LoadBankHostFile2ndDao>();
			container.RegisterType<ITruncationDao, TruncationDao>();
            container.RegisterType<ICreditAccountDao, CreditAccountDao>();
            container.RegisterType<INCHEQS.Areas.ICS.Models.HostValidation.IHostValidationDao, HostValidationDao>();
            container.RegisterType<INCHEQS.Areas.ICS.Models.DayEndSettlement.IDayEndSettlementDao, DayEndSettlementDao>();


            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}