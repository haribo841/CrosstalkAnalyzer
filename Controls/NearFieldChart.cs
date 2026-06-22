using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Controls;

public sealed class NearFieldChart : Control
{
    public static readonly StyledProperty<IEnumerable<NearFieldResult>?> MeasurementsProperty =
        AvaloniaProperty.Register<NearFieldChart, IEnumerable<NearFieldResult>?>(
            nameof(Measurements));

    public IEnumerable<NearFieldResult>? Measurements
    {
        get => GetValue(MeasurementsProperty);
        set => SetValue(MeasurementsProperty, value);
    }

    static NearFieldChart()
    {
        AffectsRender<NearFieldChart>(MeasurementsProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var rows = Measurements?.OrderBy(row => row.FrequencyMHz).ToArray() ?? [];
        if (rows.Length < 2 || Bounds.Width < 80 || Bounds.Height < 80)
            return;

        var plot = new Rect(18, 14, Math.Max(1, Bounds.Width - 32), Math.Max(1, Bounds.Height - 32));
        var gridPen = new Pen(new SolidColorBrush(Color.Parse("#D8DEE9")), 1);
        var axisPen = new Pen(new SolidColorBrush(Color.Parse("#748094")), 1.2);
        var pens = new[]
        {
            new Pen(new SolidColorBrush(Color.Parse("#2563EB")), 2.4),
            new Pen(new SolidColorBrush(Color.Parse("#16A34A")), 2.4),
            new Pen(new SolidColorBrush(Color.Parse("#EA580C")), 2.4),
        };
        var brushes = pens.Select(pen => pen.Brush).ToArray();

        context.DrawRectangle(new SolidColorBrush(Color.Parse("#FBFCFE")), gridPen, plot);
        for (var index = 1; index < 5; index++)
        {
            var y = plot.Top + plot.Height * index / 5;
            context.DrawLine(gridPen, new Point(plot.Left, y), new Point(plot.Right, y));
        }
        for (var index = 1; index < rows.Length - 1; index++)
        {
            var x = plot.Left + plot.Width * index / (rows.Length - 1);
            context.DrawLine(gridPen, new Point(x, plot.Top), new Point(x, plot.Bottom));
        }
        context.DrawLine(axisPen, new Point(plot.Left, plot.Bottom), new Point(plot.Right, plot.Bottom));
        context.DrawLine(axisPen, new Point(plot.Left, plot.Top), new Point(plot.Left, plot.Bottom));

        var series = new Func<NearFieldResult, double>[]
        {
            row => row.H30OhmDbAm,
            row => row.H50OhmDbAm,
            row => row.H100OhmDbAm,
        };
        var minimum = rows.Min(row => series.Min(selector => selector(row))) - 2;
        var maximum = rows.Max(row => series.Max(selector => selector(row))) + 2;

        Point ToPoint(int index, double value)
        {
            var x = plot.Left + plot.Width * index / (rows.Length - 1);
            var y = plot.Bottom - (value - minimum) / (maximum - minimum) * plot.Height;
            return new Point(x, y);
        }

        for (var seriesIndex = 0; seriesIndex < series.Length; seriesIndex++)
        {
            var selector = series[seriesIndex];
            for (var index = 1; index < rows.Length; index++)
            {
                context.DrawLine(
                    pens[seriesIndex],
                    ToPoint(index - 1, selector(rows[index - 1])),
                    ToPoint(index, selector(rows[index])));
            }

            for (var index = 0; index < rows.Length; index++)
            {
                context.DrawEllipse(
                    brushes[seriesIndex],
                    null,
                    ToPoint(index, selector(rows[index])),
                    3,
                    3);
            }
        }
    }
}
