using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        public IGenericRepository<Account> AccountRepository { get; }
        public IGenericRepository<Bar> BarRepository { get; }
        public IGenericRepository<Booking> BookingRepository { get; }
        public IGenericRepository<BookingDrink> BookingDrinkRepository { get; }
        public IGenericRepository<BookingTable> BookingTableRepository { get; }
        public IGenericRepository<Drink> DrinkRepository { get; }
        public IGenericRepository<DrinkCategory> DrinkCategoryRepository { get; }
        public IGenericRepository<DrinkEmotionalCategory> DrinkEmotionalCategoryRepository { get; }
        public IGenericRepository<EmotionalDrinkCategory> EmotionalDrinkCategoryRepository { get; }
        public IGenericRepository<Feedback> FeedbackRepository { get; }
        public IGenericRepository<PaymentHistory> PaymentHistoryRepository { get; }
        public IGenericRepository<Role> RoleRepository { get; }
        public IGenericRepository<Table> TableRepository { get; }
        public IGenericRepository<TableType> TableTypeRepository { get; }
        public IGenericRepository<Notification> NotificationRepository { get; }
        public IGenericRepository<NotificationDetail> NotificationDetailRepository { get; }
        public IGenericRepository<Event> EventRepository { get; }
        public IGenericRepository<TimeEvent> TimeEventRepository { get; }
        public IGenericRepository<EventVoucher> EventVoucherRepository { get; }
        public IGenericRepository<BarTime> BarTimeRepository { get; }

        void Save();
        Task SaveAsync();
        void Dispose();
        Task DisposeAsync();
        void BeginTransaction();
        void CommitTransaction();
        void RollBack();
    }
}
