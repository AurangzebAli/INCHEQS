using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
//using INCHEQS.InternalBranch.InternalBranch;
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
using INCHEQS.Areas.COMMON.Models.HubUser;
using INCHEQS.Areas.COMMON.Models.HubBranch;
using INCHEQS.Areas.COMMON.Models.Branch;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;


namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class HubBranchController : BaseController
    {
        private readonly IBranchDao branchDao;
        private readonly IHubDao hubDao;
        private readonly IHubBranchDao hubBranchDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;
        public HubBranchController(IBranchDao branchDao, IHubDao hubDao, IHubBranchDao hubBranchDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.branchDao = branchDao;
            this.pageConfigDao = pageConfigDao;
            this.hubDao = hubDao;
            this.hubBranchDao = hubBranchDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;
        }
        [CustomAuthorize(TaskIds = TaskIdsSDS.HubBranchProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsSDS.HubBranchProfile.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsSDS.HubBranchProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsSDS.HubBranchProfile.INDEX, "View_HubBranch", "", "fldBankCode =@fldBankCode", new[] {
             new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }



        [CustomAuthorize(TaskIds = TaskIdsSDS.HubBranchProfile.EDIT)]
        [GenericFilter(AllowHttpGet = true)]

        public ActionResult Edit(FormCollection collection, string hubIdParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string hubCode = "";
            if (string.IsNullOrEmpty(hubIdParam))
            {
                hubCode = filter["fldHubCode"].Trim();
            }
            else
            {
                hubCode = hubIdParam;
            }

            ViewBag.Hub = hubDao.CheckHubMasterByID(hubCode, "HubCode");
            ViewBag.SelectedBranch = hubBranchDao.ListSelectedBranchInHub(hubCode, CurrentUser.Account.BankCode);
            ViewBag.AvailableBranch = hubBranchDao.ListAvailableBranchInHub(CurrentUser.Account.BankCode, hubCode);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsSDS.HubBranchProfile.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            string sTaskID = TaskIdsSDS.HubBranchProfile.UPDATE;
            //string systemProfile = systemProfileDao.GetValueFromSystemProfile("HubChecker", CurrentUser.Account.BankCode).Trim();
            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

            try
            {
                List<string> branchIds = new List<string>();
                string hubCode = col["fldHubCode"].Trim();
                List<string> errorMessages = hubBranchDao.ValidateHubBranch(col, "SelectAll", "", CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if (hubBranchDao.CheckHubBranchMasterTempByID(col["fldHubCode"].Trim(), "HubCode") == true)
                    {
                        TempData["Warning"] = Locale.HubBranchPendingApproval;
                        return RedirectToAction("Edit", new { hubIdParam = hubCode });
                    }
                    else
                    {


                        if ("N".Equals(securityProfile))
                        {
                            HubModel beforeHub = hubDao.CheckHubMasterByID(hubCode, "HubCode");
                            //auditTrailDao.Log("Update Hub- Before Update=> Hub Code: " + beforeHub.fldHubId + " Hub Desc : " + beforeHub.fldHubDesc, CurrentUser.Account);

                            //hubBranchDao.CreateHubBranchMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId);

                            string beforeBranch = "";
                            List<BranchModel> beforeBranchLists = hubBranchDao.ListSelectedBranchInHub(hubCode, CurrentUser.Account.BankCode);
                            foreach (var beforeBranchlist in beforeBranchLists)
                            {
                                beforeBranch = beforeBranch + beforeBranchlist.fldBranchCode + ',';
                            }

                            //auditTrailDao.Log("Update Branch In Hub, Before Update =>- Hub Code: " + hubCode + " Branch : " + beforeBranch, CurrentUser.Account);

                            if ((col["selectedBranch"]) != null)
                            {
                                branchIds = col["selectedBranch"].Split(',').ToList();
                                foreach (string branchId in branchIds)
                                {
                                    hubBranchDao.AddBranchToHubBranchTempToUpdate(branchId, hubCode, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "Create");
                                    //hubBranchDao.UpdateSelectedBranch(hubCode, branchId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                                }
                            }
                            else
                            {
                                hubBranchDao.DeleteAllBranchInHubTemp(hubCode);
                            }

                            hubBranchDao.UpdateHubMaster(col, CurrentUser.Account.UserId);
                            hubBranchDao.UpdateHubBranch(hubCode);

                            HubModel afterHub = hubDao.CheckHubMasterByID(hubCode, "HubCode");
                            // auditTrailDao.Log("Update Hub- After Update=> Hub Code: " + afterHub.fldHubId + " Hub Desc : " + afterHub.fldHubDesc, CurrentUser.Account);

                            string afterBranch = "";
                            List<BranchModel> afterBranchLists = hubBranchDao.ListSelectedBranchInHub(hubCode, CurrentUser.Account.BankCode);
                            foreach (var afterBranchlist in afterBranchLists)
                            {
                                afterBranch = afterBranch + afterBranchlist.fldBranchCode + ',';
                            }
                            //auditTrailDao.Log("Update Branch In Hub , After Update => - Hub Code: " + hubCode + " Branch : " + afterBranch, CurrentUser.Account);

                            //hubBranchDao.DeleteInHubMasterBranchTemp(hubCode);
                            hubBranchDao.DeleteAllBranchInHubTemp(hubCode);

                            string ActionDetails = SecurityAuditLogDao.HubBranch_EditTemplate(beforeBranch, afterBranch, "Edit", "Update", col);
                            auditTrailDao.SecurityLog("Edit Group", ActionDetails, sTaskID, CurrentUser.Account);

                            TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                            return RedirectToAction("Edit", new { hubIdParam = hubCode });
                        }
                        else
                        {
                            //hubBranchDao.CreateHubBranchMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId);

                            if ((col["selectedBranch"]) != null)
                            {
                                branchIds = col["selectedBranch"].Split(',').ToList();
                                foreach (string branchId in branchIds)
                                {
                                    hubBranchDao.AddBranchToHubBranchTempToUpdate(branchId, hubCode, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "Create");
                                }
                                string alreadyselectedbranch = "";
                                string selectedbranchId = "";
                                List<BranchModel> alreadyselectedbranchLists = hubBranchDao.ListSelectedBranchInHub(hubCode, CurrentUser.Account.BankCode);
                                foreach (var branchlist in alreadyselectedbranchLists)
                                {
                                    alreadyselectedbranch = branchlist.fldBranchCode;
                                    selectedbranchId = branchlist.fldBranchId;
                                    if (hubBranchDao.CheckHubBranchExistInTemp(hubCode, selectedbranchId, "Update") == false)
                                    {
                                        hubBranchDao.AddBranchToHubBranchTempToUpdate(selectedbranchId, hubCode, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "Delete");
                                    }
                                }
                            }
                            else
                            {
                                string beforeBranch = "";
                                List<BranchModel> beforeBranchLists = hubBranchDao.ListSelectedBranchInHub(hubCode, CurrentUser.Account.BankCode);
                                foreach (var beforeBranchlist in beforeBranchLists)
                                {
                                    //beforeBranch = beforeBranchlist.fldBranchCode;
                                    beforeBranch = beforeBranchlist.fldBranchId;

                                    hubBranchDao.AddBranchToHubBranchTempToUpdate(beforeBranch, hubCode, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "Delete");
                                }
                            }

                            string beforeBranch1 = "";
                            List<BranchModel> beforeBranchLists1 = hubBranchDao.ListSelectedBranchInHub(hubCode, CurrentUser.Account.BankCode);
                            foreach (var beforeBranchlist in beforeBranchLists1)
                            {
                                beforeBranch1 = beforeBranch1 + beforeBranchlist.fldBranchCode + " - " + beforeBranchlist.fldBranchDesc + "\n";
                            }
                            string afterBranch1 = "";
                            List<BranchModel> afterHubLists1 = SecurityAuditLogDao.ListSelectedBranchInHubTemp_Security(hubCode, CurrentUser.Account.BankCode);
                            foreach (var afterBranchlist in afterHubLists1)
                            {
                                afterBranch1 = afterBranch1 + afterBranchlist.fldBranchCode + " - " + afterBranchlist.fldBranchDesc + "\n";
                            }
                            string ActionDetails = SecurityAuditLogDao.HubBranch_EditTemplate(beforeBranch1, afterBranch1, "Edit", "Update", col);
                            auditTrailDao.SecurityLog("Edit Group", ActionDetails, sTaskID, CurrentUser.Account);


                            //auditTrailDao.Log("Add into Temporary record to Update - Hub Code: " + hubCode, CurrentUser.Account);
                            TempData["Notice"] = Locale.HubBranchAddedToTempForUpdate;

                        }

                    }


                }
                return RedirectToAction("Edit", new { hubIdParam = hubCode });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}