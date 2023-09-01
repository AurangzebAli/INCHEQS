using ImageProcessor;
using ImageProcessor.Imaging.Filters.Photo;
using ImageProcessor.Imaging.Formats;
using INCHEQS.Security;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace INCHEQS.Helpers {
    public class ImageHelper {
        ISystemProfileDao systemProfileDao;

        public ImageHelper(ISystemProfileDao systemProfileDao) {
            this.systemProfileDao = systemProfileDao;
        }

        public class ImageInfo {
            public int filter { get; set; }
            public string filename { get; set; }
            public float angle { get; set; }
            public string sourcePath { get; set; }
            public string destinationPath { get; set; }
            public float sizeScale { get; set; }

        }

        public void convertImageFromTiff(string imagePath, string destinationPath , int format = 1, float sizeScale = 0, float rotateAngle = 0f ,  int filter = 0) {

            if (filter == 0 && rotateAngle == 0f)
            {
                Bitmap bitmap = new Bitmap(imagePath);
                if (imagePath.Contains(".jpg"))
                {
                    bitmap.Save(destinationPath, ImageFormat.Jpeg);
                }
                else if (imagePath.Contains(".tif"))
                {
                    bitmap.Save(destinationPath, ImageFormat.Gif);
                }
                

                //bitmap.Save(destinationPath, ImageFormat.Gif);
                
            }
            else
            {
                byte[] photoBytes = File.ReadAllBytes(imagePath); // change imagePath with a valid image path
                int quality = 100;
                Bitmap bitmap = new Bitmap(imagePath);
                var size = new Size(Convert.ToInt32(bitmap.Width * sizeScale), Convert.ToInt32(bitmap.Height * sizeScale));
                //bitmap = null;
                bitmap.Dispose();
                ISupportedImageFormat imageFormat = new GifFormat();
                switch (format)
                {
                    case 1:
                        imageFormat = new GifFormat();
                        break;
                    case 2:
                        imageFormat = new JpegFormat();
                        break;
                }

                using (var inStream = new MemoryStream(photoBytes))
                {
                    using (var outStream = new FileStream(destinationPath, FileMode.Create))
                    {
                        // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                        using (var imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            // Do your magic here
                            imageFactory.Load(inStream)
                                .Rotate(rotateAngle)
                                .Format(imageFormat)
                                .Quality(quality)
                                .Resize(size);
                            switch (filter)
                            {
                                case 1:
                                    imageFactory.Filter(MatrixFilters.Invert);
                                    break;
                                case 2:
                                    imageFactory.Filter(MatrixFilters.GreyScale);
                                    break;
                            }
                            imageFactory.Save(outStream);
                        }
                    }
                }
            }
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        //For filter Refer http://imageprocessor.org/imageprocessor/imagefactory/filter/

        public ImageInfo constructFileNameBasedOnParametersOCS(string imageFolder, string imageId, List<string> states, string user = null)
        {

            string tempPath = systemProfileDao.GetValueFromSystemProfile("ChequeTempFolderPath", CurrentUser.Account.BankCode);
            string imgFormatBWF = systemProfileDao.GetValueFromSystemProfile("ChequeBlackWhiteFrontOCS", CurrentUser.Account.BankCode);
            string imgFormatBWB = systemProfileDao.GetValueFromSystemProfile("ChequeBlackWhiteBackOCS", CurrentUser.Account.BankCode);
            string imgFormatGF = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleFrontOCS", CurrentUser.Account.BankCode);
            string imgFormatGB = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleBackOCS", CurrentUser.Account.BankCode);
            string imgFormatGU = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleUVOCS", CurrentUser.Account.BankCode);

            ImageInfo imageInfo = new ImageInfo();
            string tempImageFolder = string.Format("{0}{1}", tempPath, user); // + CurrentUser.Account.UserAbbr
            if (tempImageFolder != String.Empty)
            {
                if (!Directory.Exists(tempImageFolder))
                {
                    Directory.CreateDirectory(tempImageFolder);
                }

            }
            

            // string imageName = imageId + imgFormatBWF;
            string imageName = imageId.Trim() + imgFormatGF;
            float angel = 0f;
            int filter = 0;
            imageInfo.sizeScale = 0.7f;
            if (states != null)
            {
                if (states.Contains("bw") && states.Contains("front"))
                {
                    imageName = imageId.Trim() + imgFormatBWF;
                }
                //else if (states.Contains("back"))
                //{
                //    //imageName = imageId + imgFormatBWB;
                //}
                else if (states.Contains("bw") && states.Contains("back"))
                {
                    imageName = imageId.Trim() + imgFormatBWB;
                }
                else if (states.Contains("greyscale") && states.Contains("front"))
                {
                    imageInfo.sizeScale = 1.5f;
                    imageName = imageId.Trim() + imgFormatGF;
                }
                else if (states.Contains("greyscale") && states.Contains("back"))
                {
                    imageInfo.sizeScale = 1.5f;
                    imageName = imageId.Trim() + imgFormatGB;
                }
                else if (states.Contains("uv"))
                {
                    imageName = imageId.Trim() + imgFormatGU;
                }

                if (states.Contains("rotate"))
                {
                    angel = 180f;
                }

                if (states.Contains("invert"))
                {
                    filter = 1;
                }
            }
            imageInfo.sourcePath = string.Format("{0}\\{1}", imageFolder, imageName);
            imageInfo.angle = angel;
            imageInfo.filter = filter;
            imageInfo.filename = imageId.Trim();
            imageInfo.destinationPath = string.Format("{0}\\{1}-{2}-{3}.gif", tempImageFolder, imageName, angel, filter);

            return imageInfo;

        }
        //public List<ImageInfo> constructFileNameBasedOnParameters(string imageFolder, string imageId, List<string> states,string user=null) {

        //    string tempPath = systemProfileDao.GetValueFromSystemProfile("ChequeTempFolderPath", CurrentUser.Account.BankCode);
        //    string imgFormatBWF = systemProfileDao.GetValueFromSystemProfile("ChequeBlackWhiteFront", CurrentUser.Account.BankCode);
        //    string imgFormatBWB = systemProfileDao.GetValueFromSystemProfile("ChequeBlackWhiteBack", CurrentUser.Account.BankCode);
        //    string imgFormatGF = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleFront", CurrentUser.Account.BankCode);
        //    string imgFormatGB = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleBack", CurrentUser.Account.BankCode);
        //    string imgFormatGU = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleUV", CurrentUser.Account.BankCode);
        //    List<ImageInfo> imageInfoList = new List<ImageInfo>();
        //    ImageInfo imageInfo = new ImageInfo();
        //    string tempImageFolder = string.Format("{0}{1}", tempPath, user) ; // + CurrentUser.Account.UserAbbr
        //    if (!Directory.Exists(tempImageFolder)) {
        //        Directory.CreateDirectory(tempImageFolder);
        //    }

        //    // string imageName = imageId + imgFormatBWF;
        //    imgFormatGF = "F";
        //    imgFormatGB = "B";
        //    imgFormatGU = "U";
        //    string imageName = imageId.Trim() + imgFormatGF;
        //    string imageBack = imageId.Trim() + imgFormatGB;
        //    string imageUV = imageId.Trim() + imgFormatGU;
        //    List<string> lstimages = new List<string>();
        //    lstimages.Add(imageName);
        //    lstimages.Add(imageBack);
        //    lstimages.Add(imageUV);
        //    float angel = 0f;
        //    int filter = 0;
        //    //imageInfo.sizeScale = 0.7f;
        //    if (states != null) 
        //    {
        //        if (states.Contains("bw") && states.Contains("front")) {
        //            imageName = imageId.Trim() + imgFormatBWF;
        //        }
        //        //else if (states.Contains("back"))
        //        //{
        //        //    //imageName = imageId + imgFormatBWB;
        //        //}
        //        else if (states.Contains("bw") && states.Contains("back"))
        //        {
        //            imageName = imageId.Trim() + imgFormatBWB;
        //        } else if (states.Contains("greyscale") && states.Contains("front")) {
        //            //imageInfo.sizeScale = 1f;
        //            imageName = imageId.Trim() + imgFormatGF;
        //        } else if (states.Contains("greyscale") && states.Contains("back")) {
        //            //imageInfo.sizeScale = 1f;
        //            imageName = imageId.Trim() + imgFormatGB;
        //        } else if (states.Contains("uv")) {
        //            imageName = imageId.Trim() + imgFormatGU;
        //        }

        //        if (states.Contains("rotate")) {
        //            angel = 180f;
        //        }

        //        if (states.Contains("invert")) {
        //            filter = 1;
        //        }
        //    }

        //    for (int i = 0; i < lstimages.Count; i++)
        //    {
        //        imageInfo = new ImageInfo();
        //        imageInfo.sourcePath = string.Format("{0}{1}.jpg", imageFolder, lstimages[i]);
        //        imageInfo.angle = angel;
        //        imageInfo.filter = filter;
        //        imageInfo.filename = lstimages[i].Trim();
        //        imageInfo.destinationPath = string.Format("{0}\\{1}-{2}-{3}.jpg", tempImageFolder, lstimages[i], angel, filter);
        //        imageInfoList.Add(imageInfo);
        //    }
        //    //imageInfo.destinationPath = string.Format("{0}\\{1}", tempImageFolder, imageName, angel, filter);
        //    //imageInfo.destinationPath = imageInfo.sourcePath;

        //    return imageInfoList;

        //}


        public ImageInfo constructFileNameBasedOnParameters(string imageFolder, string imageId, List<string> states, string user = null)
        {

            string tempPath = systemProfileDao.GetValueFromSystemProfile("ChequeTempFolderPath", CurrentUser.Account.BankCode);
            string imgFormatBWF = systemProfileDao.GetValueFromSystemProfile("ChequeBlackWhiteFront", CurrentUser.Account.BankCode);
            string imgFormatBWB = systemProfileDao.GetValueFromSystemProfile("ChequeBlackWhiteBack", CurrentUser.Account.BankCode);
            string imgFormatGF = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleFront", CurrentUser.Account.BankCode);
            string imgFormatGB = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleBack", CurrentUser.Account.BankCode);
            string imgFormatGU = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleUV", CurrentUser.Account.BankCode);
            List<ImageInfo> imageInfoList = new List<ImageInfo>();
            ImageInfo imageInfo = new ImageInfo();
            string tempImageFolder = string.Format("{0}{1}", tempPath, user); // + CurrentUser.Account.UserAbbr
            if (!Directory.Exists(tempImageFolder))
            {
                Directory.CreateDirectory(tempImageFolder);
            }

            // string imageName = imageId + imgFormatBWF;
            imgFormatGF = "F";
            imgFormatGB = "B";
            imgFormatGU = "U";
            string imageName = imageId.Trim() + imgFormatGF;
            string imageBack = imageId.Trim() + imgFormatGB;
            string imageUV = imageId.Trim() + imgFormatGU;
            List<string> lstimages = new List<string>();
            lstimages.Add(imageName);
            lstimages.Add(imageBack);
            lstimages.Add(imageUV);
            float angel = 0f;
            int filter = 0;
            //imageInfo.sizeScale = 0.7f;
            if (states != null)
            {

                if (states.Contains("bw") && states.Contains("front"))
                {
                    imageName = imageId.Trim() + imgFormatBWF;
                }
                //else if (states.Contains("back"))
                //{
                //    //imageName = imageId + imgFormatBWB;
                //}
                else if (states.Contains("bw") && states.Contains("back"))
                {
                    imageName = imageId.Trim() + imgFormatBWB;
                }
                else if (states.Contains("greyscale") && states.Contains("front"))
                {
                    //imageInfo.sizeScale = 1f;
                    imageName = imageId.Trim() + imgFormatGF;
                }
                else if (states.Contains("greyscale") && states.Contains("back"))
                {
                    //imageInfo.sizeScale = 1f;
                    imageName = imageId.Trim() + imgFormatGB;
                }
                else if (states.Contains("uv"))
                {
                    imageName = imageId.Trim() + imgFormatGU;
                }
                else if (states.Contains("B"))
                {
                    // For Large Back Image 
                    imageName = imageId.Trim() + imgFormatGB;
                }
                else if (states.Contains("U"))
                {
                    // For Large UV Image 
                    imageName = imageId.Trim() + imgFormatGU;
                }


                if (states.Contains("rotate"))
                {
                    angel = 180f;
                }

                if (states.Contains("invert"))
                {
                    filter = 1;
                }
            }


            imageInfo.sourcePath = string.Format("{0}{1}.jpg", imageFolder, imageName);
            imageInfo.angle = angel;
            imageInfo.filter = filter;
            imageInfo.filename = imageName.Trim();
            imageInfo.destinationPath = string.Format("{0}\\{1}-{2}-{3}.jpg", tempImageFolder, imageName, angel, filter);
            
            //imageInfo.destinationPath = string.Format("{0}\\{1}", tempImageFolder, imageName, angel, filter);
            //imageInfo.destinationPath = imageInfo.sourcePath;

            return imageInfo;

        }



        #region Ali
        //public ImageInfo constructFileNameBasedOnParameters(string imageFolder, string imageId, List<string> states, string user = null)
        //{

        //    string tempPath = systemProfileDao.GetValueFromSystemProfile("ChequeTempFolderPath", CurrentUser.Account.BankCode);
        //    string imgFormatBWF = systemProfileDao.GetValueFromSystemProfile("ChequeBlackWhiteFront", CurrentUser.Account.BankCode);
        //    string imgFormatBWB = systemProfileDao.GetValueFromSystemProfile("ChequeBlackWhiteBack", CurrentUser.Account.BankCode);
        //    string imgFormatGF = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleFront", CurrentUser.Account.BankCode);
        //    string imgFormatGB = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleBack", CurrentUser.Account.BankCode);
        //    string imgFormatGU = systemProfileDao.GetValueFromSystemProfile("ChequeGreyscaleUV", CurrentUser.Account.BankCode);

        //    ImageInfo imageInfo = new ImageInfo();
        //    string tempImageFolder = string.Format("{0}{1}", tempPath, user); // + CurrentUser.Account.UserAbbr
        //    if (!Directory.Exists(tempImageFolder))
        //    {
        //        Directory.CreateDirectory(tempImageFolder);
        //    }

        //    // string imageName = imageId + imgFormatBWF;
        //    string imageName = imageId.Trim() + imgFormatGF;
        //    float angel = 0f;
        //    int filter = 0;
        //    //imageInfo.sizeScale = 0.7f;
        //    if (states != null)
        //    {
        //        if (states.Contains("bw") && states.Contains("front"))
        //        {
        //            imageName = imageId.Trim() + imgFormatBWF;
        //        }
        //        //else if (states.Contains("back"))
        //        //{
        //        //    //imageName = imageId + imgFormatBWB;
        //        //}
        //        else if (states.Contains("bw") && states.Contains("back"))
        //        {
        //            imageName = imageId.Trim() + imgFormatBWB;
        //        }
        //        else if (states.Contains("greyscale") && states.Contains("front"))
        //        {
        //            //imageInfo.sizeScale = 1f;
        //            imageName = imageId.Trim() + imgFormatGF;
        //        }
        //        else if (states.Contains("greyscale") && states.Contains("back"))
        //        {
        //            //imageInfo.sizeScale = 1f;
        //            imageName = imageId.Trim() + imgFormatGB;
        //        }
        //        else if (states.Contains("uv"))
        //        {
        //            imageName = imageId.Trim() + imgFormatGU;
        //        }

        //        if (states.Contains("rotate"))
        //        {
        //            angel = 180f;
        //        }

        //        if (states.Contains("invert"))
        //        {
        //            filter = 1;
        //        }
        //    }
        //    imageInfo.sourcePath = string.Format("{0}\\{1}", imageFolder, imageName);
        //    imageInfo.angle = angel;
        //    imageInfo.filter = filter;
        //    imageInfo.filename = imageId.Trim();
        //    imageInfo.destinationPath = string.Format("{0}\\{1}-{2}-{3}.jpg", tempImageFolder, imageName, angel, filter);
        //    //imageInfo.destinationPath = string.Format("{0}\\{1}", tempImageFolder, imageName, angel, filter);
        //    //imageInfo.destinationPath = imageInfo.sourcePath;

        //    return imageInfo;

        //}

        #endregion

        public bool convertImageFromByteToTiffFile(byte[] photoBytes, string destinationPath) {
            int quality = 100;
            using (var inStream = new MemoryStream(photoBytes)) {
                using (var outStream = new FileStream(destinationPath, FileMode.Create)) {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (var imageFactory = new ImageFactory(preserveExifData: true)) {
                        // Do your magic here
                        imageFactory.Load(inStream).Format(new TiffFormat()).Quality(quality);                        
                        //imageFactory.Resize(size);
                        imageFactory.Save(outStream);
                    }
                }
            }

            return true;
        }

        public string drawImageForSignature(string imageId, string imageNo , byte[] imageBytes , float imageScale = 1) {
            string tempPath = systemProfileDao.GetValueFromSystemProfile("ChequeTempFolderPath", CurrentUser.Account.BankCode);

            string filename = string.Format("{0}{1}{2}-orig", imageId, imageNo, imageScale);

            Directory.CreateDirectory(Path.GetDirectoryName(tempPath + @"Signatures\"));

            string sourcePath = tempPath + @"Signatures\" + filename + ".tiff";
            string destinationPath = tempPath + @"Signatures\" + filename + ".jpeg";

            if (!File.Exists(destinationPath)) {
                convertImageFromByteToTiffFile(imageBytes, sourcePath);
                convertImageFromTiff(sourcePath, destinationPath, 2, imageScale);
            }
            return destinationPath;
        }

    }
}