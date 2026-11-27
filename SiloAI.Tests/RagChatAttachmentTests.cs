using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using SiloAI.Agent.Chat;
using SiloAI.Application.Api.Features;
using SiloAI.Application.Shared;
using SiloAI.Application.Shared.Contracts.AiModels;
using SiloAI.Application.Shared.Features;
using SiloAI.Domains;
using Xunit;

namespace SiloAI.Tests;

public class RagChatAttachmentTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0];
    private static readonly byte[] Webp = [(byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0, (byte)'W', (byte)'E', (byte)'B', (byte)'P'];
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7\n...");

    private static byte[] Utf8(string s) => Encoding.UTF8.GetBytes(s);

    private static RagChatAttachment Text(string name = "a.txt", string text = "hello")
        => RagChatAttachmentReader.Prepare(name, Utf8(text));

    private static ResolvedAiModel Model(bool image, bool file) => new(
        Guid.NewGuid(), "chat-model", AiModelKind.Normal, null,
        true, true, image, false, file, false, false, false, 0m, 0m, 0m);

    // ---- rules ----

    [Theory]
    [InlineData("photo.PNG", RagChatAttachmentKind.Image)]
    [InlineData("a.jpg", RagChatAttachmentKind.Image)]
    [InlineData("a.jpeg", RagChatAttachmentKind.Image)]
    [InlineData("a.webp", RagChatAttachmentKind.Image)]
    [InlineData("doc.pdf", RagChatAttachmentKind.Pdf)]
    [InlineData("notes.txt", RagChatAttachmentKind.Text)]
    [InlineData("readme.md", RagChatAttachmentKind.Text)]
    [InlineData("data.csv", RagChatAttachmentKind.Text)]
    [InlineData("data.json", RagChatAttachmentKind.Text)]
    public void Classify_knows_the_allowed_extensions(string name, RagChatAttachmentKind kind)
        => Assert.Equal(kind, RagChatAttachmentRules.Classify(name));

    [Theory]
    [InlineData("run.exe")]
    [InlineData("report.docx")]
    [InlineData("archive.zip")]
    [InlineData("noextension")]
    [InlineData("")]
    [InlineData(null)]
    public void Classify_rejects_everything_else(string? name)
        => Assert.Null(RagChatAttachmentRules.Classify(name));

    [Fact]
    public void The_accept_attribute_lists_every_allowed_extension()
    {
        foreach (var ext in RagChatAttachmentRules.AllExtensions)
            Assert.Contains(ext, RagChatAttachmentRules.AcceptAttribute);
    }

    [Fact]
    public void Limits_are_consistent()
    {
        Assert.True(RagChatAttachmentRules.MaxFiles >= 1);
        Assert.True(RagChatAttachmentRules.MaxFileBytes <= RagChatAttachmentRules.MaxTotalBytes);
        Assert.True(RagChatAttachmentRules.MaxRequestBytes > RagChatAttachmentRules.MaxTotalBytes);
    }

    // ---- Prepare ----

    [Fact]
    public void Images_are_detected_from_their_bytes()
    {
        Assert.Equal("image/png", RagChatAttachmentReader.Prepare("a.png", Png).MediaType);
        Assert.Equal("image/jpeg", RagChatAttachmentReader.Prepare("a.jpg", Jpeg).MediaType);
        Assert.Equal("image/webp", RagChatAttachmentReader.Prepare("a.webp", Webp).MediaType);
    }

    [Fact]
    public void A_renamed_file_is_rejected_because_the_bytes_do_not_match_the_extension()
    {
        Assert.Throws<BadRequestException>(() => RagChatAttachmentReader.Prepare("fake.png", Utf8("not an image at all")));
        Assert.Throws<BadRequestException>(() => RagChatAttachmentReader.Prepare("fake.pdf", Utf8("not a pdf")));
    }

    [Fact]
    public void A_pdf_keeps_its_bytes_and_media_type()
    {
        var a = RagChatAttachmentReader.Prepare("doc.pdf", Pdf);
        Assert.Equal(RagChatAttachmentKind.Pdf, a.Kind);
        Assert.Equal("application/pdf", a.MediaType);
        Assert.Equal(Pdf, a.Data);
    }

    [Fact]
    public void A_text_file_is_decoded_and_a_utf8_bom_is_dropped()
    {
        var withBom = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Utf8("سلام دنیا")).ToArray();

        var a = RagChatAttachmentReader.Prepare("a.txt", withBom);

        Assert.Equal(RagChatAttachmentKind.Text, a.Kind);
        Assert.Equal("سلام دنیا", a.Text);
        Assert.Empty(a.Data);
    }

    [Fact]
    public void Binary_invalid_utf8_empty_and_blank_files_are_rejected()
    {
        Assert.Throws<BadRequestException>(() => RagChatAttachmentReader.Prepare("a.txt", [0x41, 0x00, 0x42]));
        Assert.Throws<BadRequestException>(() => RagChatAttachmentReader.Prepare("a.txt", [0xC3, 0x28]));
        Assert.Throws<BadRequestException>(() => RagChatAttachmentReader.Prepare("a.txt", []));
        Assert.Throws<BadRequestException>(() => RagChatAttachmentReader.Prepare("a.txt", Utf8("   \n  ")));
    }

    [Fact]
    public void Unsupported_extensions_are_rejected_with_the_allowed_list()
    {
        var ex = Assert.Throws<BadRequestException>(() => RagChatAttachmentReader.Prepare("run.exe", Png));
        Assert.Contains(".pdf", ex.Message);
    }

    [Fact]
    public void A_file_over_the_size_limit_is_rejected()
    {
        var big = new byte[RagChatAttachmentRules.MaxFileBytes + 1];
        Array.Copy(Pdf, big, Pdf.Length);

        Assert.Throws<BadRequestException>(() => RagChatAttachmentReader.Prepare("big.pdf", big));
    }

    [Fact]
    public void The_file_name_is_reduced_to_its_last_segment()
        => Assert.Equal("evil.txt", Text("../../etc/evil.txt").FileName);

    [Fact]
    public void A_very_long_text_file_is_cut_with_a_notice()
    {
        var a = Text("long.txt", new string('x', RagChatAttachmentRules.MaxTextCharsPerFile + 500));

        Assert.StartsWith(new string('x', 100), a.Text);
        Assert.Contains("was cut", a.Text);
        Assert.True(a.Text!.Length < RagChatAttachmentRules.MaxTextCharsPerFile + 200);
    }

    // ---- set limits ----

    [Fact]
    public void Up_to_the_maximum_count_is_accepted_and_one_more_is_rejected()
    {
        var ok = Enumerable.Range(0, RagChatAttachmentRules.MaxFiles).Select(i => Text($"f{i}.txt")).ToList();
        RagChatAttachmentReader.ValidateSet(ok);

        ok.Add(Text("one-too-many.txt"));
        Assert.Throws<BadRequestException>(() => RagChatAttachmentReader.ValidateSet(ok));
    }

    [Fact]
    public void The_combined_size_is_limited()
    {
        var parts = new List<RagChatAttachment>
        {
            new() { FileName = "a.pdf", Kind = RagChatAttachmentKind.Pdf, SizeBytes = (int)RagChatAttachmentRules.MaxFileBytes },
            new() { FileName = "b.pdf", Kind = RagChatAttachmentKind.Pdf, SizeBytes = (int)RagChatAttachmentRules.MaxFileBytes },
            new() { FileName = "c.pdf", Kind = RagChatAttachmentKind.Pdf, SizeBytes = (int)RagChatAttachmentRules.MaxFileBytes }
        };

        Assert.Throws<BadRequestException>(() => RagChatAttachmentReader.ValidateSet(parts));
    }

    // ---- what the model and the history get ----

    [Fact]
    public void Text_files_are_inlined_with_their_names_and_other_kinds_are_not()
    {
        var section = RagChatAttachmentReader.BuildTextSection(
            [Text("a.txt", "AAA"), RagChatAttachmentReader.Prepare("p.png", Png), Text("b.md", "BBB")]);

        Assert.Contains("Attached file: a.txt", section);
        Assert.Contains("AAA", section);
        Assert.Contains("BBB", section);
        Assert.DoesNotContain("p.png", section);
        Assert.Equal(string.Empty, RagChatAttachmentReader.BuildTextSection([]));
    }

    [Fact]
    public void The_history_note_lists_every_file_name_and_is_empty_without_files()
    {
        Assert.Equal(string.Empty, RagChatAttachmentReader.BuildHistoryNote([]));

        var note = RagChatAttachmentReader.BuildHistoryNote(
            [Text("a.txt"), RagChatAttachmentReader.Prepare("p.png", Png)]);

        Assert.Contains("a.txt", note);
        Assert.Contains("p.png", note);
    }

    [Fact]
    public void Only_images_and_pdfs_travel_as_separate_model_contents()
    {
        var contents = RagChatSendHandler.BuildModelContents(
            [Text("a.txt"), RagChatAttachmentReader.Prepare("p.png", Png), RagChatAttachmentReader.Prepare("d.pdf", Pdf)]);

        Assert.Equal(2, contents.Count);
        Assert.All(contents, c => Assert.IsType<DataContent>(c));
        Assert.Equal(["image/png", "application/pdf"], contents.Cast<DataContent>().Select(c => c.MediaType).ToArray());
    }

    [Fact]
    public void Extra_contents_reach_the_model_for_the_turn_but_the_stored_list_is_untouched()
    {
        var messages = new List<ChatMessage> { new(ChatRole.User, "question [Attached files: p.png]") };
        var extras = RagChatSendHandler.BuildModelContents([RagChatAttachmentReader.Prepare("p.png", Png)]);

        using (TurnMessageSubstitution.Begin("question [Attached files: p.png]", "question (augmented)", extras))
        {
            var sent = TurnMessageSubstitution.Apply(messages).Single();

            Assert.Equal("question (augmented)", sent.Text);
            Assert.Contains(sent.Contents, c => c is DataContent);
        }

        Assert.Single(messages[0].Contents);
        Assert.IsType<TextContent>(messages[0].Contents[0]);
    }

    // ---- model capability ----

    [Fact]
    public void Images_need_an_image_capable_model_and_pdfs_a_file_capable_one()
    {
        var image = RagChatAttachmentReader.Prepare("p.png", Png);
        var pdf = RagChatAttachmentReader.Prepare("d.pdf", Pdf);

        Assert.Throws<BadRequestException>(() => RagChatSendHandler.EnsureModelCanReadAttachments(Model(false, true), [image]));
        Assert.Throws<BadRequestException>(() => RagChatSendHandler.EnsureModelCanReadAttachments(Model(true, false), [pdf]));

        RagChatSendHandler.EnsureModelCanReadAttachments(Model(true, true), [image, pdf]);
    }

    [Fact]
    public void Text_files_work_with_any_chat_model()
        => RagChatSendHandler.EnsureModelCanReadAttachments(Model(false, false), [Text()]);

    // ---- the multipart endpoint's handler ----

    private static IFormFile File(string name, byte[] bytes)
        => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "files", name);

    [Fact]
    public async Task Only_the_message_is_required()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new RagChatSendFormCommandHandler(null!).Handle(new RagChatSendFormCommand { Message = "  " }, default));

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new RagChatSendFormCommandHandler(null!).Handle(
                new RagChatSendFormCommand { Message = null, Files = [File("a.txt", Utf8("x"))] }, default));
    }

    [Fact]
    public async Task Too_many_files_are_rejected_before_anything_else_happens()
    {
        var files = Enumerable.Range(0, RagChatAttachmentRules.MaxFiles + 1)
            .Select(i => File($"f{i}.txt", Utf8("x"))).ToList();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            new RagChatSendFormCommandHandler(null!).Handle(new RagChatSendFormCommand { Message = "hi", Files = files }, default));
    }

    [Fact]
    public async Task An_unsupported_file_is_rejected()
        => await Assert.ThrowsAsync<BadRequestException>(() =>
            new RagChatSendFormCommandHandler(null!).Handle(
                new RagChatSendFormCommand { Message = "hi", Files = [File("run.exe", Png)] }, default));

    [Fact]
    public async Task Files_are_read_and_validated()
    {
        var read = await RagChatAttachmentReader.ReadAsync(
            [File("a.txt", Utf8("hello")), File("p.png", Png)], default);

        Assert.Equal(["a.txt", "p.png"], read.Select(a => a.FileName).ToArray());
    }

    [Fact]
    public void The_form_converts_to_the_same_send_command_with_the_attachments()
    {
        var id = Guid.NewGuid();
        var attachments = new List<RagChatAttachment> { Text() };

        var command = RagChatSendFormCommandHandler.ToSendCommand(new RagChatSendFormCommand
        {
            Message = "  hi  ", ConversationId = id, TopK = 7, IsMainChat = true,
            DocType = RagDocType.Support, Key = "k", UseRag = false
        }, attachments);

        Assert.Equal("hi", command.Message);
        Assert.Equal(id, command.ConversationId);
        Assert.Equal(7, command.TopK);
        Assert.True(command.IsMainChat);
        Assert.Equal(RagDocType.Support, command.DocType);
        Assert.Equal("k", command.Key);
        Assert.False(command.UseRag);
        Assert.Same(attachments, command.Attachments);
    }

    [Fact]
    public void The_json_send_endpoint_cannot_smuggle_attachments_past_validation()
    {
        var json = System.Text.Json.JsonSerializer.Deserialize<RagChatSendCommand>(
            """{"message":"hi","attachments":[{"fileName":"a.exe","data":"AAAA"}]}""",
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

        Assert.NotNull(json);
        Assert.Empty(json!.Attachments);
    }
}
