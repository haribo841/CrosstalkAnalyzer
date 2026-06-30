using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Controls;

public sealed class PropagationChart : Control
{
    public static readonly StyledProperty<IEnumerable<PropagationResult>?> MeasurementsProperty =
        AvaloniaProperty.Register<PropagationChart, IEnumerable<PropagationResult>?>(
            nameof(Measurements));

    public IEnumerable<PropagationResult>? Measurements
    {
        get => GetValue(MeasurementsProperty);
        set => SetValue(MeasurementsProperty, value);
    }

    static PropagationChart()
    {
        AffectsRender<PropagationChart>(MeasurementsProperty);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var rows = Measurements?.OrderBy(row => row.PointNumber).ToArray() ?? [];
        if (rows.Length < 2 || Bounds.Width < 80 || Bounds.Height < 80)
            return;

        var plot = new Rect(22, 14, Math.Max(1, Bounds.Width - 36), Math.Max(1, Bounds.Height - 34));
        var gridPen = new Pen(new SolidColorBrush(Color.Parse("#D8DEE9")), 1);
        var axisPen = new Pen(new SolidColorBrush(Color.Parse("#748094")), 1.2);
        var horizontalPen = new Pen(new SolidColorBrush(Color.Parse("#2563EB")), 2.4);
        var verticalPen = new Pen(new SolidColorBrush(Color.Parse("#16A34A")), 2.4);
        var horizontalBrush = new SolidColorBrush(Color.Parse("#2563EB"));
        var verticalBrush = new SolidColorBrush(Color.Parse("#16A34A"));

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

        var minimum = rows.Min(row => Math.Min(row.HorizontalFieldDbuvPerM, row.VerticalFieldDbuvPerM)) - 2;
        var maximum = rows.Max(row => Math.Max(row.HorizontalFieldDbuvPerM, row.VerticalFieldDbuvPerM)) + 2;
        if (Math.Abs(maximum - minimum) < 0.001)
            maximum = minimum + 1;

        Point ToPoint(int index, double value)
        {
            var x = plot.Left + plot.Width * index / (rows.Length - 1);
            var y = plot.Bottom - (value - minimum) / (maximum - minimum) * plot.Height;
            return new Point(x, y);
        }

        for (var index = 1; index < rows.Length; index++)
        {
            context.DrawLine(
                horizontalPen,
                ToPoint(index - 1, rows[index - 1].HorizontalFieldDbuvPerM),
                ToPoint(index, rows[index].HorizontalFieldDbuvPerM));
            context.DrawLine(
                verticalPen,
                ToPoint(index - 1, rows[index - 1].VerticalFieldDbuvPerM),
                ToPoint(index, rows[index].VerticalFieldDbuvPerM));
        }

        for (var index = 0; index < rows.Length; index++)
        {
            context.DrawEllipse(
                horizontalBrush,
                null,
                ToPoint(index, rows[index].HorizontalFieldDbuvPerM),
                3,
                3);
            context.DrawEllipse(
                verticalBrush,
                null,
                ToPoint(index, rows[index].VerticalFieldDbuvPerM),
                3,
                3);
        }
    }
}
