using Content.Client._Sunrise.MarkingEffectsClient;
using Content.Shared._Sunrise.MarkingEffects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Sunrise.UserInterface.Controls;

public sealed class MarkingEffectSelectorSliders : Control
{
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly ILogManager _log = default!;

    private readonly ISawmill _sawmill;

    private MarkingEffect Effect { get; set; }

    private static readonly Dictionary<MarkingEffectType, IMarkingEffectUiBuilder> UiBuilders = new()
    {
        { MarkingEffectType.Color, new ColorMarkingEffectUiBuilder() },
        { MarkingEffectType.Gradient, new GradientMarkingEffectUiBuilder() },
        { MarkingEffectType.RoughGradient, new RoughGradientMarkingEffectUiBuilder() },
    };

    private readonly Dictionary<string, CustomColorSelectorSliders> _colorSelectors = new();

    private readonly OptionButton _typeSelector;
    private readonly List<MarkingEffectType> _types = [];

    private MarkingEffectType _currentType;

    private readonly BoxContainer _selectorsContainer;
    private readonly BoxContainer _slidersContainer;
    private readonly BoxContainer _toggleContainer;

    public Action<MarkingEffect>? OnColorChanged;

    public MarkingEffectType CurrentType
    {
        get => _currentType;
        set
        {
            if (_currentType == value)
                return;

            _currentType = value;
            Populate(_currentType);
        }
    }

    public MarkingEffectSelectorSliders(MarkingEffect? defaultEffect = null)
    {
        IoCManager.InjectDependencies(this);
        _sawmill = _log.GetSawmill(nameof(MarkingEffectSelectorSliders));

        defaultEffect ??= ColorMarkingEffect.White;

        _typeSelector = new OptionButton
        {
            HorizontalExpand = true,
        };

        foreach (var type in Enum.GetValues<MarkingEffectType>())
        {
            _typeSelector.AddItem(GetLocString(EffectTypeLocKey(type), EffectTypeFallback(type)));
            _types.Add(type);
        }

        _typeSelector.OnItemSelected += args =>
        {
            CurrentType = _types[args.Id];
            _typeSelector.Select(args.Id);
            OnColorsChanged();
        };

        var rootBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };
        AddChild(rootBox);

        var headerBox = new BoxContainer();
        rootBox.AddChild(headerBox);
        headerBox.AddChild(_typeSelector);

