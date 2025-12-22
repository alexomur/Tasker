using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tasker.Auth.Application.Abstractions.Persistence;
using Tasker.Auth.Infrastructure.Integration;
using Tasker.Shared.Kafka.Interfaces;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.Auth.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _db;
    private readonly IEventProducer _eventProducer;
    private readonly IAuthDomainEventMapper _mapper;
    private readonly ILogger<UnitOfWork> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public UnitOfWork(
        AuthDbContext db,
        IEventProducer eventProducer,
        IAuthDomainEventMapper mapper,
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

        foreach (var message in integrationMessages)
        {
            var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(message.Payload, JsonOptions);

            try
            {
                await _eventProducer.ProduceAsync(message.Topic, message.Key, payloadBytes, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish integration event to topic {Topic} with key {Key}",
                    message.Topic,
                    message.Key);
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
