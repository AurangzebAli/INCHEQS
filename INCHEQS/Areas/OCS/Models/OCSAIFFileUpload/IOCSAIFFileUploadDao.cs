using INCHEQS.Areas.OCS.Models.OCSAIFFileUpload;

namespace INCHEQS.Models.OCSAIFFileUploadDao
{
    public interface IOCSAIFFileUploadDao
    {
        OCSAIFFileUploadModel GetDataFromHostFileConfig(string taskId);
    }
}