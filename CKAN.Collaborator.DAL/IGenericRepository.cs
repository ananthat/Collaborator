using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CKAN.Collaborator.DAL
{
    interface IGenericRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();
        TEntity GetByID(object id);
        void Insert(TEntity entity);
        void Delete(object id);
        void Update(TEntity entity);
        ObjectResult<string> SPSave(int VaultID, string Collabemail, int UserID, int CollabID, string Token, string IPAddress);
        ObjectResult<string> SPDelete(int ID, int UserID, string IPAddress);
        ObjectResult<string> SPVerify(int VaultID, string CollabEmail, string Token);
        // void Save();
    }
}
