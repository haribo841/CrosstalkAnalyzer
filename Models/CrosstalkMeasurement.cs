using CrosstalkAnalyzer.ViewModels;

namespace CrosstalkAnalyzer.Models;

public class CrosstalkMeasurement : ViewModelBase
{
    private double? _nearEndValue;
    private double? _farEndValue;
    public double FrequencyGHz { get; set; }
    public double? NearCrosstalkDb { get; set; }
    public double? FarCrosstalkDb { get; set; }

    public double? NearEndValue
    {
        get => _nearEndValue;
        set
        {
            SetProperty(ref _nearEndValue, value);
        }
    }

    public double? FarEndValue
    {
        get => _farEndValue;
        set
        {
            SetProperty(ref _farEndValue, value);
        }
    }
}