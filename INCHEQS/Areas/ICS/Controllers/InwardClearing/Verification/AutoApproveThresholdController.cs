using INCHEQS.Areas.ICS.Concerns;
using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.ICS.Models.Verification;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.Report;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Security;
using INCHEQS.Security.User;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Models.Verification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Areas.ICS.Service;
using INCHEQS.Areas.ICS.Models.AutoApproveThresholdModel;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing
{

    public class AutoApproveThresholdController : BaseController
    {

        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IAutoApproveThresholdDao autoApproveThresholddao;
        protected readonly ISystemProfileDao systemProfileDao;

        AutoApproveThresholdModel autoApproveThreshold = new AutoApproveThresholdModel();

        public AutoApproveThresholdController(IPageConfigDao pageConfigDao, IAutoApproveThresholdDao autoApproveThresholddao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.autoApproveThresholddao = autoApproveThresholddao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.AutoApproveThreshold.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
           
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.AutoApproveThreshold.INDEX));
            ViewBag.LoadDate = autoApproveThresholddao.GetClearDate().fldClearDate;
            ViewBag.allowButton = autoApproveThresholddao.GetClearDate().allowButton;

            ViewBag.AATPerformed = autoApproveThresholddao.CheckAAT();
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsICS.AutoApproveThreshold.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            try
            {
                //AutoApproveThresholdModel autoApproveThreshold = new AutoApproveThresholdModel();
                //ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.AutoApproveThreshold.INDEX, "View_InwardItem", null), collection);
                List<String> errorMessage = autoApproveThresholddao.ValidateForm(collection);
                if ((errorMessage.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessage;
                    return RedirectToAction("Index");

                }
                else
                {
                    ViewBag.statisticRange = collection["statisticRange"];
                    ViewBag.LoadDate = autoApproveThresholddao.GetClearDate().fldClearDate; // get value from here
                    ViewBag.Percentage = collection["percentage"];
                    ViewBag.TotalRecord = autoApproveThresholddao.GetTotalInwardItem(collection).TotalRecord;
                    ViewBag.TotalAmount = autoApproveThresholddao.GetTotalInwardItem(collection).TotalAmount;
                    ViewBag.TotalRecordPending = autoApproveThresholddao.GetTotalPendingInwardItem(collection).TotalPendingRecord;
                    ViewBag.TotalAmountPending = autoApproveThresholddao.GetTotalPendingInwardItem(collection).TotalPendingAmount;
                    ViewBag.ItemsWithPercentage = Convert.ToInt32(autoApproveThresholddao.GetTotalPendingInwardItem(collection).TotalPendingRecord * ((Convert.ToDouble(collection["percentage"]) / 100)));
                    ViewBag.AmountWithPercentage = autoApproveThresholddao.GetFilteredAmount(collection, ViewBag.ItemsWithPercentage);
                    ViewBag.MaxAmount = autoApproveThresholddao.GetMaxAmount(collection, ViewBag.ItemsWithPercentage);
                    ViewBag.MinAmount = autoApproveThresholddao.GetMinAmount(collection, ViewBag.ItemsWithPercentage);



                    List<int> rangeList = autoApproveThresholddao.GetRangeList(collection, ViewBag.MaxAmount);
                    ViewBag.RangeList = rangeList;



                    List<Double> itemPercentage = autoApproveThresholddao.GetRangeItem(collection, ViewBag.ItemsWithPercentage);
                    List<int> RangeItem = new List<int>();
                    int counter = 0;

                    for (int i = 0; i < rangeList.Count; i++)
                    {
                        foreach (double item in itemPercentage)
                        {
                            if (item <= rangeList[i])
                            {
                                counter++;
                            }
                        }
                        RangeItem.Add(counter);
                        counter = 0;
                    }
                    ViewBag.RangeItem = RangeItem;

                    List<Double> rangePercentage = new List<Double>();
                    for (int i = 0; i < rangeList.Count; i++)
                    {
                        double percentage = (Convert.ToDouble(RangeItem[i]) / Convert.ToDouble(ViewBag.ItemsWithPercentage)) * Convert.ToDouble(collection["percentage"]);
                        rangePercentage.Add(percentage);
                    }
                    ViewBag.RangePercentage = rangePercentage;


                    ViewBag.AATPerformed =  autoApproveThresholddao.CheckAAT();
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }




        [CustomAuthorize(TaskIds = TaskIdsICS.AutoApproveThreshold.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult Update(FormCollection col)
        {
            try
            {
                if (col != null)
                {
                    string clearDate = col["fldClearDate"];
                    string item = col["itemswithpercentage"];
                    autoApproveThresholddao.UpdateAutoApproveThreshold(col, CurrentUser.Account.UserId, clearDate, item);
                    TempData["Notice"] = Locale.SuccessfullyUpdated;

                    String sMessage = "Auto Approve Threshold for " + clearDate + " has been successfully updated for " + item + " items.";
                    auditTrailDao.Log(sMessage, CurrentUser.Account);
                }
                else
                {
                    TempData["ErrorMsg"] = Locale.Warning;
                }
                


                ViewBag.LoadDate = col["LoadDate1"];
                ViewBag.Percentage = col["percentage1"];
                ViewBag.statisticRange = col["statisticRange1"];
                ViewBag.TotalRecord = col["TotalRecord1"];
                ViewBag.TotalAmount = col["TotalAmount1"];
                ViewBag.TotalRecordPending = col["TotalRecordPending1"];
                ViewBag.TotalAmountPending = col["TotalAmountPending1"];
                ViewBag.ItemsWithPercentage = col["ItemsWithPercentage1"];
                ViewBag.MinAmount = col["MinAmount1"];
                ViewBag.MaxAmount = col["MaxAmount1"];

                List<int> rangeList = autoApproveThresholddao.GetRangeList(col, ViewBag.MaxAmount);
                ViewBag.RangeList = rangeList;

                List<Double> itemPercentage = autoApproveThresholddao.GetRangeItem(col, Convert.ToInt32(ViewBag.ItemsWithPercentage));
                List<int> RangeItem = new List<int>();
                int counter = 0;

                for (int i = 0; i < rangeList.Count; i++)
                {
                    foreach (double item in itemPercentage)
                    {
                        if (item <= rangeList[i])
                        {
                            counter++;
                        }
                    }
                    RangeItem.Add(counter);
                    counter = 0;
                }
                ViewBag.RangeItem = RangeItem;

                List<Double> rangePercentage = new List<Double>();
                for (int i = 0; i < rangeList.Count; i++)
                {
                    double percentage = (Convert.ToDouble(RangeItem[i]) / Convert.ToDouble(ViewBag.ItemsWithPercentage)) * Convert.ToDouble(col["percentage"]);
                    rangePercentage.Add(percentage);
                }
                ViewBag.RangePercentage = rangePercentage;

                ViewBag.AATPerformed = autoApproveThresholddao.CheckAAT();
                return View("SearchResultPage");
               

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}