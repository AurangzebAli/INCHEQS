using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.Sequence {
    public class SequenceDao : ISequenceDao{

        private readonly ApplicationDbContext dbContext;
        public SequenceDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public int GetNextSequenceNo(string tableName) {
            int resultCount = 0;
            string stmt = "SELECT fldLastSeqId FROM tblSequenceMaster WITH (NOLOCK) WHERE fldTableName = @fldTableName";
            DataTable ds = dbContext.GetRecordsAsDataTable( stmt, new[] { new SqlParameter("@fldTableName", tableName) });   
            if(ds.Rows.Count > 0) {
                resultCount = Convert.ToInt32(ds.Rows[0]["fldLastSeqId"]);
            }           
            
            return resultCount + 1;
        }

        public void UpdateSequenceNo(int lastSeqId, string tableName) {

            string stmt = "UPDATE tblSequenceMaster SET fldLastSeqId = @fldLastSeqId WHERE fldTableName = @fldTableName";
       
                dbContext.ExecuteNonQuery( stmt, 
                    new[] {
                        new SqlParameter("@fldLastSeqId", lastSeqId),
                        new SqlParameter("@fldTableName", tableName)
                    });
                   
            

        }
    }
}