using System.Globalization;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.ViewModels;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace CrosstalkAnalyzer.Services;

public static class DocxReportGenerator
{
    private const string Navy = "172B4D";
    private const string Blue = "275DDB";
    private const string LightBlue = "EAF1FF";
    private const string Muted = "59677C";
    private const string Grid = "DDE3EC";
    private static readonly CultureInfo PolishCulture = CultureInfo.GetCultureInfo("pl-PL");

    public static Task WriteAsync(Stream stream, MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(viewModel);

        using var document = WordprocessingDocument.Create(
            stream,
            WordprocessingDocumentType.Document,
            autoSave: true);
        var main = document.AddMainDocumentPart();
        AddStyles(main);
        AddHeader(main);
        AddFooter(main);

        var body = new W.Body();
        main.Document = new W.Document(body);
        AddCover(body, viewModel.ScenarioName);

        switch (viewModel.CurrentScenario)
        {
            case AnalysisScenario.NearFieldProbes:
                AddNearFieldReport(body, viewModel);
                break;
            case AnalysisScenario.RadiatedEmissionAntennaCorrection:
                AddRadiatedEmissionReport(body, viewModel);
                break;
            case AnalysisScenario.PropagationMeasurements:
                AddPropagationReport(body, viewModel);
                break;
            default:
                AddCrosstalkReport(body, viewModel);
                break;
        }

        body.Append(new W.SectionProperties(
            new W.HeaderReference { Type = W.HeaderFooterValues.Default, Id = main.GetIdOfPart(main.HeaderParts.Single()) },
            new W.FooterReference { Type = W.HeaderFooterValues.Default, Id = main.GetIdOfPart(main.FooterParts.Single()) },
            new W.PageSize { Width = 12240, Height = 15840 },
            new W.PageMargin
            {
                Top = 1440,
                Right = 1440,
                Bottom = 1440,
                Left = 1440,
                Header = 708,
                Footer = 708,
                Gutter = 0,
            }));

        main.Document.Save();
        return Task.CompletedTask;
    }

