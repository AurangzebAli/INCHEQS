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
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using System.Globalization;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing
{

    public class LateReturnedMaintenanceController : BaseController
    {
        private readonly ILateReturnedMaintenanceDao LRMDao;
        private readonly IReturnCodeDao returnCodeDao;
        private IPageConfigDao pageConfigDao;

        public LateReturnedMaintenanceController(ILateReturnedMaintenanceDao LRMDao, IPageConfigDao pageConfigDao, IReturnCodeDao returnCodeDao)
        {
            this.LRMDao = LRMDao;
            this.pageConfigDao = pageConfigDao;
            this.returnCodeDao = returnCodeDao;
        }

        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LateReturnMaintenance.INDEX));
            return View();
        }

        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LateReturnMaintenance.INDEX, "View_LateReturnMaintenance", null), collection);
            return View();
        }
        

        public async Task<ActionResult> Create(FormCollection collection)
        {
            ViewBag.ClearDate = LRMDao.GetClearDateforLateMaintenance();
            ViewBag.ReturnCode = await returnCodeDao.GetAllRejectCodesWithoutInternalAsync();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LateReturnMaintenance.DELETE)]
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
                        string deleteUIC = UIC[i].Substring(0, 30);
                        string lateid = UIC[i].Substring(30);
                        clearDate[i] = DateTime.ParseExact(clearDate[i], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                        //Convert.ToDateTime(clearDate[i]).ToString("yyyy-MM-dd");
                        if (LRMDao.CheckExist(UIC[i], clearDate[i]))
                        {
                            LRMDao.deleteUPIH(deleteUIC, clearDate[i], lateid);
                            LRMDao.updateInwarditemH(deleteUIC, clearDate[i]);
                        }
                        else
                        {
                            LRMDao.deleteUPI(deleteUIC, clearDate[i], lateid);
                            LRMDao.updateInwarditem(deleteUIC, clearDate[i]);
                        }
                    }
                    TempData["Notice"] = Locale.SuccessfullyDeleted;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }

                col["fldUIC"] = "";
                // ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(clsLateReturnMaintenance.INDEX, "View_LateReturnMaintenance", null, "fldissuebankcode=@fldBankCode", new[] {
                // new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
                //col);
                //ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LateReturnMaintenance.INDEX));
                //ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LateReturnMaintenance.INDEX, "View_LateReturnMaintenance", null), col);

                return RedirectToAction("Index");
                //return View("Index");
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public async Task<JsonResult> getLateReturnMaintenance(FormCollection collection)
        {
            List<LateReturnMaintenanceModal> resultLateReturnMaintenance = await LRMDao.getLateReturnMaintenanceAsyn(collection);
            return Json(resultLateReturnMaintenance, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Save(FormCollection collection)
        {
            try
            {
                if (collection["fldClearDate"] != null && collection["fldUIC"] != null && collection["fldrejectCode"] != null)
                {
                    int lengthOfRejectCode = collection["fldRejectCode"].ToString().Trim().Length;

                    if (lengthOfRejectCode == 2)
                    {
                        collection["fldRejectCode"] = "0" + collection["fldRejectCode"].ToString().Trim();
                    }
                    else if (lengthOfRejectCode == 1)
                    {
                        collection["fldRejectCode"] = "00" + collection["fldRejectCode"].ToString().Trim();
                    }

                    if (collection["fldrejectCode"].Equals(""))
                    {
                        collection["fldrejectCode"] = "000";
                    }

                    //if ()

                    LateReturnMaintenanceModal modal = new LateReturnMaintenanceModal();
                    DataTable desc = returnCodeDao.FindWithoutInternalCode(collection["fldrejectCode"].ToString());

                    if (desc.Rows.Count == 0)
                    {
                        TempData["ErrorMsg"] = "Return code not found.";
                    }
                    else
                    {
                        modal.fldClearDate = collection["fldClearDate"];
                        modal.fldUIC = collection["fldUIC"];
                        modal.fldCharges = collection["fldCharges"];
                        modal.fldrejectCode = collection["fldrejectCode"].ToString();
                        modal.fldBankCode = collection["fldBankcode"];

                        modal.fldRejectDesc = desc.Rows[0]["fldRejectDesc"].ToString();

                        modal.fldRemarks = collection["fldRemarks"];
                        List<string> Msg = LRMDao.InsertLateReturnMaintenanceRecord(modal);
                        if (Msg.Count > 0)
                        {
                            TempData["Notice"] = "Late Return Item " + Locale.SuccessfullyCreated;
                        }
                        else
                        {
                            TempData["ErrorMsg"] = "Failed To add Item into Late Return Maintenance.";
                        }
                    } 
                }

                //collection["fldUIC"] = "";
                //collection["fldClearDate"] = "";
                //collection["fldrejectcode"] = "";
                //collection["fldRemarks"] = "";
                //collection["fldCharges"] = "";
                //ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LateReturnMaintenance.INDEX, "View_LateReturnMaintenance", null, "fldissuebankcode=@fldBankCode", new[] {
                //new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
                //collection);
                ViewBag.ClearDate = LRMDao.GetClearDateforLateMaintenance();
                ViewBag.ReturnCode = await returnCodeDao.GetAllRejectCodesWithoutInternalAsync();

                return View("Create");
                //return View("~/Areas/ICS/Views/LateReturnedMaintenance_Testing/SearchPage/SearchResultPage.cshtml");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

    }
}