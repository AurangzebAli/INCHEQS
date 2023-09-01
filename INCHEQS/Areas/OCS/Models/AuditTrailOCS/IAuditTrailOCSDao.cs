using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.Areas.OCS.Models.ChequeDateAmountEntry;
using System.Web.Mvc;
using INCHEQS.Security.Account;

namespace INCHEQS.Areas.OCS.Models.AuditTrailOCS
{
    public interface IAuditTrailOCSDao
    {

        string ChequeAmount_Confirm(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item);
        string ChequeAmount_Reject(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item);
        AuditTrailOCSModel CheckItem(string ItemId, string TransNo);
        AuditTrailOCSModel CheckItemReject(string ItemId, string TransNo);

        AuditTrailOCSModel CheckItemDataEntry(string TransNo);
        AuditTrailOCSModel CheckItemBalancing(string TransNo);

        //List<AuditTrailOCSModel>CheckItemDataEntry(string TransNo);

        //AuditTrailOCSModel CheckItemDataEntry(string ItemId, string TransNo);
        AuditTrailOCSModel CheckItemDataEntryReject(string ItemId, string TransNo);
        string ChequeDataEntry_Confirm(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item); 
        string ChequeDataEntry_Reject(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item);

        //string ChequeBalancing_Confirm(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item);
        List<AuditTrailOCSModel> ListBalancing(string ItemId,string TransNo);
        List<AuditTrailOCSModel> ListBalancingC(string ItemId, string TransNo);
        string ChequeBalancing_Confirm(AuditTrailOCSModel before, AuditTrailOCSModel after, string beforeBalancingAmountV, string afterBalancingAmountV,
           string beforeBalancingAmountC, string afterBalancingAmountC, string TableHeader, FormCollection item);
        AuditTrailOCSModel CheckItemRejectBalancing(string ItemId, string TransNo);
        string ChequeBalancing_Reject(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item);

        AuditTrailOCSModel CheckItemSearch(string ItemId, string TransNo);
        string ChequeSearch_Reject(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item);
        string ChequeSearch_Remark(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item);


        void AuditTrailOCSLog(string ActionPerformed, string ActionDetails, string TaskId, string ItemID, string TransNo, AccountModel currentUserAccount = null);

    }
}