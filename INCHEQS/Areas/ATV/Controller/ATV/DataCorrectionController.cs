﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Security;
using System.Web.Mvc;
using INCHEQS.Models.DataCorrectionDao;
using System.Threading.Tasks;
using System.Data.SqlClient;
using INCHEQS.DataAccessLayer;
using INCHEQS.Areas.ATV.Models.DataCorrection;
using System.Data;
using INCHEQS.Resources;

namespace INCHEQS.Areas.ATV.Controllers.ATV
{

    public class DataCorrectionController : BaseController
    {
        private readonly ApplicationDbContext dbContext;
        private IDataCorrectionDao dataCorrectionDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;

        public DataCorrectionController(ApplicationDbContext dbContext, IDataCorrectionDao dataCorrectionDao, ISearchPageService searchPageService, IPageConfigDao pageConfigDao)
        {
            this.dbContext = dbContext;
            this.dataCorrectionDao = dataCorrectionDao;
            this.pageConfigDao = pageConfigDao;
            this.searchPageService = searchPageService;
        }
        [CustomAuthorize(TaskIds = TaskIds.DataCorrection.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.DataCorrection.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.DataCorrection.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.DataCorrection.INDEX, "View_ATVItem"/*, null, dataCorrectionDao.condition()*/),
            //ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.DataCorrection.INDEX, "tblATVItemResult"/*, null, dataCorrectionDao.condition()*/),
            collection);
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.DataCorrection.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string stateCodeParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string itemId = "";
            if (string.IsNullOrEmpty(col["row_fldItemId"]))
            {

            }
            else
            {
                itemId = col["row_fldItemId"];
            }

            string getImage = "";
            // Get image path  
            //string imgPath = Server.MapPath("~/Content/Images/logo.jpg");
            DataTable imgPath = dataCorrectionDao.getPathCropImage(itemId);
            //Convert datatable to string
            foreach (DataRow row in imgPath.Rows)
            {
                getImage = row["fldCropDatePath"].ToString();
            }
            // Convert image to byte array
            byte[] byteData = System.IO.File.ReadAllBytes(getImage);
            //Convert byte arry to base64string
            string imreBase64Data = Convert.ToBase64String(byteData);
            string imgDataURL = string.Format("data:image/tiff;base64,{0}", imreBase64Data);
            //Passing image data in viewbag to view
            ViewBag.ImageData = imgDataURL;
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.DataCorrection.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection col, string userId)
        {
            //more codes
            dataCorrectionDao.Update(col);
            return RedirectToAction("Index");

        }
    }
}