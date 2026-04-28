using Content.Client._Sunrise.UserInterface.Controls;
using Content.Shared._Sunrise.MarkingEffects;

namespace Content.Client._Sunrise.MarkingEffectsClient;

public sealed class GradientMarkingEffectUiBuilder : IMarkingEffectUiBuilder
{
    private const float ToIntScaling = 100;

    private const int OffsetMin = -200;
    private const int OffsetMax = 100;

    private const int SizeMin = 30;
    private const int SizeMax = 100;

    private const int RotationMin = 0;
    private const int RotationMax = 360;

    public void BuildUI(MarkingEffect effect, MarkingEffectSelectorSliders parent)
    {
        if (effect is not GradientMarkingEffect gradient)
            return;

        parent.CreateSelector(type: MarkingEffectType.Gradient);
        parent.CreateSelector("gradient", MarkingEffectType.Gradient);

        parent.CreateSlider(Loc.GetString("marking-effect-gradient-parameter-offset"),
            (int) (gradient.Offset.Y * ToIntScaling), OffsetMin, OffsetMax,
            value => gradient.Offset.Y = value / ToIntScaling);
        parent.CreateSlider(Loc.GetString("marking-effect-gradient-parameter-size"),
            (int) (gradient.Size.Y * ToIntScaling), SizeMin, SizeMax,
            value => gradient.Size.Y = value / ToIntScaling);
        parent.CreateSlider(Loc.GetString("marking-effect-gradient-parameter-rotation"),
            (int) gradient.Rotation, RotationMin, RotationMax,
            value => gradient.Rotation = value);

        parent.CreateToggle(Loc.GetString("marking-effect-gradient-parameter-pixelation"),
            gradient.Pixelated,
            value => gradient.Pixelated = value);
        parent.CreateToggle(Loc.GetString("marking-effect-gradient-parameter-mirror"),
            gradient.Mirrored,
            value => gradient.Mirrored = value);
    }
}
