using System.Data;

namespace INCHEQS.Areas.ICS.Models.ChequeImage
{
    public interface IICSChequeImageDao
    {
        DataTable GetImageByte(string uic);
        //DataTable GetImageName(string uic);
    }
}