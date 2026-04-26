using System.Linq;
using Content.Shared.Body;
using Content.Shared.CCVar;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Numerics;
using Content.Shared.Humanoid.Prototypes;

namespace Content.Client.Body;

public sealed class VisualBodySystem : SharedVisualBodySystem
{
    private static readonly ProtoId<ShaderPrototype> MarkingGradientShader = "MarkingGradient";

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly MarkingManager _marking = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private readonly Dictionary<EntityUid, Direction> _lastGradientDirections = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VisualOrganComponent, OrganGotInsertedEvent>(OnOrganGotInserted);
        SubscribeLocalEvent<VisualOrganComponent, OrganGotRemovedEvent>(OnOrganGotRemoved);
        SubscribeLocalEvent<VisualOrganComponent, AfterAutoHandleStateEvent>(OnOrganState);

        SubscribeLocalEvent<VisualOrganMarkingsComponent, OrganGotInsertedEvent>(OnMarkingsGotInserted);
        SubscribeLocalEvent<VisualOrganMarkingsComponent, OrganGotRemovedEvent>(OnMarkingsGotRemoved);
        SubscribeLocalEvent<VisualOrganMarkingsComponent, AfterAutoHandleStateEvent>(OnMarkingsState);

        SubscribeLocalEvent<VisualOrganMarkingsComponent, BodyRelayedEvent<HumanoidLayerVisibilityChangedEvent>>(OnMarkingsChangedVisibility);

        Subs.CVar(_cfg, CCVars.AccessibilityClientCensorNudity, OnCensorshipChanged, true);
        Subs.CVar(_cfg, CCVars.AccessibilityServerCensorNudity, OnCensorshipChanged, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<OrganComponent, VisualOrganMarkingsComponent>();
        while (query.MoveNext(out var uid, out var organComp, out var markingsComp))
        {
            if (organComp.Body is not { } body)
                continue;

            if (!HasGradients(markingsComp.AppliedMarkings))
                continue;

            var direction = GetGradientDirection(body);
            if (_lastGradientDirections.TryGetValue(uid, out var lastDirection) && lastDirection == direction)
                continue;

            _lastGradientDirections[uid] = direction;
            RefreshMarkingGradients((uid, markingsComp), body);
        }
    }

    private void OnCensorshipChanged(bool value)
    {
        var query = AllEntityQuery<OrganComponent, VisualOrganMarkingsComponent>();
        while (query.MoveNext(out var ent, out var organComp, out var markingsComp))
        {
            if (organComp.Body is not { } body)
                continue;

            RemoveMarkings((ent, markingsComp), body);
            ApplyMarkings((ent, markingsComp), body);
        }
    }

    private void OnOrganGotInserted(Entity<VisualOrganComponent> ent, ref OrganGotInsertedEvent args)
    {
        ApplyVisual(ent, args.Target);
    }

    private void OnOrganGotRemoved(Entity<VisualOrganComponent> ent, ref OrganGotRemovedEvent args)
    {
        RemoveVisual(ent, args.Target);
    }

    private void OnOrganState(Entity<VisualOrganComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (Comp<OrganComponent>(ent).Body is not { } body)
            return;

        ApplyVisual(ent, body);
    }

    private void ApplyVisual(Entity<VisualOrganComponent> ent, EntityUid target)
    {
        if (!_sprite.LayerMapTryGet(target, ent.Comp.Layer, out var index, true))
            return;

        _sprite.LayerSetData(target, index, ent.Comp.Data);
    }

    private void RemoveVisual(Entity<VisualOrganComponent> ent, EntityUid target)
    {
        if (!_sprite.LayerMapTryGet(target, ent.Comp.Layer, out var index, true))
            return;

        _sprite.LayerSetRsiState(target, index, RSI.StateId.Invalid);
    }

    private void OnMarkingsGotInserted(Entity<VisualOrganMarkingsComponent> ent, ref OrganGotInsertedEvent args)
    {
        ApplyMarkings(ent, args.Target);
    }

    private void OnMarkingsGotRemoved(Entity<VisualOrganMarkingsComponent> ent, ref OrganGotRemovedEvent args)
    {
        RemoveMarkings(ent, args.Target);
    }

    private void OnMarkingsState(Entity<VisualOrganMarkingsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (Comp<OrganComponent>(ent).Body is not { } body)
            return;

        RemoveMarkings(ent, body);
        ApplyMarkings(ent, body);
    }

    protected override void SetOrganColor(Entity<VisualOrganComponent> ent, Color color)
    {
        base.SetOrganColor(ent, color);

        if (Comp<OrganComponent>(ent).Body is not { } body)
            return;

        ApplyVisual(ent, body);
    }

