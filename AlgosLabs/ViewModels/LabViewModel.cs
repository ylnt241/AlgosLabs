using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgosLabs.ViewModels;

/// <summary>
///     Описание лабораторной работы в главном меню: кнопка верхней навигации и её разделы.
/// </summary>
public partial class LabViewModel : ObservableObject
{
    /// <summary>Текст кнопки верхней навигации (например, «1 Лаба»).</summary>
    public required string Title { get; init; }

    /// <summary>Разделы лабы, отображаемые в нижней навигации.</summary>
    public ObservableCollection<LabSectionViewModel> Sections { get; } = new();

    /// <summary>Последний открытый раздел — восстанавливается при возврате к лабе.</summary>
    [ObservableProperty] private LabSectionViewModel? _selectedSection;

    /// <summary>Признак выбранной лабы (для переключателя в верхней навигации).</summary>
    [ObservableProperty] private bool _isSelected;
}

/// <summary>
///     Раздел лабораторной работы, отображаемый кнопкой в нижней навигации.
/// </summary>
public partial class LabSectionViewModel : ObservableObject
{
    /// <summary>Идентификатор раздела, по которому находится его представление.</summary>
    public required string Id { get; init; }

    /// <summary>Текст кнопки раздела.</summary>
    public required string Title { get; init; }

    /// <summary>
    ///     Описание раздела. Заполняется у разделов-заглушек, у которых пока нет реализации.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>Признак выбранного раздела (для переключателя в нижней навигации).</summary>
    [ObservableProperty] private bool _isSelected;
}
