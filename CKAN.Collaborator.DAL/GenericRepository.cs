using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using System.Data.Entity.Core.Objects;

namespace CKAN.Collaborator.DAL
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {

        internal CKANDBEntities _context;
        internal DbSet _dbSet;
        private bool disposed = false;

        public GenericRepository(CKANDBEntities context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        /// <summary>
        /// Generic method to get all records for the entity
        /// </summary>
        public IQueryable<TEntity> GetAll()
        {
            return _dbSet.OfType<TEntity>();
        }

        /// <summary>
        ///  Generic method to get active records for the entity
        /// </summary>
        public IQueryable<TEntity> GetActive()
        {
            return GetByProperty("IsActive", true);
        }

        public IQueryable<TEntity> GetByProperty(string property, object value)
        {
            PropertyInfo propertyName = typeof(TEntity).GetProperty(property);
            var filter = FilterByProperty<TEntity>(propertyName, value);
            return _dbSet.OfType<TEntity>().Where(filter);
        }

        /// <summary>
        /// Generic method to get specific records.
        /// </summary>
        public TEntity GetByID(object id)
        {
            return (TEntity)_dbSet.Find(id);
        }

        //public virtual string SPSave(string sql, object[] objParams)
        //{
        //    return _context.Database.SqlQuery<TEntity>(sql, objParams).ToString();
        //}

        public virtual ObjectResult<string> SPSave(int VaultID, string Collabemail, int UserID, int CollabID, string Token, string IPAddress)
        {
            return _context.SpCreateCollaborator(VaultID, Collabemail, UserID, CollabID, Token, IPAddress);
            //return _context.Database.SqlQuery<TEntity>(sql, objParams).ToString();
        }

        public virtual ObjectResult<string> SPDelete(int ID, int UserID, string IPAddress)
        {
            return _context.SpDeleteCollaborator(ID, UserID, IPAddress);
        }

        public virtual ObjectResult<string> SPVerify(int VaultID, string CollabEmail, string Token)
        {
            return _context.SpVerifyCollaborator(VaultID, CollabEmail, Token);
        }

        /// <summary>
        /// Generic method to insert new record
        /// </summary>
        public virtual void Insert(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        /// <summary>
        /// Generic method to insert bulk records
        /// </summary>
        public virtual void InsertBulk(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
        }

        /// <summary>
        /// Generic method to delete the record
        /// </summary>
        public virtual void Delete(object id)
        {
            TEntity entityToDelete = (TEntity)_dbSet.Find(id);
            _dbSet.Remove(entityToDelete);
        }

        /// <summary>
        /// Generic method to delete bulk records
        /// </summary>
        public virtual void DeleteBulk(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }
        /// <summary>
        /// Generic method to update the record
        /// </summary>
        public virtual void Update(TEntity entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        /// <summary>
        /// Method to return filter item for given property and value
        /// </summary>
        private static Expression<Func<T, bool>> FilterByProperty<T>(PropertyInfo propertyName, object value)
        {
            var item = Expression.Parameter(typeof(T), "item");
            var propertyValue = Expression.Property(item, propertyName);
            var body = Expression.Equal(propertyValue, Expression.Constant(value));
            return Expression.Lambda<Func<T, bool>>(body, item);
        }

        /// <summary>
        /// Method to release resource
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }


    }
}
