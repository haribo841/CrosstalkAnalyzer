# EMC Lab Assistant

Wieloplatformowa aplikacja w C# i Avalonia UI, która prowadzi użytkownika
krok po kroku przez ćwiczenia z kompatybilności elektromagnetycznej.

## Dostępne scenariusze

### 1. Pomiar przeników między liniami mikropaskowymi

- wybór pasma 1–2 GHz, 2–3 GHz albo 7–8 GHz,
- wprowadzanie wartości NEXT i FEXT,
- konwersja `|Z|lin = 10^(|Z|dB / 20)`,
- obliczenie błędu analizatora,
- statystyka, przedziały ufności, wykres i eksport CSV.

### 2. Sondy pola bliskiego w analizie emisji promieniowanej

Scenariusz został przygotowany na podstawie instrukcji ćwiczenia nr 2,
materiału „Obliczenia do pomiarów sondami pola bliskiego”, charakterystyk
sondy i wzmacniacza oraz wzorca sprawozdania.

Kreator obejmuje:

- listę kontrolną generatora, miernika R&S NRP, sondy H 400-1 i wzmacniacza,
- zapis warunków środowiskowych,
- pomiary od 100 MHz do 1000 MHz dla linii 30 Ω, 50 Ω i 100 Ω,
- edytowalne wartości wzmocnienia `K` i poprawki sondy `Sp`,
- obliczenie pola magnetycznego:

  `H[dBA/m] = P[dBm] − 30 + 10·log10(50) − K + Sp`,

- konwersję `H[A/m] = 10^(H[dBA/m]/20)`,
- budżet niepewności:

  `uH = √(uP² + uK² + uSp²)`,

- 95% przedziały z niepewnością rozszerzoną `U = k·uH`,
- porównanie charakterystyk, maksima oraz trendy w dB/100 MHz i dB/dekadę,
- eksport kompletnej tabeli do CSV.

Domyślne wartości to `uP = 0,066 dB`, `uK = 0,2 dB`, `uSp = 0,3 dB`
i `k = 2`, co daje `uH ≈ 0,367 dB` oraz `U ≈ 0,733 dB`.

### 3. Emisja promieniowana — poprawka antenowa EN55032

Scenariusz został przygotowany na podstawie instrukcji ćwiczenia nr 3
„Emisja promieniowana — poprawka antenowa, scenariusz pomiarowy normy
EN55032” oraz przykładowego sprawozdania.

Kreator obejmuje:

- listę kontrolną geometrii pomiaru, dwóch polaryzacji i budżetu niepewności,
- częstotliwości 30 MHz, 50 MHz oraz 100-1000 MHz co 50 MHz,
- wpisywanie surowych wskazań analizatora `MR [dBµV]`, wysokości anteny
  oraz tłumienia kabla `IL [dB]`,
- wyznaczanie poprawki antenowej dipola półfalowego:

  `AF = 20·log10(9,73 / (λ·√G)), G = 1,64`,

- korektę poprawki antenowej dla polaryzacji pionowej na podstawie kąta
  `α = atan(h / d)`,
- obliczenie pola elektrycznego:

  `E[dBµV/m] = MR[dBµV] + AF[dB/m] + IL[dB]`,

- 95% przedział ufności:

  `U_E = √(U_MR² + U_AF² + U_IL²)`,

- porównanie maksymalnej emisji z limitem EN55032 klasy B dla odległości 3 m:
  `40 dBµV/m` dla 30-230 MHz i `47 dBµV/m` dla 230-1000 MHz,
- wykres emisji z przedziałami niepewności i eksport tabeli do CSV.

## Uruchomienie

Wymagany jest .NET 8 SDK:

```powershell
dotnet restore
dotnet run
```

AvaloniaUI & DataGrid:

```CLI
dotnet new install Avalonia.Templates
dotnet add package Avalonia.Controls.DataGrid
```



Test obliczeń, nawigacji i eksportu:

```powershell
dotnet run --project Tests/CrosstalkAnalyzer.CalculationChecks
```

## Publikowanie

Windows x64:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

Linux x64:

```powershell
dotnet publish -c Release -r linux-x64 --self-contained true
```

Wynik znajduje się w katalogu
`bin/Release/net8.0/<identyfikator-systemu>/publish`.

## Zgodność systemowa

Projekt celuje w .NET 8 i Avalonia 12. Jest przeznaczony dla Linuksa oraz
Windows 10/11. Windows 7 i Windows 8.1 nie są oficjalnie obsługiwane przez
wymagany runtime .NET 8.

- [Platformy obsługiwane przez Avalonia](https://docs.avaloniaui.net/docs/overview/supported-platforms)
- [Instalacja i wymagania .NET na Windows](https://learn.microsoft.com/dotnet/core/install/windows)
