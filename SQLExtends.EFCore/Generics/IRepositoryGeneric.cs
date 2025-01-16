using System.Linq.Expressions;

namespace SQLExtends.EFCore.Generics;

public interface IRepositoryGeneric<TModel>
    where TModel : class
{
    void Insert(TModel model);
    Task InsertAsync(TModel model);
    
    void Update(TModel model);
    Task UpdateAsync(TModel model);
    
    void Delete(TModel model);
    Task DeleteAsync(TModel model);
    
    TModel? Find(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TModel>>[] includes);
    Task<TModel?> FindAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TModel>>[] includes);
    
    TModel? Find(IEnumerable<string> selects, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TModel>>[] includes);
    Task<TModel?> FindAsync(IEnumerable<string>  selects, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TModel>>[] includes);
}