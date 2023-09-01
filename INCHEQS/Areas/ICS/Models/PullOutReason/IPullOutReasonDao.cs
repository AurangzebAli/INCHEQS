using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.PullOutReason {
    public interface IPullOutReasonDao {

        DataTable ListAllPullOutReason();
        DataTable getPullOutReason(string PullOutID);
        void Update(FormCollection collection, string userId);
        void CreatePullOutReasonTemp(FormCollection collection, string userId);
        void DeleteInPullOutReason(string del);
        List<string> Validate(FormCollection collection);
        PullOutReasonModel getPullOutData(string pulloutid);
        void AddToPullOutReasonTempToDelete(string pulloutId);
        void CreateInPullOutReason(string pulloutId);
        void DeleteInPullOutReasonTemp(string pulloutId);

    }
}