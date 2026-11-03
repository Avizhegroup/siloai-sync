using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

/// <summary>
/// RagInstruction.DocType and tbl_*.fld_Feature are persisted as ints, so existing members must
/// never be renumbered and new ones must be appended.
/// </summary>
public class EnumContractTests
{
    [Theory]
    [InlineData(RagDocType.GeneralChat, 0)]
    [InlineData(RagDocType.Report, 1)]
    [InlineData(RagDocType.ImageOCR, 2)]
    [InlineData(RagDocType.PageAgent, 3)]
    [InlineData(RagDocType.Support, 4)]
    [InlineData(RagDocType.ImageGeneration, 5)]
    public void RagDocType_values_are_stable(RagDocType type, int value) => Assert.Equal(value, (int)type);

    [Theory]
    [InlineData(UsageFeature.SupportChat, 1)]
    [InlineData(UsageFeature.Report, 2)]
    [InlineData(UsageFeature.PageAgent, 3)]
    [InlineData(UsageFeature.Ocr, 4)]
    [InlineData(UsageFeature.ImageGeneration, 5)]
    public void UsageFeature_values_are_stable(UsageFeature feature, int value) => Assert.Equal(value, (int)feature);

    [Fact]
    public void Every_RagDocType_has_a_display_name()
    {
        foreach (var type in Enum.GetValues<RagDocType>())
            Assert.NotEqual(type.ToString(), type.ToDisplay());
    }

    [Fact]
    public void Every_UsageFeature_has_a_display_name()
    {
        foreach (var feature in Enum.GetValues<UsageFeature>())
            Assert.NotEqual(feature.ToString(), feature.ToDisplay());
    }
}
