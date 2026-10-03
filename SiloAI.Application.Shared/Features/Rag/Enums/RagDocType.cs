namespace SiloAI.Application.Shared.Features;

public enum RagDocType
{
    GeneralChat,
    Report,
    ImageOCR,
    PageAgent
}

public static class RagDocTypeExtensions
{
    public static string ToDisplay(this RagDocType type) => type switch
    {
        RagDocType.GeneralChat => "گفتگوی عمومی",
        RagDocType.Report => "گزارش",
        RagDocType.ImageOCR => "تصویر",
        RagDocType.PageAgent => "عامل صفحه محور",
        _ => type.ToString()
    };
}
