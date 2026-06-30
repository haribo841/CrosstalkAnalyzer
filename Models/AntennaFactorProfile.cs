namespace CrosstalkAnalyzer.Models;

public enum AntennaFactorProfileKind
{
    FreeSpace,
    ShortDistance3mTip,
    ShortDistance3mCenter,
    Custom,
}

public sealed record AntennaFactorProfile(
    AntennaFactorProfileKind Kind,
    string Name)
{
    public override string ToString() => Name;
}