    private static void AddCover(W.Body body, string scenarioName)
    {
        body.Append(Paragraph("KOMPATYBILNOŚĆ ELEKTROMAGNETYCZNA", "Subtitle", W.JustificationValues.Center));
        body.Append(Paragraph("Raport z ćwiczenia laboratoryjnego", "Title", W.JustificationValues.Center));
        body.Append(Paragraph(scenarioName, "Subtitle", W.JustificationValues.Center));
        body.Append(Paragraph($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm}", "Metadata", W.JustificationValues.Center));
        body.Append(Paragraph("Program: EMC Lab Assistant", "Metadata", W.JustificationValues.Center));
        body.Append(new W.Paragraph(new W.Run(new W.Break { Type = W.BreakValues.Page })));
    }

    private static void AddCrosstalkReport(W.Body body, MainWindowViewModel viewModel)
    {
        var results = viewModel.Step4.Results;
        Heading(body, "1. Dane badania");
        KeyValue(body, "Pasmo", viewModel.Step4.BandName);
        KeyValue(body, "Liczba punktów", results.Count.ToString(PolishCulture));

        Heading(body, "2. Zastosowane równania");
        Formula(body, @"|Z|lin = 10^(|Z|dB / 20)");
        Formula(body, @"ΔZ = |Z|lin · (10^(UD / 20) - 1)");
        Formula(body, @"CI95 = średnia ± t · s / √n");

        Heading(body, "3. Wyniki punktowe");
        body.Append(Table(
            ["f [GHz]", "NEXT [dB]", "FEXT [dB]", "U NEXT [dB]", "U FEXT [dB]", "NEXT lin", "FEXT lin"],
            results.Select(row => new[]
            {
                F(row.FrequencyGHz), F(row.NearDb), F(row.FarDb),
                F(row.NearAnalyzerUncertaintyDb), F(row.FarAnalyzerUncertaintyDb),
                F(row.NearLinear), F(row.FarLinear),
            }),
            [900, 1250, 1250, 1450, 1450, 1530, 1530]));

        Heading(body, "4. Statystyka");
        body.Append(Table(
            ["Seria", "N", "Średnia", "s", "SE", "CI95 dolny", "CI95 górny"],
            viewModel.Step4.Statistics.Select(row => new[]
            {
                row.SeriesName, row.Count.ToString(PolishCulture), F(row.Mean),
                F(row.StandardDeviation), F(row.StandardError),
                F(row.ConfidenceLower), F(row.ConfidenceUpper),
            }),
            [1200, 650, 1500, 1300, 1300, 1705, 1705]));

        AddConclusions(body);
    }

    private static void AddNearFieldReport(W.Body body, MainWindowViewModel viewModel)
    {
        var setup = viewModel.NearFieldStep1;
        var results = viewModel.NearFieldStep4.Results;
        Heading(body, "1. Warunki i niepewność");
        KeyValue(body, "Temperatura", NullableF(setup.TemperatureCelsius, "°C"));
        KeyValue(body, "Wilgotność", NullableF(setup.HumidityPercent, "%"));
        KeyValue(body, "Ciśnienie", NullableF(setup.PressureHpa, "hPa"));
        KeyValue(body, "Składniki standardowe", $"uP={F(setup.PowerMeterUncertaintyDb)} dB; uK={F(setup.AmplifierUncertaintyDb)} dB; uSp={F(setup.ProbeUncertaintyDb)} dB; uRep={F(setup.RepeatabilityUncertaintyDb)} dB");
        KeyValue(body, "Współczynnik rozszerzenia", F(setup.CoverageFactor));

        Heading(body, "2. Zastosowane równania");
        Formula(body, @"H[dBA/m] = P[dBm] - 30 + 10·log10(50) - K + Sp");
        Formula(body, @"H[A/m] = 10^(H[dBA/m] / 20)");
        Formula(body, @"U95 = k · √(uP² + uK² + uSp² + uRep²)");

        Heading(body, "3. Wyniki pola magnetycznego");
        body.Append(Table(
            ["f [MHz]", "H 30 Ω [dBA/m]", "H 50 Ω [dBA/m]", "H 100 Ω [dBA/m]", "U95 [dB]"],
            results.Select(row => new[]
            {
                F(row.FrequencyMHz), F(row.H30OhmDbAm), F(row.H50OhmDbAm),
                F(row.H100OhmDbAm), F(row.ExpandedUncertaintyDb),
            }),
            [1400, 1990, 1990, 1990, 1990]));

        Heading(body, "4. Podsumowanie serii");
        body.Append(Table(
            ["Seria", "Maksimum H [dBA/m]", "f maksimum [MHz]", "Trend [dB/100 MHz]", "Trend [dB/dekadę]"],
            viewModel.NearFieldStep4.Summaries.Select(row => new[]
            {
                row.SeriesName, F(row.PeakLevelDbAm), F(row.PeakFrequencyMHz),
                F(row.TrendDbPer100MHz), F(row.TrendDbPerDecade),
            }),
            [1800, 1890, 1890, 1890, 1890]));

        Heading(body, "5. Analiza nagrania");
        KeyValue(body, "Obserwacje", EmptyAsPlaceholder(viewModel.NearFieldStep4.VideoObservations));
        KeyValue(body, "Wnioski", EmptyAsPlaceholder(viewModel.NearFieldStep4.VideoConclusions));
        AddConclusions(body, 6);
    }

    private static void AddRadiatedEmissionReport(W.Body body, MainWindowViewModel viewModel)
    {
        var setup = viewModel.RadiatedEmissionStep1;
        var summary = viewModel.RadiatedEmissionStep4.Summary;
        Heading(body, "1. Dane badania");
        KeyValue(body, "Odległość pomiarowa", $"{F(setup.MeasurementDistanceMeters)} m");
        KeyValue(body, "Niepewność rozszerzona", $"{F(summary.ExpandedUncertaintyDb)} dB");
        KeyValue(body, "Liczba przekroczeń", summary.ExceedanceCountText);

        Heading(body, "2. Zastosowane równania");
        Formula(body, @"AF = 20·log10(9,73 / (λ·√G))");
        Formula(body, @"E[dBµV/m] = MR[dBµV] + AF[dB/m] + IL[dB]");
        Formula(body, @"UE = √(UMR² + UAF² + UIL²)");

        Heading(body, "3. Wyniki i ocena EN 55032");
        body.Append(Table(
            ["f [MHz]", "E H", "E V", "E max", "CI95 dolny", "CI95 górny", "Limit", "Ocena"],
            viewModel.RadiatedEmissionStep4.Results.Select(row => new[]
            {
                F(row.FrequencyMHz), F(row.HorizontalFieldDbuvPerM), F(row.VerticalFieldDbuvPerM),
                F(row.MaxFieldDbuvPerM), F(row.LowerConfidenceLimitDbuvPerM),
                F(row.UpperConfidenceLimitDbuvPerM), F(row.LimitDbuvPerM), row.VerdictText,
            }),
            [900, 1040, 1040, 1040, 1250, 1250, 1040, 1800]));

        Heading(body, "4. Wynik końcowy");
        KeyValue(body, "Częstotliwość krytyczna", summary.WorstFrequencyText);
        KeyValue(body, "Największy margines", summary.WorstMarginText);
        KeyValue(body, "Ocena", summary.VerdictText);
        AddConclusions(body, 5);
    }

    private static void AddPropagationReport(W.Body body, MainWindowViewModel viewModel)
    {
        var setup = viewModel.PropagationStep1;
        Heading(body, "1. Dane badania");
        KeyValue(body, "Częstotliwość", $"{F(setup.FrequencyMHz)} MHz");
        KeyValue(body, "Profil anteny", setup.SelectedAntennaFactorProfile.Name);
        KeyValue(body, "AF", $"{F(setup.AntennaFactorDb)} dB/m");
        KeyValue(body, "Konwencja odczytu", setup.SelectedInputConvention.Name);

        Heading(body, "2. Zastosowane równania");
        Formula(body, @"AF[1/m] = 10^(AF[dB/m] / 20)");
        Formula(body, @"E[µV/m] = U[µV] · AF[1/m] · 10^(ac / 20)");
        Formula(body, @"E[dBµV/m] = 20·log10(E[µV/m])");

        Heading(body, "3. Wyniki w siatce pomiarowej");
        body.Append(Table(
            ["Punkt", "L H [dB]", "E H [dBµV/m]", "L V [dB]", "E V [dBµV/m]", "Silniejsza polaryzacja"],
            viewModel.PropagationStep4.Results.Select(row => new[]
            {
                row.PointNumber.ToString(PolishCulture), F(row.HorizontalLevelDb),
                F(row.HorizontalFieldDbuvPerM), F(row.VerticalLevelDb),
                F(row.VerticalFieldDbuvPerM), row.StrongerPolarizationText,
            }),
            [850, 1350, 1800, 1350, 1800, 2210]));

        Heading(body, "4. Wynik końcowy");
        body.Append(Table(
            ["Polaryzacja", "Eav [dBµV/m]", "uE [µV/m]", "T [µV/m]", "Przedział Eav ± T [µV/m]", "Punkt max"],
            viewModel.PropagationStep4.Summaries.Select(row => new[]
            {
                row.PolarizationName, F(row.MeanFieldDbuvPerM), F(row.FieldStandardUncertaintyUvPerM),
                F(row.ToleranceUvPerM), $"{F(row.LowerFieldUvPerM)} - {F(row.UpperFieldUvPerM)}",
                row.PeakPointNumber.ToString(PolishCulture),
            }),
            [1500, 1650, 1450, 1350, 2410, 1000]));
        AddConclusions(body, 5);
    }

    private static void AddConclusions(W.Body body, int sectionNumber = 5)
    {
        Heading(body, $"{sectionNumber}. Wnioski");
        body.Append(Paragraph("Miejsce na interpretację wyników, ocenę wpływu niepewności oraz odniesienie do celu ćwiczenia.", "Normal"));
    }

    private static void Heading(W.Body body, string text)
        => body.Append(Paragraph(text, "Heading1"));

    private static void KeyValue(W.Body body, string label, string value)
    {
        var paragraph = new W.Paragraph(new W.ParagraphProperties(
            new W.ParagraphStyleId { Val = "Normal" },
            new W.SpacingBetweenLines { After = "120" }));
        paragraph.Append(new W.Run(
            new W.RunProperties(new W.Bold(), new W.Color { Val = Navy }),
            new W.Text(label + ": ")));
        paragraph.Append(new W.Run(new W.Text(value) { Space = SpaceProcessingModeValues.Preserve }));
        body.Append(paragraph);
    }

    private static void Formula(W.Body body, string formula)
    {
        body.Append(new W.Paragraph(
            new W.ParagraphProperties(
                new W.ParagraphStyleId { Val = "Formula" },
                new W.Shading { Fill = LightBlue, Val = W.ShadingPatternValues.Clear },
                new W.SpacingBetweenLines { Before = "100", After = "100" },
                new W.Justification { Val = W.JustificationValues.Center }),
            new W.Run(
                new W.RunProperties(
                    new W.RunFonts { Ascii = "Cambria Math", HighAnsi = "Cambria Math" },
                    new W.Color { Val = Navy },
                    new W.FontSize { Val = "24" }),
                new W.Text(formula))));
    }

    private static W.Table Table(
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<string>> rows,
        IReadOnlyList<int> widths)
    {
        var table = new W.Table();
        table.Append(new W.TableProperties(
            new W.TableWidth { Width = "9360", Type = W.TableWidthUnitValues.Dxa },
            new W.TableIndentation { Width = 120, Type = W.TableWidthUnitValues.Dxa },
            new W.TableBorders(
                Border<W.TopBorder>(), Border<W.LeftBorder>(), Border<W.BottomBorder>(),
                Border<W.RightBorder>(), Border<W.InsideHorizontalBorder>(), Border<W.InsideVerticalBorder>()),
            new W.TableLayout { Type = W.TableLayoutValues.Fixed },
            new W.TableCellMarginDefault(
                new W.TopMargin { Width = "80", Type = W.TableWidthUnitValues.Dxa },
                new W.TableCellLeftMargin { Width = 120, Type = W.TableWidthValues.Dxa },
                new W.BottomMargin { Width = "80", Type = W.TableWidthUnitValues.Dxa },
                new W.TableCellRightMargin { Width = 120, Type = W.TableWidthValues.Dxa })));
        table.Append(new W.TableGrid(widths.Select(width => new W.GridColumn { Width = width.ToString(PolishCulture) })));

        var headerRow = new W.TableRow(new W.TableRowProperties(new W.TableHeader()));
        for (var index = 0; index < headers.Count; index++)
            headerRow.Append(Cell(headers[index], widths[index], header: true));
        table.Append(headerRow);

        foreach (var row in rows)
        {
            var tableRow = new W.TableRow();
            for (var index = 0; index < headers.Count; index++)
                tableRow.Append(Cell(index < row.Count ? row[index] : string.Empty, widths[index], header: false));
            table.Append(tableRow);
        }

        return table;
    }

    private static W.TableCell Cell(string text, int width, bool header)
    {
        var runProperties = new W.RunProperties(
            new W.RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" });
        if (header)
            runProperties.Append(new W.Bold());
        runProperties.Append(
            new W.Color { Val = header ? "FFFFFF" : "172033" },
            new W.FontSize { Val = header ? "18" : "17" });

        var paragraph = new W.Paragraph(
            new W.ParagraphProperties(
                new W.SpacingBetweenLines { Before = "0", After = "0", Line = "240", LineRule = W.LineSpacingRuleValues.Auto },
                new W.Justification { Val = W.JustificationValues.Center }),
            new W.Run(runProperties, new W.Text(text)));
        var properties = new W.TableCellProperties(
            new W.TableCellWidth { Width = width.ToString(PolishCulture), Type = W.TableWidthUnitValues.Dxa });
        if (header)
            properties.Append(new W.Shading { Fill = Navy, Val = W.ShadingPatternValues.Clear });
        properties.Append(new W.TableCellVerticalAlignment { Val = W.TableVerticalAlignmentValues.Center });
        return new W.TableCell(properties, paragraph);
    }

    private static T Border<T>() where T : W.BorderType, new()
        => new()
        {
            Val = W.BorderValues.Single,
            Color = Grid,
            Size = 4,
            Space = 0,
        };

    private static W.Paragraph Paragraph(
        string text,
        string style,
        W.JustificationValues? justification = null)
    {
        var properties = new W.ParagraphProperties(new W.ParagraphStyleId { Val = style });
        if (justification.HasValue)
            properties.Append(new W.Justification { Val = justification.Value });
        return new W.Paragraph(properties, new W.Run(new W.Text(text)));
    }

    private static void AddStyles(MainDocumentPart main)
    {
        var part = main.AddNewPart<StyleDefinitionsPart>();
        part.Styles = new W.Styles(
            Style("Normal", "Normal", "Calibri", 22, "172033", after: 120, line: 264),
            Style("Title", "Title", "Calibri Light", 48, Navy, bold: true, after: 200),
            Style("Subtitle", "Subtitle", "Calibri", 26, Blue, after: 160),
            Style("Metadata", "Metadata", "Calibri", 20, Muted, after: 80),
            Style("Heading1", "Heading 1", "Calibri Light", 32, Blue, bold: true, before: 320, after: 160, outlineLevel: 0),
            Style("Formula", "Formula", "Cambria Math", 24, Navy, after: 120));
        part.Styles.Save();
    }

    private static W.Style Style(
        string id,
        string name,
        string font,
        int size,
        string color,
        bool bold = false,
        int before = 0,
        int after = 0,
        int? line = null,
        int? outlineLevel = null)
    {
        var style = new W.Style { Type = W.StyleValues.Paragraph, StyleId = id, Default = id == "Normal" };
        style.Append(new W.StyleName { Val = name });
        var spacing = new W.SpacingBetweenLines
        {
            Before = before.ToString(PolishCulture),
            After = after.ToString(PolishCulture),
        };
        if (line.HasValue)
        {
            spacing.Line = line.Value.ToString(PolishCulture);
            spacing.LineRule = W.LineSpacingRuleValues.Auto;
        }

        var paragraphProperties = new W.StyleParagraphProperties(spacing);
        if (outlineLevel.HasValue)
            paragraphProperties.Append(new W.OutlineLevel { Val = outlineLevel.Value });
        style.Append(paragraphProperties);
        var runProperties = new W.StyleRunProperties(
            new W.RunFonts { Ascii = font, HighAnsi = font });
        if (bold)
            runProperties.Append(new W.Bold());
        runProperties.Append(
            new W.Color { Val = color },
            new W.FontSize { Val = size.ToString(PolishCulture) });
        style.Append(runProperties);
        return style;
    }

    private static void AddHeader(MainDocumentPart main)
    {
        var part = main.AddNewPart<HeaderPart>();
        part.Header = new W.Header(new W.Paragraph(
            new W.ParagraphProperties(new W.Justification { Val = W.JustificationValues.Left }),
            new W.Run(
                new W.RunProperties(new W.Color { Val = Muted }, new W.FontSize { Val = "17" }),
                new W.Text("EMC Lab Assistant | raport laboratoryjny"))));
        part.Header.Save();
    }

    private static void AddFooter(MainDocumentPart main)
    {
        var part = main.AddNewPart<FooterPart>();
        var paragraph = new W.Paragraph(new W.ParagraphProperties(
            new W.Justification { Val = W.JustificationValues.Right }));
        paragraph.Append(new W.Run(
            new W.RunProperties(new W.Color { Val = Muted }, new W.FontSize { Val = "17" }),
            new W.Text("Strona ")));
        paragraph.Append(new W.Run(
            new W.FieldChar { FieldCharType = W.FieldCharValues.Begin },
            new W.FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve },
            new W.FieldChar { FieldCharType = W.FieldCharValues.End }));
        part.Footer = new W.Footer(paragraph);
        part.Footer.Save();
    }

    private static string F(double value)
        => value.ToString("0.####", PolishCulture);

    private static string NullableF(double? value, string unit)
        => value.HasValue ? $"{F(value.Value)} {unit}" : "nie podano";

    private static string EmptyAsPlaceholder(string value)
        => string.IsNullOrWhiteSpace(value) ? "Do uzupełnienia przez autora sprawozdania." : value.Trim();
}
