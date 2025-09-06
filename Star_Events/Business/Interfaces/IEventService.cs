using Star_Events.Data.Entities;

namespace Star_Events.Business.Interfaces
{
    

    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<Event?> GetEventByIdAsync(Guid id);
        Task CreateEventAsync(Event ev);
        Task UpdateEventAsync(Event ev);
        Task DeleteEventAsync(Guid id);

        // NEW
        Task<IEnumerable<TicketType>> GetTicketTypesByEventAsync(Guid eventId);
        Task AddTicketTypeAsync(TicketType type);
        Task RecordSaleAsync(TicketSale sale);
        Task<decimal> GetTotalRevenueAsync(Guid eventId);
    }

}
