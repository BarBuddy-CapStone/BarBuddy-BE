using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private MyDbContext _context = new MyDbContext();

        //private IGenericRepository<Animal> _animalRepository;


        public UnitOfWork()
        {
        }
        //public IGenericRepository<Animal> AnimalRepository
        //{
        //    get
        //    {

        //        if (_animalRepository == null)
        //        {
        //            _animalRepository = new GenericRepository<Animal>(_context);
        //        }
        //        return _animalRepository;
        //    }
        //}


        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async Task DisposeAsync(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_context != null)
                    {
                        await _context.DisposeAsync();
                    }
                }
            }
            disposed = true;
        }

        public async Task DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _context.Database.CommitTransaction();
        }

        public void RollBack()
        {
            _context.Database.RollbackTransaction();
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            return new GenericRepository<T>(_context);
        }
    }
}
