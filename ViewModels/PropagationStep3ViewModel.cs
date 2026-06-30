using System.Collections.ObjectModel;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class PropagationStep3ViewModel : ViewModelBase
{
    public ObservableCollection<PropagationResult> Results { get; } = [];

    public double AntennaFactorLinear { get; private set; }
    public string AntennaFactorLinearText => AntennaFactorLinear.ToString("0.00");
    public string ConversionModeText { get; private set; } = string.Empty;
    public string ConversionModeLatex { get; private set; } = string.Empty;

    public void Prepare(
        IEnumerable<PropagationMeasurementPoint> measurements,
        PropagationStep1ViewModel setup)
    {
        Results.Clear();
        AntennaFactorLinear =
            PropagationLogic.ConvertAntennaFactorDbToLinear(setup.AntennaFactorDb);
        ConversionModeText = setup.InputConventionDescription;
        ConversionModeLatex = setup.SelectedInputConvention.Kind switch
        {
            PropagationInputConventionKind.LegacyReport =>
                "U_{\\mathrm{\\mu V}}=10^{-\\frac{L}{20}}",
            PropagationInputConventionKind.DbMicrovolts =>
                "U_{\\mathrm{\\mu V}}=10^{\\frac{L_{\\mathrm{dB\\mu V}}}{20}}",
            _ =>
                "U_{\\mathrm{\\mu V}}=10^6\\sqrt{50\\cdot10^{-3}\\cdot10^{\\frac{P_{\\mathrm{dBm}}}{10}}}",
        };

        foreach (var point in measurements)
        {
            Results.Add(PropagationLogic.Calculate(
                point,
                setup.AntennaFactorDb,
                setup.CableLossDb,
                setup.SelectedInputConvention.Kind));
        }

        OnPropertyChanged(nameof(AntennaFactorLinearText));
        OnPropertyChanged(nameof(ConversionModeText));
        OnPropertyChanged(nameof(ConversionModeLatex));
    }
}
