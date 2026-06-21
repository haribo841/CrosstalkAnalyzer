namespace CrosstalkAnalyzer.Models;

public sealed record BandDefinition(
    string Name,
    double StartGHz,
    double EndGHz,
    double AnalyzerUncertaintyDb)
{
    public override string ToString() => Name;
}
