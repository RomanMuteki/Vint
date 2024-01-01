﻿using LinqToDB;
using LinqToDB.Mapping;
using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Database.Models;

[Table("Players")]
public class Player {
    public ILogger Logger { get; } = Log.Logger.ForType(typeof(Player));

    [PrimaryKey, Identity] public long Id { get; set; }

    [Column] public string Username { get; set; } = null!;
    [Column] public string Email { get; set; } = null!;

    [Column] public bool RememberMe { get; set; }

    [Column] public byte[] AutoLoginToken { get; set; } = [];
    [Column] public byte[] PasswordHash { get; set; } = [];
    [Column] public string HardwareFingerprint { get; set; } = "";

    [Column] public PlayerGroups Groups { get; set; }
    public bool IsAdmin => (Groups & PlayerGroups.Admin) == PlayerGroups.Admin;
    public bool IsModerator => IsAdmin || (Groups & PlayerGroups.Moderator) == PlayerGroups.Moderator;
    public bool IsTester => (Groups & PlayerGroups.Tester) == PlayerGroups.Tester;
    public bool IsPremium => (Groups & PlayerGroups.Premium) == PlayerGroups.Premium;
    public bool IsBanned => (Groups & PlayerGroups.Banned) == PlayerGroups.Banned;

    [Column] public bool Subscribed { get; set; }
    [Column] public string CountryCode { get; set; } = "RU";

    [Column] public long CurrentAvatarId { get; set; }
    [Column] public int CurrentPresetIndex { get; set; }
    [NotColumn] public List<Preset> UserPresets { get; set; } = [];

    [Column] public long Crystals { get; set; } = 1000000;
    [Column] public long XCrystals { get; set; } = 1000000;

    [Column] public int GoldBoxItems { get; set; }

    [Column] public long Experience { get; set; }
    public int Rank => Leveling.GetRank(Experience);

    [Column] public DateTimeOffset RegistrationTime { get; init; }
    [Column] public DateTimeOffset LastLoginTime { get; set; }

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Avatar.PlayerId))]
    public List<Avatar> Avatars { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Cover.PlayerId))]
    public List<Cover> Covers { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Graffiti.PlayerId))]
    public List<Graffiti> Graffities { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Hull.PlayerId))]
    public List<Hull> Hulls { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(HullSkin.PlayerId))]
    public List<HullSkin> HullSkins { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Module.PlayerId))]
    public List<Module> Modules { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Paint.PlayerId))]
    public List<Paint> Paints { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Preset.PlayerId))]
    public List<Preset> Presets { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Relation.SourcePlayerId))]
    public List<Relation> Relations { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Shell.PlayerId))]
    public List<Shell> Shells { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Weapon.PlayerId))]
    public List<Weapon> Weapons { get; private set; } = null!;

    [Association(ThisKey = nameof(Id), OtherKey = nameof(WeaponSkin.PlayerId))]
    public List<WeaponSkin> WeaponSkins { get; private set; } = null!;

    public void InitializeNew() {
        CurrentAvatarId = GlobalEntities.GetEntity("avatars", "Tankist").Id;

        long weaponId = GlobalEntities.GetEntity("weapons", "Smoky").Id;
        long hullId = GlobalEntities.GetEntity("hulls", "Hunter").Id;

        long shellId = GlobalEntities.DefaultShells[weaponId];
        long weaponSkinId = GlobalEntities.DefaultSkins[weaponId];
        long coverId = GlobalEntities.GetEntity("covers", "None").Id;

        long hullSkinId = GlobalEntities.DefaultSkins[hullId];
        long paintId = GlobalEntities.GetEntity("paints", "Green").Id;

        long graffitiId = GlobalEntities.GetEntity("graffities", "Logo").Id;

        using (DbConnection db = new()) {
            db.Insert(new Hull { Player = this, SkinId = hullSkinId, Id = hullId });
            db.Insert(new HullSkin { Player = this, HullId = hullId, Id = hullSkinId });
            db.Insert(new Paint { Player = this, Id = paintId });

            db.Insert(new Weapon { Player = this, Id = weaponId, SkinId = weaponSkinId, ShellId = shellId });
            db.Insert(new WeaponSkin { Player = this, WeaponId = weaponId, Id = weaponSkinId });
            db.Insert(new Cover { Player = this, Id = coverId });
            db.Insert(new Shell { Player = this, Id = shellId, WeaponId = weaponId });

            db.Insert(new Avatar { Player = this, Id = CurrentAvatarId });
            db.Insert(new Graffiti { Player = this, Id = graffitiId });
        }

        List<string> admins = ["C6OI"];
        List<string> testers = ["C6OI"];

        if (admins.Contains(Username))
            Groups |= PlayerGroups.Admin;

        if (testers.Contains(Username))
            Groups |= PlayerGroups.Tester;
    }
}

[Flags]
public enum PlayerGroups {
    None = 0,
    Admin = 1,
    Moderator = 2,
    Tester = 4,
    Premium = 8,
    Banned = 16
}