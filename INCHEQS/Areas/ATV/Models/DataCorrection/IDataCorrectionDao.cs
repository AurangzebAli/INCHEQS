using INCHEQS.Areas.ATV.Models.DataCorrection;
using INCHEQS.Models.SearchPageConfig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Models.DataCorrectionDao {
    public interface IDataCorrectionDao
    {

        DataTable getDataCorrectionList();

        DataCorrectionModel getDataCorrectionModel(string itemId);
        DataTable getPathCropImage(string itemId);

        string condition();
        void Update(FormCollection col);
    }
}