    protected override void SetOrganMarkings(Entity<VisualOrganMarkingsComponent> ent, Dictionary<HumanoidVisualLayers, List<Marking>> markings)
    {
        base.SetOrganMarkings(ent, markings);

        if (Comp<OrganComponent>(ent).Body is not { } body)
            return;

        RemoveMarkings(ent, body);
        ApplyMarkings(ent, body);
    }

    protected override void SetOrganAppearance(Entity<VisualOrganComponent> ent, PrototypeLayerData data)
    {
        base.SetOrganAppearance(ent, data);

        if (Comp<OrganComponent>(ent).Body is not { } body)
            return;

        ApplyVisual(ent, body);
    }

    private IEnumerable<Marking> AllMarkings(Entity<VisualOrganMarkingsComponent> ent)
    {
        foreach (var markings in ent.Comp.Markings.Values)
        {
            foreach (var marking in markings)
            {
                yield return marking;
            }
        }

        var censorNudity = _cfg.GetCVar(CCVars.AccessibilityClientCensorNudity) || _cfg.GetCVar(CCVars.AccessibilityServerCensorNudity);
        if (!censorNudity)
            yield break;

        var group = _prototype.Index(ent.Comp.MarkingData.Group);
        foreach (var layer in ent.Comp.MarkingData.Layers)
        {
            if (!group.Limits.TryGetValue(layer, out var layerLimits))
                continue;

            if (layerLimits.NudityDefault.Count < 1)
                continue;

            var markings = ent.Comp.Markings.GetValueOrDefault(layer) ?? [];
            if (markings.Any(marking => _marking.TryGetMarking(marking, out var proto) && proto.BodyPart == layer))
                continue;

            foreach (var marking in layerLimits.NudityDefault)
            {
                yield return new(marking, 1);
            }
        }
    }

    private void ApplyMarkings(Entity<VisualOrganMarkingsComponent> ent, EntityUid target)
    {
        var applied = new List<Marking>();
        foreach (var marking in AllMarkings(ent))
        {
            if (!_marking.TryGetMarking(marking, out var proto))
                continue;

            if (!_sprite.LayerMapTryGet(target, proto.BodyPart, out var index, true))
                continue;

            for (var i = 0; i < proto.Sprites.Count; i++)
            {
                var sprite = proto.Sprites[i];

                DebugTools.Assert(sprite is SpriteSpecifier.Rsi);
                if (sprite is not SpriteSpecifier.Rsi rsi)
                    continue;

                var layerId = $"{proto.ID}-{rsi.RsiState}";

                if (!_sprite.LayerMapTryGet(target, layerId, out var layerIndex, false))
                {
                    layerIndex = _sprite.AddLayer(target, sprite, index + i + 1);
                    _sprite.LayerMapSet(target, layerId, layerIndex);
                    _sprite.LayerSetSprite(target, layerId, rsi);
                }

                ScaleProfile(target); // Erida edit

                var color = marking.MarkingColors is not null && i < marking.MarkingColors.Count
                    ? marking.MarkingColors[i]
                    : Color.White;
                var gradient = marking.MarkingGradients is not null && i < marking.MarkingGradients.Count
                    ? marking.MarkingGradients[i]
                    : MarkingGradient.None;

                ApplyMarkingColor(target, layerIndex, layerId, color, gradient);
            }

            applied.Add(marking);
        }
        ent.Comp.AppliedMarkings = applied;
        _lastGradientDirections[ent.Owner] = GetGradientDirection(target);
    }

    private void RefreshMarkingGradients(Entity<VisualOrganMarkingsComponent> ent, EntityUid target)
    {
        foreach (var marking in ent.Comp.AppliedMarkings)
        {
            if (!HasGradient(marking))
                continue;

            if (!_marking.TryGetMarking(marking, out var proto))
                continue;

            for (var i = 0; i < proto.Sprites.Count; i++)
            {
                var sprite = proto.Sprites[i];
                if (sprite is not SpriteSpecifier.Rsi rsi)
                    continue;

                var gradient = marking.MarkingGradients is not null && i < marking.MarkingGradients.Count
                    ? marking.MarkingGradients[i]
                    : MarkingGradient.None;

                if (gradient.Type == MarkingGradientType.None)
                    continue;

                var layerId = $"{proto.ID}-{rsi.RsiState}";
                if (!_sprite.LayerMapTryGet(target, layerId, out var layerIndex, false))
                    continue;

                var color = marking.MarkingColors is not null && i < marking.MarkingColors.Count
                    ? marking.MarkingColors[i]
                    : Color.White;

                ApplyMarkingColor(target, layerIndex, layerId, color, gradient);
            }
        }
    }

