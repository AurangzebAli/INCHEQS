using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using System.Data;
using System.Data.SqlClient;


namespace INCHEQS.Models.DbJoin
{
    public interface IDbJoinDao
    {
        DataTable GetCommonDatableFromOCS(string strQueryCommon, SqlParameter[] paramsCommon, string fldCommon, string strQueryOCS, SqlParameter[] paramsOCS, string fldOCS);
    }
}