using Star_Events.Data.Entities;

namespace Star_Events.Repositories.Interfaces
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(Guid id);
        Task<IEnumerable<Event>> GetAllAsync();
        Task AddAsync(Event ev);
        Task UpdateAsync(Event ev);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<TicketType>> GetTicketTypesByEventIdAsync(Guid eventId);
        Task AddTicketTypeAsync(TicketType type);
        Task RecordSaleAsync(TicketSale sale);
        Task<decimal> GetTotalRevenueAsync(Guid eventId);
    }
}
