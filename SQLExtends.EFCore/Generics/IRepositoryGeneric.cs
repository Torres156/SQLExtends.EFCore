using System.Linq.Expressions;

namespace SQLExtends.EFCore.Generics;

public interface IRepositoryGeneric<TModel>
    where TModel : class
{
    void Insert(TModel model);
    Task InsertAsync(TModel model);
    
    void InsertRange(IEnumerable<TModel> models);
    Task InsertRangeAsync(IEnumerable<TModel> models);
    
    void Update(TModel model);
    Task UpdateAsync(TModel model);
    
    void UpdateRange(IEnumerable<TModel> models);
    Task UpdateRangeAsync(IEnumerable<TModel> models);
    
    void Delete(TModel model);
    Task DeleteAsync(TModel model);
    
    void DeleteRange(IEnumerable<TModel> models);
    Task DeleteRangeAsync(IEnumerable<TModel> models);
    
    TModel? Find(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes);
    Task<TModel?> FindAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TModel>>[] includes);
    
    object? Find(IEnumerable<string> selects, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes);
    Task<object?> FindAsync(IEnumerable<string>  selects, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TModel>>[] includes);

    IEnumerable<TModel> Get(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes);
    Task<IEnumerable<TModel>> GetAsync(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes);
    
    IEnumerable<TModel> GetWithOrder(Expression<Func<TModel, bool>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes);
    Task<IEnumerable<TModel>> GetWithOrderAsync(Expression<Func<TModel, bool>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes);
    
    int Count(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes);
    Task<int> CountAsync(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes);
    
    Paginate<TModel> Paginate(int pageNum, int take, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes);
    Task<Paginate<TModel>> PaginateAsync(int pageNum, int take, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TModel>>[] includes);

    bool Exists(Expression<Func<TModel, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<TModel, bool>> predicate);
    
    IQueryable<TModel> Query();
}