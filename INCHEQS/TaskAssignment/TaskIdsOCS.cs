using System;

namespace INCHEQS.TaskAssignment
{
    public class TaskIdsOCS
    {
        public static string PrintChequeOCS = "662000";
        public static string Login = "100000";
        public static string Logout = "999999";
        public static string System = "999998";

        public static class TCCode
        {
            public const string INDEX = "190100";
            public const string DETAIL = "190102";
            public const string UPDATE = "190102";
            public const string CREATE = "190101";
            public const string SAVECREATE = "190101";
            public const string DELETE = "190103";
        }

        public static class HubUserProfile
        {
            public const string INDEX = "107020";
            public const string EDIT = "107022";
            public const string UPDATE = "107022";
            public const string CREATE = "107021";
            public const string SAVECREATE = "107021";
            public const string DELETE = "107023";
        }

        public static class HubBranchProfile
        {
            public const string INDEX = "193730";
            public const string EDIT = "193732";
            public const string UPDATE = "193732";
        }

        public static class SystemCutOffTime
        {
            public const string INDEX = "194730";
            public const string EDIT = "194732";
            public const string UPDATE = "194732";
            public const string CREATE = "194731";
            public const string SAVECREATE = "194731";
            public const string DELETE = "194733";
        }

        public static class SystemProfile
        {
            public const string INDEX = "205700";
            public const string UPDATE = "107011";
        }

        public static class ScannerWorkStation
        {
            public const string INDEX = "999910";
            public const string EDIT = "999912";
            public const string UPDATE = "999912";
            public const string CREATE = "999911";
            public const string SAVECREATE = "999911";
            public const string DELETE = "999913";
        }

        public static class Capturing
        {
            public const string INDEX = "999920";
        }

        public static class Clearing
        {
            public const string INDEX = "999930";
            public const string ITEMCLEARING = "700103";
        }

        public static class ClearingSummary
        {
            public const string INDEX = "999940";
        }
        public static class Search
        {
            public const string INDEX = "999950";
            
        }
        public static class ClearDataProcess
        {
            public const string INDEX = "999980";
            public const string DELETE = "999980";
        }
        public static class CheckAmountEntry
        {
            public const string INDEX = "999960";
        }
        public static class ReturnChequeAdvice
        {
            public const string INDEX = "722000";
            public const string GENERATE = "722010";
            public const string PRINT = "722020";
        }
        public static class BranchClearing
        {
            public const string INDEX = "700101";
            public const string GENERATE = "700102";

        }
        public static class GenerateOutwardClearingICL
        {
            public const string INDEX = "700104";
            public const string Clear = "700105";
            public const string Cleared = "700106";

        }
        public static class GenerateCreditFile
        {
            public const string INDEX = "999830";
            public const string POSTED = "999831";
            public const string FILEBASEPOSTED = "999832";

        }

        public static class BranchEndOfDay
        {
            public const string INDEX = "999810";
        }

        public static class BranchEndOfDaySummary
        {
            public const string INDEX = "999820";
        }

        public static class HostFiles
        {
            public const string CREDIT = "999830";
            public const string DEBIT = "999840";
        }
        public static class AIFFileUpload
        {
            public const string INDEX = "999115";
        }

        public static class HolidayApprovedChecker
        {
            public const string INDEX = "800120";
            public const string VERIFY = "800121";
        }
        public static class ApprovedUserChecker
        {
            public const string INDEX = "102181";
            public const string VERIFY = "800122";
            
        }

        public static class ScannerWorkStationChecker
        {
            public const string INDEX = "999914";
            public const string VERIFY = "999915";
        }
        
