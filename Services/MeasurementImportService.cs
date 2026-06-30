using System.Globalization;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Services;

public static class MeasurementImportService
{
    private static readonly CultureInfo PolishCulture = CultureInfo.GetCultureInfo("pl-PL");

    public static IReadOnlyList<CrosstalkImportRow> ReadCrosstalkCsv(TextReader reader)
        => ReadNumericRows(reader, 3)
            .Select(values => new CrosstalkImportRow(
                values[0] > 20 ? values[0] / 1000.0 : values[0],
                values[1],
                values[2]))
            .ToArray();

    public static IReadOnlyList<NearFieldImportRow> ReadNearFieldCsv(TextReader reader)
        => ReadNumericRows(reader, 6)
            .Select(values => new NearFieldImportRow(
                values[0], values[1], values[2], values[3], values[4], values[5]))
            .ToArray();

    public static IReadOnlyList<RadiatedEmissionImportRow> ReadRadiatedEmissionCsv(TextReader reader)
        => ReadNumericRows(reader, 6)
            .Select(values => new RadiatedEmissionImportRow(
                values[0], values[1], values[2], values[3], values[4], values[5]))
            .ToArray();

    public static IReadOnlyList<PropagationImportRow> ReadPropagationCsv(TextReader reader)
        => ReadNumericRows(reader, 3)
            .Select(values => new PropagationImportRow(
                checked((int)Math.Round(values[0])),
                values[1],
                values[2]))
            .ToArray();

    private static IReadOnlyList<double[]> ReadNumericRows(TextReader reader, int minimumColumnCount)
    {
        var result = new List<double[]>();
        string? line;
        var lineNumber = 0;

        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            line = line.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            var separator = DetectSeparator(line);
            var fields = line.Split(separator)
                .Select(field => field.Trim().Trim('"'))
                .ToArray();

            if (fields.Length < minimumColumnCount)
            {
                if (result.Count == 0 && fields.Any(field => field.Any(char.IsLetter)))
                    continue;

                throw new FormatException(
                    $"Wiersz {lineNumber} ma {fields.Length} kolumn, wymagane jest co najmniej {minimumColumnCount}.");
            }

            if (!TryParseNumber(fields[0], out _))
            {
                if (result.Count == 0)
                    continue;

                throw new FormatException($"Nieprawidłowa liczba w wierszu {lineNumber}: {fields[0]}.");
            }

            var numbers = new double[minimumColumnCount];
            for (var column = 0; column < minimumColumnCount; column++)
            {
                if (!TryParseNumber(fields[column], out numbers[column]) ||
                    !double.IsFinite(numbers[column]))
                {
                    throw new FormatException(
                        $"Nieprawidłowa liczba w wierszu {lineNumber}, kolumna {column + 1}: {fields[column]}.");
                }
            }

            result.Add(numbers);
        }

        if (result.Count == 0)
            throw new FormatException("Plik nie zawiera żadnego poprawnego wiersza pomiarowego.");

        return result;
    }

    private static char DetectSeparator(string line)
    {
        if (line.Contains(';'))
            return ';';
        if (line.Contains('\t'))
            return '\t';
        return ',';
    }

    private static bool TryParseNumber(string text, out double value)
        => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
           double.TryParse(text, NumberStyles.Float, PolishCulture, out value) ||
           double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
}
