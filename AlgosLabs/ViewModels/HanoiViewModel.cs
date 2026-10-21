using System;
using System.Collections.Generic;
using System.Linq;
using Algorithms.Lab2.Hanoi;
using AlgosLabs.Models;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgosLabs.ViewModels;

/// <summary>
///     Состояние и управление визуализацией задачи «Ханойские башни»:
///     пошаговые переходы, откат, автоматическое воспроизведение и регулировка скорости.
/// </summary>
public partial class HanoiViewModel : ObservableObject
{
    /// <summary>Ограничение — 20 дисков: это 2^20 − 1 = 1 048 575 шагов.</summary>
    public const int MaxDiskCount = 20;

    private const double MaxSpeedLimit = 1000;

    /// <summary>Нижняя граница интервала таймера: не чаще ~60 обновлений в секунду.</summary>
    private const double MinTickSeconds = 1.0 / 60;

    private readonly DispatcherTimer _timer;
    private readonly List<int>[] _rods = { new(), new(), new() };
    private List<HanoiMove> _moves = new();

    /// <summary>Остаток шага между тиками таймера: иначе скорость квантовалась бы по кадрам.</summary>
    private double _stepAccumulator;

    public HanoiViewModel()
    {
        _timer = new DispatcherTimer();
        _timer.Tick += (_, _) => AdvanceAutomatically();
        Rebuild();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HanoiPlayButtonText))]
    private int _hanoiDiskCount = 5;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HanoiSpeed))]
    [NotifyPropertyChangedFor(nameof(HanoiSpeedText))]
    private double _hanoiSpeedPercent = 25;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HanoiPlayButtonText))]
    private bool _isHanoiPlaying;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanHanoiStepBack))]
    [NotifyPropertyChangedFor(nameof(CanHanoiStepForward))]
    [NotifyPropertyChangedFor(nameof(HanoiPlayButtonText))]
    private int _hanoiStepIndex;

    [ObservableProperty] private string _hanoiStepInfo = string.Empty;
    [ObservableProperty] private string _hanoiMoveInfo = string.Empty;
    [ObservableProperty] private HanoiState? _hanoiState;

    /// <summary>Можно ли откатить один ход назад.</summary>
    public bool CanHanoiStepBack => HanoiStepIndex > 0;

    /// <summary>Можно ли выполнить следующий ход.</summary>
    public bool CanHanoiStepForward => HanoiStepIndex < _moves.Count;

    /// <summary>Полное количество ходов в решении (2^N − 1).</summary>
    public int HanoiTotalSteps => _moves.Count;

    /// <summary>
    ///     Максимальная скорость для текущего N: не более половины всех шагов в секунду
    ///     и не более 1000 шагов в секунду.
    /// </summary>
    public double HanoiMaxSpeed => Math.Min(HanoiTotalSteps / 2.0, MaxSpeedLimit);

    /// <summary>Фактическая скорость в шагах в секунду: процент от максимума для текущего N.</summary>
    public double HanoiSpeed => HanoiMaxSpeed * HanoiSpeedPercent / 100.0;

    /// <summary>Подпись скорости рядом со слайдером: процент и абсолютное значение.</summary>
    public string HanoiSpeedText => $"{HanoiSpeedPercent:0}% · {HanoiSpeed:0.#} шаг/с";

    /// <summary>Текст главной кнопки воспроизведения.</summary>
    public string HanoiPlayButtonText => IsHanoiPlaying
        ? "Пауза"
        : HanoiStepIndex >= HanoiTotalSteps && HanoiTotalSteps > 0
            ? "Повторить"
            : "Играть";

    partial void OnHanoiDiskCountChanged(int value)
    {
        if (value < 1)
        {
            HanoiDiskCount = 1;
            return;
        }

        if (value > MaxDiskCount)
        {
            HanoiDiskCount = MaxDiskCount;
            return;
        }

        Rebuild();
    }

    partial void OnHanoiSpeedPercentChanged(double value)
    {
        UpdateTimerInterval();
    }

    /// <summary>Один ход вперёд вручную (автовоспроизведение останавливается).</summary>
    [RelayCommand]
    private void StepHanoiForward()
    {
        Pause();
        ApplyNextMove();
    }

    /// <summary>Один ход назад по уже выполненной последовательности.</summary>
    [RelayCommand]
    private void StepHanoiBack()
    {
        Pause();
        if (!CanHanoiStepBack) return;

        var move = _moves[HanoiStepIndex - 1];
        var target = _rods[move.To];

        // Сверху должен быть именно этот диск — иначе состояние уже разъехалось.
        if (target.Count == 0 || target[^1] != move.Disk) return;

        target.RemoveAt(target.Count - 1);
        _rods[move.From].Add(move.Disk);
        HanoiStepIndex--;

        UpdateState(isRollback: true);
    }

    /// <summary>Запуск/пауза автоматического воспроизведения.</summary>
    [RelayCommand]
    private void ToggleHanoiPlay()
    {
        if (IsHanoiPlaying)
        {
            Pause();
            return;
        }

        if (HanoiTotalSteps == 0) return;

        // Доиграли — начинаем заново.
        if (HanoiStepIndex >= HanoiTotalSteps)
        {
            ResetToStart();
            UpdateState();
        }

        IsHanoiPlaying = true;
        _stepAccumulator = 0;
        _timer.Start();
    }

    /// <summary>Возврат в начальное положение.</summary>
    [RelayCommand]
    private void ResetHanoi()
    {
        Pause();
        ResetToStart();
        UpdateState();
    }

    private void AdvanceAutomatically()
    {
        if (!IsHanoiPlaying) return;

        // Дробную часть переносим на следующий тик, чтобы скорость не терялась.
        _stepAccumulator += HanoiSpeed * _timer.Interval.TotalSeconds;

        var stepsToApply = (int)_stepAccumulator;
        if (stepsToApply <= 0) return;

        _stepAccumulator -= stepsToApply;

        var applied = false;
        for (var i = 0; i < stepsToApply && ApplyNextMove(updateState: false); i++) applied = true;

        if (applied) UpdateState();
        if (HanoiStepIndex >= _moves.Count) Pause();
    }

    /// <summary>Выполняет следующий ход. При <paramref name="updateState" /> = false состояние обновляет вызывающий.</summary>
    private bool ApplyNextMove(bool updateState = true)
    {
        if (!CanHanoiStepForward) return false;

        var move = _moves[HanoiStepIndex];
        var source = _rods[move.From];

        // Защита от рассинхронизации: переносить можно только верхний диск.
        if (source.Count == 0 || source[^1] != move.Disk) return false;

        source.RemoveAt(source.Count - 1);
        _rods[move.To].Add(move.Disk);
        HanoiStepIndex++;

        if (updateState) UpdateState();

        return true;
    }

    private void Pause()
    {
        _timer.Stop();
        _stepAccumulator = 0;
        if (IsHanoiPlaying) IsHanoiPlaying = false;
    }

    /// <summary>Строит новое решение при изменении N и возвращает башню в исходное положение.</summary>
    private void Rebuild()
    {
        Pause();
        _moves = TowerOfHanoi.Solve(HanoiDiskCount);
        ResetToStart();

        OnPropertyChanged(nameof(HanoiTotalSteps));
        OnPropertyChanged(nameof(HanoiMaxSpeed));
        OnPropertyChanged(nameof(HanoiSpeed));
        OnPropertyChanged(nameof(HanoiSpeedText));
        OnPropertyChanged(nameof(CanHanoiStepBack));
        OnPropertyChanged(nameof(CanHanoiStepForward));
        OnPropertyChanged(nameof(HanoiPlayButtonText));

        UpdateTimerInterval();
        UpdateState();
    }

    private void ResetToStart()
    {
        foreach (var rod in _rods) rod.Clear();

        // Снизу вверх: от самого большого диска к самому маленькому.
        for (var disk = HanoiDiskCount; disk >= 1; disk--) _rods[0].Add(disk);

        HanoiStepIndex = 0;
        _stepAccumulator = 0;
    }

    private void UpdateState(bool isRollback = false)
    {
        var snapshot = new int[HanoiState.RodCount][];
        for (var i = 0; i < _rods.Length; i++) snapshot[i] = _rods[i].ToArray();

        HanoiState = new HanoiState(HanoiDiskCount, snapshot, HanoiStepIndex);

        HanoiStepInfo = HanoiTotalSteps == 0
            ? "Ходов нет"
            : $"Шаг {HanoiStepIndex} из {HanoiTotalSteps}";

        if (HanoiStepIndex == 0)
        {
            HanoiMoveInfo = "Начальное положение";
            return;
        }

        var move = _moves[HanoiStepIndex - 1];
        var from = HanoiState.RodName(move.From);
        var to = HanoiState.RodName(move.To);

        HanoiMoveInfo = isRollback
            ? $"Откат: диск {move.Disk}, {to} → {from}"
            : $"Диск {move.Disk}: {from} → {to}";
    }

    private void UpdateTimerInterval()
    {
        var speed = Math.Max(HanoiSpeed, 0.001);
        _timer.Interval = TimeSpan.FromSeconds(Math.Max(1.0 / speed, MinTickSeconds));
    }
}