    private void ApplyMarkingColor(EntityUid target, int layerIndex, string layerId, Color color, MarkingGradient gradient)
    {
        if (!TryComp<SpriteComponent>(target, out var sprite))
        {
            _sprite.LayerSetColor(target, layerId, color);
            return;
        }

        if (gradient.Type == MarkingGradientType.None)
        {
            sprite.LayerSetShader(layerIndex, null, null);
            _sprite.LayerSetColor(target, layerId, color);
            return;
        }

        var shader = _prototype.Index(MarkingGradientShader).InstanceUnique();
        shader.SetParameter("startColor", ToVector4(color));
        shader.SetParameter("endColor", ToVector4(gradient.Color));
        shader.SetParameter("gradientType", (float) gradient.Type);
        shader.SetParameter("gradientAngle", GetFacingGradientAngle(target, gradient.Angle));
        shader.SetParameter("gradientOffset", gradient.Offset);
        shader.SetParameter("gradientSoftness", gradient.Softness);
        sprite.LayerSetShader(layerIndex, shader, MarkingGradientShader.Id);
        _sprite.LayerSetColor(target, layerId, Color.White);
    }

    private float GetFacingGradientAngle(EntityUid target, float angle)
    {
        return angle + (float) GetGradientDirection(target).ToAngle().Degrees;
    }

    private Direction GetGradientDirection(EntityUid target)
    {
        if (TryComp<SpriteComponent>(target, out var sprite) && sprite.EnableDirectionOverride)
            return sprite.DirectionOverride;

        return _transform.GetWorldRotation(Transform(target)).GetDir();
    }

    private static bool HasGradients(IEnumerable<Marking> markings)
    {
        return markings.Any(HasGradient);
    }

    private static bool HasGradient(Marking marking)
    {
        return marking.MarkingGradients.Any(gradient => gradient.Type != MarkingGradientType.None);
    }

    private static Vector4 ToVector4(Color color)
    {
        return new Vector4(color.R, color.G, color.B, color.A);
    }

    // Erida start
    private void ScaleProfile(EntityUid target)
    {
        var humanoidProfile = _entityManager.GetComponent<HumanoidProfileComponent>(target);
        var humanoidSprite = _entityManager.GetComponent<SpriteComponent>(target);
        var speciesPrototype = _prototype.Index(humanoidProfile.Species);

        var height = Math.Clamp(humanoidProfile.Height, speciesPrototype.MinHeight, speciesPrototype.MaxHeight);
        var width = Math.Clamp(humanoidProfile.Width, speciesPrototype.MinWidth, speciesPrototype.MaxWidth);

        _sprite.SetScale((target, humanoidSprite), new Vector2(width, height));
    }
    // Erida end

    private void RemoveMarkings(Entity<VisualOrganMarkingsComponent> ent, EntityUid target)
    {
        foreach (var marking in ent.Comp.AppliedMarkings)
        {
            if (!_marking.TryGetMarking(marking, out var proto))
                continue;

            foreach (var sprite in proto.Sprites)
            {
                DebugTools.Assert(sprite is SpriteSpecifier.Rsi);
                if (sprite is not SpriteSpecifier.Rsi rsi)
                    continue;

                var layerId = $"{proto.ID}-{rsi.RsiState}";

                if (!_sprite.LayerMapTryGet(target, layerId, out var index, false))
                    continue;

                _sprite.LayerMapRemove(target, layerId);
                _sprite.RemoveLayer(target, index);
            }
        }
    }

    private void OnMarkingsChangedVisibility(Entity<VisualOrganMarkingsComponent> ent, ref BodyRelayedEvent<HumanoidLayerVisibilityChangedEvent> args)
    {
        if (!ent.Comp.HideableLayers.Contains(args.Args.Layer))
            return;

        foreach (var markings in ent.Comp.Markings.Values)
        {
            foreach (var marking in markings)
            {
                if (!_marking.TryGetMarking(marking, out var proto))
                    continue;

                if (proto.BodyPart != args.Args.Layer && !(ent.Comp.DependentHidingLayers.TryGetValue(args.Args.Layer, out var dependent) && dependent.Contains(proto.BodyPart)))
                    continue;

                foreach (var sprite in proto.Sprites)
                {
                    DebugTools.Assert(sprite is SpriteSpecifier.Rsi);
                    if (sprite is not SpriteSpecifier.Rsi rsi)
                        continue;

                    var layerId = $"{proto.ID}-{rsi.RsiState}";

                    if (!_sprite.LayerMapTryGet(args.Body.Owner, layerId, out var index, true))
                        continue;

                    _sprite.LayerSetVisible(args.Body.Owner, index, args.Args.Visible);
                }
            }
        }
    }
}
