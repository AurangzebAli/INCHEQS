using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.SearchPageConfig {
    public class ConfigTable {
        public string ViewOrTableName { get; set; }
        public DataTable dataTable { get; set; }

        public ConfigTable(DataTable dataTable , string tableName) {
            this.dataTable = dataTable;
            this.ViewOrTableName = tableName;
        }
    }
}