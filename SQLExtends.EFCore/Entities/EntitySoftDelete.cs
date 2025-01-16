namespace SQLExtends.EFCore.Entities;

public abstract class EntitySoftDelete : EntityGeneric
{
    public DateTime? DeletedAt { get; set; }
}