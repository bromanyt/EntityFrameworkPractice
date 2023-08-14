
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace WebRetail.Models;

public partial class RetailContext : DbContext
{
    private readonly Serilog.ILogger _logger;


    public RetailContext(DbContextOptions<RetailContext> options, Serilog.ILogger logger)
        : base(options)
    {
        _logger = logger;
    }

    public virtual DbSet<Card> Cards { get; set; } = null!;

    public virtual DbSet<Check> Checks { get; set; } = null!;

    public virtual DbSet<DateOfAnalysisFormation> DateOfAnalysisFormations { get; set; } = null!;

    public virtual DbSet<PersonalInformation> PersonalInformations { get; set; } = null!;

    public virtual DbSet<ProductGrid> ProductGrids { get; set; } = null!;

    public virtual DbSet<SkuGroup> SkuGroups { get; set; } = null!;

    public virtual DbSet<Store> Stores { get; set; } = null!;

    public virtual DbSet<Transaction> Transactions { get; set; } = null!;

    public List<IRetailTable>? GetTable(string? tableName)
    {
        CheckDataBaseConnect();
        return tableName switch
        {
            "ProductGrid" => ProductGrids.OrderBy(p => p.GroupId).ToList<IRetailTable>(),
            "SkuGroup" => SkuGroups.OrderBy(p => p.GroupId).ToList<IRetailTable>(),
            "Check" => Checks.OrderBy(p => p.TransactionId).ToList<IRetailTable>(),
            "Card" => Cards.OrderBy(p => p.CustomerCardId).ToList<IRetailTable>(),
            "PersonalInformation" => PersonalInformations.OrderBy(p => p.CustomerId).ToList<IRetailTable>(),
            "Store" => Stores.OrderBy(p => p.TransactionStoreId).ToList<IRetailTable>(),
            "Transaction" => Transactions.OrderBy(p => p.TransactionId).ToList<IRetailTable>(),
            "DateOfAnalysisFormation" => DateOfAnalysisFormations.OrderBy(p => p.AnalysisFormation).ToList<IRetailTable>(),
            _ => throw new RetailException("Table not found"),
        };

    }

    public IRetailTable? GetTableElement(string[] items)
    {
        CheckDataBaseConnect();
        try
        {
            string tableName = items[0];
            switch (tableName)
            {
                case "Card":
                    return Cards.Where(c => c.CustomerCardId == long.Parse(items[1])).First();
                case "Check":
                    return Checks.Where(c => c.TransactionId == long.Parse(items[1]) && c.SkuId == long.Parse(items[2])).First();
                case "DateOfAnalysisFormation":
                    DateTime date;
                    DateTime.TryParse(items[1], new CultureInfo("en-GB"), out date);
                    return DateOfAnalysisFormations.Where(c => c.AnalysisFormation == date).First();
                case "SkuGroup":
                    return SkuGroups.Where(s => s.GroupId == long.Parse(items[1])).First();
                case "PersonalInformation":
                    return PersonalInformations.Where(p => p.CustomerId == long.Parse(items[1])).First();
                case "ProductGrid":
                    return ProductGrids.Where(p => p.SkuId == long.Parse(items[1])).First();
                case "Store":
                    return Stores.Where(p => p.TransactionStoreId == long.Parse(items[1]) && p.SkuId == long.Parse(items[2])).First();
                case "Transaction":
                    return Transactions.Where(t => t.TransactionId == long.Parse(items[1])).First();
                default:
                    throw new RetailException("Table not found");
            }
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException || ex is Npgsql.PostgresException)
        {
            string message = ex is Npgsql.PostgresException ? ex.Message : "Invalid argument";
            throw new RetailException(message);
        }

    }

    public string? ChangeState(IRetailTable obj, EntityState state)
    {
        CheckDataBaseConnect();
        try
        {
            Entry(obj).State = state;
            SaveChanges();
            return obj.GetTableName();
        }
        catch (Exception ex)
        {
            throw SendErrorMessage(ex);
        }
    }

    private RetailException SendErrorMessage(Exception ex)
    {
        string message = ex.Message;
        if (ex is DbUpdateException && ex.InnerException is not null)
            message = ex.InnerException.Message;
        _logger!.Warning(message);
        ChangeTracker.Clear();
        throw new RetailException(message);
    }

