using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.HubBranchProfile;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {
    
    public class HubBranchProfileController : BaseController {
        private IHubBranchProfileDao hubBranch;
        private readonly IAuditTrailDao auditTrailDao;
        public HubBranchProfileController(IHubBranchProfileDao hubBranch,IAuditTrailDao auditTrailDao) {
            this.hubBranch = hubBranch;
            this.auditTrailDao = auditTrailDao;
        }
        // GET: HubBranchProfile
        [CustomAuthorize(TaskIds = TaskIds.HubBranchProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.HubBranch = hubBranch.ListAllHubBranch();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HubBranchProfile.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(string hubId) {
            if (hubId == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DataTable result = hubBranch.getHubBranch(hubId);
            if (result.Rows.Count > 0) {
                ViewBag.HubBranchIds = result.Rows[0];
                ViewBag.AvailableBranchList = hubBranch.getAvailableHubBranchList(hubId);
                ViewBag.SelectedBranchList = hubBranch.getSelectedHubBranchList(hubId);

            }
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.HubBranchProfile.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) {
            try {
                List<string> errorMessages = hubBranch.Validate(col);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {
                    hubBranch.UpdateHubMaster(col);

                    List<string> branches = new List<string>();
                    string hubbranchid = col["hubId"].Trim();

                    if ((col["selectedhubbranch"]) != null) {

                        //Update Process Start Here
                        hubBranch.DeleteNotSelected(hubbranchid, col["selectedhubbranch"]);
                        branches = col["selectedhubbranch"].Split(',').ToList();
                        foreach (string branch in branches) {
                            if ((hubBranch.CheckHubBranchExistInGroup(hubbranchid, branch))) {
                                hubBranch.UpdateSelectedBranch(hubbranchid, branch);
                            } else {
                                hubBranch.InsertBranchInGroup(hubbranchid, branch);
                            }
                        }
                    } else {
                        hubBranch.DeleteAllHubBranch(hubbranchid);
                    }
                    TempData["Notice"] = Locale.SuccessfullyUpdated;

                }
            } catch (Exception ex) {
                throw ex;
            }
            return RedirectToAction("Edit", new { hubId = col["hubId"] });
        }

        [CustomAuthorize(TaskIds = TaskIds.HubBranchProfile.CREATE)]
        public ActionResult Create() {
            ViewBag.AvailableBranchList = hubBranch.getHubBranchList();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HubBranchProfile.CREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col) {
            try {
                List<string> errorMessages = hubBranch.Validate(col);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {
                    hubBranch.CreateHubMaster(col);
                    List<string> branches = new List<string>();
                    string hubbranchid = col["hubId"].Trim();



                    if ((col["selectedhubbranch"]) != null) {
                        branches = col["selectedhubbranch"].Split(',').ToList();
                        foreach (string branch in branches) {
                            hubBranch.InsertBranchInGroup(hubbranchid, branch);
                        }

                    }
                    TempData["Notice"] = Locale.SuccessfullyCreated;


                }
            } catch (Exception ex) {
                throw ex;
            }
            return RedirectToAction("Index");
        }
        [CustomAuthorize(TaskIds = TaskIds.HubBranchProfile.DELETE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col) {
            try {
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null) {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (var arrResult in arrResults) {
                        hubBranch.DeleteHubMasterBranches(arrResult);
                        hubBranch.DeleteHubMaster(arrResult);
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