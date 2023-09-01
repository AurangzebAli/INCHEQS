using INCHEQS.Processes.DataProcess;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
//using INCHEQS.Models.DataProcess;
using INCHEQS.Models.FileInformation;
using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Security;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Resources;
using INCHEQS.Areas.OCS.Models.Clearing;
using INCHEQS.Common;
using System;
using System.Data.SqlTypes;

namespace INCHEQS.Areas.OCS.Controllers.Submission.Clearing.Base
{
    public abstract class ClearingBaseController : BaseController {

        protected readonly IPageConfigDaoOCS pageConfigDao;
        protected readonly IDataProcessDao dataProcessDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly IClearingDao clearingDao;
        protected readonly FileInformationService fileInformationService;
        protected readonly SequenceDao sequenceDao;
        protected readonly ISystemProfileDao systemProfileDao;

        //Readables by inheriting class
        protected PageSqlConfig pageSqlConfig { get; private set; }

        //Must be implemented by inheriting class
        protected abstract PageSqlConfig setupPageSqlConfig();

        protected string searchPageHtml;
        protected string generateReportHtml;

        //Readables by inheriting class
        protected string currentAction;
        protected FormCollection currentFormCollection;
        protected QueueSqlConfig gQueueSqlConfig { get; private set; }
        protected ClearingModel gClearingModel { get; private set; }