    public string? DeleteObject(string[] items)
    {
        CheckDataBaseConnect();
        try
        {
            string tableName = items[0];
            switch (tableName)
            {
                case "Card":
                    Cards.Where(c => c.CustomerCardId == long.Parse(items[1])).ExecuteDelete();
                    break;
                case "Check":
                    Checks.Where(c => c.TransactionId == long.Parse(items[1]) && c.SkuId == long.Parse(items[2])).ExecuteDelete();
                    break;
                case "DateOfAnalysisFormation":
                    DateTime date;
                    DateTime.TryParse(items[1], new CultureInfo("en-GB"), out date);
                    DateOfAnalysisFormations.Where(c => c.AnalysisFormation == date).ExecuteDelete();
                    break;
                case "SkuGroup":
                    SkuGroups.Where(s => s.GroupId == long.Parse(items[1])).ExecuteDelete();
                    break;
                case "PersonalInformation":
                    PersonalInformations.Where(p => p.CustomerId == long.Parse(items[1])).ExecuteDelete();
                    break;
                case "ProductGrid":
                    ProductGrids.Where(p => p.SkuId == long.Parse(items[1])).ExecuteDelete();
                    break;
                case "Store":
                    Stores.Where(p => p.TransactionStoreId == long.Parse(items[1]) && p.SkuId == long.Parse(items[2])).ExecuteDelete();
                    break;
                case "Transaction":
                    Transactions.Where(t => t.TransactionId == long.Parse(items[1])).ExecuteDelete();
                    break;
                default:
                    return tableName;
            }
            SaveChanges();
            return tableName;
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException || ex is Npgsql.PostgresException)
        {
            _logger!.Warning(ex.Message);
            ChangeTracker.Clear();
            string message = ex is Npgsql.PostgresException ? ex.Message : "Invalid argument";
            throw new RetailException(message);
        }

    }

    public IQueryable<OfferByAverageCheck> GetOffersByAverageCheck(int method, DateOnly firstDate,
        DateOnly lastDate, int transactionsNumber, decimal coefAverCheckIncrease,
        int maxChurnIndex, decimal maxShareDiscountTransactions, decimal marginShare)
    {
        CheckDataBaseConnect();
        try
        {
            return FromExpression(() => GetOffersByAverageCheck(method, firstDate, lastDate,
                transactionsNumber, coefAverCheckIncrease, maxChurnIndex, maxShareDiscountTransactions, marginShare));
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException || ex is Npgsql.PostgresException)
        {
            string message = ex is Npgsql.PostgresException ? ex.Message : "Invalid argument";
            throw new RetailException(message);
        }

    }

    public IQueryable<OfferByFrequencyVisits> GetOffersByFrequencyVisits(DateOnly firstDate,
        DateOnly lastDate, long addedTransactionsNumber,
        decimal maxChurnIndex, decimal maxShareDiscountTransactions,
        decimal marginShare)
    {
        CheckDataBaseConnect();
        try
        {
            return FromExpression(() => GetOffersByFrequencyVisits(firstDate,
                lastDate, addedTransactionsNumber, maxChurnIndex, maxShareDiscountTransactions,
                marginShare));
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException || ex is Npgsql.PostgresException)
        {
            string message = ex is Npgsql.PostgresException ? ex.Message : "Invalid argument";
            throw new RetailException(message);
        }
    }

    public IQueryable<OfferByCrossSelling> GetOffersByCrossSelling(long groupsNumber,
        decimal maxChurnIndex, decimal maxConsumptionStabilityIndex,
        decimal maxSkuShare, decimal marginShare)
    {
        CheckDataBaseConnect();
        try
        {
            return FromExpression(() =>
            GetOffersByCrossSelling(groupsNumber, maxChurnIndex, maxConsumptionStabilityIndex,
                maxSkuShare, marginShare));
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentNullException || ex is Npgsql.PostgresException)
        {
            string message = ex is Npgsql.PostgresException ? ex.Message : "Invalid argument";
            throw new RetailException(message);
        }
    }

