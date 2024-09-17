using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        //public IGenericRepository<Animal> AnimalRepository { get; }

        void Save();
        Task SaveAsync();
        void Dispose();
        Task DisposeAsync();
        void BeginTransaction();
        void CommitTransaction();
        void RollBack();
    }
}
