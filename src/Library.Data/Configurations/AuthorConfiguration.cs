using Library.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Data.Configurations;

internal sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors", "catalog");

        builder.HasKey(author => author.Id);

        builder.Property(author => author.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(author => author.MiddleName)
            .HasMaxLength(100);

        builder.Property(author => author.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(author => author.Country)
            .HasMaxLength(100);

        builder.Property(author => author.Biography)
            .HasMaxLength(4_000);

        builder.HasIndex(author => new
        {
            author.LastName,
            author.FirstName,
            author.YearOfBirth
        });

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_Authors_YearOfBirth",
                "[YearOfBirth] IS NULL OR [YearOfBirth] BETWEEN 1 AND 9999");

            table.HasCheckConstraint(
                "CK_Authors_YearOfDeath",
                "[YearOfDeath] IS NULL OR [YearOfDeath] BETWEEN 1 AND 9999");

            table.HasCheckConstraint(
                "CK_Authors_YearRange",
                "[YearOfBirth] IS NULL OR [YearOfDeath] IS NULL " +
                "OR [YearOfDeath] >= [YearOfBirth]");
        });
    }
}