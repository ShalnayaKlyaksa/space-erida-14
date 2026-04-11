using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Popups;
using Content.Shared._Erida.WelderHeating;
using Content.Shared.Interaction;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Tools.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Audio;

namespace Content.Server._Erida.Tools.WelderHeating;

public sealed class WelderHeatingSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WelderHeatingComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<WelderHeatingComponent> ent, ref AfterInteractEvent args)
    {
        if (TryComp<WelderComponent>(ent, out var welder) && !welder.Enabled)
            return;

        if (args.Target is not { } target || !args.CanReach)
            return;

        if (!TryComp<SolutionContainerManagerComponent>(target, out var manager))
            return;

        var targetEnt = (target, manager);

        foreach (var (name, solEnt) in _solution.EnumerateSolutions(targetEnt))
        {
            _solution.AddThermalEnergy(solEnt, ent.Comp.HeatPerUse);
        }

        var msg = Loc.GetString(ent.Comp.Popup,
            ("container", Name(target)),
            ("welder", Name(ent.Owner)));

        _popup.PopupEntity(msg, args.User, args.User);

        if (_timing.CurTime >= ent.Comp.NextSoundAt)
        {
            _audio.PlayPvs(ent.Comp.HeatSound, target, AudioParams.Default.WithVolume(-2f));

            ent.Comp.NextSoundAt = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.SoundCooldown);
        }
    }
}
