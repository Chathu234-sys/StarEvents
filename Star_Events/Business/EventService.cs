using Star_Events.Data.Entities;
using Star_Events.Repositories;
using Star_Events.Business.Interfaces;

namespace Star_Events.Business.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        // ----------------- Event CRUD -----------------

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
            => await _eventRepository.GetAllAsync();

        public async Task<Event?> GetEventByIdAsync(Guid id)
            => await _eventRepository.GetByIdAsync(id);

        public async Task CreateEventAsync(Event ev)
            => await _eventRepository.AddAsync(ev);

        public async Task UpdateEventAsync(Event ev)
            => await _eventRepository.UpdateAsync(ev);

        public async Task DeleteEventAsync(Guid id)
            => await _eventRepository.DeleteAsync(id);

        // ----------------- Ticket Types -----------------

        public async Task<IEnumerable<TicketType>> GetTicketTypesAsync(Guid eventId)
            => await _eventRepository.GetTicketTypesByEventIdAsync(eventId);

        // Satisfy interface member naming used by IEventService
        public async Task<IEnumerable<TicketType>> GetTicketTypesByEventAsync(Guid eventId)
            => await _eventRepository.GetTicketTypesByEventIdAsync(eventId);

        public async Task AddTicketTypeAsync(TicketType type)
            => await _eventRepository.AddTicketTypeAsync(type);

        // ----------------- Ticket Sales -----------------

        public async Task RecordSaleAsync(TicketSale sale)
            => await _eventRepository.RecordSaleAsync(sale);

        public async Task<decimal> GetTotalRevenueAsync(Guid eventId)
            => await _eventRepository.GetTotalRevenueAsync(eventId);
    }
}
