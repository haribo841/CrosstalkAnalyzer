using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Controls;

public sealed class PropagationHeatmap : Control
{
    public static readonly StyledProperty<IEnumerable<PropagationResult>?> MeasurementsProperty =
        AvaloniaProperty.Register<PropagationHeatmap, IEnumerable<PropagationResult>?>(
            nameof(Measurements));

    public static readonly StyledProperty<bool> UseVerticalPolarizationProperty =
        AvaloniaProperty.Register<PropagationHeatmap, bool>(nameof(UseVerticalPolarization));

    public IEnumerable<PropagationResult>? Measurements
    {
        get => GetValue(MeasurementsProperty);
        set => SetValue(MeasurementsProperty, value);
    }

    public bool UseVerticalPolarization
    {
        get => GetValue(UseVerticalPolarizationProperty);
        set => SetValue(UseVerticalPolarizationProperty, value);
    }

    static PropagationHeatmap()
    {
        AffectsRender<PropagationHeatmap>(MeasurementsProperty, UseVerticalPolarizationProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var rows = Measurements?.OrderBy(row => row.PointNumber).Take(16).ToArray() ?? [];
        if (rows.Length != 16 || Bounds.Width < 180 || Bounds.Height < 180)
            return;

        var values = rows
            .Select(row => UseVerticalPolarization
                ? row.VerticalFieldDbuvPerM
                : row.HorizontalFieldDbuvPerM)
            .ToArray();
        var minimum = values.Min();
        var maximum = values.Max();
        if (Math.Abs(maximum - minimum) < 0.001)
            maximum = minimum + 1;

        var margin = 10.0;
        var titleHeight = 30.0;
        var cellSize = Math.Min(
            (Bounds.Width - 2 * margin) / 4,
            (Bounds.Height - titleHeight - 2 * margin) / 4);
        var gridWidth = cellSize * 4;
        var left = (Bounds.Width - gridWidth) / 2;
        var top = titleHeight;
        var borderPen = new Pen(new SolidColorBrush(Color.Parse("#FFFFFF")), 2);
        var title = new FormattedText(
            UseVerticalPolarization ? "Polaryzacja pionowa" : "Polaryzacja pozioma",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Inter", FontStyle.Normal, FontWeight.SemiBold),
            14,
            new SolidColorBrush(Color.Parse("#172033")));
        context.DrawText(title, new Point(left, 5));

        for (var index = 0; index < rows.Length; index++)
        {
            var column = index % 4;
            var row = index / 4;
            var value = values[index];
            var fraction = (value - minimum) / (maximum - minimum);
            var color = InterpolateColor(fraction);
            var rect = new Rect(
                left + column * cellSize,
                top + row * cellSize,
                cellSize,
                cellSize);
            context.DrawRectangle(new SolidColorBrush(color), borderPen, rect);

            var label = new FormattedText(
                $"{rows[index].PointNumber}\n{value:0.0}",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter", FontStyle.Normal, FontWeight.SemiBold),
                12,
                Brushes.White);
            context.DrawText(
                label,
                new Point(
                    rect.X + (rect.Width - label.Width) / 2,
                    rect.Y + (rect.Height - label.Height) / 2));
        }
    }

    private static Color InterpolateColor(double value)
    {
        value = Math.Clamp(value, 0, 1);
        var red = (byte)(37 + value * (220 - 37));
        var green = (byte)(99 + value * (38 - 99));
        var blue = (byte)(235 + value * (38 - 235));
        return Color.FromRgb(red, green, blue);
    }
}
