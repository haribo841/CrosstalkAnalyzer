using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CrosstalkAnalyzer.Services;
using CrosstalkAnalyzer.ViewModels;

namespace CrosstalkAnalyzer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ExportCsv_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Zapisz tabelę wyników",
            SuggestedFileName = $"przeniki_{DateTime.Now:yyyyMMdd_HHmm}.csv",
            DefaultExtension = "csv",
            FileTypeChoices =
            [
                new FilePickerFileType("Plik CSV")
                {
                    Patterns = ["*.csv"],
                    MimeTypes = ["text/csv"],
                },
            ],
        });

        if (file is null)
            return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);
            await ReportGenerator.WriteCsvAsync(
                stream,
                viewModel.Step4.BandName,
                viewModel.Step4.Results,
                viewModel.Step4.Statistics);
            ExportStatusText.Foreground =
                new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#28734A"));
            ExportStatusText.Text = "Zapisano plik CSV z wynikami i statystyką.";
        }
        catch (Exception exception)
        {
            ExportStatusText.Foreground =
                new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#B42318"));
            ExportStatusText.Text = $"Nie udało się zapisać pliku: {exception.Message}";
        }
    }
}
