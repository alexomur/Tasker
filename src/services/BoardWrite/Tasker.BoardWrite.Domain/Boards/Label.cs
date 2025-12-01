using Tasker.Shared.Kernel.Abstractions;

namespace Tasker.BoardWrite.Domain.Boards;

/// <summary>
/// Метка, используемая для классификации и визуального выделения карточек на доске.
/// </summary>
public sealed class Label : Entity
{
    /// <summary>
    /// Название метки, отображаемое пользователю.
    /// </summary>
    public string Title { get; private set; } = null!;
    
    /// <summary>
    /// Дополнительное текстовое описание метки. Может отсутствовать.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Строковое представление цвета метки (например, HEX-код вида #RRGGBB).
    /// </summary>
    public string Color { get; private set; } = null!;
    
    protected Label() { }

    /// <summary>
    /// Создаёт новую метку с указанным названием, описанием и цветом.
    /// </summary>
    /// <param name="title">Название метки.</param>
    /// <param name="description">Описание метки, может быть пустым или null.</param>
    /// <param name="color">Цвет метки (например, HEX-код).</param>
    public Label(string title, string? description, string color)
    {
        SetTitle(title);
        SetDescription(description);
        SetColor(color);
    }

    /// <summary>
    /// Обновляет название метки.
    /// </summary>
    /// <param name="title">Новое название метки.</param>
    public void Rename(string title) => SetTitle(title);

    /// <summary>
    /// Обновляет описание метки.
    /// </summary>
    /// <param name="description">Новое описание метки, может быть пустым или null.</param>
    public void ChangeDescription(string? description) => SetDescription(description);

    /// <summary>
    /// Обновляет цвет метки.
    /// </summary>
    /// <param name="color">Новый цвет метки (например, HEX-код).</param>
    public void ChangeColor(string color) => SetColor(color);

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Название метки не может быть пустым.", nameof(title));
        }
        
        Title = title.Trim();
    }

    private void SetDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    private void SetColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            throw new ArgumentException("Цвет метки не может быть пустым.", nameof(color));
        }
        
        Color = color.Trim();
    }
}