        /// <summary>
        /// This function should be called inside All Actions in ClearingBaseController and it's Inheritence Controller.
        /// This is to protect from UNAUTHORIZED ACCESS to the page through TASKID returned by setupPageSqlConfig().
        /// This function is important and have to be called or else, the page won't work
        /// Returns: PageSqlConfig set in setupPageSqlConfig();
        /// </summary>
        [NonAction]
        protected PageSqlConfig initializeBeforeAction() {

            //Expose 'currentAction' to Children Controller so that it can be intercepted and add logics accordingly
            //currentAction is action URL accessed
            //currentFormCollection is FormCollection sent to URL accessed
            //The actions are:
            // - Index
            // - GenerateReport
            currentAction = currentAction != null ? currentAction : (string)ControllerContext.RouteData.Values["action"];
            currentFormCollection = new FormCollection(ControllerContext.HttpContext.Request.Form);


            //Initializ PageSqlConfig based on: 
            // - Inherited Controller initialization of setupPageSqlConfig()
            // - Request Query String of TaskId and ViewId
            pageSqlConfig = setupPageSqlConfig();

            gQueueSqlConfig = pageConfigDao.GetQueueConfigNew(setupPageSqlConfig().TaskId, CurrentUser.Account);

            //Constructor for MIRCimageModel
            //gClearingModel = micrImageDao.GetDataFromMICRImportConfig(pageSqlConfig.TaskId, CurrentUser.Account.BankCode);

            //ViewBagForCheckerMaker
            ViewBag.TaskRole = gQueueSqlConfig.TaskRole;

            //ViewBagForAllowedAction
            ViewBag.AllowedAction = gQueueSqlConfig.AllowedActions;

            try {
                //Check for UNAUTHORIZED ACCESS
                //Reject if User Does Not Have Access               
                RequestHelper.RestrictAccessToUserBasedOnTaskId(ControllerContext, pageSqlConfig.TaskId);

                return pageSqlConfig;
            } catch (HttpException ex) {
                systemProfileDao.Log("ClearingBaseController/initializeBeforeAction errors :" + ex.Message,"SystemLog", CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }


        public ClearingBaseController(IClearingDao clearingDao, IPageConfigDaoOCS pageConfigDao, IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, IMICRImageDao micrImageDao, FileInformationService fileInformationService, SequenceDao sequenceDao, ISystemProfileDao systemProfileDao) {
            this.pageConfigDao = pageConfigDao;
            this.dataProcessDao = dataProcessDao;
            this.fileManagerDao = fileManagerDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.clearingDao = clearingDao;
            this.fileInformationService = fileInformationService;
            this.sequenceDao = sequenceDao;
            this.systemProfileDao = systemProfileDao;
        }

        public virtual async Task<ActionResult> Index() {
            initializeBeforeAction();
            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            //ViewBag.DataPosPayType = gMircImageModel.fldPosPayType;
            //ViewBag.ProcessName = gMircImageModel.fldProcessName;
            return View();
        }

        public virtual async Task<ActionResult> SearchResultPage(FormCollection collection) {
            initializeBeforeAction();
            //ViewBag.SearchResult = await pageConfigDao.getResultListFromDatabaseViewAsync(pageSqlConfig, collection);
            ViewBag.SearchResult = await pageConfigDao.getQueueResultListFromDatabaseViewAsync(gQueueSqlConfig, collection);
            return View();
        }

        public virtual async Task<ActionResult> Process(FormCollection col)
        {
            initializeBeforeAction();

            string strIgnoreIQA = "N";//Dim strIgnoreIQA As String = "N"
            string strClearingErrMessage = "";//Dim strClearingErrMessage As String = ""

            string strErrorMessage;//Dim strErrMessage As New StringBuilder
            //Dim arrClearingBatchWithErrors As New ArrayList
            DateTime dtClearingDateTime = DateTime.Now;//Dim dtClearingDateTime As DateTime = Now

            string strTrfImgXmlErrorMsg = "";//Dim strTrfImgXmlErrorMsg As String = ""
            string strReferenceType = "";//Dim strReferenceType As String = ""
            string strErrMsg = "";//Dim strErrMsg As String = ""
            int intReturnCode = 0;//Dim intReturnCode As Integer = 0
            long intSystemLog = 0; int intLogReturnCode = 0;//Dim intSystemLog As Long = 0 : Dim intLogReturnCode As Integer = 0
            string strSuccess = ""; string strErrorJS = "";//Dim strSuccessJS As String = "" : Dim strErrorJS As String = ""

            string strBatchMessage;//Dim strBatchMessage As String = xmlRes.getValue("strBatchMessage")
            //Dim intSleep As Integer = CInt(xmlRes.getValue("Sleep"))


            //Try
            //    If hidUIC.Value = "" Then
            //        Exit Sub
            //    End If
            //    strIgnoreIQA = IIf(chkIgnoreIQA.Checked, "Y", "N").ToString

            //    '/********** Start to batch the selected Clearing UIC ***************/
            //    Try
            clearingDao.GenerateNewBatches(Convert.ToInt32(CurrentUser.Account.UserId), dtClearingDateTime, col["deleteBox"].Replace(",", "','"), strIgnoreIQA, ref strClearingErrMessage, CurrentUser.Account.BankCode);//        objClearingProcessThai.GenerateNewBatches(intUserId, dtClearingDateTime, strSelectedUIC.Replace(",", "','"), strIgnoreIQA, strClearingErrMessage, intSleep, intReturnCode)
            //    Catch exBatch As Exception
            //        eventCall(Me, exBatch.GetBaseException.Message)
            //    End Try

            //    'If intReturnCode <> 0 Then
            //    '    strErrMessage.Append(strClearingErrMessage)
            //    '    eventCall(Me, strClearingErrMessage)
            //    'Else
            //    '    '/************** Start to transfer image files and generate Xml file to clearing agent *********************/
            //    '    If clsClearingProcessThai.GenerateXmlAndImageTransferToClearingAgent(intUserId, Nothing, arrClearingBatchWithErrors, strTrfImgXmlErrorMsg, strTaskId, intUserId) Then
            //    '        If arrClearingBatchWithErrors.Count > 0 Then
            //    '            strErrMessage.Append(strTrfImgXmlErrorMsg)
            //    '        End If
            //    '    Else
            //    '        strErrMessage.Append(strTrfImgXmlErrorMsg)
            //    '    End If
            //    '    '/************** Start to transfer image files and generate Xml file to clearing agent *********************/
            //    '    If clsClearingProcessThai.ClearBatch(intUserId, Nothing, arrClearingBatchWithErrors, strTrfImgXmlErrorMsg, strTaskId, intUserId) Then
            //    '        If arrClearingBatchWithErrors.Count > 0 Then
            //    '            strErrMessage.Append(strTrfImgXmlErrorMsg)
            //    '        End If
            //    '    Else
            //    '        strErrMessage.Append(strTrfImgXmlErrorMsg)
            //    '    End If
            //    '    eventCall(Me, strBatchMessage)
            //    'End If
            //    eventCall(Me, strBatchMessage)

            //Catch ex As Exception
            //    intReturnCode = -1 : strErrMessage.Append(Chr(9) & "Exception: " & ex.GetBaseException.Message)
            //Finally
            //    strErrMsg = strErrMessage.ToString
            //    strReferenceType = "I"
            //    clsSystemLog.AddSystemLog(strTaskId, "Clearing process - batch generation and copy file to clearing agent", _
            //        strReferenceType, "", "", "", CType(intUserId, Int32), intSystemLog, intLogReturnCode)
            //    strReferenceType = "T"
            //    If strErrMsg.Length > 500 Then strErrMsg = strErrMsg.Substring(0, 500)
            //    If intReturnCode = -1 Then
            //        clsErrorLog.AddErrorLog(intSystemLog, strReferenceType, strErrMsg, "", CType(intUserId, Int32), intLogReturnCode)
            //    End If
            //    If strErrMsg <> "" Then
            //        clsClearingProcessThai.AddMessageMaster(strTaskId, intSystemLog.ToString, "Clearing resume", strErrMsg, "Y", intUserId, 0, 0)
            //    End If
            //End Try

















            //==============================================================================

            TempData["Notice"] = "==" + col["deleteBox"];
            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            return View("Index");
        }

        //public virtual ActionResult FileListing(FormCollection col) {
        //    initializeBeforeAction();
        //    string clearDate = col["fldClearDate"];
        //    string folderName = systemProfileDao.GetValueFromSystemProfile(gMircImageModel.fldSystemProfileCode, CurrentUser.Account.BankCode);
        //    int dateSubString = gMircImageModel.fldDateSubString;
        //    int bankCodeSubString = gMircImageModel.fldBankCodeSubString;

        //    ViewBag.FileList = fileInformationService.GetAllFileInFolder(folderName, bankCodeSubString, CurrentUser.Account.BankCode);

        //    return View();
        //}

        //public virtual ActionResult ExceptionFileListing(FormCollection col) {
        //    initializeBeforeAction();
        //    string clearDate = col["fldClearDate"];
        //    ViewBag.FileList = micrImageDao.GetErrorListFromICLException(clearDate);
        //    return View();
        //}

        //public virtual ActionResult PushImport(FormCollection col)
        //{
        //    initializeBeforeAction();
        //    micrImageDao.Update(CurrentUser.Account.BankCode);
        //    return Json(new { notice = "Data transfer will start" }, JsonRequestBehavior.AllowGet);
        //}

        //public virtual ActionResult DownloadHistory(FormCollection col) {
        //    initializeBeforeAction();
        //    string clearDate = col["fldClearDate"];
        //    ViewBag.FileList = micrImageDao.GetErrorListFromICLException(clearDate);
        //    ViewBag.SearchResult = pageConfigDao.getQueueResultListFromDatabaseViewAsync(gQueueSqlConfig, col);
        //    return View();
        //}

        //public JsonResult DownloadImport(FormCollection col)
        //{
        //    initializeBeforeAction();

        //    string notice = "";
        //    string processName = gMircImageModel.fldProcessName;
        //    string posPayType = gMircImageModel.fldPosPayType;
        //    string clearDate = col["fldClearDate"];
        //    string reUpload = "N";
        //    string taskId = pageSqlConfig.TaskId;
        //    string batchId = "";//TODO: unknown batchId
        //    string folderName = systemProfileDao.GetValueFromSystemProfile(gMircImageModel.fldSystemProfileCode, CurrentUser.Account.BankCode);
        //    int dateSubString = gMircImageModel.fldDateSubString;
        //    int bankCodeSubString = gMircImageModel.fldBankCodeSubString;
        //    string fileExt = gMircImageModel.fldFileExt;

        //    //Formatting fileExt if type is "selectList", file et should take from select collection form
        //    List<FileInformationModel> fileList = new List<FileInformationModel>();
        //    if (fileExt.ToLower().Equals("selectlist"))
        //    {
        //        fileExt = col["fileType"].Substring(col["fileType"].Length - 4, 4);
        //    }

        //    //Get file information
        //    fileList = fileInformationService.GetFileInFolder(folderName, clearDate, dateSubString, bankCodeSubString, fileExt, CurrentUser.Account.BankCode);


        //    //If file exists
        //    if (fileList.Count > 0)
        //    {
        //        //Formatting fldPosPayType if type is "filename"
        //        if (posPayType.ToLower().Equals("filename"))
        //        {
        //            posPayType = fileList[0].fileName;
        //        }

        //        //Check running process if not "" OR "4" then dont Do process
        //        bool runningProcess = dataProcessDao.CheckRunningProcess(processName, posPayType, clearDate, CurrentUser.Account.BankCode);

        //        //Do process
        //        if (runningProcess)
        //        {
        //            string sequenceTableName = "tblinwardcleardate";
        //            int nextSequenceNo = sequenceDao.GetNextSequenceNo(sequenceTableName);

        //            //Insert new clear Date
        //            dataProcessDao.InsertClearDate(clearDate, nextSequenceNo, CurrentUser.Account.BankCode);
        //            //Delete previous data process
        //            dataProcessDao.DeleteDataProcess(processName, posPayType, CurrentUser.Account.BankCode);
        //            //Insert new data process
        //            dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
        //            //Update sequence number
        //            sequenceDao.UpdateSequenceNo(nextSequenceNo, sequenceTableName);

        //            auditTrailDao.Log("Perform Download and Import", CurrentUser.Account);
        //            notice = Locale.PerformDownloadandImport;
        //        }
        //        else
        //        {
        //            notice = Locale.CurrentlyRunningPleaseWaitAMoment;
        //        }
        //    }
        //    else
        //    {
        //        notice = Locale.FileNotFound;
        //    }

        //    return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        //}



    }
}