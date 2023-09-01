using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using INCHEQS.Security;
using INCHEQS.Resources;
using System.Globalization;

namespace INCHEQS.Areas.PPS.Models.LoadPPSFile
{
    public class LoadPPSFileDao : ILoadPPSFileDao
    {

        private readonly ApplicationDbContext dbContext;


        public LoadPPSFileDao(ApplicationDbContext dbContext)
        {

            this.dbContext = dbContext;
        }
        
        public string LoadPPSFilePath()
        {
            string Path = "";

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@profileCode ", "LoadPPSFile"));

            DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgSystemProfile", sqlParameterNext.ToArray());
            if (recordsAsDataTable.Rows.Count > 0)
            {
                Path = recordsAsDataTable.Rows[0]["fldSystemProfileValue"].ToString();
            }
            else
            {
                Path = "PPS Path not found in DB";
            }
            return Path;
        }
        

    }
}