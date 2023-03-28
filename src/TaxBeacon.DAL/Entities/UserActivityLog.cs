﻿using TaxBeacon.Common.Enums.Activities;

namespace TaxBeacon.DAL.Entities
{
    public class UserActivityLog
    {
        public Guid TenantId { get; set; }

        public Guid UserId { get; set; }

        public DateTime Date { get; set; }

        public EventType EventType { get; set; }

        public uint Revision { get; set; }

        public string Event { get; set; } = string.Empty;

        public User User { get; set; } = null!;
    }
}