﻿using Domain.Entities;
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
        private IGenericRepository<Event> _eventRepository;
        private IGenericRepository<TimeEvent> _timeEventRepository;
        private IGenericRepository<EventVoucher> _eventVoucherRepository;
        private IGenericRepository<BarTime> _barTimeRepository;
        private IGenericRepository<Token> _tokenRepository;
        private IGenericRepository<FcmNotification> _fcmNotification;
        private IGenericRepository<FcmNotificationCustomer> _fcmNotificationCustomer;
        private IGenericRepository<FcmUserDevice> _fcmUserDevice;
        private IGenericRepository<BookingExtraDrink> _bookingExtraDrinkRepository;
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

        public IGenericRepository<Event> EventRepository
        {
            get
            {

                if (_eventRepository == null)
                {
                    _eventRepository = new GenericRepository<Event>(_context);
                }
                return _eventRepository;
            }
        }

        public IGenericRepository<TimeEvent> TimeEventRepository
        {
            get
            {

                if (_timeEventRepository == null)
                {
                    _timeEventRepository = new GenericRepository<TimeEvent>(_context);
                }
                return _timeEventRepository;
            }
        }

        public IGenericRepository<EventVoucher> EventVoucherRepository
        {
            get
            {

                if (_eventVoucherRepository == null)
                {
                    _eventVoucherRepository = new GenericRepository<EventVoucher>(_context);
                }
                return _eventVoucherRepository;
            }
        }

        public IGenericRepository<BarTime> BarTimeRepository
        {
            get
            {

                if (_barTimeRepository == null)
                {
                    _barTimeRepository = new GenericRepository<BarTime>(_context);
                }
                return _barTimeRepository;
            }
        }
        public IGenericRepository<Token> TokenRepository
        {
            get
            {

                if (_tokenRepository == null)
                {
                    _tokenRepository = new GenericRepository<Token>(_context);
                }
                return _tokenRepository;
            }
        }

        public IGenericRepository<FcmNotification> FcmNotificationRepository
        {
            get
            {
                if (_fcmNotification == null) {
                    _fcmNotification = new GenericRepository<FcmNotification>(_context);
                }
                return _fcmNotification;
            }
        }

        public IGenericRepository<FcmNotificationCustomer> FcmNotificationCustomerRepository
        {
            get
            {
                if (_fcmNotificationCustomer == null)
                {
                    _fcmNotificationCustomer = new GenericRepository<FcmNotificationCustomer>(_context);
                }
                return _fcmNotificationCustomer;
            }
        }

        public IGenericRepository<FcmUserDevice> FcmUserDeviceRepository
        {
            get
            {
                if (_fcmUserDevice == null)
                {
                    _fcmUserDevice = new GenericRepository<FcmUserDevice>(_context);
                }
                return _fcmUserDevice;
            }
        }

        public IGenericRepository<BookingExtraDrink> BookingExtraDrinkRepository
        {
            get
            {
                if (_bookingExtraDrinkRepository == null)
                {
                    _bookingExtraDrinkRepository = new GenericRepository<BookingExtraDrink>(_context);
                }
                return _bookingExtraDrinkRepository;
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
