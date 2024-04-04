using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Components.Modules.Slot;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Items.Module;

[ProtocolId(1485777830853)]
public class UnmountModuleFromSlotEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        entities = (IEntity[])entities;

        IEntity moduleUserItem = entities.ElementAt(0);
        IEntity slotUserItem = entities.ElementAt(1);

        if (!moduleUserItem.HasComponent<MountedItemComponent>() ||
            !slotUserItem.HasComponent<ModuleGroupComponent>()) return;

        Player player = connection.Player;
        Slot slot = slotUserItem.GetComponent<SlotUserItemInfoComponent>().Slot;

        using DbConnection db = new();

        db.PresetModules
            .Where(pModule => pModule.PlayerId == player.Id &&
                              pModule.PresetIndex == player.CurrentPresetIndex &&
                              pModule.Slot == slot)
            .Delete();

        player.CurrentPreset.Modules.RemoveAll(pModule => pModule.Slot == slot);

        slotUserItem.RemoveComponent<ModuleGroupComponent>();
        moduleUserItem.RemoveComponent<MountedItemComponent>();
    }
}