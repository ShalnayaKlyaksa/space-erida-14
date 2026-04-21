using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Popups;
using Content.Shared._Erida.WelderHeating;
using Content.Shared.Interaction;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Tools.Components;
using Content.Shared.Item;


namespace Content.Server._Erida.Tools.WelderHeating;

public sealed class WelderHeatingSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WelderHeatingComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<WelderHeatingComponent> ent, ref AfterInteractEvent args)
    {
        if (TryComp<WelderComponent>(ent, out var welder) && !welder.Enabled)
            return;

        if (!HasComp<ItemComponent>(args.Target))
            return;
        if (args.Target is not { } target || !args.CanReach)
            return;

        if (!TryComp<SolutionContainerManagerComponent>(target, out var container))
            return;
        foreach (var (_, soln) in _solutionContainer.EnumerateSolutions((target, container)))
        {
            if (soln.Comp.Solution.Temperature > ent.Comp.HeatThreshold)
                continue;
            _solutionContainer.AddThermalEnergy(soln, ent.Comp.HeatPerUse);
        }

        var msg = Loc.GetString("warming-with-welder",
            ("container", Name(target)),
            ("welder", Name(ent.Owner)));

        _popup.PopupEntity(msg, args.User, args.User);
    }
}
