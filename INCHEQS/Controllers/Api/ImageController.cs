using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.IO.Compression;
using System.Web.Services.Description;

namespace INCHEQS.Controllers.Api {

    [CustomAuthorize(TaskIds = "all")]
    public class ImageController : BaseController {

        private readonly ISystemProfileDao systemProfileDao;
        private ImageHelper imageHelper;

        public ImageController(ISystemProfileDao systemProfileDao, ImageHelper imageHelper) {
            this.systemProfileDao = systemProfileDao;
            this.imageHelper = imageHelper;
        }



        //[GenericFilter(AllowHttpGet = true)]
        //public FileResult Cheque(string imageId, string imageState, string imageFolder = null)
        //{


        //    List<ImageHelper.ImageInfo> imageInfo = getImageInfo(imageId, imageState, imageFolder);

        //    foreach (var item in imageInfo)
        //    {

        //        string sourcePath = item.sourcePath.Replace("\\\\", @"\");
        //        if (!System.IO.File.Exists(sourcePath))
        //        {
        //            return null;
        //        }
        //        if (!System.IO.File.Exists(item.destinationPath))
        //        {
        //            try
        //            {
        //                Response.BufferOutput = false;
        //                imageHelper.convertImageFromTiff(item.sourcePath, item.destinationPath, 1, item.sizeScale, item.angle, item.filter);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw ex;
        //            }
        //        }
        //    }

        //     return new FileStreamResult(new FileStream("", FileMode.Open), "image/gif");
        //}

        [GenericFilter(AllowHttpGet = true)]
        public FileResult Cheque(string imageId, string imageState, string imageFolder = null)
        {


            ImageHelper.ImageInfo imageInfo = getImageInfo(imageId, imageState, imageFolder);
            if (!System.IO.File.Exists(imageInfo.sourcePath))
            {
                return null;
            }
            if (!System.IO.File.Exists(imageInfo.destinationPath))
            {
                try
                {
                    Response.BufferOutput = false;
                    imageHelper.convertImageFromTiff(imageInfo.sourcePath, imageInfo.destinationPath, 1, imageInfo.sizeScale, imageInfo.angle, imageInfo.filter);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            return new FileStreamResult(new FileStream(imageInfo.destinationPath, FileMode.Open), "image/gif");
        }

        [GenericFilter(AllowHttpGet = true)]
        public FileResult ChequeOCS(string imageId, string imageState, string imageFolder = null)
        {

            ImageHelper.ImageInfo imageInfo = getImageInfoOCS(imageId, imageState, imageFolder);
            if (!System.IO.File.Exists(imageInfo.sourcePath))
            {
                return null;
            }
            if (!System.IO.File.Exists(imageInfo.destinationPath))
            {
                try
                {
                    Response.BufferOutput = false;
                    imageHelper.convertImageFromTiff(imageInfo.sourcePath, imageInfo.destinationPath, 1, imageInfo.sizeScale, imageInfo.angle, imageInfo.filter);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            return new FileStreamResult(new FileStream(imageInfo.destinationPath, FileMode.Open), "image/gif");
        }

        [GenericFilter(AllowHttpGet = true)]
        public FileResult LargeCheque(string imageId, string imageState, string imageFolder = null) {

            //List<ImageHelper.ImageInfo> images = new List<ImageHelper.ImageInfo>();
            //images = 
            ImageHelper.ImageInfo imageInfo = getImageInfo(imageId, imageState, imageFolder); ;


            if (!System.IO.File.Exists(imageInfo.sourcePath)) {
                return null;
            }

            if (!System.IO.File.Exists(imageInfo.destinationPath)) {
                try {
                    imageHelper.convertImageFromTiff(imageInfo.sourcePath, imageInfo.destinationPath, 1, 1, imageInfo.angle, imageInfo.filter);
                } catch (Exception ex) {
                    throw ex;
                }
            }


            return new FileStreamResult(new FileStream(imageInfo.destinationPath, FileMode.Open), "image/gif");
        }

        [GenericFilter(AllowHttpGet = true)]
        public FileResult LargeChequeOCS(string imageId, string imageState, string imageFolder = null)
        {

            ImageHelper.ImageInfo imageInfo = getImageInfoOCS(imageId, imageState, imageFolder);


            if (!System.IO.File.Exists(imageInfo.sourcePath))
            {
                return null;
            }

            if (!System.IO.File.Exists(imageInfo.destinationPath))
            {
                try
                {
                    imageHelper.convertImageFromTiff(imageInfo.sourcePath, imageInfo.destinationPath, 1, 1, imageInfo.angle, imageInfo.filter);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            return new FileStreamResult(new FileStream(imageInfo.destinationPath, FileMode.Open), "image/gif");
        }

        //[NonAction]
        //private List<ImageHelper.ImageInfo> getImageInfo(string imageId, string imageState, string imageFolder = null) {
        //    if (imageFolder == null) {
        //        imageFolder = systemProfileDao.GetValueFromSystemProfile("InwardImageFolder", CurrentUser.Account.BankCode);
        //    }

        //    if ((string.IsNullOrEmpty(imageFolder) | string.IsNullOrEmpty(imageId))) {
        //        return null;
        //    }


        //    List<string> states = imageState.Split(',').ToList();
        //    List<ImageHelper.ImageInfo> imageInfoList = imageHelper.constructFileNameBasedOnParameters(imageFolder, imageId, states,CurrentUser.Account.UserAbbr);

        //    return imageInfoList;

        //}

        [NonAction]
        private ImageHelper.ImageInfo getImageInfo(string imageId, string imageState, string imageFolder = null)
        {
            if (imageFolder == null)
            {
                imageFolder = systemProfileDao.GetValueFromSystemProfile("InwardImageFolder", CurrentUser.Account.BankCode);
            }

            if ((string.IsNullOrEmpty(imageFolder) | string.IsNullOrEmpty(imageId)))
            {
                return null;
            }

            List<string> states = imageState.Split(',').ToList();
            ImageHelper.ImageInfo imageInfo = imageHelper.constructFileNameBasedOnParameters(imageFolder, imageId, states, CurrentUser.Account.UserAbbr);
            return imageInfo;

        }

        [NonAction]
        private ImageHelper.ImageInfo getImageInfoOCS(string imageId, string imageState, string imageFolder = null)
        {
            if (imageFolder == null)
            {
                imageFolder = systemProfileDao.GetValueFromSystemProfile("ChequeTempFolderPath", CurrentUser.Account.BankCode);
            }

            if ((string.IsNullOrEmpty(imageFolder) | string.IsNullOrEmpty(imageId)))
            {
                return null;
            }

            List<string> states = imageState.Split(',').ToList();
            ImageHelper.ImageInfo imageInfo = imageHelper.constructFileNameBasedOnParametersOCS(imageFolder, imageId, states, CurrentUser.Account.UserAbbr);
            return imageInfo;

        }
    }
}