using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using CSharpMath.Avalonia;
using CrosstalkAnalyzer.ViewModels;
using CrosstalkAnalyzer.Views;
using Xunit;

namespace CrosstalkAnalyzer.UiTests;

public sealed class LayoutTests
{
    [AvaloniaFact]
    public void ScenarioSelection_DoesNotOverlapActionsAtMinimumWindowSize()
    {
        var window = CreateWindow(new MainWindowViewModel());
        try
        {
            AssertInsideWindow(window, "MainContentBorder");
            AssertInsideWindow(window, "FooterBorder");
            AssertNoExternalTextIsCoveredByButtons(window);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void FormulaStep_RendersMathAndKeepsFooterVisible()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.ScenarioSelection.SelectCrosstalkCommand.Execute(null);
        viewModel.Step1.SelectedBand = viewModel.Step1.AvailableBands[0];
        viewModel.Step1.FillExampleDataCommand.Execute(null);
        viewModel.NextStepCommand.Execute(null);

        var window = CreateWindow(viewModel);
        try
        {
            var formulas = window.GetVisualDescendants().OfType<MathView>().ToArray();
            Assert.NotEmpty(formulas);
            Assert.All(formulas, formula =>
            {
                Assert.True(formula.Bounds.Width > 50, "Wzór ma zbyt małą szerokość.");
                Assert.True(formula.Bounds.Height > 12, "Wzór ma zbyt małą wysokość.");
            });
            AssertInsideWindow(window, "FooterBorder");
            AssertNoExternalTextIsCoveredByButtons(window);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void LearningAndCoverageModules_AreReachableFromPicker()
    {
        var viewModel = new MainWindowViewModel();

        viewModel.ScenarioSelection.SelectLearningCommand.Execute(null);
        Assert.Same(viewModel.Learning, viewModel.CurrentStepViewModel);
        Assert.False(viewModel.IsWizardScenario);

        viewModel.ChangeScenarioCommand.Execute(null);
        viewModel.ScenarioSelection.SelectSourceRequirementsCommand.Execute(null);
        Assert.Same(viewModel.SourceRequirements, viewModel.CurrentStepViewModel);
        Assert.False(viewModel.IsWizardScenario);
    }

    [AvaloniaFact]
    public void EveryCalculationScenario_RendersFormattedEquationsAtMinimumSize()
    {
        foreach (var viewModel in CreateFormulaStepViewModels())
        {
            var window = CreateWindow(viewModel);
            try
            {
                var formulas = window.GetVisualDescendants().OfType<MathView>().ToArray();
                Assert.NotEmpty(formulas);
                Assert.All(formulas, formula => Assert.True(formula.Bounds.Height > 12));
                AssertInsideWindow(window, "FooterBorder");
                AssertNoExternalTextIsCoveredByButtons(window);
            }
            finally
            {
                window.Close();
            }
        }
    }

    [AvaloniaFact]
    public void FinalStep_KeepsBothExportButtonsVisibleWithoutOverlap()
    {
        var viewModel = CreateCrosstalkFormulaStep();
        viewModel.NextStepCommand.Execute(null);
        viewModel.NextStepCommand.Execute(null);
        var window = CreateWindow(viewModel);
        try
        {
            var visibleLabels = window.GetVisualDescendants()
                .OfType<Button>()
                .Where(IsActuallyVisible)
                .Select(button => button.Content?.ToString())
                .ToArray();
            Assert.Contains("Eksportuj CSV", visibleLabels);
            Assert.Contains("Eksportuj DOCX", visibleLabels);
            AssertNoExternalTextIsCoveredByButtons(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static IReadOnlyList<MainWindowViewModel> CreateFormulaStepViewModels()
    {
        var nearField = new MainWindowViewModel();
        nearField.ScenarioSelection.SelectNearFieldCommand.Execute(null);
        nearField.NearFieldStep1.ProbeConnected = true;
        nearField.NearFieldStep1.GeneratorConfigured = true;
        nearField.NearFieldStep1.PowerMeterConfigured = true;
        nearField.NearFieldStep1.MaximumSearchUnderstood = true;
        nearField.NextStepCommand.Execute(null);
        nearField.NearFieldStep2.FillExampleDataCommand.Execute(null);
        nearField.NextStepCommand.Execute(null);

        var radiated = new MainWindowViewModel();
        radiated.ScenarioSelection.SelectRadiatedEmissionCommand.Execute(null);
        radiated.RadiatedEmissionStep1.MeasurementDistanceConfirmed = true;
        radiated.RadiatedEmissionStep1.HorizontalPolarizationConfirmed = true;
        radiated.RadiatedEmissionStep1.VerticalPolarizationConfirmed = true;
        radiated.RadiatedEmissionStep1.UncertaintyBudgetConfirmed = true;
        radiated.NextStepCommand.Execute(null);
        radiated.RadiatedEmissionStep2.FillExampleDataCommand.Execute(null);
        radiated.NextStepCommand.Execute(null);

        var propagation = new MainWindowViewModel();
        propagation.ScenarioSelection.SelectPropagationCommand.Execute(null);
        propagation.PropagationStep1.ReceiverConfigured = true;
        propagation.PropagationStep1.AntennaSetupConfirmed = true;
        propagation.PropagationStep1.GridUnderstood = true;
        propagation.PropagationStep1.MaximumSearchConfirmed = true;
        propagation.NextStepCommand.Execute(null);
        propagation.PropagationStep2.FillExampleDataCommand.Execute(null);
        propagation.NextStepCommand.Execute(null);

        return [CreateCrosstalkFormulaStep(), nearField, radiated, propagation];
    }

    private static MainWindowViewModel CreateCrosstalkFormulaStep()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.ScenarioSelection.SelectCrosstalkCommand.Execute(null);
        viewModel.Step1.SelectedBand = viewModel.Step1.AvailableBands[0];
        viewModel.Step1.FillExampleDataCommand.Execute(null);
        viewModel.NextStepCommand.Execute(null);
        return viewModel;
    }

    private static MainWindow CreateWindow(MainWindowViewModel viewModel)
    {
        var window = new MainWindow
        {
            Width = 820,
            Height = 600,
            DataContext = viewModel,
        };
        window.Show();
        window.UpdateLayout();
        return window;
    }

    private static void AssertInsideWindow(MainWindow window, string controlName)
    {
        var control = window.FindControl<Control>(controlName)
            ?? throw new InvalidOperationException($"Nie znaleziono kontrolki {controlName}.");
        var topLeft = control.TranslatePoint(default, window)
            ?? throw new InvalidOperationException($"Nie można odczytać położenia {controlName}.");
        var rect = new Rect(topLeft, control.Bounds.Size);
        var client = new Rect(window.ClientSize);

        Assert.True(client.Contains(rect.TopLeft), $"{controlName} wychodzi poza lewy lub górny brzeg.");
        Assert.True(rect.Right <= client.Right + 0.5, $"{controlName} wychodzi poza prawy brzeg.");
        Assert.True(rect.Bottom <= client.Bottom + 0.5, $"{controlName} wychodzi poza dolny brzeg.");
    }

    private static void AssertNoExternalTextIsCoveredByButtons(MainWindow window)
    {
        var client = new Rect(window.ClientSize);
        var buttons = window.GetVisualDescendants()
            .OfType<Button>()
            .Where(IsActuallyVisible)
            .Select(button => (Button: button, Rect: GetWindowRect(button, window)))
            .Where(item => item.Rect.Intersects(client))
            .ToArray();

        for (var first = 0; first < buttons.Length; first++)
        {
            for (var second = first + 1; second < buttons.Length; second++)
            {
                Assert.False(
                    buttons[first].Rect.Intersects(buttons[second].Rect),
                    $"Przyciski '{buttons[first].Button.Content}' i '{buttons[second].Button.Content}' nachodzą na siebie.");
            }
        }

        var textBlocks = window.GetVisualDescendants()
            .OfType<TextBlock>()
            .Where(IsActuallyVisible)
            .Where(text => text.FindAncestorOfType<Button>() is null)
            .Select(text => (Text: text, Rect: GetVisibleWindowRect(text, window)))
            .Where(item => item.Rect.Intersects(client) && item.Rect.Width > 1 && item.Rect.Height > 1)
            .ToArray();

        foreach (var button in buttons)
        {
            foreach (var text in textBlocks)
            {
                Assert.False(
                    button.Rect.Intersects(text.Rect),
                    $"Przycisk '{button.Button.Content}' zasłania tekst '{text.Text.Text}'.");
            }
        }
    }

    private static bool IsActuallyVisible(Control control)
        => control.IsVisible && control.Opacity > 0 && control.Bounds.Width > 0 && control.Bounds.Height > 0;

    private static Rect GetWindowRect(Control control, Window window)
    {
        var topLeft = control.TranslatePoint(default, window)
            ?? throw new InvalidOperationException("Nie można przeliczyć położenia kontrolki.");
        return new Rect(topLeft, control.Bounds.Size);
    }

    private static Rect GetVisibleWindowRect(Control control, Window window)
    {
        var visible = GetWindowRect(control, window);
        foreach (var ancestor in control.GetVisualAncestors().OfType<Control>())
        {
            if (!ancestor.ClipToBounds)
                continue;

            visible = Intersect(visible, GetWindowRect(ancestor, window));
            if (visible.Width <= 0 || visible.Height <= 0)
                return default;
        }

        return visible;
    }

    private static Rect Intersect(Rect first, Rect second)
    {
        var left = Math.Max(first.Left, second.Left);
        var top = Math.Max(first.Top, second.Top);
        var right = Math.Min(first.Right, second.Right);
        var bottom = Math.Min(first.Bottom, second.Bottom);
        return right <= left || bottom <= top
            ? default
            : new Rect(left, top, right - left, bottom - top);
    }
}
