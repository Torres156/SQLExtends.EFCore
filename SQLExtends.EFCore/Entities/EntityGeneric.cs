using System.ComponentModel.DataAnnotations.Schema;

namespace SQLExtends.EFCore.Entities;

public abstract class EntityGeneric
{
    [Column("id")]
    public int Id { get; set; }

    [Column("CreatedAt")]
    public DateTime? CreatedAt { get; set; }
    
    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }
}