using Library.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Data.Configurations;

internal sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans", "circulation");

        builder.HasKey(loan => loan.Id);

        builder.Property(loan => loan.Notes)
            .HasMaxLength(1_000);

        builder.HasIndex(loan => new
        {
            loan.BorrowerId,
            loan.ReturnedUtc
        });

        /*
         * A copy can have many historical loans, but only one active loan.
         * SQL Server filtered indexes allow this to be enforced by the DB.
         */
        builder.HasIndex(loan => loan.BookCopyId)
            .IsUnique()
            .HasFilter("[ReturnedUtc] IS NULL")
            .HasDatabaseName("UX_Loans_OneActiveLoanPerCopy");

        builder.HasOne(loan => loan.BookCopy)
            .WithMany(copy => copy.Loans)
            .HasForeignKey(loan => loan.BookCopyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(loan => loan.Borrower)
            .WithMany(borrower => borrower.Loans)
            .HasForeignKey(loan => loan.BorrowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_Loans_DueAfterLoaned",
                "[DueUtc] > [LoanedUtc]");

            table.HasCheckConstraint(
                "CK_Loans_ReturnedAfterLoaned",
                "[ReturnedUtc] IS NULL OR [ReturnedUtc] >= [LoanedUtc]");

            table.HasCheckConstraint(
                "CK_Loans_RenewalCount",
                "[RenewalCount] >= 0");
        });
    }
}