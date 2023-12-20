﻿using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Weapons.Item;

[ProtocolId(1435138131935)]
public class FlamethrowerMarketItemTemplate : MarketEntityTemplate {
    public override UserEntityTemplate UserTemplate => new FlamethrowerUserItemTemplate();
}