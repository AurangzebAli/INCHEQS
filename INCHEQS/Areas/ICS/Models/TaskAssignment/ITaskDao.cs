using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.TaskAssignment {
    public interface ITaskDao {
        List<TaskModel> ListAvailableTaskInGroup(string groupId);
        List<TaskModel> ListSelectedTaskInGroup(string groupId);
        List<TaskModel> ListAvailableTaskInGroupTemp(string groupId);
        List<TaskModel> ListSelectedTaskInGroupTemp(string groupId);
        void DeleteTaskNotSelected(string groupId, string taskIds);
        void DeleteTaskNotSelectedTemp(string groupId, string taskIds);
        void DeleteAllTaskInGroup(string groupId);
        bool CheckGroupExist(string groupId, string taskId);
        void UpdateSelectedTaskId(string groupId, string taskId);
        void UpdateSelectedTaskIdTemp(string groupId, string taskId);
        void InsertSelectedTaskId(string groupId, string taskId);
        void InsertSelectedTaskIdTemp(string groupId, string taskId);
        void DeleteTaskNotSelectedApproval(string groupId);
        void UpdateSelectedTaskIdApproval(string groupId);
        void DeleteAllTaskInGroupApproval(string groupId);
        void UpdateRejectTaskIdApproval(string groupId);
    }
}