        public static class ApprovedGroupChecker
        {
            public const string DELETE = "102183";
            public const string INDEX = "107100";
            public const string VERIFY = "107100";
            public const string VERIFYA = "107100";
        }
        public static class ApprovedHubUserChecker
        {
            public const string DELETE = "102183";
            public const string DETAILS = "107400";
            public const string INDEX = "107400";
            public const string VERIFY = "107400";
            public const string VERIFYA = "107400";
        }
        public static class AccountProfile
        {
            public const string INDEX = "107060";
            public const string EDIT = "107062";
            public const string UPDATE = "107062";
            public const string CREATE = "107061";
            public const string SAVECREATE = "107061";
            public const string DELETE = "107063";
        }
        public static class AccountProfileChecker
        {
            public const string INDEX = "107064";
        }
        public static class VerificationLimit
        {
            public const string CREATE = "102381";
            public const string DELETE = "102383";
            public const string EDIT = "102382";
            public const string INDEX = "102380";
            public const string SAVECREATE = "102381";
            public const string UPDATE = "102382";
        }
        public static class VerificationLimitChecker
        {
            public const string INDEX = "800127";
            public const string VERIFY = "800128";
        }
        public static class ThresholdSettingChecker
        {
            public const string INDEX = "102684";
            public const string VERIFY = "102685";

        }
        public static class SecurityProfileChecker
        {
            public const string INDEX = "205181";
            public const string VERIFY = "205181";

        }
        public static class SecurityAuditLog
        {
            public const string INDEX = "205131";
            public const string VERIFY = "205131";

        }
        public static class InwardReturnICL
        {
            public const string INDEX = "712000";
            public const string DOWNLOAD = "712100";
            public const string SUMMARY = "712200";
            public const string DOWNLOADFILE = "712300";

        }

        //Bank Zone (Azim)
        public static class BankZone
        {
            public const string INDEX = "900000";
            public const string CREATE = "900001";
            public const string SAVECREATE = "900001";
            public const string EDIT = "900002";
            public const string UPDATE = "900002";
            public const string DELETE = "900003";
        }

        public static class BankZoneChecker
        {
            public const string INDEX = "900004";
            public const string VERIFY = "900004";
        }

        //Bank Charges Type (Azim)
        public static class BankChargesType
        {
            public const string INDEX = "910000";
            public const string CREATE = "910001";
            public const string SAVECREATE = "910001";
            public const string EDIT = "910002";
            public const string UPDATE = "910002";
            public const string DELETE = "910003";
        }

        public static class BankChargesTypeChecker
        {
            public const string INDEX = "910004";
            public const string VERIFY = "910004";
        }

        //Bank Charges (Azim)
        public static class BankCharges
        {
            public const string INDEX = "920000";
            public const string CREATE = "920001";
            public const string SAVECREATE = "920001";
            public const string EDIT = "920002";
            public const string UPDATE = "920002";
            public const string DELETE = "920003";
        }

        public static class BankChargesChecker
        {
            public const string INDEX = "920004";
            public const string VERIFY = "920004";
        }
        //Internal Branch (Azim)
        public static class InternalBranchKBZ
        {
            public const string INDEX = "102696";
            public const string CREATE = "102697";
            public const string SAVECREATE = "102697";
            public const string EDIT = "102698";
            public const string UPDATE = "102698";
            public const string DELETE = "102699";
        }

        public static class InternalBranchCheckerKBZ
        {
            public const string INDEX = "102700";
            public const string VERIFY = "102701";
        }

        //Internal Branch MAB (Azim)
        public static class InternalBranch
        {
            public const string INDEX = "102690";
            public const string CREATE = "102691";
            public const string SAVECREATE = "102691";
            public const string EDIT = "102692";
            public const string UPDATE = "102692";
            public const string DELETE = "102693";
        }

        public static class InternalBranchChecker
        {
            public const string INDEX = "102694";
            public const string VERIFY = "102695";
        }

       
      public static class AuditLogOCS
        {
            public const string INDEX = "205132";
        }
        public static class OCSProcessDate
        {
            public const string INDEX = "999900";
        }
        //OCS Retention Period
        public static class OCSRetentionPeriod
        {
            public const string INDEX = "205210";
            public const string UPDATE = "205210";
        }

        public static class OCSRetentionPeriodChecker
        {
            public const string INDEX = "205211";
            public const string VERIFY = "205211";
        }
      public static class GenerateDebitFile
        {
            public const string INDEX = "999840";
            public const string POSTED = "999841";

        }

        //Azim Start (13 Jan 2021)
        public static class Message
        {
            public const string INDEX = "102420";
            public const string CREATE = "102421";
            public const string SAVECREATE = "102421";
            public const string EDIT = "102422";
            public const string UPDATE = "102422";
            public const string DELETE = "102423";
        }

        public static class MessageChecker
        {
            public const string INDEX = "102424";
            public const string VERIFY = "102425";
        }

        public static class ReturnCode
        {
            public const string INDEX = "102150";
            public const string CREATE = "102151";
            public const string SAVECREATE = "102151";
            public const string EDIT = "102152";
            public const string UPDATE = "102152";
            public const string DELETE = "102153";
        }

        public static class ReturnCodeChecker
        {
            public const string INDEX = "102154";
            public const string VERIFY = "102155";
        }

        //Azim End
    }
}