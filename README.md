# EMC Lab Assistant

Wieloplatformowa aplikacja C# i Avalonia UI prowadząca użytkownika przez
ćwiczenia z kompatybilności elektromagnetycznej. Interfejs działa jako kreator,
waliduje dane, składa równania matematyczne, wykonuje obliczenia i przygotowuje
wyniki do sprawozdania.

## Moduły programu

### 1. Pomiar przeników między liniami mikropaskowymi

- pasma 1-2 GHz, 2-3 GHz oraz 7-8 GHz,
- import 11 punktów z CSV/TXT,
- niezależne niepewności NEXT i FEXT,
- konwersja `|Z|lin = 10^(|Z|dB / 20)`,
- błąd analizatora i granice wyniku,
- statystyka, 95% przedziały ufności i wykres.

### 2. Sondy pola bliskiego w analizie emisji promieniowanej

- lista kontrolna generatora, miernika R&S NRP, sondy i wzmacniacza,
- warunki środowiskowe,
- import CSV/TXT dla 100-1000 MHz,
- pomiary linii 30 Ω, 50 Ω i 100 Ω,
- edytowalne charakterystyki `K` i `Sp`,
- pole magnetyczne w dBA/m i A/m,
- budżet `uP`, `uK`, `uSp`, `uRep` oraz `U95 = k * uH`,
- maksima, trendy, wykres i analiza dodatkowego nagrania.

### 3. Emisja promieniowana - poprawka antenowa EN 55032

- import CSV/TXT i plików MATLAB z oryginalną strukturą `Data`,
- wskazania `MR`, tłumienie `IL` i wysokości anteny,
- poprawka antenowa i korekta polaryzacji pionowej,
- pole E dla obu polaryzacji,
- budżet niepewności, limity klasy B i margines zgodności.

### 4. Pomiary propagacyjne DVB-T

- siatka 16 punktów dla polaryzacji poziomej i pionowej,
- import CSV/TXT,
- profile AF anteny UHALP 9108 A1 z interpolacją,
- tryb ręcznego AF,
- trzy konwencje wejścia: historyczna, dBµV oraz dBm/50 Ω,
- średnia przestrzenna, niepewność i tolerancja `Eav ± T`,
- wykres oraz mapy cieplne 4 x 4.

Scenariusz nr 4 jest oparty na sprawozdaniach i rekomendacjach ITU-R. Wymaga
zatwierdzenia względem oryginalnej instrukcji prowadzącego, której nie było w
audytowanym katalogu.

### 5. Nauka

Siedem bloków tematycznych obejmuje mechanizmy zakłóceń, przesłuchy,
uziemianie, ekranowanie, normy, aparaturę i dopasowanie. Moduł zawiera pytania
kontrolne oraz kalkulatory długości fali, prądu pojemnościowego i napięcia
indukowanego.

### 6. Pokrycie materiału

Program jawnie wskazuje elementy, których nie uznano za wdrożone bez źródeł:

- pomiary pola dla ochrony środowiska,
- badanie analizatora widma i pomiar promieniowania,
- zatwierdzenie instrukcji pomiarów propagacyjnych,
- wykłady cz02 i cz07.

Szczegóły audytu: [Documentation/COURSE_COVERAGE.md](Documentation/COURSE_COVERAGE.md).

## Interfejs i wzory

- minimalny rozmiar okna: 820 x 600,
- pionowe przewijanie treści i przewijanie tabel,
- formularze zawijające pola przy mniejszej szerokości,
- osobny obszar statusu eksportu,
- równania składane przez CSharpMath zamiast surowego tekstu,
- testy headless wykrywające kolizje przycisków z tekstem.

## Import i eksport

CSV/TXT może używać średnika, tabulatora albo przecinka. Parser obsługuje
polski i niezmienny format liczb. Scenariusz nr 3 odczytuje dodatkowo MATLAB 5.

Na ostatnim kroku dostępne są:

- `Eksportuj CSV` - dane surowe, pośrednie i podsumowanie,
- `Eksportuj DOCX` - raport z metadanymi, równaniami, tabelami i miejscem na
  wnioski.

## Budowanie i testy

Wymagany jest .NET 8 SDK.

```powershell
dotnet restore CrosstalkAnalyzer.sln
dotnet build CrosstalkAnalyzer.sln -c Release
dotnet run --project CrosstalkAnalyzer.csproj
```

Testy obliczeń, importu, eksportu i nawigacji:

```powershell
dotnet run --project Tests/CrosstalkAnalyzer.CalculationChecks
```

Testy układu Avalonia w trybie headless:

```powershell
dotnet run --project Tests/CrosstalkAnalyzer.UiTests
```

## Publikowanie

Windows x64:

```powershell
dotnet publish CrosstalkAnalyzer.csproj -c Release -r win-x64 --self-contained true
```

Linux x64:

```powershell
dotnet publish CrosstalkAnalyzer.csproj -c Release -r linux-x64 --self-contained true
```

Projekt celuje w .NET 8 i Avalonia 12. Jest przeznaczony dla Windows 10/11
oraz Linux x64. Windows 7 i Windows 8.1 nie są oficjalnie wspierane przez
runtime .NET 8.
