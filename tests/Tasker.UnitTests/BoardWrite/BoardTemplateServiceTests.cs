using FluentAssertions;
using Tasker.BoardWrite.Application.Boards.Templates;
using Tasker.BoardWrite.Domain.Boards;

namespace Tasker.UnitTests.BoardWrite;

public class BoardTemplateServiceTests
{
    private readonly BoardTemplateService _service = new();

    private static Board CreateEmptyBoard()
    {
        var ownerId = Guid.NewGuid();
        var now = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        return Board.Create("Test board", ownerId, now);
    }

    [Fact]
    public void GetTemplates_ShouldReturnAllKnownTemplateCodes()
    {
        var templates = _service.GetTemplates();

        templates.Should().HaveCount(3);

        var codes = templates
            .Select(t => t.Code)
            .ToArray();

        codes.Should().BeEquivalentTo(new[]
        {
            BoardTemplateCodes.SoftwareKanban,
            BoardTemplateCodes.GameDevFeature,
            BoardTemplateCodes.GameDevContent
        });
    }

    [Fact]
    public void ApplyTemplate_ShouldDoNothing_WhenTemplateCodeIsNullOrWhitespace()
    {
        var board = CreateEmptyBoard();
        var now = DateTimeOffset.UtcNow;
        var ownerId = board.OwnerUserId;

        _service.ApplyTemplate(board, null, ownerId, now);

        board.Columns.Should().BeEmpty();
        board.Labels.Should().BeEmpty();

        _service.ApplyTemplate(board, "   ", ownerId, now);

        board.Columns.Should().BeEmpty();
        board.Labels.Should().BeEmpty();
    }

    [Fact]
    public void ApplyTemplate_ShouldNotOverrideExistingColumns()
    {
        var board = CreateEmptyBoard();
        var now = DateTimeOffset.UtcNow;

        board.AddColumn("Existing", now);

        _service.ApplyTemplate(board, BoardTemplateCodes.SoftwareKanban, board.OwnerUserId, now);

        board.Columns.Should().HaveCount(1);
        board.Columns.Single().Title.Should().Be("Existing");
        board.Labels.Should().BeEmpty();
    }

    [Fact]
    public void ApplyTemplate_ShouldNotOverrideExistingLabels()
    {
        var board = CreateEmptyBoard();
        var now = DateTimeOffset.UtcNow;

        board.AddLabel("Existing label", "#000000", null);

        _service.ApplyTemplate(board, BoardTemplateCodes.SoftwareKanban, board.OwnerUserId, now);

        board.Columns.Should().BeEmpty();
        board.Labels.Should().HaveCount(1);
        board.Labels.Single().Title.Should().Be("Existing label");
    }

    [Fact]
    public void ApplyTemplate_SoftwareKanban_ShouldCreateExpectedColumnsAndLabels()
    {
        var board = CreateEmptyBoard();
        var now = DateTimeOffset.UtcNow;

        _service.ApplyTemplate(board, BoardTemplateCodes.SoftwareKanban, board.OwnerUserId, now);

        var columns = board.Columns
            .OrderBy(c => c.Order)
            .ToArray();

        columns.Should().HaveCount(5);

        columns.Select(c => c.Title).Should().Equal(
            "Backlog",
            "To Do",
            "In Progress",
            "In Review",
            "Done"
        );

        columns.Select(c => c.Order).Should().Equal(0, 1, 2, 3, 4);

        var labels = board.Labels.ToArray();
        labels.Should().HaveCount(4);

        labels.Select(l => l.Title).Should().BeEquivalentTo(new[]
        {
            "Bug",
            "Feature",
            "Tech Debt",
            "Research"
        });

        labels.Single(l => l.Title == "Bug").Color.Should().Be("#d32f2f");
        labels.Single(l => l.Title == "Feature").Color.Should().Be("#1976d2");
        labels.Single(l => l.Title == "Tech Debt").Color.Should().Be("#5d4037");
        labels.Single(l => l.Title == "Research").Color.Should().Be("#7b1fa2");
    }

    [Fact]
    public void ApplyTemplate_GameDevFeature_ShouldCreateExpectedColumnsAndLabels()
    {
        var board = CreateEmptyBoard();
        var now = DateTimeOffset.UtcNow;

        _service.ApplyTemplate(board, BoardTemplateCodes.GameDevFeature, board.OwnerUserId, now);

        var columns = board.Columns
            .OrderBy(c => c.Order)
            .ToArray();

        columns.Should().HaveCount(6);

        columns.Select(c => c.Title).Should().Equal(
            "Ideas",
            "Design in progress",
            "Design ready",
            "Implementation",
            "Playtest",
            "Ready for release"
        );

        var labels = board.Labels.ToArray();
        labels.Should().HaveCount(5);

        labels.Select(l => l.Title).Should().BeEquivalentTo(new[]
        {
            "Core mechanic",
            "Content",
            "UI/UX",
            "Balancing",
            "Tech"
        });
    }

    [Fact]
    public void ApplyTemplate_GameDevContent_ShouldCreateExpectedColumnsAndLabels()
    {
        var board = CreateEmptyBoard();
        var now = DateTimeOffset.UtcNow;

        _service.ApplyTemplate(board, BoardTemplateCodes.GameDevContent, board.OwnerUserId, now);

        var columns = board.Columns
            .OrderBy(c => c.Order)
            .ToArray();

        columns.Should().HaveCount(6);

        columns.Select(c => c.Title).Should().Equal(
            "Concept",
            "Blockout / Prototype",
            "Production",
            "Integration",
            "QA / Polishing",
            "Released"
        );

        var labels = board.Labels.ToArray();
        labels.Should().HaveCount(5);

        labels.Select(l => l.Title).Should().BeEquivalentTo(new[]
        {
            "Environment",
            "Character",
            "VFX",
            "UI Art",
            "Optimization"
        });
    }
}
