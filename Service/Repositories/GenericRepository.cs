using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Service.IRepositories;
using Restaurant.Data;

namespace Service.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var items = await _context.Set<T>().ToListAsync();
            return items;
        }

        public IEnumerable<T> GetAll()
        {
            var items = _context.Set<T>().ToList();
            return items;
        }

        public IEnumerable<T> GetAll(params Func<IQueryable<T>, IQueryable<T>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            // Apply includes
            foreach (var include in includes)
            {
                query = include(query);
            }

            return query.ToList();

        }

        public async Task<T> GetLast<TKey>(Expression<Func<T, TKey>> orderBy)
        {
            var lastItem = await _context.Set<T>().OrderByDescending(orderBy).FirstOrDefaultAsync();
            return lastItem;
        }

        public T GetById(int id)
        {
            var item = _context.Set<T>().Find(id);
            return item;
        }

        public T GetById(int? id)
        {
            if (id == null)
            {
                return null;
            }
            var item = _context.Set<T>().Find(id);
            return item;
        }

        public async Task<T> GetByIdAsync(int id)
        {
            var item = await _context.Set<T>().FindAsync(id);
            return item;
        }

        public T GetById(Expression<Func<T, bool>> criteria, string[] includes = null, string[] thenIncludes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (thenIncludes != null)
            {
                foreach (var include in thenIncludes)
                {
                    query = query.Include(include);
                }
            }

            return query.SingleOrDefault(criteria);
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
            _context.SaveChanges();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public T Find(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var incluse in includes)
                    query = query.Include(incluse);

            return query.SingleOrDefault(criteria);
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return query.Where(criteria).ToList();
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
            _context.SaveChanges();
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _context.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> perdicate = null, string Includeword = null)
        {

            IQueryable<T> query = _context.Set<T>();
            if (perdicate != null)
            {
                query = query.Where(perdicate);
            }
            if (Includeword != null)
            {

                foreach (var item in Includeword.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            return query.ToList();
        }

        public T GetFirstorDefault(Expression<Func<T, bool>> perdicate = null, string Includeword = null)
        {

            IQueryable<T> query = _context.Set<T>();
            if (perdicate != null)
            {
                query = query.Where(perdicate);
            }
            if (Includeword != null)
            {

                foreach (var item in Includeword.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            return query.SingleOrDefault();
        }

        public bool Any()
        {
            return _context.Set<T>().Any();
        }

        public bool Any(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return _context.Set<T>().Any();
            }
            else
            {
                return _context.Set<T>().Any(predicate);
            }
        }

        //public IEnumerable<T> GetBetweenDates(Func<T, DateTime> dateSelector, DateTime startDate, DateTime endDate)
        //{
        //    return _context.Set<T>()
        //        .Where(e => dateSelector(e) >= startDate && dateSelector(e) <= endDate)
        //        .ToList();
        //}



        //public IEnumerable<T> GetEntitiesBetweenDates(
        //      Func<T, DateTime> dateSelector,
        //      DateTime startDate,
        //      DateTime endDate,
        //      Func<IQueryable<T>, IQueryable<T>> include = null,
        //      Func<IQueryable<T>, IQueryable<T>> thenInclude = null)
        //{
        //    IQueryable<T> query = _context.Set<T>();

        //    if (include != null)
        //    {
        //        query = include(query);
        //    }

        //    if (thenInclude != null)
        //    {
        //        query = thenInclude(query);
        //    }

        //    query = query.Where(e => dateSelector(e) >= startDate && dateSelector(e) <= endDate);

        //    return query.ToList();
        //}



    }
}