    private void CheckDataBaseConnect()
    {
        if (Database.CanConnect() is not true)
        {
            _logger.Error("Failed to connect to the database server.");
            throw new RetailException($"The database server is not available. Please contact the administrator.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.CustomerCardId).HasName("cards_pkey");

            entity.ToTable("cards");

            entity.Property(e => e.CustomerCardId)
                .ValueGeneratedNever()
                .HasColumnName("customer_card_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Cards)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cards_customer_id");
        });

        modelBuilder.Entity<Check>(entity =>
        {
            entity.HasKey(e => new { e.TransactionId, e.SkuId }).HasName("checks_pkey");

            entity.ToTable("checks");

            entity.Property(e => e.SkuAmount).HasColumnName("sku_amount");
            entity.Property(e => e.SkuDiscount).HasColumnName("sku_discount");
            entity.Property(e => e.SkuId).HasColumnName("sku_id");
            entity.Property(e => e.SkuSumm).HasColumnName("sku_summ");
            entity.Property(e => e.SkuSummPaid).HasColumnName("sku_summ_paid");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Sku).WithMany()
                .HasForeignKey(d => d.SkuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_checks_sku_id");

            entity.HasOne(d => d.Transaction).WithMany()
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_transaction_id_sku_id");
        });

        modelBuilder.Entity<DateOfAnalysisFormation>(entity =>
        {
            entity.ToTable("date_of_analysis_formation");

            entity.HasKey(e => e.AnalysisFormation).HasName("date_of_analysis_formation_pkey");

            entity.Property(e => e.AnalysisFormation)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("analysis_formation");
        });

        modelBuilder.Entity<PersonalInformation>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("personal_information_pkey");

            entity.ToTable("personal_information");

            entity.Property(e => e.CustomerId)
                .ValueGeneratedNever()
                .HasColumnName("customer_id");
            entity.Property(e => e.CustomerName)
                .HasColumnType("character varying")
                .HasColumnName("customer_name");
            entity.Property(e => e.CustomerPrimaryEmail)
                .HasColumnType("character varying")
                .HasColumnName("customer_primary_email");
            entity.Property(e => e.CustomerPrimaryPhone)
                .HasMaxLength(12)
                .HasColumnName("customer_primary_phone");
            entity.Property(e => e.CustomerSurname)
                .HasColumnType("character varying")
                .HasColumnName("customer_surname");
        });

        modelBuilder.Entity<ProductGrid>(entity =>
        {
            entity.HasKey(e => e.SkuId).HasName("product_grid_pkey");

            entity.ToTable("product_grid");

            entity.Property(e => e.SkuId)
                .ValueGeneratedNever()
                .HasColumnName("sku_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.SkuName)
                .HasColumnType("character varying")
                .HasColumnName("sku_name");

            entity.HasOne(d => d.Group).WithMany(p => p.ProductGrids)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_grid_group_id");
        });

        modelBuilder.Entity<SkuGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("sku_group_pkey");

            entity.ToTable("sku_group");

            entity.Property(e => e.GroupId)
                .ValueGeneratedNever()
                .HasColumnName("group_id");
            entity.Property(e => e.GroupName)
                .HasColumnType("character varying")
                .HasColumnName("group_name");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.ToTable("stores");

            entity.HasKey(e => new { e.TransactionStoreId, e.SkuId }).HasName("stores_pkey");

            entity.Property(e => e.SkuId).HasColumnName("sku_id");
            entity.Property(e => e.SkuPurchasePrice).HasColumnName("sku_purchase_price");
            entity.Property(e => e.SkuRetailPrice).HasColumnName("sku_retail_price");
            entity.Property(e => e.TransactionStoreId).HasColumnName("transaction_store_id");

            entity.HasOne(d => d.Sku).WithMany()
                .HasForeignKey(d => d.SkuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_stores_sku_id");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("transactions_pkey");

            entity.ToTable("transactions");

            entity.Property(e => e.TransactionId)
                .ValueGeneratedNever()
                .HasColumnName("transaction_id");
            entity.Property(e => e.CustomerCardId).HasColumnName("customer_card_id");
            entity.Property(e => e.TransactionDatetime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("transaction_datetime");
            entity.Property(e => e.TransactionStoreId).HasColumnName("transaction_store_id");
            entity.Property(e => e.TransactionSumm).HasColumnName("transaction_summ");

            entity.HasOne(d => d.CustomerCard).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CustomerCardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_transactions_customer_card_id");
        });

        modelBuilder.HasDbFunction(typeof(RetailContext).GetMethod(nameof(GetOffersByAverageCheck))!)
       .HasName("fn_grow_check")
       .IsBuiltIn(false);

        modelBuilder.HasDbFunction(typeof(RetailContext).GetMethod(nameof(GetOffersByFrequencyVisits))!)
       .HasName("fn_increasing_frequency_of_visits")
       .IsBuiltIn(false);

        modelBuilder.HasDbFunction(typeof(RetailContext).GetMethod(nameof(GetOffersByCrossSelling))!)
       .HasName("fn_cross_selling")
       .IsBuiltIn(false);

        OnModelCreatingPartial(modelBuilder);

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
