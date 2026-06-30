# Rejestr pokrycia przedmiotu

Data audytu: 30 czerwca 2026 r.

## Zakres audytu

Sprawdzono katalog:

`C:\Studia\StudiaMagisterskie\3semestr\Kompatybilność elektromagnetyczna`

Inwentaryzacja wykazała 183 pliki. Z zakresu aplikacji EMC wyłączono:

- 19 plików istniejących projektów `Project` i `ProjectV1`, ponieważ są
  wariantami kodu, a nie materiałem merytorycznym przedmiotu,
- 47 plików osobnego przedmiotu Teoria Pola Elektromagnetycznego.

Do oceny pokrycia pozostawiono 117 plików. Pokrycie jest oceniane według
obszarów merytorycznych, ponieważ pojedynczy scenariusz korzysta równocześnie z
instrukcji, sprawozdań, kart aparatury i danych pomiarowych.

## Wynik

| Obszar | Stan | Realizacja w programie | Brak lub warunek |
|---|---|---|---|
| Przeniki linii mikropaskowych | wdrożony | kreator 4 kroków, osobne U NEXT/U FEXT, import CSV, statystyka, CSV/DOCX | wartości niepewności należy potwierdzić dla poziomu i pasma analizatora |
| Sondy pola bliskiego nr 2 | wdrożony | stanowisko, H dla 30/50/100 Ω, K, Sp, uRep, wykres, analiza nagrania, CSV/DOCX | brak automatycznego odczytu charakterystyk z plików SVG |
| Emisja promieniowana EN 55032 nr 3 | wdrożony | import MATLAB/CSV, AF, korekta pionowa, niepewność, limit i margines | wersję normy i klasę urządzenia trzeba potwierdzić przed badaniem |
| Pomiary propagacyjne nr 4 | wdrożony warunkowo | 16 punktów, 3 konwencje danych, UHALP 9108, wykres i mapy cieplne | brak oryginalnej instrukcji prowadzącego, wymagane zatwierdzenie wzorów i jednostek |
| Pomiary pola dla ochrony środowiska | niewdrożony | karta wymagań źródłowych | brak procedury, aparatury, zakresów, limitów i budżetu niepewności |
| Badanie analizatora widma i pomiar promieniowania | niewdrożony | karta wymagań źródłowych | brak scenariusza stanowiska, RBW/VBW, detektorów, sygnałów i kryteriów wyniku |
| Wykłady cz01, cz03-cz06, cz08 | wdrożone dydaktycznie | moduł Nauka, równania, pytania i kalkulatory | jest to opracowanie pomocnicze, nie kopia slajdów |
| Wykłady cz02 i cz07 | niewdrożone | karta wymagań źródłowych | plików nie było w audytowanym katalogu |
| Polecenia i pytania do opracowania | wdrożone częściowo | siedem pytań kontrolnych i wskazówki odpowiedzi | pełne odpowiedzi należy porównać z wymaganiami prowadzącego |
| Instrukcje aparatury i karty katalogowe | wykorzystane | ZVL, FSH, NRP, sondy, wzmacniacz, UHALP | instrukcja urządzenia nie zastępuje instrukcji ćwiczenia |
| Normy i niepewność pomiaru | wykorzystane częściowo | budżety niepewności, limity EN 55032, reguły oceny | przed zaliczeniem trzeba potwierdzić aktualne wydanie normy i regułę decyzyjną |

## Zasada bezpieczeństwa merytorycznego

Program nie tworzy brakującej procedury laboratoryjnej na podstawie samej
nazwy ćwiczenia lub instrukcji obsługi urządzenia. Pozycje bez wystarczającego
źródła są pokazane jako oczekujące. Ich odblokowanie wymaga materiału od
prowadzącego i przeglądu wzorów, jednostek, walidacji oraz raportu.

## Kolejność uzupełniania braków

1. Pozyskać instrukcję pomiarów pola dla ochrony środowiska.
2. Pozyskać instrukcję badania analizatora widma i pomiaru promieniowania.
3. Pozyskać oryginalną instrukcję pomiarów propagacyjnych nr 4 i wykonać
   porównanie punkt po punkcie z obecnym kreatorem.
4. Pozyskać wykłady cz02 i cz07 oraz uzupełnić moduł Nauka bez dublowania
   istniejących bloków.
5. Dla każdego nowego scenariusza dodać model danych, walidację, czytelnie
   złożone równania, testy wartości referencyjnych, test UI 820 x 600, import i
   eksport CSV/DOCX.
6. Po wdrożeniu zaktualizować ten rejestr i trzy dokumenty w katalogu
   `Documentation/Final`.
