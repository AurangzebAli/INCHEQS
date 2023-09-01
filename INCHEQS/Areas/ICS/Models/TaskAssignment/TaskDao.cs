using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace INCHEQS.Models.TaskAssignment {
    public class TaskDao : ITaskDao {

        private readonly ApplicationDbContext dbContext;
        public TaskDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public List<TaskModel> ListAvailableTaskInGroupTemp(string groupId) {
            List<TaskModel> taskList = new List<TaskModel>();
            string stmt = " select fldTaskId,fldTaskDesc from tblTaskMaster where fldTaskId not in(select fldTaskId from tblGroupTask where fldGroupId=@fldGroupId AND fldApproved='Y') AND fldRecordStatus=1 order by fldTaskDesc ";

            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldGroupId", groupId) });

            if (((ds.Rows.Count > 0))) {
                foreach (DataRow row in ds.Rows) {
                    TaskModel task = new TaskModel();
                    task.fldTaskId = row["fldTaskId"].ToString();
                    task.fldTaskDesc = row["fldTaskDesc"].ToString();
                    taskList.Add(task);
                }
            }
            return taskList;
        }
        public List<TaskModel> ListAvailableTaskInGroup(string groupId)
        {
            List<TaskModel> taskList = new List<TaskModel>();
            string stmt = " select fldTaskId,fldTaskDesc from tblTaskMaster where fldTaskId not in(select fldTaskId from tblGroupTask where fldGroupId=@fldGroupId ) AND fldRecordStatus=1 order by fldTaskDesc ";

            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldGroupId", groupId) });

            if (((ds.Rows.Count > 0)))
            {
                foreach (DataRow row in ds.Rows)
                {
                    TaskModel task = new TaskModel();
                    task.fldTaskId = row["fldTaskId"].ToString();
                    task.fldTaskDesc = row["fldTaskDesc"].ToString();
                    taskList.Add(task);
                }
            }
            return taskList;
        }
        public List<TaskModel> ListSelectedTaskInGroupTemp(string groupId) {
            string stmt = " select fldTaskId,fldTaskDesc from tblTaskMaster where (fldRecordStatus <> 0 or fldRecordStatus is null) and fldTaskId in(select fldTaskId from tblGroupTask where fldGroupId=@fldGroupId AND fldApproved = 'Y') AND fldTaskDesc is not null order by fldTaskDesc ";
            List<TaskModel> taskList = new List<TaskModel>();
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                    new SqlParameter("@fldGroupId", groupId)});

            if (((ds.Rows.Count > 0))) {
                foreach (DataRow row in ds.Rows) {
                    TaskModel task = new TaskModel();
                    task.fldTaskId = row["fldTaskId"].ToString();
                    task.fldTaskDesc = row["fldTaskDesc"].ToString();
                    taskList.Add(task);
                }
            }
            return taskList;
        }
        public List<TaskModel> ListSelectedTaskInGroup(string groupId)
        {
            string stmt = " select fldTaskId,fldTaskDesc from tblTaskMaster where (fldRecordStatus <> 0 or fldRecordStatus is null) and fldTaskId in(select fldTaskId from tblGroupTask where fldGroupId=@fldGroupId) AND fldTaskDesc is not null order by fldTaskDesc ";
            List<TaskModel> taskList = new List<TaskModel>();
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                    new SqlParameter("@fldGroupId", groupId)});

            if (((ds.Rows.Count > 0)))
            {
                foreach (DataRow row in ds.Rows)
                {
                    TaskModel task = new TaskModel();
                    task.fldTaskId = row["fldTaskId"].ToString();
                    task.fldTaskDesc = row["fldTaskDesc"].ToString();
                    taskList.Add(task);
                }
            }
            return taskList;
        }

        public void DeleteTaskNotSelected(string groupId, string taskIds) {
            string[] aryText = taskIds.Split(',');
            List<SqlParameter> sqlParams = DatabaseUtils.getSqlParametersFromArray(aryText);
            sqlParams.Add(new SqlParameter("@fldGroupId", groupId));
            if ((aryText.Length > 0)) {
                string stmt = "Delete from tblGroupTask where fldTaskId not in (" + DatabaseUtils.getParameterizedStatementFromArray(aryText) + ") AND fldGroupId=@fldGroupId";
                dbContext.ExecuteNonQuery(stmt, sqlParams.ToArray());
            }
        }

        public void DeleteTaskNotSelectedApproval(string groupId)
        {
            //string[] aryText = taskIds.Split(',');
            //List<SqlParameter> sqlParams = DatabaseUtils.getSqlParametersFromArray(aryText);
            //sqlParams.Add(new SqlParameter("@fldGroupId", groupId));
            //if ((aryText.Length > 0))
            //{
            //    string stmt = "Delete from tblGroupTask where fldTaskId not in (" + DatabaseUtils.getParameterizedStatementFromArray(aryText) + ") AND fldGroupId=@fldGroupId";
            //    dbContext.ExecuteNonQuery(stmt, sqlParams.ToArray());
            //}

            string stmt = " Delete from tblGroupTask where fldGroupId=@fldGroupId and fldApproved='U'";
            try
            {
                dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldGroupId", groupId) });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteTaskNotSelectedTemp(string groupId, string taskIds)
        {
            string[] aryText = taskIds.Split(',');
            List<SqlParameter> sqlParams = DatabaseUtils.getSqlParametersFromArray(aryText);
            sqlParams.Add(new SqlParameter("@fldGroupId", groupId));
            if ((aryText.Length > 0))
            {
                string stmt = "Update tblGroupTask set fldApproved = 'U' where fldTaskId not in (" + DatabaseUtils.getParameterizedStatementFromArray(aryText) + ") AND fldGroupId=@fldGroupId";
                dbContext.ExecuteNonQuery(stmt, sqlParams.ToArray());
            }

        }

        public void DeleteAllTaskInGroup(string groupId) {
            string stmt = " Delete from tblGroupTask where fldGroupId=@fldGroupId";
            try {
                dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldGroupId", groupId) });
            } catch (Exception ex) {
                throw ex;
            }
        }

        public void DeleteAllTaskInGroupApproval(string groupId)
        {
            string stmt = "Update tblGroupTask set fldApproved = 'U' where fldGroupId=@fldGroupId and fldApproved = 'Y'";
            try
            {
                dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldGroupId", groupId) });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckGroupExist(string groupId, string taskId) {
            string stmt = " SELECT * FROM tblGroupTask WHERE fldTaskId=@fldTaskId AND fldGroupId=@fldGroupId";
            return dbContext.CheckExist(stmt, new[] {
                new SqlParameter("@fldTaskId", taskId),
                new SqlParameter("@fldGroupId", groupId)
            });
        }

        public void UpdateSelectedTaskIdTemp(string groupId, string taskId) {
            string stmt = " update tblGroupTask set fldUpdateUserId=@fldUpdateUserId, fldUpdateTimeStamp=@fldUpdateTimeStamp where fldGroupId=@fldGroupId AND fldTaskId=@fldTaskId AND fldApproved=@fldApproved";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldUpdateTimeStamp", DateTime.Now),
                new SqlParameter("@fldGroupId", groupId),
                new SqlParameter("@fldTaskId", taskId),
                new SqlParameter("@fldApproved", "S")
            });

        }
        public void UpdateSelectedTaskId(string groupId, string taskId)
        {
            string stmt = " update tblGroupTask set fldUpdateUserId=@fldUpdateUserId, fldUpdateTimeStamp=@fldUpdateTimeStamp where fldGroupId=@fldGroupId AND fldTaskId=@fldTaskId AND fldApproved=@fldApproved";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldUpdateTimeStamp", DateTime.Now),
                new SqlParameter("@fldGroupId", groupId),
                new SqlParameter("@fldTaskId", taskId),
                new SqlParameter("@fldApproved", "Y")
            });

        }
        public void UpdateSelectedTaskIdApproval(string groupId)
        {
            string stmt = " update tblGroupTask set fldApproved=@fldApproved,fldUpdateUserId=@fldUpdateUserId, fldUpdateTimeStamp=@fldUpdateTimeStamp where fldGroupId=@fldGroupId AND fldApproved='S' ";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldUpdateTimeStamp", DateTime.Now),
                new SqlParameter("@fldGroupId", groupId),
                new SqlParameter("@fldApproved", "Y")
            });

        }

        public void UpdateRejectTaskIdApproval(string groupId)
        {
            string stmt = " update tblGroupTask set fldApproved=@fldApproved,fldUpdateUserId=@fldUpdateUserId, fldUpdateTimeStamp=@fldUpdateTimeStamp where fldGroupId=@fldGroupId AND fldApproved='U'; Delete from tblGroupTask where fldGroupId=@fldGroupId AND fldApproved = 'S' ";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldUpdateTimeStamp", DateTime.Now),
                new SqlParameter("@fldGroupId", groupId),
                new SqlParameter("@fldApproved", "Y")
            });

        }
        public void InsertSelectedTaskIdTemp(string groupId, string taskId) {
            string stmt = " insert into tblGroupTask(fldGroupId, fldTaskId, fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp,fldApproved) VALUES (@fldGroupId, @fldTaskId, @fldCreateUserId, @fldCreateTimeStamp, @fldUpdateUserId, @fldUpdateTimeStamp, @fldApproved)";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldGroupId", groupId),
                new SqlParameter("@fldTaskId", taskId),
                new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldCreateTimeStamp", DateTime.Now),
                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldUpdateTimeStamp", DateTime.Now),
                new SqlParameter("@fldApproved", "S")
            });

        }
        public void InsertSelectedTaskId(string groupId, string taskId)
        {
            string stmt = " insert into tblGroupTask(fldGroupId, fldTaskId, fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp,fldApproved) VALUES (@fldGroupId, @fldTaskId, @fldCreateUserId, @fldCreateTimeStamp, @fldUpdateUserId, @fldUpdateTimeStamp, @fldApproved)";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldGroupId", groupId),
                new SqlParameter("@fldTaskId", taskId),
                new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldCreateTimeStamp", DateTime.Now),
                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldUpdateTimeStamp", DateTime.Now),
                new SqlParameter("@fldApproved", "Y")
            });

        }

    }
}