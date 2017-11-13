using System;
using System.Collections.Generic;


namespace Plugin.Notifications
{
    public interface INotificationRepository
    {
        Notification GetById(int id);
        IEnumerable<Notification> GetScheduled();
        void Insert(Notification notification);
        void Delete(int id);
        void DeleteAll();
        //void CleanUpOld();

        int CurrentScheduleId { get; set; }
        int CurrentBadge { get; set; }
    }
}