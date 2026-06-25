using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Controls;

public sealed class RadiatedEmissionChart : Control
{
    public static readonly StyledProperty<IEnumerable<RadiatedEmissionResult>?> MeasurementsProperty =
        AvaloniaProperty.Register<RadiatedEmissionChart, IEnumerable<RadiatedEmissionResult>?>(
            nameof(Measurements));

    public IEnumerable<RadiatedEmissionResult>? Measurements
    {
        get => GetValue(MeasurementsProperty);
        set => SetValue(MeasurementsProperty, value);
    }

    static RadiatedEmissionChart()
    {
        AffectsRender<RadiatedEmissionChart>(MeasurementsProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var rows = Measurements?.OrderBy(row => row.FrequencyMHz).ToArray() ?? [];
        if (rows.Length < 2 || Bounds.Width < 80 || Bounds.Height < 80)
            return;

        const double left = 26;
        const double top = 14;
        const double right = 14;
        const double bottom = 20;

        var plot = new Rect(
            left,
            top,
            Math.Max(1, Bounds.Width - left - right),
            Math.Max(1, Bounds.Height - top - bottom));

        var gridPen = new Pen(new SolidColorBrush(Color.Parse("#D8DEE9")), 1);
        var axisPen = new Pen(new SolidColorBrush(Color.Parse("#748094")), 1.2);
        var emissionPen = new Pen(new SolidColorBrush(Color.Parse("#2563EB")), 2.4);
        var limitPen = new Pen(new SolidColorBrush(Color.Parse("#DC2626")), 2.2);
        var uncertaintyPen = new Pen(new SolidColorBrush(Color.Parse("#93B4EA")), 1.4);
        var emissionBrush = new SolidColorBrush(Color.Parse("#2563EB"));

        context.DrawRectangle(new SolidColorBrush(Color.Parse("#FBFCFE")), gridPen, plot);

        for (var index = 1; index < 5; index++)
        {
            var y = plot.Top + plot.Height * index / 5;
            context.DrawLine(gridPen, new Point(plot.Left, y), new Point(plot.Right, y));
        }

        foreach (var frequency in new[] { 230, 400, 600, 800 })
        {
            var x = FrequencyToX(frequency);
            context.DrawLine(gridPen, new Point(x, plot.Top), new Point(x, plot.Bottom));
        }

        context.DrawLine(
            axisPen,
            new Point(plot.Left, plot.Bottom),
            new Point(plot.Right, plot.Bottom));
        context.DrawLine(
            axisPen,
            new Point(plot.Left, plot.Top),
            new Point(plot.Left, plot.Bottom));

        var minimum = Math.Min(
            rows.Min(row => row.LowerConfidenceLimitDbuvPerM),
            rows.Min(row => row.LimitDbuvPerM)) - 3;
        var maximum = Math.Max(
            rows.Max(row => row.UpperConfidenceLimitDbuvPerM),
            rows.Max(row => row.LimitDbuvPerM)) + 3;
        if (Math.Abs(maximum - minimum) < 0.001)
            maximum = minimum + 1;

        double FrequencyToX(double frequencyMHz)
        {
            var minFrequency = rows.First().FrequencyMHz;
            var maxFrequency = rows.Last().FrequencyMHz;
            return plot.Left + (frequencyMHz - minFrequency) /
                (maxFrequency - minFrequency) * plot.Width;
        }

        double LevelToY(double value)
            => plot.Bottom - (value - minimum) / (maximum - minimum) * plot.Height;

        Point ToPoint(RadiatedEmissionResult row, double value)
            => new(FrequencyToX(row.FrequencyMHz), LevelToY(value));

        var limit40Y = LevelToY(40);
        var limit47Y = LevelToY(47);
        var x30 = FrequencyToX(rows.First().FrequencyMHz);
        var x230 = FrequencyToX(230);
        var x1000 = FrequencyToX(rows.Last().FrequencyMHz);
        context.DrawLine(limitPen, new Point(x30, limit40Y), new Point(x230, limit40Y));
        context.DrawLine(limitPen, new Point(x230, limit40Y), new Point(x230, limit47Y));
        context.DrawLine(limitPen, new Point(x230, limit47Y), new Point(x1000, limit47Y));

        for (var index = 1; index < rows.Length; index++)
        {
            context.DrawLine(
                emissionPen,
                ToPoint(rows[index - 1], rows[index - 1].MaxFieldDbuvPerM),
                ToPoint(rows[index], rows[index].MaxFieldDbuvPerM));
        }

        foreach (var row in rows)
        {
            var x = FrequencyToX(row.FrequencyMHz);
            var low = LevelToY(row.LowerConfidenceLimitDbuvPerM);
            var high = LevelToY(row.UpperConfidenceLimitDbuvPerM);
            context.DrawLine(uncertaintyPen, new Point(x, low), new Point(x, high));
            context.DrawLine(uncertaintyPen, new Point(x - 4, low), new Point(x + 4, low));
            context.DrawLine(uncertaintyPen, new Point(x - 4, high), new Point(x + 4, high));
            context.DrawEllipse(
                emissionBrush,
                null,
                ToPoint(row, row.MaxFieldDbuvPerM),
                3.2,
                3.2);
        }
    }
}
