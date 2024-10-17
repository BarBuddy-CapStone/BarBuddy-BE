using Domain.Entities;
using Domain.IRepository;
using Persistence.Data;

namespace Persistence.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private MyDbContext _context = new MyDbContext();

        private IGenericRepository<Account> _accountRepository;
        private IGenericRepository<Bar> _barRepository;
        private IGenericRepository<Booking> _bookingRepository;
        private IGenericRepository<BookingDrink> _bookingDrinkRepository;
        private IGenericRepository<BookingTable> _bookingTableRepository;
        private IGenericRepository<Drink> _drinkRepository;
        private IGenericRepository<DrinkCategory> _drinkCategoryRepository;
        private IGenericRepository<DrinkEmotionalCategory> _drinkEmotionalCategoryRepository;
        private IGenericRepository<EmotionalDrinkCategory> _emotionalDrinkCategoryRepository;
        private IGenericRepository<Feedback> _feedbackRepository;
        private IGenericRepository<PaymentHistory> _paymentHistoryRepository;
        private IGenericRepository<Role> _roleRepository;
        private IGenericRepository<Table> _tableRepository;
        private IGenericRepository<TableType> _tableTypeRepository;
        private IGenericRepository<Notification> _notificationRepository;
        private IGenericRepository<NotificationDetail> _notificationDetailRepository;


        public UnitOfWork()
        {
        }

        public IGenericRepository<Account> AccountRepository
        {
            get
            {

                if (_accountRepository == null)
                {
                    _accountRepository = new GenericRepository<Account>(_context);
                }
                return _accountRepository;
            }
        }

        public IGenericRepository<Bar> BarRepository
        {
            get
            {

                if (_barRepository == null)
                {
                    _barRepository = new GenericRepository<Bar>(_context);
                }
                return _barRepository;
            }
        }

        public IGenericRepository<Booking> BookingRepository
        {
            get
            {

                if (_bookingRepository == null)
                {
                    _bookingRepository = new GenericRepository<Booking>(_context);
                }
                return _bookingRepository;
            }
        }

        public IGenericRepository<BookingDrink> BookingDrinkRepository
        {
            get
            {

                if (_bookingDrinkRepository == null)
                {
                    _bookingDrinkRepository = new GenericRepository<BookingDrink>(_context);
                }
                return _bookingDrinkRepository;
            }
        }

        public IGenericRepository<BookingTable> BookingTableRepository
        {
            get
            {

                if (_bookingTableRepository == null)
                {
                    _bookingTableRepository = new GenericRepository<BookingTable>(_context);
                }
                return _bookingTableRepository;
            }
        }

        public IGenericRepository<Drink> DrinkRepository
        {
            get
            {

                if (_drinkRepository == null)
                {
                    _drinkRepository = new GenericRepository<Drink>(_context);
                }
                return _drinkRepository;
            }
        }

        public IGenericRepository<DrinkCategory> DrinkCategoryRepository
        {
            get
            {

                if (_drinkCategoryRepository == null)
                {
                    _drinkCategoryRepository = new GenericRepository<DrinkCategory>(_context);
                }
                return _drinkCategoryRepository;
            }
        }

        public IGenericRepository<DrinkEmotionalCategory> DrinkEmotionalCategoryRepository
        {
            get
            {

                if (_drinkEmotionalCategoryRepository == null)
                {
                    _drinkEmotionalCategoryRepository = new GenericRepository<DrinkEmotionalCategory>(_context);
                }
                return _drinkEmotionalCategoryRepository;
            }
        }

        public IGenericRepository<EmotionalDrinkCategory> EmotionalDrinkCategoryRepository
        {
            get
            {

                if (_emotionalDrinkCategoryRepository == null)
                {
                    _emotionalDrinkCategoryRepository = new GenericRepository<EmotionalDrinkCategory>(_context);
                }
                return _emotionalDrinkCategoryRepository;
            }
        }

        public IGenericRepository<Feedback> FeedbackRepository
        {
            get
            {

                if (_feedbackRepository == null)
                {
                    _feedbackRepository = new GenericRepository<Feedback>(_context);
                }
                return _feedbackRepository;
            }
        }

        public IGenericRepository<PaymentHistory> PaymentHistoryRepository
        {
            get
            {

                if (_paymentHistoryRepository == null)
                {
                    _paymentHistoryRepository = new GenericRepository<PaymentHistory>(_context);
                }
                return _paymentHistoryRepository;
            }
        }

        public IGenericRepository<Role> RoleRepository
        {
            get
            {

                if (_roleRepository == null)
                {
                    _roleRepository = new GenericRepository<Role>(_context);
                }
                return _roleRepository;
            }
        }

        public IGenericRepository<Table> TableRepository
        {
            get
            {

                if (_tableRepository == null)
                {
                    _tableRepository = new GenericRepository<Table>(_context);
                }
                return _tableRepository;
            }
        }

        public IGenericRepository<TableType> TableTypeRepository
        {
            get
            {

                if (_tableTypeRepository == null)
                {
                    _tableTypeRepository = new GenericRepository<TableType>(_context);
                }
                return _tableTypeRepository;
            }
        }

        public IGenericRepository<Notification> NotificationRepository
        {
            get
            {

                if (_notificationRepository == null)
                {
                    _notificationRepository = new GenericRepository<Notification>(_context);
                }
                return _notificationRepository;
            }
        }


        public IGenericRepository<NotificationDetail> NotificationDetailRepository
        {
            get
            {

                if (_notificationDetailRepository == null)
                {
                    _notificationDetailRepository = new GenericRepository<NotificationDetail>(_context);
                }
                return _notificationDetailRepository;
            }
        }
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
