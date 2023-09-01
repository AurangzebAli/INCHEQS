using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;

using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{

    public class InternalBranchController : BaseController
    {

        private IInternalBranchDao internalBranchDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        private readonly IAuditTrailDao auditTrailDao;

        public InternalBranchController(IInternalBranchDao InternalBranch, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao, IAuditTrailDao auditTrailDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.internalBranchDao = InternalBranch;

            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
            this.auditTrailDao = auditTrailDao;
        }
        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranch.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.InternalBranch.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranch.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {


            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.InternalBranch.INDEX, "View_InternalBranch", "", ""), collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranch.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string intBranchIdParam = "")
        {

            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);


            string branchid = "";

            if (string.IsNullOrEmpty(intBranchIdParam))
            {
                branchid = filter["fldInternalBranchId"].Trim();
            }
            else
            {
                branchid = intBranchIdParam;
            }

            ViewBag.ClearingBranchIdConv = internalBranchDao.ListInternalBranch("C");
            ViewBag.ClearingBranchIdIslamic = internalBranchDao.ListInternalBranch("I");
            ViewBag.Country = internalBranchDao.ListCountry();

            DataTable dataTable = internalBranchDao.GetInternalBranchData(branchid); //Done


            if ((dataTable.Rows.Count > 0))
            {
                ViewBag.InternalBranch = dataTable.Rows[0];

                if ((ViewBag.InternalBranch["fldCBranchId"].ToString().Trim() == ""))
                {
                    ViewBag.SelfClearingConvDisabled = "disabled";
                    ViewBag.SelfClearingConv = "";
                    ViewBag.Conv = "disabled";
                    ViewBag.CheckConv = "";
                }
                else
                {
                    if (ViewBag.InternalBranch["fldCClearingBranchId"].ToString() == ViewBag.InternalBranch["fldCBranchId"].ToString())
                    {
                        ViewBag.SelfClearingConv = "checked";
                        ViewBag.SelfClearingConvDisabled = "disabled";
                    }
                    else
                    {
                        ViewBag.SelfClearingConv = "";
                        ViewBag.SelfClearingConvDisabled = "";
                    }
                    ViewBag.CheckConv = "checked";
                    ViewBag.Conv = "";
                }

                if ((ViewBag.InternalBranch["fldIBranchId"].ToString().Trim() == ""))
                {
                    ViewBag.SelfClearingIslamic = "";
                    ViewBag.SelfClearingIslamicDisabled = "disabled";
                    ViewBag.Islamic = "disabled";
                    ViewBag.CheckIslamic = "";
                }
                else
                {
                    if (ViewBag.InternalBranch["fldIClearingBranchId"].ToString() == ViewBag.InternalBranch["fldIBranchId"].ToString())
                    {
                        ViewBag.SelfClearingIslamic = "checked";
                        ViewBag.SelfClearingIslamicDisabled = "disabled";
                    }
                    else
                    {
                        ViewBag.SelfClearingIslamic = "";
                        ViewBag.SelfClearingIslamicDisabled = "";
                    }
                    ViewBag.CheckIslamic = "checked";
                    ViewBag.Islamic = "";
                }
            }

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranch.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection)
        {
            ActionResult action;
            string sTaskId = TaskIdsOCS.InternalBranch.INDEX;
            CurrentUser.Account.TaskId = sTaskId;
            try
            {
                List<String> errorMessages = internalBranchDao.ValidateUpdate(collection);

                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();

                if ((errorMessages.Count > 0))
                {

                    TempData["ErrorMsg"] = errorMessages;
                    action = RedirectToAction("Edit", new { intBranchIdParam = collection["internalBranchID"].Trim() });

                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        InternalBranchModel before = MaintenanceAuditLogDao.GetInternalBranchDataById(collection["internalBranchID"], "Master");
                        auditTrailDao.Log("Edit Internal Branch ID - Before Update=> Internal Conv Branch ID: " + before.fldCBranchId + ", Internal Conv Branch Desc: " + before.fldCBranchDesc + "Internal Islamic Branch ID: " + before.fldIBranchId + ", Internal Islamic Branch Desc: " + before.fldIBranchDesc, CurrentUser.Account);

                        internalBranchDao.UpdateInternalBranch(collection); //Done

                        InternalBranchModel after = MaintenanceAuditLogDao.GetInternalBranchDataById(collection["internalBranchID"], "Master");
                        auditTrailDao.Log("Edit Internal Branch ID - Before Update=> Internal Conv Branch ID: " + after.fldCBranchId + ", Internal Conv Branch Desc: " + after.fldCBranchDesc + "Internal Islamic Branch ID: " + after.fldIBranchId + ", Internal Islamic Branch Desc: " + after.fldIBranchDesc, CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfile_EditTemplate(collection["internalBranchID"], before, after, "Edit");
                        //auditTrailDao.SecurityLog("Edit Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);

                        TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                    }
                    else
                    {
                        bool IsInternalBranchMasterTempExist = internalBranchDao.CheckInternalBranchTempById(collection["internalBranchID"]);

                        if (IsInternalBranchMasterTempExist == true)
                        {
                            TempData["Warning"] = Locale.InternalBranchAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            InternalBranchModel before = MaintenanceAuditLogDao.GetInternalBranchDataById(collection["internalBranchID"], "Master");

                            internalBranchDao.CreateInternalBranchTemp(collection, "update"); //Done

                            InternalBranchModel after = MaintenanceAuditLogDao.GetInternalBranchDataById(collection["internalBranchID"], "Temp");

                            //string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfile_EditTemplate(collection["internalBranchID"], before, after, "Edit");
                            //auditTrailDao.SecurityLog("Edit Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);

                            TempData["Notice"] = Locale.InternalBranchUpdateVerify;

                            auditTrailDao.Log("User Record Successfully Added to Temp Table for Check to Approve . Internal Branch ID : " + collection["internalBranchID"], CurrentUser.Account);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return RedirectToAction("Edit", new { intBranchIdParam = collection["internalBranchID"].Trim() /*+ "_" + CurrentUser.Account.BankCode*/ });
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranch.CREATE)] //DOne
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            ViewBag.InternalBranchID = internalBranchDao.GetInternalBranchID();
            ViewBag.ClearingBranchIdConv = internalBranchDao.ListInternalBranch("C");
            ViewBag.ClearingBranchIdIslamic = internalBranchDao.ListInternalBranch("I");
            ViewBag.Country = internalBranchDao.ListCountry();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranch.SAVECREATE)]
        public ActionResult SaveCreate(FormCollection collection, string bankCode)
        {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIdsOCS.InternalBranch.INDEX;
            CurrentUser.Account.TaskId = sTaskId;

            try
            {

                List<string> errorMessages = internalBranchDao.ValidateCreate(collection);

                if ((errorMessages.Count > 0))
                {

                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Create");

                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {

                        internalBranchDao.CreateInternalBranch(collection); //Done

                        //string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfile_AddTemplate(collection["internalBranchID"], "Add", "N");
                        //auditTrailDao.SecurityLog("Add Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);

                        TempData["Notice"] = Locale.RecordsuccesfullyCreated;

                        auditTrailDao.Log("Add into Internal Branch Table Master -  Internal Branch ID : " + collection["internalBranchID"] + "Internal Conv Branch ID: " + collection["branchIdConv"] + ", Internal Conv Branch Desc: " + collection["branchDescConv"] + "Internal Islamic Branch ID: " + collection["branchIdIslamic"] + ", Internal Islamic Branch Desc: " + collection["branchDescIslamic"], CurrentUser.Account);

                    }
                    else
                    {
                        internalBranchDao.CreateInternalBranchTemp(collection, "create"); //Done

                        //string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfile_AddTemplate(collection["internalBranchID"], "Add", "Y");
                        //auditTrailDao.SecurityLog("Add Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);

                        TempData["Notice"] = Locale.InternalBranchCreateVerify;

                        auditTrailDao.Log("Add into Internal Branch Table Master -  Internal Branch ID : " + collection["internalBranchID"], CurrentUser.Account);

                    }
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranch.DELETE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection collection)
        {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIdsOCS.InternalBranch.INDEX;
            CurrentUser.Account.TaskId = sTaskId;

            if (collection != null & collection["deleteBox"] != null)
            {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();

                foreach (string arrResult in arrResults)
                {

                    if ("N".Equals(systemProfile))
                    {
                        //string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfile_DeleteTemplate(arrResult, "Delete");
                        //auditTrailDao.SecurityLog("Delete Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);

                        internalBranchDao.DeleteInternalBranch(arrResult); //Done

                        TempData["Notice"] = Locale.RecordsuccesfullyDeleted;

                        auditTrailDao.Log("Delete - Internal Branch ID : " + arrResult, CurrentUser.Account);
                    }
                    else
                    {
                        bool IsInternalBranchTempExist = internalBranchDao.CheckInternalBranchTempById(arrResult);

                        if (IsInternalBranchTempExist == true)
                        {
                            TempData["Warning"] = Locale.InternalBranchAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            internalBranchDao.CreateInternalBranchTempToDelete(arrResult); //Done
                            TempData["Notice"] = Locale.InternalBranchVerifyDelete;
                            auditTrailDao.Log("Delete - Internal Branch ID : " + arrResult, CurrentUser.Account);

                            //string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfile_DeleteTemplate(arrResult, "Delete");
                            //auditTrailDao.SecurityLog("Delete Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                    }

                }

            }
            else

                TempData["Warning"] = Locale.Nodatawasselected;

            return RedirectToAction("Index");
        }

    }

}