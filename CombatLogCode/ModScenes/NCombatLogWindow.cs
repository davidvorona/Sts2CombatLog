using Godot;
using System.Collections.Immutable;

namespace Sts2CombatLog.CombatLogCode.ModScenes;

[GlobalClass]
public partial class NCombatLogWindow : Window
{
    private static readonly LimitedLog _log = new(512);
    private static readonly Lock _logLock = new();
    private static ImmutableList<NCombatLogWindow> _listeners = ImmutableList<NCombatLogWindow>.Empty;

    public enum CombatEntryType
    {
        Start,
        CardPlay,
        Damage,
        Block,
        Power,
        Potion,
        Misc
    }

    public static bool IsOpen => _listeners.Count > 0;

    public static void AddLog(string msg, CombatEntryType type)
    {
        lock (_logLock)
        {
            EnsureLogLimit();
            _log.Enqueue(msg, type);
        }

        foreach (var window in _listeners)
        {
            window.SetDirty();
        }
    }

    private ScrollContainer? _scrollContainer;
    private RichTextLabel? _logLabel;

    private bool _isFollowingLog = true;
    private int _currentFontSize; // Set on load
    private bool _needsRefresh;
    private double _timeSinceRefresh;

    private void SetDirty() => _needsRefresh = true;

    public override void _EnterTree()
    {
        base._EnterTree();
        ImmutableInterlocked.Update(ref _listeners, list => list.Add(this));
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ImmutableInterlocked.Update(ref _listeners, list => list.Remove(this));
    }

    public override void _Ready()
    {
       // Fix hilarious issue of resting causing the log window to fade to gray
       OwnWorld3D = true;

        base._Ready();
        lock (_logLock) EnsureLogLimit();

        _scrollContainer = GetNode<ScrollContainer>("MainVBox/Scroll");
        _logLabel = GetNode<RichTextLabel>("MainVBox/Scroll/Log");

        _logLabel?.AddThemeFontOverride("normal_font", ResourceLoader.Load<Font>("res://fonts/source_code_pro_medium.ttf"));

        CloseRequested += QueueFree;

        var scrollbar = _scrollContainer.GetVScrollBar();
        scrollbar.ValueChanged += OnScrollbarValueChanged;

        _isFollowingLog = true;

        SetFontSize(_currentFontSize);
        ApplyMinSizeForScale();

        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        _timeSinceRefresh += delta;
        if (!_needsRefresh || !Visible || Mode == ModeEnum.Minimized) return;
        if (_timeSinceRefresh < 1d / 30d) return;

        _timeSinceRefresh = 0;
        _needsRefresh = false;
        Refresh();
    }

    private void ApplyMinSizeForScale()
    {
        float s = ContentScaleFactor > 0f ? ContentScaleFactor : 1f;
        MinSize = new Vector2I((int)(360 * s), (int)(66 * s));
    }

    private void ApplyChromeFontSize(int size)
    {
        string[] fontTypes = [
           "font_size",
            "normal_font_size",
            "bold_font_size",
            "italics_font_size",
            "bold_italics_font_size",
            "mono_font_size"
       ];

        foreach (var fontType in fontTypes)
        {
            _logLabel?.AddThemeFontSizeOverride(fontType, size);
        }
    }

    public void Refresh()
    {
        if (!IsNodeReady()) return;
        UpdateText();
    }

    private void UpdateText()
    {
        if (!IsNodeReady()) return;
        if (_logLabel is null || _scrollContainer is null) return;

        _isFollowingLog = _isFollowingLog || IsNearBottom();
        _logLabel.Clear();

        // Copying should be cheaper than filtering while locked; we don't want to block the game's threads.
        (string, CombatEntryType)[] snapshot;
        lock (_logLock) snapshot = [.. _log];

        foreach (var (line, type) in snapshot)
            LimitedLog.RenderLine(line, type, _logLabel);

        if (_isFollowingLog) ScrollToBottomAsync();
    }

    private async void ScrollToBottomAsync()
    {
        try
        {
            var tree = GetTree();
            if (tree == null) return;

            // If we got here because RichTextLabel.Finished fired, we still need to draw the frame
            // before scroll offsets are valid
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            if (_scrollContainer is null || !IsInsideTree()) return;

            var scrollbar = _scrollContainer.GetVScrollBar();
            if (scrollbar != null)
            {
                _scrollContainer.ScrollVertical = (int)scrollbar.MaxValue;
                _isFollowingLog = true;
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void OnScrollbarValueChanged(double value)
    {
        if (_scrollContainer is null) return;

        var scrollbar = _scrollContainer.GetVScrollBar();
        if (scrollbar != null)
        {
            _isFollowingLog = IsNearBottom(scrollbar, value);
        }
    }

    private bool IsNearBottom()
    {
        if (_scrollContainer is null) return true;

        var scrollbar = _scrollContainer.GetVScrollBar();
        if (scrollbar == null) return true;
        return IsNearBottom(scrollbar, scrollbar.Value);
    }

    private static bool IsNearBottom(VScrollBar scrollbar, double value)
    {
        double bottomValue = scrollbar.MaxValue - scrollbar.Page;
        return bottomValue - value <= 8;
    }

    private static void EnsureLogLimit()
    {
        _log.SetLimit(_log.Limit);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { CtrlPressed: true } mouseEvent) return;
        if (mouseEvent.ButtonIndex != MouseButton.WheelUp && mouseEvent.ButtonIndex != MouseButton.WheelDown) return;
        if (!mouseEvent.IsReleased()) return; // Don't double-count: pressed, then released
        ChangeFontSize(mouseEvent.ButtonIndex == MouseButton.WheelUp ? 1 : -1);
        GetViewport().SetInputAsHandled();
    }

    private void ChangeFontSize(int deltaPx) =>
        SetFontSize(Math.Clamp(14 + deltaPx, 8, 48));

    private void SetFontSize(int newSize)
    {
        ApplyChromeFontSize(newSize);
        _currentFontSize = newSize;
        ScrollToBottomAsync();
    }

    private class LimitedLog : Queue<(string, CombatEntryType)>
    {
        public int Limit { get; private set; }

        private static readonly Color DamageColor = Color.FromHtml("#ff6d6d");
        private static readonly Color BlockColor = Color.FromHtml("#7fdfff");
        private static readonly Color PowerColor = Color.FromHtml("#ffb86c");
        private static readonly Color PotionColor = Color.FromHtml("80288f");

        public LimitedLog(int limit) : base(limit)
        {
            Limit = limit;
            Enqueue("Combat log initialized.", CombatEntryType.Misc);
        }

        public void SetLimit(int limit)
        {
            Limit = limit;
            while (Count > Limit)
            {
                Dequeue();
            }
        }

        public void Enqueue(string item, CombatEntryType type)
        {
            while (Count >= Limit)
            {
                Dequeue();
            }
            base.Enqueue((item, type));
        }

        public static void RenderLine(string line, CombatEntryType type, RichTextLabel? label)
        {
            if (label is null) return;

            var color = GetColorForLine(type);
            if (color is not null) label.PushColor(color.Value);

            if (type == CombatEntryType.Start) label.Newline();

            label.AddText(line);
            label.Newline();

            if (color is not null) label.Pop();
        }

        private static Color? GetColorForLine(CombatEntryType type) => type switch
        {
            CombatEntryType.CardPlay => null,
            CombatEntryType.Damage => DamageColor,
            CombatEntryType.Block => BlockColor,
            CombatEntryType.Power => PowerColor,
            CombatEntryType.Potion => PotionColor,
            CombatEntryType.Start => null,
            CombatEntryType.Misc => null,
            _ => null
        };
    }
}
