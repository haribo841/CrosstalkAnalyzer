namespace CrosstalkAnalyzer.Models;

public sealed record LearningModule(
    string Title,
    string Summary,
    string Content,
    string FormulaLatex,
    string FormulaDescription,
    string Sources)
{
    public override string ToString() => Title;
}

public sealed record StudyQuestion(
    string Prompt,
    string KeyPoints)
{
    public override string ToString() => Prompt;
}

public sealed record SourceRequirement(
    string Area,
    string Status,
    string RequiredMaterial,
    string Reason);
