namespace EnergyOptimizer.Core.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string TokenHash { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DateTime ExpiresOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? RevokedOn { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? CreatedByIp { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public bool IsRevoked => RevokedOn != null;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
