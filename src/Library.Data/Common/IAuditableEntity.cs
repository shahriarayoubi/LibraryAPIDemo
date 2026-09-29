namespace Library.Data.Common;
public interface IAuditableEntity
{
    DateTime CreatedUtc { get; set; }
    DateTime UpdatedUtc { get; set; }
}