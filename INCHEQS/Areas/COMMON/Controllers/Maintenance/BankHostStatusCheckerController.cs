using INCHEQS.Areas.ICS.Models.HostReturnReason;
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
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankHostStatus;


namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class BankHostStatusCheckerController : BaseController
    {

        private readonly ISystemProfileDao systemProfileDao;
        private IBankHostStatusDao bankHostStatusDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;

        public BankHostStatusCheckerController(ISystemProfileDao systemProfileDao, IBankHostStatusDao bankHostStatusDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService)
        {
            this.systemProfileDao = systemProfileDao;
            this.pageConfigDao = pageConfigDao;
            this.bankHostStatusDao = bankHostStatusDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BankHostStatusChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BankHostStatusChecker.INDEX, "View_BankHostStatusMasterChecker", "fldBankHostStatusCode"),
            collection);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string statusCodeParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string strStatusCode = "";
            if (string.IsNullOrEmpty(statusCodeParam))
            {
                strStatusCode = filter["fldBankHostStatusCode"].Trim();
            }
            else
            {
                strStatusCode = statusCodeParam;
            }

            //ViewBag.PageTitle = bankHostStatusDao.GetPageTitle(TaskIds.BankHostStatusChecker.INDEX);


            ViewBag.BankHostStatusChecker = bankHostStatusDao.GetBankHostStatusMasterTemp(strStatusCode);
            ViewBag.BankHostStatus = bankHostStatusDao.GetBankHostStatusMaster(strStatusCode);

            ViewBag.HostStatusAction = bankHostStatusDao.ListHostStatusAction();
            ViewBag.RejectCode = bankHostStatusDao.ListHostRejectCode();
            return View();
        }


        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        if (action.Equals("A"))
                        {
                            bankHostStatusDao.CreateBankHostStatusCodeinMain(id);
                        }
                        else if (action.Equals("D"))
                        {
                            BankHostStatusModel objBankHostCode = bankHostStatusDao.GetBankHostStatusCodeData(id);
                            bankHostStatusDao.DeleteInBankHostStatusCode(id);
                            //auditTrailDao.Log("Deleted BankHostCode from Main - Bank Host Status Code :" + objBankHostCode.fldBankHostStatusCode + ", Bank Host Status Desc: " + objBankHostCode.fldBankHostStatusDesc + ", Bank Host Status Action : " + objBankHostCode.fldBankHostStatusAction + ",Reject Code " + objBankHostCode.fldrejectcode + ", UpdateBy : " + objBankHostCode.fldUpdateUserId, CurrentUser.Account);

                        }
                        else if (action.Equals("U"))
                        {
                            BankHostStatusModel before = bankHostStatusDao.GetBankHostStatusCodeData(id);
                            //auditTrailDao.Log("Deleted BankHostCode Created Before - Bank Host Status Code :" + before.fldBankHostStatusCode + ", Bank Host Status Desc: " + before.fldBankHostStatusDesc + ", Bank Host Status Action  : " + before.fldBankHostStatusAction + ",Reject Code " + before.fldrejectcode + ", UpdateBy : " + before.fldUpdateUserId, CurrentUser.Account);

                            bankHostStatusDao.UpdateBankHostStatusCodeToMainById(id);

                            BankHostStatusModel after = bankHostStatusDao.GetBankHostStatusCodeData(id);
                            //auditTrailDao.Log("Deleted BankHostCode - Bank Host Status Code :" + after.fldBankHostStatusCode + ", Bank Host Status Desc: " + after.fldBankHostStatusDesc + ", Bank Host Status Action : " + after.fldBankHostStatusAction + ",Reject Code " + before.fldrejectcode + ", UpdateBy : " + after.fldUpdateUserId, CurrentUser.Account);

                        }

                        bankHostStatusDao.DeleteBankHostStatusCodeinTemp(id);
                        //auditTrailDao.Log("Approve BankHostCode - Task Assigment :" + taskId + " Bank HostStatus Code : " + id, CurrentUser.Account);

                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusChecker.INDEX)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        bankHostStatusDao.DeleteBankHostStatusCodeinTemp(id);
                        //auditTrailDao.Log("Reject Update, Delete or Created New - Task Assigment :" + taskId + "Bank Host Status Code : " + id, CurrentUser.Account);

                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.SecurityProfileChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col, string statusCodeParam="")
        {

            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
                string strStatusCode = "";
                if (string.IsNullOrEmpty(statusCodeParam))
                {
                    strStatusCode = filter["fldBankHostStatusCode"].Trim();
                }
                else
                {
                    strStatusCode = statusCodeParam;
                }

                BankHostStatusModel before = bankHostStatusDao.GetBankHostStatusCodeData(strStatusCode);
                //auditTrailDao.Log("Deleted BankHostCode Created Before - Bank Host Status Code :" + before.fldBankHostStatusCode + ", Bank Host Status Desc: " + before.fldBankHostStatusDesc + ", Bank Host Status Action  : " + before.fldBankHostStatusAction + ",Reject Code " + before.fldrejectcode + ", UpdateBy : " + before.fldUpdateUserId, CurrentUser.Account);

                bankHostStatusDao.DeleteInBankHostStatusCode(strStatusCode);
                bankHostStatusDao.CreateBankHostStatusCodeinMain(strStatusCode); 
                bankHostStatusDao.DeleteBankHostStatusCodeinTemp(strStatusCode);

                BankHostStatusModel after = bankHostStatusDao.GetBankHostStatusCodeData(strStatusCode);
                //auditTrailDao.Log("Deleted BankHostCode - Bank Host Status Code :" + after.fldBankHostStatusCode + ", Bank Host Status Desc: " + after.fldBankHostStatusDesc + ", Bank Host Status Action : " + after.fldBankHostStatusAction + ",Reject Code " + before.fldrejectcode + ", UpdateBy : " + after.fldUpdateUserId, CurrentUser.Account);

                TempData["Notice"] = "Record(s) successfully approved";


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.SecurityProfileChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col, string statusCodeParam = "")
        {
            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
                string strStatusCode = "";
                if (string.IsNullOrEmpty(statusCodeParam))
                {
                    strStatusCode = filter["fldBankHostStatusCode"].Trim();
                }
                else
                {
                    strStatusCode = statusCodeParam;
                }
                BankHostStatusModel objBankHostCode = bankHostStatusDao.GetBankHostStatusCodeData(strStatusCode);
                bankHostStatusDao.DeleteBankHostStatusCodeinTemp(strStatusCode);
                //auditTrailDao.Log("Deleted BankHostCode from Main - Bank Host Status Code :" + objBankHostCode.fldBankHostStatusCode + ", Bank Host Status Desc: " + objBankHostCode.fldBankHostStatusDesc + ", Bank Host Status Action : " + objBankHostCode.fldBankHostStatusAction + ",Reject Code " + objBankHostCode.fldrejectcode + ", UpdateBy : " + objBankHostCode.fldUpdateUserId, CurrentUser.Account);


                TempData["Notice"] = "Record(s) successfully rejected";


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }






        }
}