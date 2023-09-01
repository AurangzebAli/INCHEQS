using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCHEQS.Areas.OCS.Models.BankBranchesOcs
{
    public interface IBankBranchesOcsDao
    {
        DataTable GetBankBranchesDataTable(string BankCode);
        DataTable GetBankBranchesDataTableWorkStation(string BankCode);
        DataTable GetBankBranchDetails(string BranchId);
        DataTable GetIslamicBankCodeDetails(string BranchId);
        DataTable GetConvBankCodeDetails(string BranchId);
        DataTable GetBankCodeDetails(string BranchId);
        DataTable GetLocCodeDetails(string BranchId);
        DataTable GetStateCodeDetails(string BranchId);
    }
}
