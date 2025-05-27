using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace course.Models
{
    public class Moderator
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        [StringLength(50)]
        public string AccessLevel { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<BlacklistEntry> ManagedBlacklistEntries { get; set; } = new List<BlacklistEntry>();
        public virtual ICollection<Complaint> HandledComplaints { get; set; } = new List<Complaint>();
    }
}