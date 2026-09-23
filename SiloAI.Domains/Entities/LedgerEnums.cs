namespace SiloAI.Domains;

public enum LedgerTransactionType
{
    TopUp = 1,
    Usage = 2,
    Refund = 3,
    Adjustment = 4
}

public enum UsageFeature
{
    SupportChat = 1,
    Report = 2,
    PageAgent = 3,
    Ocr = 4
}
