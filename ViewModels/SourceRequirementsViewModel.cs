using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class SourceRequirementsViewModel : ViewModelBase
{
    public IReadOnlyList<SourceRequirement> Requirements { get; } =
    [
        new(
            "Pomiary pola dla ochrony środowiska",
            "Oczekuje na instrukcję",
            "Pełna instrukcja laboratorium, aparatura, zakresy, limity i budżet niepewności.",
            "Sylabus wymienia ćwiczenie, ale katalog nie zawiera procedury pozwalającej bezpiecznie odtworzyć pomiar."),
        new(
            "Badanie analizatora widma i pomiar promieniowania",
            "Oczekuje na instrukcję",
            "Scenariusz stanowiska, ustawienia RBW/VBW/detektora, badane sygnały i kryteria wyniku.",
            "Instrukcja aparatury nie zastępuje instrukcji ćwiczenia i kryteriów zaliczenia."),
        new(
            "Pomiary propagacyjne nr 4",
            "Wymaga zatwierdzenia",
            "Oryginalna instrukcja prowadzącego.",
            "Obecny kreator jest oparty na sprawozdaniach i rekomendacjach ITU-R, które zawierają niejednoznaczne jednostki."),
        new(
            "Wykłady cz02 i cz07",
            "Brak plików",
            "Slajdy lub lista tematów od prowadzącego.",
            "Treści nie będą rekonstruowane na podstawie numeracji pozostałych wykładów."),
    ];
}
