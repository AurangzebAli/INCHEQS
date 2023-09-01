using System;
using System.Threading;
using Microsoft.Owin;
using Owin;


namespace INCHEQS
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}