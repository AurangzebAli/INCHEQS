using INCHEQS.Areas.ICS.Models.MICRImage;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Threading.Tasks;

namespace INCHEQS.Areas.ICS.Hubs
{
    [HubName("notificationHub")]
    public class NotificationHub : Hub
    {
        
        
        
        private string broadcastmessage = string.Empty;
        
        [HubMethodName("sendNotification")]
        public async Task<string> SendNotification()
        {
            DataTable dtResult = new DataTable();

            try
            {
                string errorMsg = "";


                using (SqlConnection conn = new SqlConnection())
                {


                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["default"].ConnectionString;
                    conn.Open();
                    string query = "select top 1 fldProcessName from tblDataProcess order by fldPrimaryID desc";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Notification = null;
                        SqlDependency dependency = new SqlDependency();
                        dependency.OnChange += Dependency_OnChange;
                        if (conn.State == ConnectionState.Closed)
                        {
                            conn.Open();
                        }
                        var reader = cmd.ExecuteReader();
                        dtResult.Load(reader);
                        if (dtResult.Rows.Count>0)
                        {
                            broadcastmessage = dtResult.Rows[0]["fldProcessName"].ToString();
                        }
                        

                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            return await context.Clients.All.RecieveNotification(broadcastmessage);
        }
        
        private void Dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                NotificationHub nHub = new NotificationHub();
                nHub.SendNotification();

            }
        }



    }
}