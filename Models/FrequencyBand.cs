using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrosstalkAnalyzer.Models;

public sealed class FrequencyBand
{
    public string Name { get; init; } = string.Empty;
    public int StartMHz { get; init; }
    public int EndMHz { get; init; }

    public override string ToString() => Name;
}