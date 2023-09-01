using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.Message
{
    public interface IMessageDao
    {
        MessageModel GetMessage(string Message);
        MessageModel GetMessageTemp(string Message);
        MessageModel GetMessagebyId(string MessageId);
        MessageModel GetMessageTempbyId(string MessageId);
        List<string> ValidateMessage(FormCollection col, string action);
        bool UpdateBroadcastMessageMaster(FormCollection col);
        void CreateBroadcastMessageMasterTemp(FormCollection col, string crtUser, string messageId, string Action);
        bool DeleteBroadcastMessageMaster(string messageId);
        bool CheckBroadcastMessageMasterTempById(string messageId);
        bool CreateBroadcastMessageMaster(FormCollection col);
        void MoveToBroadcastMessageMasterFromTemp(string messageId, string Action);
        bool DeleteBroadcastMessageMasterTemp(string messageId);
    }
}
