using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Controls;

public sealed class CrosstalkChart : Control
{
    public static readonly StyledProperty<IEnumerable<MeasurementResult>?> MeasurementsProperty =
        AvaloniaProperty.Register<CrosstalkChart, IEnumerable<MeasurementResult>?>(
            nameof(Measurements));

    public IEnumerable<MeasurementResult>? Measurements
    {
        get => GetValue(MeasurementsProperty);
        set => SetValue(MeasurementsProperty, value);
    }

    static CrosstalkChart()
    {
        AffectsRender<CrosstalkChart>(MeasurementsProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var rows = Measurements?.OrderBy(row => row.FrequencyGHz).ToArray() ?? [];
        if (rows.Length < 2 || Bounds.Width < 80 || Bounds.Height < 80)
            return;

        const double left = 18;
        const double top = 14;
        const double right = 14;
        const double bottom = 18;

        var plot = new Rect(
            left,
            top,
            Math.Max(1, Bounds.Width - left - right),
            Math.Max(1, Bounds.Height - top - bottom));

        var gridPen = new Pen(new SolidColorBrush(Color.Parse("#D8DEE9")), 1);
        var axisPen = new Pen(new SolidColorBrush(Color.Parse("#748094")), 1.2);
        var nearPen = new Pen(new SolidColorBrush(Color.Parse("#2563EB")), 2.4);
        var farPen = new Pen(new SolidColorBrush(Color.Parse("#EA580C")), 2.4);

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

        context.DrawLine(
            axisPen,
            new Point(plot.Left, plot.Bottom),
            new Point(plot.Right, plot.Bottom));
        context.DrawLine(
            axisPen,
            new Point(plot.Left, plot.Top),
            new Point(plot.Left, plot.Bottom));

        var minimumDb = rows.Min(row => Math.Min(row.NearDb, row.FarDb)) - 2;
        var maximumDb = rows.Max(row => Math.Max(row.NearDb, row.FarDb)) + 2;
        if (Math.Abs(maximumDb - minimumDb) < 0.001)
            maximumDb = minimumDb + 1;

        Point ToPoint(int index, double valueDb)
        {
            var x = plot.Left + plot.Width * index / (rows.Length - 1);
            var normalized = (valueDb - minimumDb) / (maximumDb - minimumDb);
            var y = plot.Bottom - normalized * plot.Height;
            return new Point(x, y);
        }

        for (var index = 1; index < rows.Length; index++)
        {
            context.DrawLine(
                nearPen,
                ToPoint(index - 1, rows[index - 1].NearDb),
                ToPoint(index, rows[index].NearDb));
            context.DrawLine(
                farPen,
                ToPoint(index - 1, rows[index - 1].FarDb),
                ToPoint(index, rows[index].FarDb));
        }

        var nearBrush = new SolidColorBrush(Color.Parse("#2563EB"));
        var farBrush = new SolidColorBrush(Color.Parse("#EA580C"));
        for (var index = 0; index < rows.Length; index++)
        {
            context.DrawEllipse(nearBrush, null, ToPoint(index, rows[index].NearDb), 3, 3);
            context.DrawEllipse(farBrush, null, ToPoint(index, rows[index].FarDb), 3, 3);
        }
    }
}
