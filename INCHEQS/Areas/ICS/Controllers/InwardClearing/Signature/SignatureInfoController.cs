using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.Models.Signature;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.TaskAssignment;


namespace INCHEQS.Areas.ICS.Controllers.InwardClearing
{

    public class SignatureInfoController : BaseController
    {
        private readonly ISignatureDao signatureDao;
        private readonly IReturnCodeDao returnCodeDao;
        private IPageConfigDao pageConfigDao;

        public SignatureInfoController(ISignatureDao signatureDao, IPageConfigDao pageConfigDao, IReturnCodeDao returnCodeDao)
        {
            this.signatureDao = signatureDao;
            this.pageConfigDao = pageConfigDao;
            this.returnCodeDao = returnCodeDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.Signature.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            //ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.Message.INDEX));
            return View();
        }
    }


}