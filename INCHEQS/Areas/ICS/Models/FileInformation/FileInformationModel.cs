using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.FileInformationModels {
    public class FileInformationModel {
        public string fileStatus { get; set; }
        public string fileName { get; set; }
        public string fldPosPayType { get; set; }
        public string fileSize { get; set; }
        public string fileTimeStamp { get; set; }
        public string filebankcode { get; set; }
        public string fileBankType { get; set; }
        public string filebankcode1 { get; set; }
    }
    public class FolderInformationModel
    {
        public string folderName { get; set; }
        public string folderCount { get; set; }
        public string fileSize { get; set; }
        public string fileTimeStamp { get; set; }
    }
}