using System;
using System.Collections.Generic;
using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using System.Data;
using System.Data.SqlClient;
using INCHEQS.Models.DbJoin;
using System.Data.SqlTypes;
using System.Text;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security;

namespace INCHEQS.Areas.OCS.Models.CheckAmountEntry
{
    public class CheckAmountEntryDao : ICheckAmountEntryDao
    {
        

        private readonly ApplicationDbContext dbContext;
        private readonly OCSDbContext ocsdbContext;
        private readonly DbJoinDao dbJoinDao;
        private readonly ISystemProfileDao systemProfileDao;

        public CheckAmountEntryDao(ApplicationDbContext dbContext, OCSDbContext ocsdbContext, DbJoinDao dbJoinDao, ISystemProfileDao systemProfileDao)
        {
            this.dbContext = dbContext;
            this.ocsdbContext = ocsdbContext;
            this.dbJoinDao = dbJoinDao;
            this.systemProfileDao = systemProfileDao;
        }

        

    }
}