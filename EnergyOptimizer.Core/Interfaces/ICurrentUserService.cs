namespace EnergyOptimizer.Core.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string RequireUserId();
    }
}
