using System.ComponentModel.DataAnnotations;

namespace Star_Events.Helper
{
    /// <summary>
    /// Base entity class providing common properties for all entities
    /// Demonstrates inheritance and encapsulation principles
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Computed properties demonstrating encapsulation
        public bool IsNew => Id == 0;
        public bool IsModified => UpdatedAt.HasValue;
        public bool IsDeleted => !IsActive;
    }
}

