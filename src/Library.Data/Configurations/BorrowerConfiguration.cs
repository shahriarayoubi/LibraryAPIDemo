using Library.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;

namespace Library.Data.Configurations;

internal sealed class BorrowerConfiguration
    : IEntityTypeConfiguration<Borrower>
{
    public void Configure(EntityTypeBuilder<Borrower> builder)
    {
        builder.ToTable("Borrowers", "circulation");

        builder.HasKey(borrower => borrower.Id);

        builder.Property(borrower => borrower.MembershipNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(borrower => borrower.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(borrower => borrower.MiddleName)
            .HasMaxLength(100);

        builder.Property(borrower => borrower.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(borrower => borrower.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(borrower => borrower.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(borrower => borrower.IdentityUserId)
            .HasMaxLength(450);

        builder.HasIndex(borrower => borrower.MembershipNumber)
            .IsUnique();

        builder.HasIndex(borrower => borrower.Email)
            .IsUnique();

        builder.HasIndex(borrower => borrower.IdentityUserId)
            .IsUnique()
            .HasFilter("[IdentityUserId] IS NOT NULL");

        builder.HasOne<IdentityUser>()
            .WithOne()
            .HasForeignKey<Borrower>(
                borrower => borrower.IdentityUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}