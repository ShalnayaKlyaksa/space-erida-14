using System.Numerics;
using Content.Shared._Sunrise.MarkingEffects;
using Robust.Client.Graphics;

namespace Content.Client._Sunrise.MarkingEffectsClient;

public static class MarkingEffectShaders
{
    public static Vector3 ColorToVec(Color color)
    {
        return new Vector3(color.R, color.G, color.B);
    }

    public static void ApplyShaderParams(this ShaderInstance instance, MarkingEffect effect, Vector2 texScale)
    {
        instance.SetParameter("useDisplacement", false);

        switch (effect.Type)
        {
            case MarkingEffectType.Gradient:
                if (effect is not GradientMarkingEffect gradient)
                    return;

                SetColors(gradient, instance);

                instance.SetParameter("texScale", texScale);
                instance.SetParameter("offset", gradient.Offset);
                instance.SetParameter("size", gradient.Size);
                instance.SetParameter("rotation", gradient.Rotation);
                instance.SetParameter("pixelated", gradient.Pixelated);
                instance.SetParameter("mirrored", gradient.Mirrored);
                break;
            case MarkingEffectType.RoughGradient:
                if (effect is not RoughGradientMarkingEffect roughGradient)
                    return;

                SetColors(roughGradient, instance);

                instance.SetParameter("horizontal", roughGradient.Horizontal);
                break;
        }
    }

    private static void SetColors(MarkingEffect effect, ShaderInstance instance)
    {
        var baseColor = effect.Colors.TryGetValue("base", out var bColor) ? bColor : Color.White;
        var gradientColor = effect.Colors.TryGetValue("gradient", out var gColor) ? gColor : baseColor;

        instance.SetParameter("color1", ColorToVec(baseColor));
        instance.SetParameter("color2", ColorToVec(gradientColor));
    }
}
