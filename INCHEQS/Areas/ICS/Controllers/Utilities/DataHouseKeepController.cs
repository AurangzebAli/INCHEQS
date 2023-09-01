using System;
using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.Helpers;
using INCHEQS.Resources;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.EOD.DataHouseKeep;
using System.Collections.Generic;
using INCHEQS.Common;

namespace INCHEQS.Areas.ICS.Controllers.Utilities {
        
    public class DataHouseKeepController : BaseController {

        private readonly IAuditTrailDao auditTrailDao;
        private readonly IDataHouseKeepDao datahouse;
        public DataHouseKeepController(IDataHouseKeepDao datahouse, IAuditTrailDao auditTrailDao) {
            this.datahouse = datahouse;
            this.auditTrailDao = auditTrailDao; 
        }
        // GET: DataHouseKeep
        [CustomAuthorize(TaskIds = TaskIds.DataHousKeep.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.DataHouseKeepRetention = datahouse.GetDataHouseKeep();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.DataHousKeep.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection col) {
            try {
                List<string> errorMessages = datahouse.Validate(col);
                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {
                    DataHouseKeepModel before = datahouse.getData(col);
                    auditTrailDao.Log("Edit Data Housekeeping Retention - Before Update =>PPS Audit Log :" + before.auditLog + " Archived Audit Log Files :" + before.archiveLog, CurrentUser.Account);

                    datahouse.UpdateData(Utils.ConvertFormCollectionToDictionary(col),CurrentUser.Account.UserId);

                    DataHouseKeepModel after = datahouse.getData(col);
                    auditTrailDao.Log("Edit Data Housekeeping Retention - After Update =>PPS Audit Log :" + after.auditLog + " Archived Audit Log Files :" + after.archiveLog, CurrentUser.Account);

                    TempData["Notice"] = Locale.Recordsavesuccessfully;
                }


            } catch (Exception ex) {
                throw ex;
            }
            return RedirectToAction("Index");
        }
    }
}