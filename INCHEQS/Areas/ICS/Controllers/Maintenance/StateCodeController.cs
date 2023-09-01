// bank of Malaysia
//using INCHEQS.BNM.StateCode;

// bank of Philipine
using INCHEQS.PCHC.StateCode;

using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
//using INCHEQS.Security.AuditTrail;
//using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class StateCodeController : BaseController
    {
       
        private IStateCodeDao stateCodeDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ISearchPageService searchPageService;

        public StateCodeController(IStateCodeDao statecodedao , IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao) {
            this.pageConfigDao = pageConfigDao;
            this.stateCodeDao = statecodedao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.StateCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.StateCode.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {

            /*edit by umar to filterStateCode 3/5/2018*/
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.StateCode.INDEX, "View_SateCode", null, stateCodeDao.condition()),
            
            collection);
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.StateCode.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string stateCodeParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string stateCode = "";
 if (string.IsNullOrEmpty(stateCode))
            {
                stateCode = filter["fldStatecode"].Trim();
            }
            else
            {
                stateCode = stateCodeParam;
            }
            DataTable dataTable = stateCodeDao.GetStateCode(stateCode);

            if (dataTable.Rows.Count > 0)
            {
                ViewBag.StateCode = dataTable.Rows[0];

            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCode.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection) {
            ActionResult action;
            try {
                List<String> errorMessages = stateCodeDao.ValidateUpdate(collection);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                    action = RedirectToAction("Edit", new { stateCodeParam = collection["StateCode"] });
                } else {
                    StateCodeModel before = stateCodeDao.GetStateCodeModel(collection["StateCode"]);
                    auditTrailDao.Log("Edit - State - Before Update => State Code : " + before.stateCode + ", State Description : " + before.stateDesc, CurrentUser.Account);

                    stateCodeDao.Update(collection, CurrentUser.Account.UserId);
                    TempData["Notice"] = Locale.SuccessfullyUpdated;
                    StateCodeModel after = stateCodeDao.GetStateCodeModel(collection["StateCode"]);
                    auditTrailDao.Log("Edit - State - Before Update => State Code : " + after.stateCode + ", State Description : " + after.stateDesc, CurrentUser.Account);
                    action = RedirectToAction("Index");
                }
            } catch (Exception ex) {

                throw ex;
            }
            return action;
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCode.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("StateCodeChecker", CurrentUser.Account.BankCode).Trim();
            if (collection != null & collection["deleteBox"] != null) {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {
                    if ("N".Equals(systemProfile)) {

                        stateCodeDao.DeleteInStateMaster(arrResult);
                        TempData["notice"] = Locale.SuccessfullyDeleted;
                        
                    } else {
                        stateCodeDao.AddtoStateMasterTempToDelete(arrResult);
                        TempData["notice"] = Locale.StateCodeDeleteVerify;
                    }
                }
                //audittrail
                if ("N".Equals(systemProfile)) {
                    auditTrailDao.Log("Delete State Record - State Code : "+collection["deleteBox"], CurrentUser.Account);
                } else {
                    auditTrailDao.Log("Add Temporary Record to Delete - State Code : " + collection["deleteBox"], CurrentUser.Account);
                }
            } else
                TempData["Warning"] = Locale.Nodatawasselected;
            return RedirectToAction("Index");
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCode.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.BankCode.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection collection) {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("StateCodeChecker", CurrentUser.Account.BankCode).Trim();
            try {
                List<String> errorMessages = stateCodeDao.ValidateCreate(collection);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Create");
                } else {
                    if ("N".Equals(systemProfile)) {
                        stateCodeDao.CreateStateCodeTemp(collection, CurrentUser.Account.UserId);
                        stateCodeDao.CreateInStateMaster(collection["stateCode"]);
                        stateCodeDao.DeleteInStateMasterTemp(collection["stateCode"]);

                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        auditTrailDao.Log("Add - State Code : " + collection["stateCode"] + " State Desc : " + collection["StateDescription"], CurrentUser.Account);
                    } else {
                        stateCodeDao.CreateStateCodeTemp(collection, CurrentUser.Account.UserId);

                        TempData["Notice"] = Locale.StateCodeCreateVerify;
                        auditTrailDao.Log("Add State Temporary Record - State Code : " + collection["stateCode"] + " State Desc : " + collection["StateDescription"], CurrentUser.Account);
                    }
                    return RedirectToAction("Index");
                }
            } catch (Exception ex) {

                throw ex;
            }
        }
    }
}