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
using INCHEQS.Security.Account;


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
            ViewBag.ReturnCode = await returnCodeDao.GetAllRejectCodesAsync();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LateReturnMaintenance.DELETE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Delete(FormCollection col)
        {
            try
            {
                string taskid = TaskIdsICS.LateReturnMaintenance.DELETE;
                if (CurrentUser.HasTask(taskid))
                {
                    CurrentUser.Account.TaskId = taskid;
                }


                if ((col["deleteBox"]) != null)
                {
                  
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();
                    string clearDate = DateTime.ParseExact(col["fldLateReturnClearDate"].Trim().ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                    foreach (string arrResult in arrResults)
                    {

                        string UIC = LRMDao.getBetween(col["deleteBox"], "ae", "bf");
                        string lrID = LRMDao.getBetween(col["deleteBox"], "bf", "cg");
                        

                        //if (LRMDao.CheckExist(UIC, clearDate))
                        //{
                        //    LRMDao.deleteUPIH(UIC,lrID, clearDate);
                        //    //LRMDao.updateInwarditemH(UIC, clearDate);
                        //}
                        //else
                        //{
                            LRMDao.deleteUPI(UIC, lrID,clearDate);
                            //LRMDao.updateInwarditem(UIC, clearDate);
                        //}
                    }
                    TempData["Notice"] = Locale.SuccessfullyDeleted;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }

        
                // ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(clsLateReturnMaintenance.INDEX, "View_LateReturnMaintenance", null, "fldissuebankcode=@fldBankCode", new[] {
                // new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
                //col);
                //ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LateReturnMaintenance.INDEX));
                //ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LateReturnMaintenance.INDEX, "View_LateReturnMaintenance", null), col);

                return RedirectToAction("Index");
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
                string taskid = TaskIdsICS.LateReturnMaintenance.INDEX;
                if (CurrentUser.HasTask(taskid))
                {
                    CurrentUser.Account.TaskId = taskid;
                }

                List<string> errorMessages = LRMDao.ValidateLateReturnItem(collection);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    //if (collection["fldClearDate"] != null && collection["fldUIC"] != null && collection["fldrejectCode"] != null)
                    //{
                        LateReturnMaintenanceModal modal = new LateReturnMaintenanceModal();
                        DataTable desc = returnCodeDao.Find(collection["fldrejectCode"].ToString());
                        if (collection["fldrejectCode"].Equals(""))
                        {
                            collection["fldrejectCode"] = "000";
                        }
                        modal.fldClearDate = collection["fldClearDate"];
                        modal.fldUIC = collection["fldUIC"];
                        modal.fldCharges = collection["fldCharges"];
                        modal.fldrejectCode = collection["fldrejectCode"].ToString();
                        //modal.fldBankCode = collection["fldBankcode"];

                        modal.fldRejectDesc = desc.Rows[0]["fldRejectDesc"].ToString();

                        modal.fldRemarks = collection["fldRemarks"];
                        List<string> Msg = LRMDao.InsertLateReturnMaintenanceRecord(modal);
                        if (Msg.Count > 0)
                        {
                            TempData["ErrorMsg"] = Msg[0].ToString();
                        }
                        else
                        {

                            TempData["Notice"] = Locale.SuccessfullyCreated;
                        }
                    //}
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
                ViewBag.ReturnCode = await returnCodeDao.GetAllRejectCodesAsync();

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