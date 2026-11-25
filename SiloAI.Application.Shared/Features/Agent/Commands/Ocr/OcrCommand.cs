using SiloAI.Application.Shared.Contracts.Customers;
namespace SiloAI.Application.Shared.Features;

public class OcrCommand : IRequest<OcrResponse>, ICustomerBound
{
    public byte[] ImageData { get; set; }
    public string MediaType { get; set; }
    public RagDocType DocType { get; set; }
    public string Key { get; set; }
    public int? CustomerId { get; set; }

    /// <summary>Optional client-generated id that makes a retried request idempotent (not charged twice).</summary>
    public Guid? RequestId { get; set; }
}
