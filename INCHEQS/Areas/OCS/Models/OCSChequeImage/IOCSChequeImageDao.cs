using System.Data;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.ChequeImage
{
    public interface IOCSChequeImageDao
    {
        DataTable GetImageByte(string uic);
        DataTable GetImageByte(string uic, FormCollection col);
    }
}