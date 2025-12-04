using Cassandra;
using Tasker.Shared.Kernel.Abstractions.ReadModel;

namespace Tasker.Shared.ReadModel.Cassandra;

public sealed class CassandraBoardSnapshotStore : IBoardSnapshotStore
{
    private readonly ISession _session;
    private readonly PreparedStatement _getStmt;
    private readonly PreparedStatement _upsertStmt;

    private const string Keyspace = "tasker_read";
    private const string Table = "board_snapshots";

    public CassandraBoardSnapshotStore(ISession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _getStmt = _session.Prepare($"SELECT payload FROM {Keyspace}.{Table} WHERE board_id = ?");
        _upsertStmt = _session.Prepare($"INSERT INTO {Keyspace}.{Table} (board_id, payload) VALUES (?, ?) USING TTL ?");
    }

    public async Task<string?> TryGetAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        var bound = _getStmt.Bind(boardId);
        var rs = await _session.ExecuteAsync(bound).ConfigureAwait(false);
        var row = rs.FirstOrDefault();

        return row?.GetValue<string>("payload");
    }

    public async Task UpsertAsync(Guid boardId, string payloadJson, int ttlSeconds, CancellationToken cancellationToken = default)
    {
        var bound = _upsertStmt.Bind(boardId, payloadJson, ttlSeconds);
        await _session.ExecuteAsync(bound).ConfigureAwait(false);
    }
}