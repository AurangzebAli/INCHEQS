//using INCHEQS.Models.Account;
using INCHEQS.Security.Account;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace INCHEQS.Models.TaskMenu {
    public interface ITaskMenuDao {
        Task<ArrayList> GetTaskListForUserAsync(AccountModel account, string strMenu);
    }
}