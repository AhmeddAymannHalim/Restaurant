using System.Linq.Expressions;

namespace Service.IRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();
        IEnumerable<T> GetAll(params Func<IQueryable<T>, IQueryable<T>>[] includes);
        T GetById(int id);
        T GetById(int? id);

        Task<T> GetByIdAsync(int id);
        T GetById(Expression<Func<T, bool>> criteria, string[] includes = null, string[] thenIncludes = null);

        Task AddAsync(T entity);
        void Delete(T entity);
        void Update(T entity);
        Task UpdateAsync(T entity);

        // This is a new code . writen by Daif 
        T Find(Expression<Func<T, bool>> match, string[] include = null);
        IEnumerable<T> FindAll(Expression<Func<T, bool>> criteria, string[] includes = null);
        Task<T> GetLast<TKey>(Expression<Func<T, TKey>> orderBy);


        void AddRange(IEnumerable<T> entities);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateRangeAsync(IEnumerable<T> entities);



        IEnumerable<T> GetAll(Expression<Func<T, bool>> perdicate = null, string Includeword = null);
        T GetFirstorDefault(Expression<Func<T, bool>> perdicate = null, string Includeword = null);
    }
}
