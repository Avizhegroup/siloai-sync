namespace SiloAI.Application.Shared.Features;

public static class FinancialEnumExtensions
{
    public static string ToDisplay(this LedgerTransactionType type) => type switch
    {
        LedgerTransactionType.TopUp => "شارژ",
        LedgerTransactionType.Usage => "مصرف",
        LedgerTransactionType.Refund => "بازگشت",
        LedgerTransactionType.Adjustment => "تعدیل",
        _ => type.ToString()
    };

    public static string ToDisplay(this UsageFeature feature) => feature switch
    {
        UsageFeature.SupportChat => "چت پشتیبانی",
        UsageFeature.Report => "گزارش",
        UsageFeature.PageAgent => "عامل صفحه محور",
        UsageFeature.Ocr => "تصویر (OCR)",
        _ => feature.ToString()
    };
}
