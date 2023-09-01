using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.TaskAssignment {
    public class TaskIds {
        public static string PrintCheque = "62000";
        public static string PrintChequeRetriever = "62010";

        public static class Dashboard {
            public const string MAIN = "309010";
        }

        public static class Group {
            public const string INDEX = "102220";
            public const string EDIT = "102222";
            public const string UPDATE = "102222";
            public const string CREATE = "102221";
            public const string SAVECREATE = "102221";
            public const string DELETE = "102223";

        }

        public static class ReturnCode {
            public const string INDEX = "102150";
            public const string EDIT = "102152";
            public const string UPDATE = "102152";
            public const string CREATE = "102151";
            public const string SAVECREATE = "102151";
            public const string DELETE = "102153";
        }
        public static class Task {
            public const string INDEX = "102230";
            public const string EDIT = "102231";
            public const string UPDATE = "102231";


        }
        public static class TransactionCode {
            public const string INDEX = "102140";
            public const string EDIT = "102142";
            public const string UPDATE = "102142";
            public const string CREATE = "102141";
            public const string SAVECREATE = "102141";
            public const string DELETE = "102143";

        }
        public static class TransactionType {
            
            public const string INDEX = "102270";
            public const string DETAIL = "102272";
            public const string UPDATE = "102272";
            public const string CREATE = "102271";
            public const string SAVECREATE = "102271";
            public const string DELETE = "102274";
        }

        public static class ChangePassword {
            public const string UPDATE = "205110";
            public const string INDEX = "205110";
        }
        public static class AuditLog {
            public const string INDEX = "205130";
        }
        public static class Archive {
            public const string INDEX = "205400";
        }
        public static class ClearDataProcess {
            public const string INDEX = "205300";
            public const string DELETE = "205300";

        }
        public static class ClearUserSession {
            public const string INDEX = "205200";
            public const string DELETE = "205200";

        }
        public static class ReleaseLockedCheque {
            public const string INDEX = "205500";
            public const string DELETE = "205500";
        }
        public static class ReleaseBranchLockedCheque
        {
            public const string INDEX = "205600";
            public const string DELETE = "205600";
        }
        public static class DataHousKeep {
            public const string INDEX = "205210";
            public const string UPDATE = "205210";

        }
        public static class DayEndProcess {
            public const string INDEX = "205220";
            public const string SAVECREATE = "205220";

        }
        public static class SecurityProfile {
            public const string INDEX = "205180";
            public const string UPDATE = "205180";

        }
        public static class SendEmail {
            public const string INDEX = "205280";
            public const string CREATE = "205280";

        }
        public static class User {
            public const string INDEX = "102160";
            public const string EDIT = "102162";
            public const string UPDATE = "102162";
            public const string CREATE = "102161";
            public const string SAVECREATE = "102161";
            public const string DELETE = "102163";

        }
        public static class BandQueueSetting {
            public const string INDEX = "102570";
            public const string EDIT = "102572";
            public const string UPDATE = "102572";
            public const string CREATE = "102571";
            public const string SAVECREATE = "102571";
            public const string DELETE = "102573";

        }
        public static class VerificationLimit {
            public const string INDEX = "102380";
            public const string EDIT = "102382";
            public const string UPDATE = "102382";
            public const string CREATE = "102381";
            public const string SAVECREATE = "102381";
            public const string DELETE = "102383";
        }
        public static class ReviewMine {
            public const string INDEX = "306340";
        }

        public static class ProgressStatus {
            public const string INDEX = "306360";
        }
        public static class BranchProgressStatus
        {
            public const string INDEX = "306370";
        }
        public static class ThresholdSetting {
            public const string MAIN = "102680";
            public const string CREATE = "102681";
            public const string SAVECREATE = "102682";
            public const string UPDATE = "102682";
            public const string EDIT = "102682";
            public const string DELETE = "102683";
        }
        public static class InternalBranch {
            public const string INDEX = "102690";
            public const string EDIT = "102692";
            public const string UPDATE = "102692";
            public const string CREATE = "102691";
            public const string SAVECREATE = "102691";
            public const string DELETE = "102693";

        }
        public static class HubBranchProfile {
            public const string INDEX = "2720";
            public const string EDIT = "2722";
            public const string UPDATE = "2722";
            public const string CREATE = "2721";
            public const string SAVECREATE = "2721";
            public const string DELETE = "2723";

        }
        public static class HubUserProfile {
            public const string INDEX = "2730";
            public const string EDIT = "2732";
            public const string UPDATE = "2732";
            public const string CREATE = "2731";
            public const string SAVECREATE = "2731";
            public const string DELETE = "2733";

        }
        public static class BankCode {
            public const string INDEX = "102110";
            public const string EDIT = "102112";
            public const string UPDATE = "102112";
            public const string CREATE = "102111";
            public const string SAVECREATE = "102111";
            public const string DELETE = "102113";

        }
        public static class PullOutReason {
            public const string INDEX = "102390";
            public const string EDIT = "102392";
            public const string UPDATE = "102392";
            public const string CREATE = "102391";
            public const string SAVECREATE = "102391";
            public const string DELETE = "102393";

        }
        public static class BranchCode {
            public const string INDEX = "102340";
            public const string EDIT = "102342";
            public const string UPDATE = "102342";
            public const string CREATE = "102341";
            public const string SAVECREATE = "102341";
            public const string DELETE = "102343";

        }
        public static class StateCode
        {
            public const string INDEX = "102130";
            public const string EDIT = "102131";
            public const string UPDATE = "102131";
            public const string CREATE = "102132";
            public const string SAVECREATE = "102132";
            public const string DELETE = "102133";

        }
        public static class DashboardProgressStatus {
            public const string MAIN = "309012";
        }

        public static class BranchActivation {
            public const string INDEX = "306290";
        }
        public static class HostReturnReason {
            public const string INDEX = "102120";
            public const string CREATE = "102121";
            public const string SAVECREATE = "102121";
            public const string EDIT = "102122";
            public const string UPDATE = "102122";
            public const string DELETE = "102123";
        }
        public static class TaskScheduler {
            public const string INDEX = "205420";
        }
        public static class ApprovedChecker {
            public const string INDEX = "102180";
            public const string DELETE = "102183";
            public const string VERIFY = "102184";
            public const string VERIFYA = "102185";
        }
        public static class LargeAmount {
            public const string INDEX = "102670";
        }

        public static class AccountMaintenance {
            public const string INDEX = "101230";
            public const string CREATE = "101231";
            public const string SAVECREATE = "101231";
            public const string EDIT = "101232";
            public const string UPDATE = "101232";
            public const string DELETE = "101233";
        }

        public static class AccountType {
            public const string INDEX = "101240";
            public const string CREATE = "101241";
            public const string SAVECREATE = "101241";
            public const string EDIT = "101242";
            public const string UPDATE = "101242";
            public const string DELETE = "101243";
        }

        public static class HighRiskAccount {
            public const string INDEX = "101110";
            public const string CREATE = "101111";
            public const string SAVECREATE = "101111";
            public const string EDIT = "101112";
            public const string UPDATE = "101112";
            public const string DELETE = "101113";
        }

        public static class CMSAccountInfo {
            public const string INDEX = "102520";
            public const string CREATE = "102521";
            public const string SAVECREATE = "102521";
            public const string EDIT = "102522";
            public const string UPDATE = "102522";
            public const string DELETE = "102523";
        }

        public static class GLReplacement {
            public const string INDEX = "103010";
            public const string CREATE = "103011";
            public const string SAVECREATE = "103011";
            public const string EDIT = "103012";
            public const string UPDATE = "103012";
            public const string DELETE = "103013";
        }

        public static class DedicatedBranchDay
        {
            public const string INDEX = "104010";
            public const string CREATE = "104011";
            public const string EDIT = "104011";
            public const string UPDATE = "104012";
            public const string RELOAD = "104011";
            public const string SAVECREATE = "104012";
            public const string DELETE = "104013";
        }

        public static class DedicatedBranchOfficer
        {
            public const string INDEX = "105010";
            public const string EDIT = "105011";
            public const string UPDATE = "105012";
        }

        public static class Holiday
        {
            public const string INDEX = "106010";
            public const string CREATE = "106011";
            public const string EDIT = "106011";
            public const string UPDATE = "106012";
            public const string SAVECREATE = "106012";
            public const string DELETE = "106013";
        }

        public static class ICSDashboard
        {
            public const string INDEX = "601110";
        }

        public static class InvalidDate
        {
            public const string INDEX = "400000";
        }

        public static class ScannerWorkStation
        {
            public const string INDEX = "999910";
            public const string DELETE = "999913";
        }
    }

}
