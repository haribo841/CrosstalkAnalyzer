namespace CrosstalkAnalyzer.Models;

public enum PropagationInputConventionKind
{
    LegacyReport,
    DbMicrovolts,
    Dbm50Ohm,
}

public sealed record PropagationInputConvention(
    PropagationInputConventionKind Kind,
    string Name,
    string Unit)
{
    public override string ToString() => Name;
}
