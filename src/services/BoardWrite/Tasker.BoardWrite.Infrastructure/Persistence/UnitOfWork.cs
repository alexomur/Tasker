using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tasker.BoardWrite.Infrastructure.Integration;
using Tasker.Shared.Kafka.Interfaces;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BoardWriteDbContext _db;
    private readonly IEventProducer _eventProducer;
    private readonly IDomainEventToIntegrationEventMapper _mapper;
    private readonly ILogger<UnitOfWork> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public UnitOfWork(
        BoardWriteDbContext db,
        IEventProducer eventProducer,
        IDomainEventToIntegrationEventMapper mapper,
        ILogger<UnitOfWork> logger)
    {
        _db = db;
        _eventProducer = eventProducer;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = _db.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();

        var integrationMessages = domainEntities
            .SelectMany(e => e.Entity.DomainEvents)
            .SelectMany(_mapper.Map)
            .ToList();

        var result = await _db.SaveChangesAsync(cancellationToken);

        foreach (var msg in integrationMessages)
        {
            var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(msg.Payload, JsonOptions);

            try
            {
                await _eventProducer.ProduceAsync(msg.Topic, msg.Key, payloadBytes, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish integration event to topic {Topic} with key {Key}",
                    msg.Topic,
                    msg.Key);
                throw;
            }
        }

        foreach (var entityEntry in domainEntities)
        {
            entityEntry.Entity.ClearDomainEvents();
        }

        return result;
    }
}
