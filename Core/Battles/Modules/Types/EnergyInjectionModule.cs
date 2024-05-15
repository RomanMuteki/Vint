using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Components.Battle.Effect.Type;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class EnergyInjectionModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/energyinjection";

    public override EnergyInjectionEffect GetEffect() => new(Tank, Level, ReloadEnergyPercent);

    float ReloadEnergyPercent { get; set; }

    public override void Activate() {
        if (!CanBeActivated) return;

        EnergyInjectionEffect? effect = Tank.Effects.OfType<EnergyInjectionEffect>().SingleOrDefault();

        if (effect != null) return;

        effect = GetEffect();
        effect.Activate();

        base.Activate();
        IEntity effectEntity = effect.Entity!;
        IEntity weaponEntity = Tank.Weapon;

        ReloadWeapon();
        Tank.BattlePlayer.PlayerConnection.Send(new ExecuteEnergyInjectionEvent(), effectEntity, weaponEntity);
    }

    public override void Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        base.Init(tank, userSlot, marketModule);

        ReloadEnergyPercent = Leveling.GetStat<ModuleEnergyInjectionEffectReloadPercentPropertyComponent>(ConfigPath, Level);
        SlotEntity.AddComponent(new EnergyInjectionModuleReloadEnergyComponent(ReloadEnergyPercent)); // component for module on slot entity?
    }

    void ReloadWeapon() {
        if (Tank.WeaponHandler is HammerWeaponHandler hammer)
            hammer.FillMagazine();
    }
}