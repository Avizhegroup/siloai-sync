namespace SiloAI.Application.Shared.Features;

public enum RagDocType
{
    GeneralChat,
    Report,
    ImageOCR,
    PageAgent,
    Support,

    /// <summary>Instructions + text request + input images in, generated image out. No RAG.</summary>
    ImageGeneration
}

public static class RagDocTypeExtensions
{
    public static string ToDisplay(this RagDocType type) => type switch
    {
        RagDocType.GeneralChat => "گفتگوی عمومی",
        RagDocType.Report => "گزارش",
        RagDocType.ImageOCR => "تصویر",
        RagDocType.PageAgent => "عامل صفحه محور",
        RagDocType.Support => "پشتیبانی",
        RagDocType.ImageGeneration => "تولید تصویر",
        _ => type.ToString()
    };
}


