using System;

namespace INCHEQS.TaskAssignment
{
    public class TaskIdsSDS
    {
	
	  	public static string PrintCIF = "800161";

        public static class ApprovedUserChecker
        {
            public const string DELETE = "102183";
            public const string INDEX = "102181";
            public const string VERIFY = "102181";
            public const string VERIFYA = "102181";
        }
	public static class ApprovedBranchChecker
        {
            public const string DELETE = "102183";
            public const string INDEX = "107300";
            public const string VERIFY = "107300";
            public const string VERIFYA = "107300";
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
        public static class ApprovedHubBranchChecker
        {
            public const string DELETE = "102183";
            public const string DETAILS = "107500";
            public const string INDEX = "107500";
            public const string VERIFY = "107500";
            public const string VERIFYA = "107500";
        }
        public static class HubBranchProfile
        {
            public const string EDIT = "107032";
            public const string INDEX = "107030";
            public const string UPDATE = "107032";
        }
        public static class CountryCode
        {
            public const string INDEX = "800000";
            public const string DETAIL = "800002";
            public const string UPDATE = "800002";
            public const string CREATE = "800001";
            public const string SAVECREATE = "800001";
            public const string DELETE = "800003";
        }

        public static class AccountType
        {
            public const string INDEX = "800010";
            public const string DETAIL = "800012";
            public const string UPDATE = "800012";
            public const string CREATE = "800011";
            public const string SAVECREATE = "800011";
            public const string DELETE = "800013";
        }

        public static class AccountTypeChecker
        {
            public const string INDEX = "800115";
            public const string VERIFY = "800115";
        }

        public static class ProductCode
        {
            public const string INDEX = "800020";
            public const string DETAIL = "800022";
            public const string UPDATE = "800022";
            public const string CREATE = "800021";
            public const string SAVECREATE = "800021";
            public const string DELETE = "800023";
        }

        public static class Scan
        {
            public const string INDEX = "800030";
        }

        public static class LoadCIFFile
        {
            public const string INDEX = "800230";
            public const string VERIFY = "800230";
        }
        public static class LoadAIFFile
        {
            public const string INDEX = "800210";
            public const string VERIFY = "800210";
        }
        public static class LoadSourceImageFile
        {
            public const string INDEX = "800130";
            public const string VERIFY = "800130";
        }

        public static class ReleaseLockedItem
        {
            public const string INDEX = "205800";
            public const string DELETE = "205800";
        }
        public static class AccountVerificationSummary
        {
            public const string INDEX = "800040";
            public const string NEW = "800041";
            public const string VALIDATED = "800042";
            public const string INVALIDATE = "800043";
            public const string REJECT = "800044";
        }
	public static class ApprovedTaskChecker
        {
            public const string DELETE = "102183";
            public const string INDEX = "107200";
            public const string VERIFY = "107200";
            public const string VERIFYA = "107200";
        }
	
        public static class AccountMaintenance
        {
            public const string INDEX = "800020";
        }

        public static class AccountStatus
        {
            public const string INDEX = "800070";
            public const string CREATE = "800071";
            public const string SAVECREATE = "800071";
            public const string UPDATE = "800072";
            public const string DETAIL = "800072";
            public const string DELETE = "800073";
        }

        public static class AccountStatusChecker
        {
            public const string INDEX = "800080";
            public const string VERIFY = "800080";
	}
	public static class SignatureCondition
        {
            public const string INDEX = "800050";
            public const string DETAIL = "800052";
            public const string UPDATE = "800052";
            public const string CREATE = "800051";
            public const string SAVECREATE = "800051";
            public const string DELETE = "800053";
        }
		public static class SignatureConditionChecker
        {
            public const string INDEX = "800060";
            public const string VERIFY = "800060";
        }
	
        public static class RetentionPeriod
        {
            public const string INDEX = "800110";
            public const string UPDATE = "800110";
        }
		
		   public static class SourceImageScheduler
        {
            public const string INDEX = "800090";
            public const string UPDATE = "800090";

        }
        public static class SDSMigrationActivation
        {
            public const string INDEX = "800180";
            public const string VERIFY = "800180";
        }
        public static class SourceImageSchedulerChecker
        {
            public const string INDEX = "800100";
            //public const string VERIFY = "102184";
            public const string VERIFY = "800100";
        }

        public static class ApprovedRetentionPeriodChecker
        {
            public const string INDEX = "800120";
            public const string VERIFY = "800120";
            public const string VERIFYA = "800120";
        }
        public static class ApprovedSecurityProfileChecker
        {
            public const string INDEX = "205181";
            public const string VERIFY = "205181";

        }
		   public static class SecurityAuditLog
        {
            public const string INDEX = "205131";
        }

        public static class Login
        {
            public const string INDEX = "100001";
        }
		public static class AccountVerification
        {
            public const string INDEX = "800140";
        }
		        public static class CIFVerification
        {
            public const string INDEX = "800240";
        }
		
		        public static class SignatureViewerCIF
        {
            public const string INDEX = "800160";
        }
   		public static class SignatureViewer
        {
            public const string INDEX = "800150";
        }

 			public static class CIFVerificationSummary
       	 {
            public const string INDEX = "800200";
            public const string NEW = "800203";
            public const string VALIDATED = "800201";
            public const string INVALIDATE = "800202";
            public const string REJECT = "800203";
        	}
        
        public static class ClearDataProcess
        {
            public const string INDEX = "803000";
            public const string DELETE = "803000";
        }
		   public static class CIFMaintenance
        {
            public const string INDEX = "800220";
            public const string CREATE = "800221";
            public const string SAVECREATE = "800221";
            public const string DELETE = "800222";
            public const string UPDATE = "800223";
        }
        public static class ReleaseLockedCIFItem
        {
            public const string INDEX = "205801";
        }
    }
}