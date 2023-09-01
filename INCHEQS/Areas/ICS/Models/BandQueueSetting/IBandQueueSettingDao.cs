using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.BandQueueSetting {
    public interface IBandQueueSettingDao {
        BandQueueSettingModel GetBandQueueSetting(string taskId);
        void CreateBandQueueSetting(FormCollection col, AccountModel currentUser);
        List<string> ValidateCreate(FormCollection col);
        List<string> ValidateUpdate(FormCollection col);
        void UpdateBandQueueSetting(FormCollection col);
        void DeleteFromBandQueueSetting(string taskId);
    }
}