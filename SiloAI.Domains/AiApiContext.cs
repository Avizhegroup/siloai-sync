namespace SiloAI.Domains;

public class AiApiContext(DbContextOptions<AiApiContext> options) : DbContext(options)
{
    public DbSet<AiApiKey> AiApiKeys { get; set; }
    public DbSet<AiAdminUser> AiAdminUsers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<AiConversation> AiConversations { get; set; }
    public DbSet<RagDocument> RagDocuments { get; set; }
    public DbSet<RagDocumentChunk> RagDocumentChunks { get; set; }
    public DbSet<RagInstruction> RagInstructions { get; set; }
    public DbSet<AiChatSession> AiChatSessions { get; set; }
    public DbSet<LedgerAccount> LedgerAccounts { get; set; }
    public DbSet<LedgerTransaction> LedgerTransactions { get; set; }
    public DbSet<UsageRecord> UsageRecords { get; set; }
    public DbSet<PricingSetting> PricingSettings { get; set; }
    public DbSet<FxRateSetting> FxRateSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RagDocument>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.FileHash).HasDatabaseName("IX_tbl_RagDocuments_fld_FileHash");
            b.HasIndex(x => x.Category).HasDatabaseName("IX_tbl_RagDocuments_fld_Category");
            b.HasIndex(x => x.DocType).HasDatabaseName("IX_tbl_RagDocuments_fld_DocType");
        });

        modelBuilder.Entity<RagDocumentChunk>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Document)
                .WithMany(d => d.Chunks)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.DocumentId).HasDatabaseName("IX_tbl_RagDocumentChunks_fld_DocumentId");
            b.HasIndex(x => x.ChunkIndex).HasDatabaseName("IX_tbl_RagDocumentChunks_fld_ChunkIndex");
        });

        modelBuilder.Entity<RagInstruction>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.DocType).HasDatabaseName("IX_tbl_RagInstructions_fld_DocType");
            b.HasIndex(x => x.Category).HasDatabaseName("IX_tbl_RagInstructions_fld_Category");
        });

        modelBuilder.Entity<AiChatSession>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.OwnerKey).HasDatabaseName("IX_tbl_AiChatSessions_fld_OwnerKey");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(x => x.RemainingCredit)
                .HasColumnName("fld_RemainingCredit")
                .HasColumnType("decimal(18,8)")
                .IsRequired();
        });

        modelBuilder.Entity<LedgerAccount>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Customer)
                .WithOne(c => c.LedgerAccount)
                .HasForeignKey<LedgerAccount>(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.CustomerId)
                .IsUnique()
                .HasDatabaseName("IX_tbl_LedgerAccounts_fld_CustomerId");
        });

        modelBuilder.Entity<LedgerTransaction>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.UsageRecord)
                .WithMany()
                .HasForeignKey(x => x.UsageRecordId)
                // NO ACTION: SQL Server rejects SetNull here as a second cascade path
                // from tbl_AiCustomers (via tbl_UsageRecords). The ledger is append-only
                // anyway, so a transaction row must never be rewritten on usage deletes.
                .OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasDatabaseName("UX_tbl_LedgerTransactions_fld_IdempotencyKey");
            b.HasIndex(x => x.AccountId).HasDatabaseName("IX_tbl_LedgerTransactions_fld_AccountId");
            b.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_tbl_LedgerTransactions_fld_CreatedAt");
        });

        modelBuilder.Entity<UsageRecord>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Customer)
                .WithMany(c => c.UsageRecords)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.CustomerId).HasDatabaseName("IX_tbl_UsageRecords_fld_CustomerId");
            b.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_tbl_UsageRecords_fld_CreatedAt");
        });

        modelBuilder.Entity<PricingSetting>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.Feature, x.EffectiveFrom })
                .HasDatabaseName("IX_tbl_PricingSettings_fld_Feature_fld_EffectiveFrom");
        });

        modelBuilder.Entity<FxRateSetting>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.EffectiveFrom)
                .HasDatabaseName("IX_tbl_FxRateSettings_fld_EffectiveFrom");
        });
    }
}
