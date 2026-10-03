namespace SiloAI.Application.Shared.Features;

public enum RagDocType
{
    GeneralChat,
    Report,
    Image,
    PageAgent
}

public static class RagDocTypeExtensions
{
    public static string ToDisplay(this RagDocType type) => type switch
    {
        RagDocType.GeneralChat => "گفتگوی عمومی",
        RagDocType.Report => "گزارش",
        RagDocType.Image => "تصویر",
        RagDocType.PageAgent => "عامل صفحه محور",
        _ => type.ToString()
    };
}

public  class DocTypeItem
{
    public RagDocType Code { get; set; }
    public string Title { get; set; } = string.Empty;
}
