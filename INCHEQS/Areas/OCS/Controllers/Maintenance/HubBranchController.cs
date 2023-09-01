using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Areas.OCS.Models.HubBranch;
using INCHEQS.Areas.COMMON.Models.HubUser;
using INCHEQS.Areas.COMMON.Models.InternalBranch;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance
{
    public class HubBranchController : BaseController
    {
        private readonly IHubDao hubDao;
        private readonly IHubBranchDao hubBranchDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;

        public HubBranchController(IHubDao hubDao, IHubBranchDao hubBranchDao, IAuditTrailDao auditTrailDao, IPageConfigDaoOCS pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.hubDao = hubDao;
            this.hubBranchDao = hubBranchDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.HubBranchProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.HubBranchProfile.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HubBranchProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.HubBranchProfile.INDEX, "View_Hub", "fldHubCode", "fldBankCode =@fldBankCode", new[] {
             new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HubBranchProfile.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string hubIdParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string hubId = "";
            if (string.IsNullOrEmpty(hubIdParam))
            {
                hubId = filter["fldHubId"].Trim();
            }
            else
            {
                hubId = hubIdParam;
            }

           // ViewBag.Hub = hubDao.GetHub(hubId, CurrentUser.Account.BankCode);
            ViewBag.SelectedBranch = hubBranchDao.ListSelectedBranchInHub(hubId, CurrentUser.Account.BankCode);
            ViewBag.AvailableBranch = hubBranchDao.ListAvailableBranchInHub(hubId, CurrentUser.Account.BankCode);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HubBranchProfile.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("HubBranchChecker", CurrentUser.Account.BankCode).Trim();
            try
            {
                List<string> branchIds = new List<string>();
                string hubId = col["fldHubId"].Trim();

                if (hubBranchDao.CheckPendingApproval(hubId, CurrentUser.Account.BankCode))
                {
                    TempData["ErrorMsg"] = Locale.HubBranchPendingApproval;
                    return RedirectToAction("Edit", new { hubIdParam = hubId });
                }
                else if ("N".Equals(systemProfile))
                {
                    hubBranchDao.AddHubToHubMasterBranchTempToUpdate(col, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);

                    string beforeBranch = "";
                    List<InternalBranchModel> beforeBranchLists = hubBranchDao.ListSelectedBranchInHub(hubId, CurrentUser.Account.BankCode);
                    foreach (var beforeBranchlist in beforeBranchLists)
                    {
                      //  beforeBranch = beforeBranch + beforeBranchlist.fldBranchAbb + ',';
                    }
                    auditTrailDao.Log("Update Branch In Hub, Before Update =>- Hub ID: " + hubId + " Branch : " + beforeBranch, CurrentUser.Account);

                    if ((col["selectedBranch"]) != null)
                    {
                        branchIds = col["selectedBranch"].Split(',').ToList();
                        foreach (string branchId in branchIds)
                        {
                            hubBranchDao.AddBranchToHubBranchTempToUpdate(branchId, hubId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                        }
                    }
                    else
                    {
                        hubBranchDao.DeleteAllBranchInHubTemp(hubId);
                    }

                    hubBranchDao.UpdateHubMaster(hubId);
                    auditTrailDao.Log("Approve - Update Hub ID : " + hubId, CurrentUser.Account);

                    string afterBranch = "";
                    List<InternalBranchModel> afterBranchLists = hubBranchDao.ListSelectedBranchInHub(hubId, CurrentUser.Account.BankCode);
                    foreach (var afterBranchlist in afterBranchLists)
                    {
                       // afterBranch = afterBranch + afterBranchlist.fldBranchCode + ',';
                    }
                    auditTrailDao.Log("Update Branch In Hub , After Update => - Hub ID: " + hubId + " Branch : " + afterBranch, CurrentUser.Account);

                    hubBranchDao.DeleteInHubMasterBranchTemp(hubId);
                    hubBranchDao.DeleteAllBranchInHubTemp(hubId);

                    TempData["Notice"] = Locale.SuccessfullyUpdated;
                    return RedirectToAction("Index");
                }
                else
                {
                    hubBranchDao.AddHubToHubMasterBranchTempToUpdate(col, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);

                    string beforeBranch = "";
                    List<InternalBranchModel> beforeBranchLists = hubBranchDao.ListSelectedBranchInHub(hubId, CurrentUser.Account.BankCode);
                    foreach (var beforeBranchlist in beforeBranchLists)
                    {
                        //beforeBranch = beforeBranch + beforeBranchlist.fldBranchAbb + ',';
                    }
                    auditTrailDao.Log("Update Branch In Hub, Before Update =>- Hub ID: " + hubId + " Branch : " + beforeBranch, CurrentUser.Account);

                    if ((col["selectedBranch"]) != null)
                    {
                        branchIds = col["selectedBranch"].Split(',').ToList();
                        foreach (string branchId in branchIds)
                        {
                            hubBranchDao.AddBranchToHubBranchTempToUpdate(branchId, hubId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                        }
                    }
                    else
                    {
                        hubBranchDao.DeleteAllBranchInHubTemp(hubId);
                    }

                    auditTrailDao.Log("Add into Temporary record to Update - Hub Id: " + hubId, CurrentUser.Account);
                    TempData["Notice"] = Locale.HubBranchSuccessfullyUpdated;
                    return RedirectToAction("Index");
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}