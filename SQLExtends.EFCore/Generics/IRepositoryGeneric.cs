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
    
    TModel? Find<TProperty>(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes);
    Task<TModel?> FindAsync<TProperty>(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TProperty>>[] includes);
    TModel? Find(Expression<Func<TModel, bool>>? predicate);
    Task<TModel?> FindAsync(Expression<Func<TModel, bool>> predicate);
    
    object? Find<TProperty>(IEnumerable<string> selects, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes);
    Task<object?> FindAsync<TProperty>(IEnumerable<string>  selects, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, TProperty>>[] includes);
    object? Find(IEnumerable<string> selects, Expression<Func<TModel, bool>>? predicate);
    Task<object?> FindAsync(IEnumerable<string>  selects, Expression<Func<TModel, bool>> predicate);

    IEnumerable<TModel> Get<TProperty>(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes);
    Task<IEnumerable<TModel>> GetAsync<TProperty>(Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes);
    IEnumerable<TModel> Get(Expression<Func<TModel, bool>>? predicate);
    Task<IEnumerable<TModel>> GetAsync(Expression<Func<TModel, bool>>? predicate);
    
    IEnumerable<TModel> GetWithOrder<TProperty>(Expression<Func<TModel, TProperty>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes);
    Task<IEnumerable<TModel>> GetWithOrderAsync<TProperty>(Expression<Func<TModel, TProperty>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes);
    
    int Count(Expression<Func<TModel, bool>>? predicate);
    Task<int> CountAsync(Expression<Func<TModel, bool>>? predicate);
    
    Paginate<TModel> Paginate<TProperty>(int pageNum, int take, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes);
    Task<Paginate<TModel>> PaginateAsync<TProperty>(int pageNum, int take, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes);
    Paginate<TModel> Paginate(int pageNum, int take, Expression<Func<TModel, bool>>? predicate);
    Task<Paginate<TModel>> PaginateAsync(int pageNum, int take, Expression<Func<TModel, bool>>? predicate);
    
    Paginate<TModel> PaginateWithOrder<TProperty>(int pageNum, int take, Expression<Func<TModel, TProperty>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes);
    Task<Paginate<TModel>> PaginateWithOrderAsync<TProperty>(int pageNum, int take, Expression<Func<TModel, TProperty>> orderPropertity, QueryOrders order, Expression<Func<TModel, bool>>? predicate, params Expression<Func<TModel, TProperty>>[] includes);

    bool Exists(Expression<Func<TModel, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<TModel, bool>> predicate);
    
    IQueryable<TModel> Query();
}