using INCHEQS.Areas.ICS.Models.HubUserProfile;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    
    public class HubUserProfileController : BaseController {
        private IHubUserProfileDao hubUser;
        public HubUserProfileController(IHubUserProfileDao hubUser) {
            this.hubUser = hubUser;
        }
        // GET: ICS/HubUserProfile
        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.HubUserList = hubUser.ListAllHubUser();
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(string hubId) {
            if (hubId == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DataTable result = hubUser.getHubUser(hubId);
            if (result.Rows.Count > 0) {
                ViewBag.HubUserIds = result.Rows[0];
                ViewBag.AvailableUserList = hubUser.getAvailableHubUserList();
                ViewBag.SelectedUserList = hubUser.getSelectedHubUserList(hubId);

            }
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) {
            try {
                List<string> errorMessages = hubUser.Validate(col);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {
                    hubUser.UpdateHubMaster(col);
                    List<string> users = new List<string>();
                    string hubuserid = col["hubId"].Trim();
                    if ((col["selectedhubbranch"]) != null) {

                        //Update Process Start Here
                        hubUser.DeleteNotSelected(hubuserid, col["selectedhubbranch"]);
                        users = col["selectedhubbranch"].Split(',').ToList();
                        foreach (string user in users) {
                            if ((hubUser.CheckHubUserExistInGroup(hubuserid, user))) {
                                hubUser.UpdateSelectedUser(hubuserid, user);
                            } else {
                                hubUser.InsertUserInGroup(hubuserid, user);
                            }
                        }
                    } else {
                        hubUser.DeleteAllHubUser(hubuserid);
                    }
                    TempData["Notice"] = Locale.SuccessfullyUpdated;
                    
                }
            } catch (Exception ex) {
                throw ex;
            }
            return RedirectToAction("Edit", new { hubId = col["hubId"] });
        }
        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.DELETE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col) {
            try {
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null) {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (var arrResult in arrResults) {
                        hubUser.DeleteHubMasterUsers(arrResult);
                        hubUser.DeleteHubMaster(arrResult);
                        //AuditTrailDao.Log();
                    }
                    TempData["Notice"] = Locale.SuccessfullyDeleted;
                } else {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            } catch (Exception ex) {
                throw ex;
            }
        }
    }
}