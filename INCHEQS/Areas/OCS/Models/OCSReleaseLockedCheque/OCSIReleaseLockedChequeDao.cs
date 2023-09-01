﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.OCSReleaseLockedCheque {
    public interface OCSIReleaseLockedChequeDao {
        DataTable ListLockedCheque(string bankCode);
        void DeleteProcessUsingCheckbox(string dataProcess);
        void UpdateReleaseLockedCheque(string InwardItemId);
    }
}