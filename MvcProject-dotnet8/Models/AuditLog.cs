using System;

namespace MvcProject.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string UserId { get; set; }

        public string Action { get; set; }

        public string EntityName { get; set; }

        public int? EntityId { get; set; }
    }
}
