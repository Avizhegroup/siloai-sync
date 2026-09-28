namespace SiloAI.Application.Shared.Features;

public static class AiModelKindExtensions
{
    public static string ToDisplay(this AiModelKind kind) => kind switch
    {
        AiModelKind.Rag => "مدل بازیابی (RAG)",
        AiModelKind.Normal => "مدل عمومی",
        _ => kind.ToString()
    };
}
