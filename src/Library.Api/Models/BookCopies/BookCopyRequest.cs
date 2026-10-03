using System.ComponentModel.DataAnnotations;
using Library.Data.Entities;

namespace Library.Api.Models.BookCopies;

public abstract class BookCopyRequest
{
    [Range(1, int.MaxValue)]
    public int BookId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string InventoryNumber { get; set; }

    [EnumDataType(typeof(BookCopyStatus))]
    public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;

    [MaxLength(50)]
    public string? ShelfLocation { get; set; }

    public DateTime AcquiredUtc { get; set; }
}
