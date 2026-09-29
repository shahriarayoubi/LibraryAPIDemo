using Library.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Data.Configurations;

internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books", "catalog");

        builder.HasKey(book => book.Id);

        builder.Property(book => book.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(book => book.Isbn)
            .HasMaxLength(20);

        builder.Property(book => book.Description)
            .HasMaxLength(4_000);

        builder.Property(book => book.LanguageCode)
            .HasMaxLength(10);

        builder.HasIndex(book => book.Isbn)
            .IsUnique()
            .HasFilter("[Isbn] IS NOT NULL");

        builder.HasIndex(book => book.Title);

        builder.HasOne(book => book.Author)
            .WithMany(author => author.Books)
            .HasForeignKey(book => book.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_Books_PublicationYear",
                "[YearOfFirstPublication] BETWEEN 1 AND 9999");

            table.HasCheckConstraint(
                "CK_Books_PageCount",
                "[PageCount] IS NULL OR [PageCount] > 0");
        });
    }
}