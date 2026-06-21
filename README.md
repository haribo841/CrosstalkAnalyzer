# Crosstalk Analyzer

Wieloplatformowa aplikacja w C# i Avalonia UI, która prowadzi użytkownika
krok po kroku przez analizę przeników między liniami mikropaskowymi.

## Zakres programu

1. Wybór pasma 1–2 GHz, 2–3 GHz albo 7–8 GHz i wprowadzenie wartości
   przeniku bliskiego (NEXT) oraz dalekiego (FEXT).
2. Konwersja z dB do skali liniowej:
   `|Z|lin = 10^(|Z|dB / 20)`.
3. Obliczenie błędu analizatora:
   `ΔZ = |Z|lin · (10^(U_D / 20) − 1)`.
4. Obliczenie statystyk, 95% przedziału ufności średniej, prezentacja
   wykresu i eksport pełnego zestawienia do CSV.

Dla pasm 1–2 GHz i 2–3 GHz przyjęto `U_D = 0,2 dB`, a dla pasma
7–8 GHz `U_D = 0,3 dB`. Wartości te są założeniem projektu i przed
oddaniem sprawozdania należy je porównać z tabelą dokładności używanego
egzemplarza analizatora R&S ZVL-13.

## Założenie statystyczne

Odchylenie standardowe `s` jest liczone osobno z 11 punktów serii NEXT
i FEXT w całym wybranym paśmie. Przedział ufności dotyczy średniej serii:

`x̄ ± t(0,975; n−1) · s/√n`

Dla 11 punktów program stosuje wartość krytyczną rozkładu t-Studenta
równą 2,228.

## Uruchomienie

Wymagany jest zestaw .NET 8 SDK:

```powershell
dotnet restore
dotnet run
```

Test obliczeń i eksportu:

```powershell
dotnet run --project Tests/CrosstalkAnalyzer.CalculationChecks
```

## Publikowanie

Samowystarczalna paczka dla 64-bitowego Windows:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

Samowystarczalna paczka dla 64-bitowego Linuksa:

```powershell
dotnet publish -c Release -r linux-x64 --self-contained true
```

Wynik znajduje się w katalogu
`bin/Release/net8.0/<identyfikator-systemu>/publish`.

## Zgodność systemowa

Projekt celuje w .NET 8 i Avalonia 12. Jest przeznaczony dla Linuksa
oraz Windows 10/11. Windows 7 i Windows 8.1 nie są oficjalnie obsługiwane
przez wymagany runtime .NET 8.

- [Platformy obsługiwane przez Avalonia](https://docs.avaloniaui.net/docs/overview/supported-platforms)
- [Instalacja i wymagania .NET na Windows](https://learn.microsoft.com/dotnet/core/install/windows)
