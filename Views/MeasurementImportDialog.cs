using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace CrosstalkAnalyzer.Views;

internal static class MeasurementImportDialog
{
    public static async Task<IStorageFile?> PickAsync(Control owner, bool includeMat = false)
    {
        var topLevel = TopLevel.GetTopLevel(owner);
        if (topLevel?.StorageProvider is null)
            return null;

        var fileTypes = new List<FilePickerFileType>
        {
            new("Dane pomiarowe CSV")
            {
                Patterns = ["*.csv", "*.txt"],
                MimeTypes = ["text/csv", "text/plain"],
            },
        };

        if (includeMat)
        {
            fileTypes.Insert(0, new FilePickerFileType("Dane MATLAB 5")
            {
                Patterns = ["*.mat"],
                MimeTypes = ["application/octet-stream"],
            });
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Wybierz plik z danymi pomiarowymi",
            AllowMultiple = false,
            FileTypeFilter = fileTypes,
        });

        return files.FirstOrDefault();
    }
}
