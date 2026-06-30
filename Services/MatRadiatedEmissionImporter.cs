using CrosstalkAnalyzer.Models;
using MatFileHandler;

namespace CrosstalkAnalyzer.Services;

public static class MatRadiatedEmissionImporter
{
    public static IReadOnlyList<RadiatedEmissionImportRow> Read(Stream stream)
    {
        var matFile = new MatFileReader(stream).Read();
        var dataVariable = matFile["Data"] ??
            throw new FormatException("Plik MAT nie zawiera zmiennej Data.");

        if (dataVariable.Value is not IStructureArray data)
            throw new FormatException("Zmienna Data w pliku MAT nie jest strukturą MATLAB.");

        var horizontal = ReadField(data, "Pomiar_H");
        var vertical = ReadField(data, "Pomiar_V");
        var horizontalHeight = ReadField(data, "H2_H");
        var verticalHeight = ReadField(data, "H2_VH");
        var cableLoss = ReadField(data, "IL");
        var frequencies = DefaultFrequencies();

        var expectedCount = frequencies.Length;
        var arrays = new[] { horizontal, vertical, horizontalHeight, verticalHeight, cableLoss };
        if (arrays.Any(values => values.Length != expectedCount))
        {
            throw new FormatException(
                $"Plik MAT powinien zawierać {expectedCount} wartości w każdym polu Data.");
        }

        return Enumerable.Range(0, expectedCount)
            .Select(index => new RadiatedEmissionImportRow(
                frequencies[index],
                cableLoss[index],
                horizontal[index],
                horizontalHeight[index],
                vertical[index],
                verticalHeight[index]))
            .ToArray();
    }

    private static double[] ReadField(IStructureArray structure, string fieldName)
    {
        if (!structure.FieldNames.Contains(fieldName))
            throw new FormatException($"Struktura Data nie zawiera pola {fieldName}.");

        var values = structure[fieldName, 0, 0].ConvertToDoubleArray();
        return values ??
            throw new FormatException($"Pola Data.{fieldName} nie można przeliczyć na liczby.");
    }

    private static double[] DefaultFrequencies()
        => [30, 50, .. Enumerable.Range(0, 19).Select(index => 100.0 + 50 * index)];
}
