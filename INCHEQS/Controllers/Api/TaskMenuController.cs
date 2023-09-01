using INCHEQS.Models.TaskMenu;
using INCHEQS.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Controllers.Api
{
    [CustomAuthorize(TaskIds = "all")]
    public class TaskMenuController : BaseController {


        private readonly ITaskMenuDao taskMenuDao;
        public TaskMenuController(ITaskMenuDao taskMenuDao) {
            this.taskMenuDao = taskMenuDao;
        }

        //GET: TaskMenu    
        public async Task<JsonResult> GetCurrentUserMenuJSon()
        {
            
            ArrayList menus = await taskMenuDao.GetTaskListForUserAsync(CurrentUser.Account, "");
            

            return Json(menus, JsonRequestBehavior.AllowGet);
        }

    }
}