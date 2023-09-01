// bank of Malaysia
//using INCHEQS.BNM.ReturnCode;

//bank of Philipine 
//using INCHEQS.PCHC.ReturnCode;

using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Controllers.Api {

    [CustomAuthorize(TaskIds = "all")]
    public class RejectCodesApiController : BaseController {

        private readonly IReturnCodeDao rejectCodeDao;
        public RejectCodesApiController(IReturnCodeDao rejectCodeDao) {
            this.rejectCodeDao = rejectCodeDao;
        }

        public JsonResult Index() {

            List<INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel> rejectCodes = rejectCodeDao.FindAllRejectCodesDictionary();
            List<INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel> rejectTypes = rejectCodeDao.ListRejectType();
            Response.BufferOutput = false;
            return Json(
                new {
                rejectCode = rejectCodes,
                rejectType = rejectTypes
                }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getRejectCodeBranch(string rejectCode)
        {
            List<INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel> rejectCodes = rejectCodeDao.FindRejectCodesForBranchDictionary();
            List<INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel> rejectTypes = rejectCodeDao.ListRejectTypeForBranch();
            Response.BufferOutput = false;
            return Json(
                new
                {
                    rejectCode = rejectCodes,
                    rejectType = rejectTypes
                }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getCharges(string rejectCode)
        {
            string charges = rejectCodeDao.getCharges(rejectCode);
            return Json(charges, JsonRequestBehavior.AllowGet);
        }
    }
}