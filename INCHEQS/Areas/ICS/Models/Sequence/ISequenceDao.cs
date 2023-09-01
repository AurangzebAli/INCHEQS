using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.Sequence {
    public interface ISequenceDao {
        int GetNextSequenceNo(string tableName);
        void UpdateSequenceNo(int lastSeqId, string tableName);
    }
}