using INCHEQS.ConfigVerification.ReleaseLockedCheque;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.UserSession;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Web.Mvc;
using INCHEQS.Security.User;
using System.Collections.Generic;
using INCHEQS.Security.SystemProfile;
using System;
using INCHEQS.Areas.ICS.Models.LateReturnedMaintenance;
using System.Threading.Tasks;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using System.Data;
using System.Linq;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{

    public class LateReturnedMaintenanceController : BaseController
    {
        private readonly ILateReturnedMaintenanceDao LRMDao;
        private IPageConfigDao pageConfigDao;

        public LateReturnedMaintenanceController(ILateReturnedMaintenanceDao LRMDao, IPageConfigDao pageConfigDao)
        {
            this.LRMDao = LRMDao;
            this.pageConfigDao = pageConfigDao;
        }
        [CustomAuthorize(TaskIds = clsLateReturnMaintenance.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(clsLateReturnMaintenance.INDEX));
            return View();
        }
        [CustomAuthorize(TaskIds = clsLateReturnMaintenance.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            LateReturnMaintenanceModal modal = new LateReturnMaintenanceModal();

            if (collection["fldClearDate"] == "" && collection["fldUIC"] == "" && collection["fldrejectCode"] == "")
            {
                return View();
            }
            else
            {
                if (collection["fldrejectCode"].Equals(""))
                {
                    collection["fldrejectCode"] = "0";
                }
                modal.fldClearDate = collection["fldClearDate"];
                modal.fldUIC = collection["fldUIC"];
                modal.fldCharges = collection["fldCharges"];
                modal.fldrejectCode = Convert.ToInt32(collection["fldrejectCode"]);
                modal.fldRejectDesc = collection["fldRejectDesc"];
                modal.fldRemarks = collection["fldRemarks"];
                List<string> errorMsg = LRMDao.ValidateSearch(modal);
                if (errorMsg.Count > 0)
                {
                    TempData["ErrorMsg"] = errorMsg;
                }
                else
                {
                    ViewBag.LateReturnMaintenanceItems = LRMDao.GetLateMaintenanceItems(modal);
                }
                return View();
            }
        }

        [CustomAuthorize(TaskIds = clsLateReturnMaintenance.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                if (collection["fldClearDate"] != null && collection["fldUIC"] != null && collection["fldrejectCode"] != null)
                {
                    LateReturnMaintenanceModal modal = new LateReturnMaintenanceModal();

                    if (collection["fldrejectCode"].Equals(""))
                    {
                        collection["fldrejectCode"] = "0";
                    }
                    modal.fldClearDate = collection["fldClearDate"];
                    modal.fldUIC = collection["fldUIC"];
                    modal.fldCharges = collection["fldCharges"];
                    modal.fldrejectCode = Convert.ToInt32(collection["fldrejectCode"]);
                    modal.fldRejectDesc = collection["fldRejectDesc"];
                    modal.fldRemarks = collection["fldRemarks"];
                    List<string> Msg = LRMDao.InsertLateReturnMaintenanceRecord(modal);
                    if (Msg.Count > 0)
                    {
                        TempData["Notice"] = Locale.SuccessfullyCreated;
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "Failed To add Item into Late Return Maintenance.";
                    }
                }

                collection["fldUIC"] = "";
                collection["fldClearDate"] = "";
                collection["fldrejectcode"] = "";
                collection["fldRemarks"] = "";
                collection["fldCharges"] = "";
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(clsLateReturnMaintenance.INDEX, "View_LateReturnMaintenance", null, "fldissuebankcode=@fldBankCode", new[] {
                new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
               collection);
                return View("Delete");
                //return View("~/Areas/ICS/Views/LateReturnedMaintenance_Testing/SearchPage/SearchResultPage.cshtml");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = clsLateReturnMaintenance.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Delete(FormCollection col)
        {
            try
            {
                List<string> UIC = new List<string>();
                List<string> clearDate = new List<string>();
                if ((col["deleteBox"]) != null)
                {
                    UIC = col["deleteBox"].Trim().Split(',').ToList();
                    clearDate = col["row_fldClearDate"].Trim().Split(',').ToList();
                    for (int i = 0; i < UIC.Count; i++)
                    {
                        if (LRMDao.CheckExist(UIC[i], clearDate[i]))
                        {
                            LRMDao.deleteUPIH(UIC[i], clearDate[i]);
                            LRMDao.updateInwarditemH(UIC[i], clearDate[i]);
                        }
                        else
                        {
                            LRMDao.deleteUPI(UIC[i], clearDate[i]);
                            LRMDao.updateInwarditem(UIC[i], clearDate[i]);
                        }
                    }
                    TempData["Notice"] = Locale.SuccessfullyDeleted;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }

                col["fldUIC"] = "";
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(clsLateReturnMaintenance.INDEX, "View_LateReturnMaintenance", null, "fldissuebankcode=@fldBankCode", new[] {
                new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
               col);
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}