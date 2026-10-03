namespace Library.Api.Services;

public static class MembershipNumberGenerator
{
    public static string Generate()
    {
        return $"LIB-{Guid.NewGuid():N}"[..12]
            .ToUpperInvariant();
    }
}