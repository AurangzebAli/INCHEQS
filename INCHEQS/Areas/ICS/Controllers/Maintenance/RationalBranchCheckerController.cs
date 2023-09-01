using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Areas.ICS.Models.RationalBranch;


namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class RationalBranchCheckerController : BaseController
    {

        private readonly IRationalBranchDao RationalBranchDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;

        public RationalBranchCheckerController(IPageConfigDao pageConfigDao, IRationalBranchDao RationalBranchDao, ISearchPageService searchPageService)
        {
            this.pageConfigDao = pageConfigDao;
            this.RationalBranchDao = RationalBranchDao;
            this.searchPageService = searchPageService;
        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranchChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.RationalBranchChecker.INDEX));
            ViewBag.PageTitle = RationalBranchDao.GetPageTitle(TaskIds.RationalBranchChecker.INDEX);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranchChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.RationalBranchChecker.INDEX, "View_ApprovedGroupChecker"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranchChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string groupIdParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string branchId = "";
            string IbranchId = "";
            if (string.IsNullOrEmpty(groupIdParam))
            {
                branchId = filter["fldBranchId"].Trim();
                IbranchId = filter["fldIBranchId"].Trim();
            }
            else
            {
                branchId = groupIdParam;
            }

            ViewBag.PageTitle = RationalBranchDao.GetPageTitle(TaskIds.RationalBranchChecker.INDEX);

            ViewBag.RationalBranch = RationalBranchDao.GetBranch(branchId, "Update");
            ViewBag.SelectedRationalBranch = RationalBranchDao.ListSelectedRationalBranchChecker(branchId);
            ViewBag.AvailableRationalBranch = RationalBranchDao.ListAvailableRationalBranchChecker(CurrentUser.Account.BankCode);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranchChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            int count = 0;
            string sTaskId = TaskIds.RationalBranchChecker.VERIFY;
            if (CurrentUser.HasTask(sTaskId))
            {
                CurrentUser.Account.TaskId = sTaskId;
            }

            try
            {
                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[3];

                string action = "";
                string branchId = "";

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        count++;
                        tmpArr = arrResult.Split(':');
                        string actTask = tmpArr[0].Trim().ToString();

                        if (tmpArr.Length > 1)
                        {
                             action = tmpArr[0].Trim().ToString();
                             branchId = tmpArr[1].Trim().ToString();
                        }


                        if (action.Equals("A")|| action.Equals("U"))
                        {
                            string CBranchId = "";

                            List<RationalBranchModel> selectedBranchs = RationalBranchDao.ListSelectedRationalBranchCheckerWithAllTempRecord(branchId);
                            foreach (var selectedBranch in selectedBranchs)
                            {
                                CBranchId = selectedBranch.fldCBranchId;                                                      

                            if (branchId != "")
                            {
                                //check rational branch's status in temp table is 'd' then delete in tblRationalBranch from temp
                                if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, CBranchId, "Delete") == true)
                                {
                                    RationalBranchDao.DeleteRationalBranchTempRowByRow(branchId, CBranchId, "Delete");
                                }
                                if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, CBranchId, "Update") == true)
                                {
                                    RationalBranchDao.DeleteRationalBranchTempRowByRow(branchId, CBranchId, "Delete");
                                }
                                //check rational branch record got inside temp table or not, then just update UpdateUserid and updateTime 
                                if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, CBranchId, "Add") == true)
                                {
                                    RationalBranchDao.MoveRationalBranchFromTempRowByRow(branchId, CBranchId, "Update");
                                }
                                else
                                {
                                    //Inside to rationalBranch from temp table
                                    RationalBranchDao.MoveRationalBranchFromTempRowByRow(branchId, CBranchId, "Edit");
                                        
                                    //check the selected branch have child branch or not, if got then move to same new branch
                                    RationalBranchDao.CheckRationalBranchSeletedBranchHaveChildOrNot(branchId, CBranchId);
                                }
                                //"VERIFYA" = delete from tblRationalBranchtemp
                                RationalBranchDao.DeleteRationalBranchTempRowByRow(branchId, CBranchId, "VERIFYA");
                            }
                            }
                        }
                        else if (action.Equals("D"))
                        {
                            string CBranchId = "";
                            List<RationalBranchModel> selectedBranchs = RationalBranchDao.ListSelectedRationalBranchCheckerWithAllTempRecord(branchId);
                            foreach (var selectedBranch in selectedBranchs)
                            {
                                if (branchId != "")
                                {
                                    CBranchId = selectedBranch.fldCBranchId;

                                    if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, CBranchId, "Delete") == true)
                                    {
                                        //"Delete" = delete from tblRationalBranch
                                        RationalBranchDao.DeleteRationalBranchTempRowByRow(branchId, CBranchId, "Delete");
                                    }
                                    //"VERIFYA" = delete from tblRationalBranchtemp
                                    RationalBranchDao.DeleteRationalBranchTempRowByRow(branchId, CBranchId, "VERIFYA");
                                }
                            }

                        }
  
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

        [CustomAuthorize(TaskIds = TaskIds.RationalBranchChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            int count = 0;
            string sTaskId = TaskIds.RationalBranchChecker.VERIFY;
            if (CurrentUser.HasTask(sTaskId))
            {
                CurrentUser.Account.TaskId = sTaskId;
            }

            try
            {
                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[3];
       
                        string actTask = col["fldCBranchId"];
                        string action = actTask.Substring(0, 1);
                        string branchId = col["fldCBranchId"];  

                        if (branchId != "")
                        {
                            string CBranchId = "";
                            List<RationalBranchModel> selectedBranchs = RationalBranchDao.ListSelectedRationalBranchCheckerWithAllTempRecord(branchId);
                            foreach (var selectedBranch in selectedBranchs)
                            {
                                CBranchId = selectedBranch.fldCBranchId;

                                if (branchId != "")
                                {
                                    //check rational branch's status in temp table is 'd' then delete in tblRationalBranch from temp
                                    if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, CBranchId, "Delete") == true)
                                    {
                                        RationalBranchDao.DeleteRationalBranchTempRowByRow(branchId, CBranchId, "Delete");
                                    }
                                    if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, CBranchId, "Update") == true)
                                    {
                                        RationalBranchDao.DeleteRationalBranchTempRowByRow(branchId, CBranchId, "Delete");
                                    }
                                    //check rational branch record got inside temp table or not, then just update UpdateUserid and updateTime 
                                    if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, CBranchId, "Add") == true)
                                    {
                                        RationalBranchDao.MoveRationalBranchFromTempRowByRow(branchId, CBranchId, "Update");
                                    }
                                    else
                                    {
                                        //Inside to rationalBranch from temp table
                                        RationalBranchDao.MoveRationalBranchFromTempRowByRow(branchId, CBranchId, "Edit");

                                        //check the selected branch have child branch or not, if got then move to same new branch
                                        RationalBranchDao.CheckRationalBranchSeletedBranchHaveChildOrNot(branchId, CBranchId);
                                    }

                                    RationalBranchDao.DeleteRationalBranchTempRowByRow(branchId, CBranchId, "VERIFYA");
                                }
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

        [CustomAuthorize(TaskIds = TaskIds.RationalBranchChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            int count = 0;
            string sTaskId= TaskIds.RationalBranchChecker.VERIFY;
            if (CurrentUser.HasTask(sTaskId))
            {
                CurrentUser.Account.TaskId = sTaskId;
            }

            try
            {
                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[3];
                string action = "";
                string branchId = "";

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        count++;
                        tmpArr = arrResult.Split(':');
                        string actTask = tmpArr[0].Trim().ToString();

                        if (tmpArr.Length > 1)
                        {
                            action = tmpArr[0].Trim().ToString();
                            branchId = tmpArr[1].Trim().ToString();
                        }


                        if (branchId!="")
                        {
                            //string afterUser = "";
                            //List<RationalBranchModel> afterUserLists = RationalBranchDao.ListSelectedRationalBranchChecker(branchId);
                            //foreach (var afterUserlist in afterUserLists)
                            //{
                            //    afterUser = afterUser + afterUserlist.fldUserAbb + "\n";
                            //}
                            //if (count == 1)
                            //{
                            //    string ActionDetails = SecurityAuditLogDao.GroupProfileChecker_AddTemplate(afterUser, "Reject", "Reject", branchId);
                            //    auditTrailDao.SecurityLog("Add Group", ActionDetails, sTaskId, CurrentUser.Account);
                            //}
                            if (branchId != "")
                            {
                                RationalBranchDao.DeleteRationalBranchTemp(branchId, "VERIFYR");
                            }
                            RationalBranchDao.DeleteRationalBranchTemp(branchId, "VERIFYR");
                        }                                               
                        //auditTrailDao.Log("Reject Create ,Update or Delete - Group ID :" + id, CurrentUser.Account);
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

        [CustomAuthorize(TaskIds = TaskIds.RationalBranchChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            int count = 0;
            string sTaskId = TaskIds.RationalBranchChecker.VERIFY;
            if (CurrentUser.HasTask(sTaskId))
            {
                CurrentUser.Account.TaskId = sTaskId;
            }

            try
            {
                List<string> arrResults = new List<string>();

                        string actTask = col["fldCBranchId"];
                        string action = actTask.Substring(0, 1);
                        string branchId = col["fldCBranchId"];
                        
                        if (branchId != "")
                        {

                            if (branchId != "")
                            {
                                RationalBranchDao.DeleteRationalBranchTemp(branchId, "VERIFYR");
                            }
                            RationalBranchDao.DeleteRationalBranchTemp(branchId, "VERIFYR");
                        }                        
                        //auditTrailDao.Log("Reject Create ,Update or Delete - Group ID :" + id, CurrentUser.Account);
                    
                    TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}