        var bodyBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };
        rootBox.AddChild(bodyBox);

        _selectorsContainer = new BoxContainer();
        bodyBox.AddChild(_selectorsContainer);

        _slidersContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };
        bodyBox.AddChild(_slidersContainer);

        _toggleContainer = new BoxContainer();
        bodyBox.AddChild(_toggleContainer);

        _currentType = defaultEffect.Type;
        _typeSelector.TrySelect(_types.IndexOf(_currentType));
        Effect = defaultEffect;
        Populate(_currentType, defaultEffect);
    }

    public void CreateSelector(string key = "base", MarkingEffectType type = MarkingEffectType.Color)
    {
        var colorSelector = new CustomColorSelectorSliders(
            CustomColorSelectorSliders.ColorSelectorType.Hsv,
            GetLocString(EffectColorLocKey(type, key), EffectColorFallback(key)))
        {
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Stretch,
        };

        if (Effect.Colors.TryGetValue(key, out var defaultColor))
            colorSelector.Color = defaultColor;

        colorSelector.OnColorChanged += _ => OnColorsChanged();

        _colorSelectors.TryAdd(key, colorSelector);

        var selectorContainer = new BoxContainer
        {
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Stretch,
        };

        _selectorsContainer.AddChild(selectorContainer);
        selectorContainer.AddChild(colorSelector);
    }

    public void CreateSlider(string label,
        int defaultValue,
        int minValue,
        int maxValue,
        Action<float> onValueChanged)
    {
        var slider = new Slider
        {
            HorizontalExpand = true,
            VerticalAlignment = VAlignment.Center,
            MinValue = minValue,
            MaxValue = maxValue,
            Value = defaultValue,
        };

        var sliderContainer = new BoxContainer();

        var sliderLabel = new Label
        {
            Text = label,
        };

        var spinBox = new SpinBox
        {
            IsValid = value => IsSpinBoxValid(value, minValue, maxValue),
            Value = defaultValue,
        };
        spinBox.InitDefaultButtons();

        sliderContainer.AddChild(sliderLabel);
        sliderContainer.AddChild(slider);
        sliderContainer.AddChild(spinBox);
        _slidersContainer.AddChild(sliderContainer);

        BindSlider(slider, spinBox, onValueChanged);
    }

    private void BindSlider(Slider slider, SpinBox spinBox, Action<float> setValue)
    {
        slider.OnReleased += value =>
        {
            setValue(value.Value);
            spinBox.Value = (int) value.Value;
            OnColorsChanged();
        };

        spinBox.ValueChanged += value =>
        {
            setValue(value.Value);
            slider.SetValueWithoutEvent(value.Value);
            OnColorsChanged();
        };
    }

    public void CreateToggle(string label, bool defaultValue, Action<bool> onValueChanged)
    {
        var button = new Button
        {
            Text = label,
            ToggleMode = true,
            Pressed = defaultValue,
            HorizontalExpand = true,
        };

        _toggleContainer.AddChild(button);

        BindToggle(button, onValueChanged);
    }

    private void BindToggle(Button toggle, Action<bool> setValue)
    {
        toggle.OnToggled += value =>
        {
            setValue(value.Pressed);
            OnColorsChanged();
        };
    }

    private static bool IsSpinBoxValid(int value, float min, float max)
    {
        return value >= min && value <= max;
    }

    private string GetLocString(string key, string fallback)
    {
        return _loc.TryGetString(key, out var value)
            ? value
            : fallback;
    }

    private static string EffectTypeLocKey(MarkingEffectType type)
    {
        return $"marking-effect-type-{type.ToString().ToLowerInvariant()}";
    }

    private static string EffectColorLocKey(MarkingEffectType type, string key)
    {
        return $"marking-effect-{type.ToString().ToLowerInvariant()}-color-{key}";
    }

    private static string EffectTypeFallback(MarkingEffectType type)
    {
        return type switch
        {
            MarkingEffectType.Color => "Color",
            MarkingEffectType.Gradient => "Gradient",
            MarkingEffectType.RoughGradient => "Rough gradient",
            _ => type.ToString(),
        };
    }

    private static string EffectColorFallback(string key)
    {
        return key switch
        {
            "base" => "Base",
            "gradient" => "Gradient",
            _ => key,
        };
    }

    private void OnColorsChanged()
    {
        foreach (var (key, selector) in _colorSelectors)
        {
            Effect.Colors[key] = selector.Color;
        }

        OnColorChanged?.Invoke(Effect);
    }

    private void Populate(MarkingEffectType type, MarkingEffect? defaultEffect = null)
    {
        _colorSelectors.Clear();
        _selectorsContainer.DisposeAllChildren();
        _slidersContainer.DisposeAllChildren();
        _toggleContainer.DisposeAllChildren();

        _sawmill.Verbose($"{defaultEffect}");

        defaultEffect ??= type switch
        {
            MarkingEffectType.Color => ColorMarkingEffect.White,
            MarkingEffectType.Gradient => new GradientMarkingEffect(),
            MarkingEffectType.RoughGradient => new RoughGradientMarkingEffect(),
            _ => ColorMarkingEffect.White,
        };

        Effect = defaultEffect;

        if (UiBuilders.TryGetValue(type, out var builder))
            builder.BuildUI(Effect, this);
        else
            _sawmill.Warning($"No UI builder for marking effect: {type}");
    }
}
