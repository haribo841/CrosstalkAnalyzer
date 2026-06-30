using Avalonia;
using Avalonia.Controls;

namespace CrosstalkAnalyzer.Controls;

public partial class MathFormulaControl : UserControl
{
    public static readonly StyledProperty<string> CaptionProperty =
        AvaloniaProperty.Register<MathFormulaControl, string>(nameof(Caption), "RÓWNANIE");

    public static readonly StyledProperty<string> LatexProperty =
        AvaloniaProperty.Register<MathFormulaControl, string>(nameof(Latex), string.Empty);

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<MathFormulaControl, string>(nameof(Description), string.Empty);

    public static readonly StyledProperty<string> AccessibleTextProperty =
        AvaloniaProperty.Register<MathFormulaControl, string>(nameof(AccessibleText), string.Empty);

    public string Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public string Latex
    {
        get => GetValue(LatexProperty);
        set => SetValue(LatexProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string AccessibleText
    {
        get => GetValue(AccessibleTextProperty);
        set => SetValue(AccessibleTextProperty, value);
    }

    public MathFormulaControl()
    {
        InitializeComponent();
    }
}
