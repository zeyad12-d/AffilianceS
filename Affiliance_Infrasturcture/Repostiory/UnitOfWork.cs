using Affiliance_core.interfaces;
using Affiliance_Infrasturcture.Data;

namespace Affiliance_Infrasturcture.Repostiory
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly AffiliancesDBcontext _context;
        private bool disposed = false;

       
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(AffiliancesDBcontext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

          
            if (_repositories.ContainsKey(type))
            {
                return (IGenericRepository<T>)_repositories[type];
            }

            
            var repository = new GenericRepository<T>(_context);
            _repositories[type] = repository;

            return repository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

     
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
           
            GC.SuppressFinalize(this);
        }
    }
}
