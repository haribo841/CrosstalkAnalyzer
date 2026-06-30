using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosstalkAnalyzer.Models;

public partial class PropagationMeasurementPoint : ObservableObject
{
    [ObservableProperty]
    private int _pointNumber;

    [ObservableProperty]
    private double? _horizontalLevelDb;

    [ObservableProperty]
    private double? _verticalLevelDb;

    public string PointText => PointNumber.ToString("0");
}
