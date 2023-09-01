using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Models.Signature;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Security.Account;
using INCHEQS.Areas.ICS.Service;

namespace INCHEQS.Controllers.Api
{

    [CustomAuthorize(TaskIds = "all")]
    [GenericFilter(AllowHttpGet = true)]
    public class SignatureController : BaseController
    {
        ISignatureDao signatureDao;
        ISystemProfileDao systemProfileDao;
        ImageHelper imageHelper;
        
        public SignatureController(ISignatureDao signatureDao, ISystemProfileDao systemProfileDao, ImageHelper imageHelper)
        {
            this.signatureDao = signatureDao;
            this.systemProfileDao = systemProfileDao;
            this.imageHelper = imageHelper;
        }

        // GET: Signature
        [GenericFilter(AllowHttpGet = true)]
        public async Task<FileResult> Index(string imageId, string imageNo)
        {

            byte[] imageBytes = await signatureDao.GetSignatureImage(imageId, imageNo);
            string destinationPath = imageHelper.drawImageForSignature(imageId, imageNo, imageBytes , 1);            
            FileStreamResult result = new FileStreamResult(new FileStream(destinationPath, FileMode.Open), "image/jpg");        
            return result;

        }



        // GET: Signature
        //[GenericFilter(AllowHttpGet = true)]
        //public async Task<FileResult> Thumb(string imageId, string imageNo) {
        //    byte[] imageBytes = await signatureDao.GetSignatureImage(imageId, imageNo);
        //    string destinationPath = imageHelper.drawImageForSignature(imageId, imageNo, imageBytes, 0.3f);
        //    FileStreamResult result = new FileStreamResult(new FileStream(destinationPath, FileMode.Open), "image/jpg");
        //    return result;

        //}
        // GET: Signature
        [GenericFilter(AllowHttpGet = true)]
        public async Task<FileResult> Thumb(string imageId, string imageNo)
        {
            byte[] imageBytes = await signatureDao.GetSignatureImage(imageId, imageNo);
            string destinationPath = imageHelper.drawImageForSignature(imageId, imageNo, imageBytes, 1);
            FileStreamResult result = new FileStreamResult(new FileStream(destinationPath, FileMode.Open), "image/jpg");
            return result;

        }


        // GET: Signature
        [GenericFilter(AllowHttpGet = true)]
        public async Task<FileResult> Large(string imageId, string imageNo)
        {
            byte[] imageBytes = await signatureDao.GetSignatureImage(imageId, imageNo);
            string destinationPath = imageHelper.drawImageForSignature(imageId, imageNo, imageBytes, 1);
            FileStreamResult result = new FileStreamResult(new FileStream(destinationPath, FileMode.Open), "image/jpg");
            return result;

        }


        public async Task<JsonResult> List(string accountNo, string issuingBankBranch)
        {
            List<AccountInfo> signatureList = await signatureDao.GetSignatureDetailsAsync(accountNo, issuingBankBranch);
            return Json(signatureList, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> RulesList(string accountNo, string issuingBankBranch)
        {
            List<AccountInfo> signatureRuleList = await signatureDao.GetSignatureRulesAsync(accountNo, issuingBankBranch);
            return Json(signatureRuleList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidateSignature(FormCollection collection)
        {
            bool validateResult = false;
            string result = "UNMATCH";

            if (collection["signatureBox"] != null)
            {
                validateResult = signatureDao.CheckValidateSignature(collection, CurrentUser.Account);
        ////        signatureDao.DeleteSignatureHistoryImage(collection);
        // //       signatureDao.InsertSignatureHistoryImage(collection, CurrentUser.Account);
            }
            if (validateResult)
            {
                result = "MATCH";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetImageInfoList(string AccNo, string issuingBankBranch, string ImageNo)
        {
            List<AccountInfo> list = await signatureDao.GetImageDetailsAsync(AccNo, issuingBankBranch, ImageNo);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSignatureRulesInfo(string AccNo)
        {
            ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(AccNo);
            //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(AccNo);
            return Json(ViewBag.SignatureRules, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSignatureInformation(string AccNo)
        {
            //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(AccNo);
            ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(AccNo);
            return Json(ViewBag.SignatureInfo, JsonRequestBehavior.AllowGet);
        }


        //public void DeleteAllSignatureHistory(FormCollection collection)
        //{
        //    signatureDao.DeleteSignatureHistoryImage(collection);
        //}

        //public async Task<JsonResult> GetCheckedSignature(string itemId)
        //{
        //    List<AccountInfo> imageNoList = await signatureDao.GetCheckedSignatureAsync(itemId);
        //    return Json(imageNoList, JsonRequestBehavior.AllowGet);
        //}

        public async Task<JsonResult> IsSDSServerConnected()
        {
            return Json(await signatureDao.ISSDSConnectionAvailable(), JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //string tempPath = systemProfileDao.GetFolderPathFromSystemProfile("ChequeTempFolderPath");

                //DirectoryInfo di = new DirectoryInfo(tempPath + "Signatures");
                //foreach (FileInfo file in di.GetFiles()) {
                //    file.Delete();
                //}
            }

            base.Dispose(disposing);
        }
    }
}