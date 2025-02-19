using System.ComponentModel.DataAnnotations.Schema;

namespace SQLExtends.EFCore.Entities;

public abstract class EntitySoftDelete : EntityGeneric
{
    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }
}