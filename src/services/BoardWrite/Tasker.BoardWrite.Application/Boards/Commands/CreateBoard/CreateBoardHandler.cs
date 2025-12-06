using MediatR;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Abstractions.ReadModel;
using Tasker.BoardWrite.Application.Abstractions.Services;
using Tasker.BoardWrite.Application.Boards.Templates;
using Tasker.BoardWrite.Domain.Boards;
using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Application.Boards.Commands.CreateBoard;

/// <summary>
/// Обработчик команды создания доски.
/// </summary>
public sealed class CreateBoardHandler
    : IRequestHandler<CreateBoardCommand, CreateBoardResult>
{
    private readonly IBoardRepository _boards;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IBoardReadModelWriter _boardReadModelWriter;
    private readonly IBoardTemplateService _boardTemplateService;

    public CreateBoardHandler(
        IBoardRepository boards,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IBoardReadModelWriter boardReadModelWriter,
        IBoardTemplateService boardTemplateService)
    {
        _boards = boards;
        _uow = uow;
        _currentUser = currentUser;
        _boardReadModelWriter = boardReadModelWriter;
        _boardTemplateService = boardTemplateService;
    }

    public async Task<CreateBoardResult> Handle(CreateBoardCommand cmd, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
        {
            throw new InvalidOperationException("Текущий пользователь не определён.");
        }

        var ownerUserId = _currentUser.UserId.Value;
        var now = DateTimeOffset.UtcNow;

        var board = Board.Create(
            title: cmd.Title,
            ownerUserId: ownerUserId,
            now: now,
            description: cmd.Description);

        _boardTemplateService.ApplyTemplate(board, cmd.TemplateCode, ownerUserId, now);

        await _boards.AddAsync(board, ct);
        await _uow.SaveChangesAsync(ct);
        await _boardReadModelWriter.RefreshBoardAsync(board.Id, ct);

        return new CreateBoardResult(board.Id);
    }
}
