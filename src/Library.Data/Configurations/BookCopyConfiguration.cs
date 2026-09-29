using Library.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Data.Configurations;

internal sealed class BookCopyConfiguration
    : IEntityTypeConfiguration<BookCopy>
{
    public void Configure(EntityTypeBuilder<BookCopy> builder)
    {
        builder.ToTable("BookCopies", "catalog");

        builder.HasKey(copy => copy.Id);

        builder.Property(copy => copy.InventoryNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(copy => copy.ShelfLocation)
            .HasMaxLength(50);

        builder.Property(copy => copy.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(copy => copy.InventoryNumber)
            .IsUnique();

        builder.HasIndex(copy => new { copy.BookId, copy.Status });

        builder.HasOne(copy => copy.Book)
            .WithMany(book => book.Copies)
            .HasForeignKey(copy => copy.BookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}