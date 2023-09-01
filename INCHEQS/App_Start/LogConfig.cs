using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS {
    public class LogConfig {
        public static void RegisterLog() {
            XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
        }
    }
}