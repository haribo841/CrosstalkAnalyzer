using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.ViewModels;

public sealed partial class LearningViewModel : ViewModelBase
{
    public IReadOnlyList<LearningModule> Modules { get; } =
    [
        new(
            "1. Mechanizmy i drogi zakłóceń",
            "Źródło, droga sprzężenia i odbiornik zakłócenia.",
            "Zakłócenie wymaga źródła, drogi przenikania i obwodu podatnego. Należy rozróżniać sprzężenie galwaniczne, pojemnościowe, indukcyjne i promieniowane oraz sygnały trybu wspólnego i różnicowego. Szybkie zbocza zwiększają emisję nawet przy małej częstotliwości podstawowej.",
            @"\begin{aligned}i_C&=C\frac{\mathrm{d}v}{\mathrm{d}t}\\u_L&=L\frac{\mathrm{d}i}{\mathrm{d}t}\end{aligned}",
            "Pojemności pasożytnicze reagują na szybkość zmian napięcia, a indukcyjności pasożytnicze na szybkość zmian prądu.",
            "KEM_wykład_cz01.pdf; KEM - Polecenia do opracowania 01.pdf"),
        new(
            "2. Przesłuchy i sprzężenia polowe",
            "Przesłuch pojemnościowy, indukcyjny, NEXT i FEXT.",
            "Sprzężenie pojemnościowe rośnie z częstotliwością, pojemnością wzajemną i impedancją obwodu ofiary. Sprzężenie indukcyjne zależy od indukcyjności wzajemnej, pola pętli i szybkości zmian prądu. Zmniejsza się je przez skracanie odcinków równoległych, zwiększanie odstępu, redukcję powierzchni pętli i prowadzenie blisko płaszczyzny odniesienia.",
            @"\begin{aligned}U_C&\approx\omega C_m Z_L U_S\\U_L&\approx\omega M I_S\end{aligned}",
            "Cm - pojemność wzajemna, M - indukcyjność wzajemna, ZL - impedancja obwodu podatnego.",
            "KEM_wykład_cz03.pdf; KEM - Pytania do opracowania 02.pdf"),
        new(
            "3. Uziemianie, masa i oprzewodowanie",
            "Ekwipotencjalność i impedancja połączeń masy.",
            "Masa nie jest idealnym węzłem o zerowym potencjale. Przy wysokich częstotliwościach dominują indukcyjność i geometria połączenia. Połączenia powinny być krótkie i szerokie, a prądy powrotne powinny mieć ciągłą drogę pod sygnałem. Należy unikać wspólnych odcinków powrotu dla obwodów o dużych i małych prądach.",
            @"Z_{\mathrm{połączenia}}=R+j\omega L",
            "Wzrost częstotliwości powoduje wzrost udziału reaktancji indukcyjnej nawet dla przewodu o małej rezystancji.",
            "KEM_wykład_cz04.pdf"),
        new(
            "4. Ekranowanie, filtry i przewody",
            "Ograniczanie emisji i zwiększanie odporności.",
            "Skuteczność ekranu zależy od materiału, częstotliwości, ciągłości powierzchni, szczelin i sposobu zakończenia ekranów kabli. Filtr musi być umieszczony przy granicy strefy EMC i mieć krótkie połączenie z odniesieniem. Przewód ekranowany wymaga poprawnego zakończenia ekranu odpowiednio do zakresu częstotliwości.",
            @"\begin{aligned}SE&=20\log_{10}\left(\frac{E_0}{E_1}\right)\\f_c&=\frac{1}{2\pi RC}\end{aligned}",
            "SE opisuje skuteczność ekranowania pola elektrycznego, a fc jest częstotliwością graniczną prostego filtru RC.",
            "KEM_wykład_cz05.pdf; KEM_wykład_cz06.pdf"),
        new(
            "5. Dyrektywy, normy i niepewność",
            "Ocena zgodności i wiarygodność wyniku pomiaru.",
            "Procedura pomiarowa musi wskazywać normę, konfigurację EUT, zakres częstotliwości, detektor, pasmo RBW, odległość, polaryzację i budżet niepewności. Wynik należy porównywać z limitem według reguły decyzyjnej przyjętej w danej procedurze. Wersja normy i klasa urządzenia muszą być jawne.",
            @"u_c=\sqrt{\sum_i(c_i u_i)^2},\qquad U=k\,u_c",
            "Niepewności standardowe są łączone z uwzględnieniem współczynników wrażliwości ci.",
            "KEM_wykład_cz08.pdf; EMC and Measurement Uncertainty CISPR 16-4-2.pdf"),
        new(
            "6. Aparatura i analiza widma",
            "Analizator widma, VNA, miernik mocy i anteny.",
            "RBW wpływa na rozdzielczość częstotliwościową i poziom szumu. VBW wygładza wskazanie. Detektor szczytowy, quasi-szczytowy i średni nie są zamienne. Analizator sieci mierzy parametry S, a analizator widma poziom składowych częstotliwościowych. Przed pomiarem trzeba sprawdzić impedancję, zakres, tłumik wejściowy i możliwość przesterowania.",
            @"P_{\mathrm{dBm}}=10\log_{10}\left(\frac{P}{1\,\mathrm{mW}}\right)",
            "Skala dBm opisuje moc odniesioną do 1 mW, a nie napięcie bez znajomości impedancji.",
            "cwWDTKompatybilnoscEM/cw1WDT.pdf; FSH_Operating_Manual_15.pdf; RS ZVL DataSheet.pdf"),
        new(
            "7. Filtry, sprzęgacz i dopasowanie",
            "Materiały uzupełniające z katalogu cwWDT.",
            "Analiza filtrów obejmuje odpowiedź amplitudową i fazową, częstotliwość graniczną oraz stabilność. Sprzęgacz kierunkowy pozwala rozdzielić falę padającą i odbitą. Dopasowanie anteny ocenia się przez współczynnik odbicia, WFS i straty odbiciowe.",
            @"\mathrm{WFS}=\frac{1+|\Gamma|}{1-|\Gamma|},\qquad RL=-20\log_{10}|\Gamma|",
            "Γ jest zespolonym współczynnikiem odbicia. Im mniejszy jego moduł, tym lepsze dopasowanie.",
            "cwWDTKompatybilnoscEM/cw2WDT.pdf; cwWDTKompatybilnoscEM/cw3WDT.pdf"),
    ];

