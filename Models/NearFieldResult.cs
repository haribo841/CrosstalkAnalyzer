using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.Models;

public sealed class NearFieldResult
{
    public double FrequencyMHz { get; init; }
    public double Power30OhmDbm { get; init; }
    public double Power50OhmDbm { get; init; }
    public double Power100OhmDbm { get; init; }
    public double AmplifierGainDb { get; init; }
    public double ProbeCorrectionDb { get; init; }
    public double H30OhmDbAm { get; init; }
    public double H50OhmDbAm { get; init; }
    public double H100OhmDbAm { get; init; }
    public double ExpandedUncertaintyDb { get; init; }

    public double H30OhmAm => NearFieldLogic.ConvertDbAmToAm(H30OhmDbAm);
    public double H50OhmAm => NearFieldLogic.ConvertDbAmToAm(H50OhmDbAm);
    public double H100OhmAm => NearFieldLogic.ConvertDbAmToAm(H100OhmDbAm);

    public string FrequencyText => FrequencyMHz.ToString("0");
    public string AmplifierGainText => AmplifierGainDb.ToString("0.00");
    public string ProbeCorrectionText => ProbeCorrectionDb.ToString("0.00");
    public string H30OhmText => H30OhmDbAm.ToString("0.00");
    public string H50OhmText => H50OhmDbAm.ToString("0.00");
    public string H100OhmText => H100OhmDbAm.ToString("0.00");
    public string H30OhmLinearText => H30OhmAm.ToString("0.000E+0");
    public string H50OhmLinearText => H50OhmAm.ToString("0.000E+0");
    public string H100OhmLinearText => H100OhmAm.ToString("0.000E+0");
    public string UncertaintyText => ExpandedUncertaintyDb.ToString("0.00");
    public string H30IntervalText => FormatInterval(H30OhmDbAm);
    public string H50IntervalText => FormatInterval(H50OhmDbAm);
    public string H100IntervalText => FormatInterval(H100OhmDbAm);

    private string FormatInterval(double value)
        => $"⟨{value - ExpandedUncertaintyDb:0.00}; {value + ExpandedUncertaintyDb:0.00}⟩";
}
