namespace EnergyOptimizer.Core.DTOs.AuthDTOs
{
    public record IdentityResultDto(bool Succeeded, IEnumerable<string> Errors)
    {
        public static IdentityResultDto Success() => new(true, Enumerable.Empty<string>());
        public static IdentityResultDto Failure(IEnumerable<string> errors) => new(false, errors);
        public static IdentityResultDto Failure(string error) => new(false, new[] { error });
    }

    public record UserAuthInfo(string Id, string Email, string FullName, bool EmailConfirmed);
}
