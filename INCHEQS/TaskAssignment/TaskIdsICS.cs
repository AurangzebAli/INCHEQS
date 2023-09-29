using System;

namespace INCHEQS.TaskAssignment
{
    public class TaskIdsICS
    {
        public static class LargeAmount
        {
            public const string INDEX = "102670";
            public const string EDIT = "102677";
            public const string UPDATE = "102677";
        }

        public static class LargeAmountChecker
        {
            public const string INDEX = "102671";
            public const string VERIFY = "102671";
            public const string VERIFYA = "102671";
        }
        public static class AutoApproveThreshold
        {
            public const string INDEX = "306420";
            public const string EDIT = "306421";
        }
        public static class GenerateOutwardReturnICL
        {
            public const string INDEX = "309220";
            public const string Clear = "309221";
            public const string Cleared = "309222";

        }
       public static class GenerateDebitFile
        {
            public const string INDEX = "999850";
            public const string POSTED = "999851";

        }

        public static class GenerateRepairedDebitFile
        {
            public const string INDEX = "309124";
            public const string POSTED = "309125";
        }
        public static class GenerateUPI
        {
            public const string INDEX = "309300";
            public const string LATERETURN = "309305";
            public const string GENERATED = "309310";
        }
        public static class HostValidation
        {
            public const string INDEX = "306250";
        }
        public static class DayEndSettlement
        {
            public const string INDEX = "306720";
        }

        public static class GenerateCreditFileBased
        {
            public const string INDEX = "309123";
            public const string POSTED = "309132";
        }
        public static class GenerateCreditFile
        {
            public const string INDEX = "999860";
            public const string POSTED = "999861";

        }
        public static class ICSRetentionPeriod
        {
            public const string INDEX = "205310";
            public const string UPDATE = "205310";
        }
        public static class ICSRetentionPeriodChecker
        {
            public const string INDEX = "205311";
            public const string VERIFY = "205311";
        }

        /*public static class ICSProcessDate
        {
            public const string INDEX = "999901";
        }*/

        public static class ICSStartOfDayMaker1
        {
            public const string INDEX = "999901";
        }

        public static class ICSStartOfDayChecker
        {
            public const string INDEX = "999902";
            public const string VERIFY = "999902";
        }

        public static class ICSStartOfDay
        {
            public const string INDEX = "999903";
        }
		
		  public static class MICRImage
        {
            public const string INDEX = "306010";
            public const string DOWNLOAD = "306011";
           
        }
        public static class LoadDailyFile
        {
            public const string INDEX = "103110";
            public const string DOWNLOAD = "103111";
            public const string STATUS = "103112";
        }

        public static class LoadNcf
        {
            public const string INDEX = "309127";
            public const string DOWNLOAD = "309128";
        }
		
		public static class BranchSubmission
        {
            public const string INDEX = "306280";
        }

        public static class LoadBankHostFile
        {
            public const string INDEX = "309126";
            public const string DOWNLOAD = "309129";

        }

        public static class LoadBankHostFile2nd
        {
            public const string INDEX = "309130";
            public const string DOWNLOAD = "309131";

        }

        public static class LateReturnMaintenance
        {
            public const string INDEX = "102675";
            public const string DELETE = "102674";
            public const string CREATE = "102672";
            public const string UPDATE = "102673";
        }
		
		public static class ViewUPI
        {
            public const string INDEX = "309230";
        }

        public static class Signature
        {
            public const string INDEX = "401000";
        }

        
    }
}