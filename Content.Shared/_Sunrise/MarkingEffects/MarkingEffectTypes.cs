namespace Content.Shared._Sunrise.MarkingEffects;

public enum MarkingEffectType
{
    Color,
    Gradient,
    RoughGradient,
}

public static class MarkingEffectTypes
{
    public static readonly Dictionary<MarkingEffectType, Func<Dictionary<string, string>, MarkingEffect?>> TypeParsers = new()
    {
        { MarkingEffectType.Color, ColorMarkingEffect.Parse },
        { MarkingEffectType.Gradient, GradientMarkingEffect.Parse },
        { MarkingEffectType.RoughGradient, RoughGradientMarkingEffect.Parse },
    };
}
