using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.Areas.PPS.Models.ViewPPSFile;


namespace INCHEQS.Areas.PPS.Controllers
{
    public class ViewPPSFileController : BaseController
    {

        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected PageSqlConfig pageSqlConfig { get; set; }

        protected IViewPPSFileDao viewPPSFiledao;

        public ViewPPSFileController(IPageConfigDao pageConfigDao, IViewPPSFileDao viewPPSFiledao)
        {
            this.pageConfigDao = pageConfigDao;
            this.viewPPSFiledao = viewPPSFiledao;
        }

        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult Index()
        {
            
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig("318130"));



            return View();
        }

        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig("318130", "view_ViewPositvePayFile", null, null),
            collection);

            return View();
        }

        public virtual ActionResult View(FormCollection collection)
        {
            ViewBag.PayeeDetails = viewPPSFiledao.getPayeeDetail(collection["row_fldChequeNo"], collection["row_fldAccNo"]);
            return View("View");
        }


    }
}