    public IReadOnlyList<StudyQuestion> Questions { get; } =
    [
        new("Mechanizmy powstawania zakłóceń dla prądu stałego i zmiennego.",
            "Wskaż źródło, drogę sprzężenia, odbiornik, elementy pasożytnicze i wpływ szybkości zmian."),
        new("Sygnały symetryczne i asymetryczne w analizie sprzężeń.",
            "Wyjaśnij prąd różnicowy i wspólny, równowagę impedancji oraz konwersję trybów."),
        new("Zakłócenia w obudowie przewodzącej i izolującej.",
            "Porównaj ekranowanie, filtrację wejść, ekwipotencjalność i promieniowanie przewodów."),
        new("Maksymalna częstotliwość i dI/dt dla ścieżki miedzianej.",
            "Uwzględnij rezystancję, indukcyjność ścieżki i dopuszczalny spadek napięcia."),
        new("Impuls przez pojemność pasożytniczą separacji galwanicznej.",
            "Użyj i=C dv/dt, impedancji obwodu oraz czasu narastania impulsu."),
        new("Ilościowy opis przesłuchu pojemnościowego i indukcyjnego.",
            "Zdefiniuj Cm, M, impedancję ofiary, długość sprzężenia, NEXT i FEXT."),
        new("Sprzężenie pola z przewodem i pętlą.",
            "Powiąż pole E z napięciem, pole H ze strumieniem oraz geometrię z powierzchnią pętli."),
    ];

    [ObservableProperty]
    private LearningModule _selectedModule;

    [ObservableProperty]
    private StudyQuestion _selectedQuestion;

    [ObservableProperty]
    private bool _showKeyPoints;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WavelengthMeters))]
    private double _frequencyMHz = 100;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CapacitiveCurrentAmps))]
    private double _parasiticCapacitancePf = 10;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CapacitiveCurrentAmps))]
    private double _voltageSlopeVPerNs = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InductiveVoltageVolts))]
    private double _parasiticInductanceNh = 10;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InductiveVoltageVolts))]
    private double _currentSlopeAPerNs = 0.1;

    public double WavelengthMeters => FrequencyMHz > 0 ? 299.792458 / FrequencyMHz : 0;
    public double CapacitiveCurrentAmps => ParasiticCapacitancePf * VoltageSlopeVPerNs / 1000.0;
    public double InductiveVoltageVolts => ParasiticInductanceNh * CurrentSlopeAPerNs;

    public IRelayCommand ToggleKeyPointsCommand { get; }

    public LearningViewModel()
    {
        _selectedModule = Modules[0];
        _selectedQuestion = Questions[0];
        ToggleKeyPointsCommand = new RelayCommand(() => ShowKeyPoints = !ShowKeyPoints);
    }
}
