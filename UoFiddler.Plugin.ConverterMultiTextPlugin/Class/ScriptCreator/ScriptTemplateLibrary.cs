using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ScriptCreator
{
    /// <summary>
    /// Built-in templates (ServUO and Sphere) plus the own templates in the folder ScriptTemplates next to UoFiddler (*.uotpl).
    /// An own template with the same name as a built-in one replaces it.
    /// </summary>
    internal static class ScriptTemplateLibrary
    {
        public const string Extension = ".uotpl";

        public static string UserFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptTemplates");

        public static List<ScriptTemplate> LoadAll(List<string> errors)
        {
            var list = new List<ScriptTemplate>();
            foreach (string text in BuiltInTexts)
            {
                try
                {
                    list.Add(ScriptTemplate.Parse(text));
                }
                catch (Exception ex)
                {
                    errors?.Add("built-in template: " + ex.Message);
                }
            }
            try
            {
                if (Directory.Exists(UserFolder))
                {
                    foreach (string file in Directory.GetFiles(UserFolder, "*" + Extension, SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        try
                        {
                            ScriptTemplate t = ScriptTemplate.Parse(File.ReadAllText(file, Encoding.UTF8));
                            t.SourceFile = file;
                            list.RemoveAll(b => b.IsBuiltIn && string.Equals(b.Name, t.Name, StringComparison.OrdinalIgnoreCase));
                            list.Add(t);
                        }
                        catch (Exception ex)
                        {
                            errors?.Add($"{Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors?.Add(ex.Message);
            }
            return list;
        }

        /// <summary>Saves an own template (new file from the name, or its existing file).</summary>
        public static string Save(ScriptTemplate t)
        {
            Directory.CreateDirectory(UserFolder);
            string file = t.SourceFile;
            if (string.IsNullOrEmpty(file))
            {
                string name = new string(t.Name.Select(c => Path.GetInvalidFileNameChars().Contains(c) || c == ' ' ? '_' : c).ToArray());
                file = Path.Combine(UserFolder, name + Extension);
            }
            File.WriteAllText(file, t.ToText().Replace("\r\n", "\n").Replace("\n", "\r\n"), new UTF8Encoding(false));
            t.SourceFile = file;
            return file;
        }

        // ------------------------------------------------------------------ built-in templates

        private const string Serial = """
                public {{ClassName}}(Serial serial)
                    : base(serial)
                {
                }

                public override void Serialize(GenericWriter writer)
                {
                    base.Serialize(writer);
                    writer.Write((int)0); // version
                }

                public override void Deserialize(GenericReader reader)
                {
                    base.Deserialize(reader);
                    int version = reader.ReadInt();
                }
        """;

        private const string DamageFields = """
        @field Phys | int:0:100 | Damage physical % | 100
        @field Fire | int:0:100 | Damage fire % | 0
        @field Cold | int:0:100 | Damage cold % | 0
        @field Pois | int:0:100 | Damage poison % | 0
        @field Nrgy | int:0:100 | Damage energy % | 0
        @field Chaos | int:0:100 | Damage chaos % | 0
        @field Direct | int:0:100 | Damage direct % | 0
        @check sum:Phys,Fire,Cold,Pois,Nrgy,Chaos,Direct=100
        """;

        private const string DamageMethod = """

                public override void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
                {
                    phys = {{Phys}};
                    fire = {{Fire}};
                    cold = {{Cold}};
                    pois = {{Pois}};
                    nrgy = {{Nrgy}};
                    chaos = {{Chaos}};
                    direct = {{Direct}};
                }
        """;

        private static IEnumerable<string> BuiltInTexts => new[]
        {
            // ---------------------------------------------------------------- weapons
            """
            @name Ranged weapon (bow / crossbow)
            @group ServUO – Weapons
            @output cs
            @file {{ClassName}}.cs
            @description Bow or crossbow with attributes and damage types (old tabs Bow and Crossbow).
            @field ClassName | class | Class name | StormBow
            @field BaseClass | combo:@RangedBase | Base class | Bow
            @field ItemID | itemid | Item ID | 0x13B2
            @field Name | string | Name | a storm bow
            @field Hue | hue | Hue | 0
            @field StrRequirement | int:0:150 | Strength needed | 30
            @field MinDamage | int:0:200 | Min damage | 15
            @field MaxDamage | int:0:200 | Max damage | 19
            @field LootType | choice:@LootType | Loot type | Regular
            @field Attributes | list:@AosAttribute | Attributes | SpellChanneling=1;WeaponSpeed=20
            @field WeaponAttributes | list:@AosWeaponAttribute | Weapon attributes | HitLightning=30;HitLeechHits=5;DurabilityBonus=100
            """ + "\n" + DamageFields + "\n" + """
            @template
            using System;
            using Server;

            namespace Server.Items
            {
                public class {{ClassName}} : {{BaseClass}}
                {
                    [Constructable]
                    public {{ClassName}}()
                    {
                        ItemID = {{ItemID}};
            {{#if Name}}
                        Name = "{{Name|cs}}";
            {{/if}}
                        Hue = {{Hue}};
                        StrRequirement = {{StrRequirement}};
                        MinDamage = {{MinDamage}};
                        MaxDamage = {{MaxDamage}};
                        LootType = LootType.{{LootType}};
            {{#each Attributes}}
                        Attributes.{{key}} = {{value}};
            {{/each}}
            {{#each WeaponAttributes}}
                        WeaponAttributes.{{key}} = {{value}};
            {{/each}}
                    }
            """ + DamageMethod + "\n\n" + Serial + """

                }
            }

            """,

            """
            @name Melee weapon (sword, axe, staff, mace …)
            @group ServUO – Weapons
            @output cs
            @file {{ClassName}}.cs
            @description Sword, axe, staff, spear or mace with attributes and damage types (old tabs Sword, Staff, Axe).
            @field ClassName | class | Class name | FlameBlade
            @field BaseClass | combo:@MeleeBase | Base class | Longsword
            @field ItemID | itemid | Item ID | 0xF61
            @field Name | string | Name | a flame blade
            @field Hue | hue | Hue | 0x489
            @field StrRequirement | int:0:150 | Strength needed | 35
            @field MinDamage | int:0:200 | Min damage | 13
            @field MaxDamage | int:0:200 | Max damage | 16
            @field LootType | choice:@LootType | Loot type | Regular
            @field Attributes | list:@AosAttribute | Attributes | WeaponDamage=40;AttackChance=10
            @field WeaponAttributes | list:@AosWeaponAttribute | Weapon attributes | HitFireball=25;SelfRepair=3
            """ + "\n" + DamageFields.Replace("Phys | int:0:100 | Damage physical % | 100", "Phys | int:0:100 | Damage physical % | 50").Replace("Fire | int:0:100 | Damage fire % | 0", "Fire | int:0:100 | Damage fire % | 50") + "\n" + """
            @template
            using System;
            using Server;

            namespace Server.Items
            {
                public class {{ClassName}} : {{BaseClass}}
                {
                    [Constructable]
                    public {{ClassName}}()
                    {
                        ItemID = {{ItemID}};
            {{#if Name}}
                        Name = "{{Name|cs}}";
            {{/if}}
                        Hue = {{Hue}};
                        StrRequirement = {{StrRequirement}};
                        MinDamage = {{MinDamage}};
                        MaxDamage = {{MaxDamage}};
                        LootType = LootType.{{LootType}};
            {{#each Attributes}}
                        Attributes.{{key}} = {{value}};
            {{/each}}
            {{#each WeaponAttributes}}
                        WeaponAttributes.{{key}} = {{value}};
            {{/each}}
                    }
            """ + DamageMethod + "\n\n" + Serial + """

                }
            }

            """,

            // ---------------------------------------------------------------- armor, clothing
            """
            @name Armor piece / shield
            @group ServUO – Armor & clothing
            @output cs
            @file {{ClassName}}.cs
            @description Armor or shield based on an existing ServUO armor class; bonus resistances and attributes.
            @field ClassName | class | Class name | DragonPlateChest
            @field BaseClass | combo:@ArmorBase | Base class | PlateChest
            @field ItemID | itemid | Item ID | 0x1415
            @field Name | string | Name | dragon plate
            @field Hue | hue | Hue | 0x66D
            @field LootType | choice:@LootType | Loot type | Regular
            @field PhysicalBonus | int:-100:100 | Physical bonus | 5
            @field FireBonus | int:-100:100 | Fire bonus | 5
            @field ColdBonus | int:-100:100 | Cold bonus | 0
            @field PoisonBonus | int:-100:100 | Poison bonus | 0
            @field EnergyBonus | int:-100:100 | Energy bonus | 0
            @field Attributes | list:@AosAttribute | Attributes | BonusHits=5;DefendChance=5
            @field ArmorAttributes | list:@AosArmorAttribute | Armor attributes | SelfRepair=3
            @template
            using System;
            using Server;

            namespace Server.Items
            {
                public class {{ClassName}} : {{BaseClass}}
                {
                    [Constructable]
                    public {{ClassName}}()
                    {
                        ItemID = {{ItemID}};
            {{#if Name}}
                        Name = "{{Name|cs}}";
            {{/if}}
                        Hue = {{Hue}};
                        LootType = LootType.{{LootType}};
                        PhysicalBonus = {{PhysicalBonus}};
                        FireBonus = {{FireBonus}};
                        ColdBonus = {{ColdBonus}};
                        PoisonBonus = {{PoisonBonus}};
                        EnergyBonus = {{EnergyBonus}};
            {{#each Attributes}}
                        Attributes.{{key}} = {{value}};
            {{/each}}
            {{#each ArmorAttributes}}
                        ArmorAttributes.{{key}} = {{value}};
            {{/each}}
                    }

            """ + Serial + """

                }
            }

            """,

            """
            @name Clothing
            @group ServUO – Armor & clothing
            @output cs
            @file {{ClassName}}.cs
            @description Wearable clothing. The tiledata of the item needs Wearable, the layer of the base class and an AnimID (paperdoll gump = AnimID + 50000 / 60000 – see the UO calculator).
            @field ClassName | class | Class name | SilkRobe
            @field BaseClass | combo:@ClothingBase | Base class | BaseOuterTorso
            @field ItemID | itemid | Item ID | 0x1F03
            @field Name | string | Name | a silk robe
            @field Hue | hue | Hue | 0x481
            @field Weight | double | Weight | 3.0
            @field LootType | choice:@LootType | Loot type | Regular
            @field Attributes | list:@AosAttribute | Attributes | Luck=100
            @field Resistances | list:Physical,Fire,Cold,Poison,Energy | Resistances | Physical=3
            @template
            using System;
            using Server;

            namespace Server.Items
            {
                public class {{ClassName}} : {{BaseClass}}
                {
                    [Constructable]
                    public {{ClassName}}()
                        : this({{Hue}})
                    {
                    }

                    [Constructable]
                    public {{ClassName}}(int hue)
                        : base({{ItemID}}, hue)
                    {
            {{#if Name}}
                        Name = "{{Name|cs}}";
            {{/if}}
                        Weight = {{Weight}};
                        LootType = LootType.{{LootType}};
            {{#each Attributes}}
                        Attributes.{{key}} = {{value}};
            {{/each}}
            {{#each Resistances}}
                        Resistances.{{key}} = {{value}};
            {{/each}}
                    }

            """ + Serial + """

                }
            }

            """,

            // ---------------------------------------------------------------- creatures
            """
            @name Creature / mount
            @group ServUO – Creatures
            @output cs
            @file {{ClassName}}.cs
            @description Monster, animal or mount with stats, resistances, skills and loot (old tab Animation). Ranges are "min,max".
            @field ClassName | class | Class name | StormDrake
            @field Name | string | Name | a storm drake
            @field CorpseName | string | Corpse name | a storm drake corpse
            @field Body | body | Body ID | 0x3C
            @field BaseSoundID | sound | Base sound ID | 362
            @field Hue | hue | Hue | 0
            @field AI | choice:@AIType | AI | AI_Mage
            @field FightMode | choice:@FightMode | Fight mode | Closest
            @field Str | range | Strength min,max | 401, 430
            @field Dex | range | Dexterity min,max | 133, 152
            @field Int | range | Intelligence min,max | 101, 140
            @field Hits | range | Hit points min,max | 241, 258
            @field Damage | range | Damage min,max | 11, 17
            @field DamageTypes | list:Physical,Fire,Cold,Poison,Energy | Damage types % | Physical=80;Energy=20
            @field Resistances | list:Physical,Fire,Cold,Poison,Energy | Resistances min,max | Physical=45, 50;Fire=50, 60;Cold=40, 50;Poison=20, 30;Energy=60, 70
            @field Skills | list:@SkillName | Skills min,max | EvalInt=30.1, 40.0;Magery=30.1, 40.0;MagicResist=65.1, 80.0;Tactics=65.1, 90.0;Wrestling=65.1, 80.0
            @field Fame | int:-100000:100000 | Fame | 5500
            @field Karma | int:-100000:100000 | Karma | -5500
            @field VirtualArmor | int:0:200 | Virtual armor | 46
            @field Loot | list:@LootPack | Loot packs (pack = count) | Rich=1;MedScrolls=2
            @field Tamable | bool | Tamable | false
            @field ControlSlots | int:1:6 | Control slots | 2
            @field MinTameSkill | double | Min taming skill | 84.3
            @field IsMount | bool | Rideable mount | false
            @field MountItemID | itemid | Mount item ID (mounts only) | 0x3EBB
            @field Meat | int:0:100 | Meat | 10
            @field Hides | int:0:100 | Hides | 20
            @field HideType | choice:@HideType | Hide type | Horned
            @check sum:DamageTypes=100
            @template
            using System;
            using Server;
            using Server.Items;

            namespace Server.Mobiles
            {
                [CorpseName("{{CorpseName|cs}}")]
            {{#if IsMount}}
                public class {{ClassName}} : BaseMount
                {
                    [Constructable]
                    public {{ClassName}}()
                        : this("{{Name|cs}}")
                    {
                    }

                    [Constructable]
                    public {{ClassName}}(string name)
                        : base(name, {{Body}}, {{MountItemID}}, AIType.{{AI}}, FightMode.{{FightMode}}, 10, 1, 0.2, 0.4)
                    {
            {{else}}
                public class {{ClassName}} : BaseCreature
                {
                    [Constructable]
                    public {{ClassName}}()
                        : base(AIType.{{AI}}, FightMode.{{FightMode}}, 10, 1, 0.2, 0.4)
                    {
                        Name = "{{Name|cs}}";
                        Body = {{Body}};
            {{/if}}
                        BaseSoundID = {{BaseSoundID}};
                        Hue = {{Hue}};

                        SetStr({{Str}});
                        SetDex({{Dex}});
                        SetInt({{Int}});
                        SetHits({{Hits}});
                        SetDamage({{Damage}});

            {{#each DamageTypes}}
                        SetDamageType(ResistanceType.{{key}}, {{value}});
            {{/each}}

            {{#each Resistances}}
                        SetResistance(ResistanceType.{{key}}, {{value}});
            {{/each}}

            {{#each Skills}}
                        SetSkill(SkillName.{{key}}, {{value}});
            {{/each}}

                        Fame = {{Fame}};
                        Karma = {{Karma}};
                        VirtualArmor = {{VirtualArmor}};
            {{#if Tamable}}

                        Tamable = true;
                        ControlSlots = {{ControlSlots}};
                        MinTameSkill = {{MinTameSkill}};
            {{/if}}
                    }

                    public override void GenerateLoot()
                    {
            {{#each Loot}}
            {{#if value}}
                        AddLoot(LootPack.{{key}}, {{value}});
            {{else}}
                        AddLoot(LootPack.{{key}});
            {{/if}}
            {{/each}}
                    }

                    public override int Meat { get { return {{Meat}}; } }
                    public override int Hides { get { return {{Hides}}; } }
                    public override HideType HideType { get { return HideType.{{HideType}}; } }

            """ + Serial + """

                }
            }

            """,

            """
            @name Vendor NPC
            @group ServUO – Creatures
            @output cs
            @file {{ClassName}}.cs
            @description Shop keeper with title, skills and the shops (SBInfo) it sells from.
            @field ClassName | class | Class name | DragonSmith
            @field Title | string | Title | the dragon smith
            @field Name | string | Fixed name (empty = random) |
            @field Skills | list:@SkillName | Skills min,max | Blacksmith=90.0, 100.0;ArmsLore=75.0, 98.0
            @field Shops | list:@SBInfo | Shops (SBInfo) | SBBlacksmith;SBWeaponSmith
            @template
            using System;
            using System.Collections.Generic;
            using Server;

            namespace Server.Mobiles
            {
                public class {{ClassName}} : BaseVendor
                {
                    private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

                    protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

                    [Constructable]
                    public {{ClassName}}()
                        : base("{{Title|cs}}")
                    {
            {{#if Name}}
                        Name = "{{Name|cs}}";
            {{/if}}
            {{#each Skills}}
                        SetSkill(SkillName.{{key}}, {{value}});
            {{/each}}
                    }

                    public override void InitSBInfo()
                    {
            {{#each Shops}}
                        m_SBInfos.Add(new {{key}}());
            {{/each}}
                    }

            """ + Serial + """

                }
            }

            """,

            // ---------------------------------------------------------------- furniture, containers
            """
            @name Chair / furniture
            @group ServUO – Furniture & containers
            @output cs
            @file {{ClassName}}.cs
            @description Furniture that can be turned (old tab Chair). Flip IDs: all directions, comma separated.
            @field ClassName | class | Class name | FancyWoodenChairCushion
            @field BaseClass | combo:@FurnitureBase | Base class | CraftableFurniture
            @field ItemID | itemid | Item ID | 0xB4F
            @field FlipIDs | string | Flip IDs | 0xB4F, 0xB4E, 0xB50, 0xB51
            @field Name | string | Name |
            @field Hue | hue | Hue | 0
            @field Weight | double | Weight | 20.0
            @template
            using System;

            namespace Server.Items
            {
                [Furniture]
            {{#if FlipIDs}}
                [Flipable({{FlipIDs}})]
            {{/if}}
                public class {{ClassName}} : {{BaseClass}}
                {
                    [Constructable]
                    public {{ClassName}}()
                        : base({{ItemID}})
                    {
            {{#if Name}}
                        Name = "{{Name|cs}}";
            {{/if}}
                        Hue = {{Hue}};
                        Weight = {{Weight}};
                    }

            """ + Serial + """

                }
            }

            """,

            """
            @name Container (box, barrel, gift box …)
            @group ServUO – Furniture & containers
            @output cs
            @file {{ClassName}}.cs
            @description Container with item ID, gump, label number and flip IDs (old tabs GiftBox and closed barrel).
            @field ClassName | class | Class name | HolidayGiftBox
            @field BaseClass | combo:BaseContainer,LockableContainer,TrapableContainer,FillableContainer | Base class | BaseContainer
            @field ItemID | itemid | Item ID | 0x232A
            @field FlipIDs | string | Flip IDs (optional) | 0x232A, 0x232B
            @field Name | string | Name (optional) |
            @field LabelNumber | int:0:9999999 | Cliloc label number (0 = none) | 1156382
            @field GumpID | int:0:65535 | Container gump (0 = default) | 0
            @field Hue | hue | Hue | 0
            @field RandomHue | bool | Random dyed hue | true
            @field Weight | double | Weight | 2.0
            @field Furniture | bool | Furniture attribute | true
            @template
            using System;

            namespace Server.Items
            {
            {{#if Furniture}}
                [Furniture]
            {{/if}}
            {{#if FlipIDs}}
                [Flipable({{FlipIDs}})]
            {{/if}}
                public class {{ClassName}} : {{BaseClass}}
                {
            {{#if LabelNumber}}
                    public override int LabelNumber { get { return {{LabelNumber}}; } }

            {{/if}}
            {{#if GumpID}}
                    public override int DefaultGumpID { get { return {{GumpID}}; } }

            {{/if}}
                    [Constructable]
                    public {{ClassName}}()
                        : base({{ItemID}})
                    {
            {{#if Name}}
                        Name = "{{Name|cs}}";
            {{/if}}
            {{#if RandomHue}}
                        Hue = Utility.RandomDyedHue();
            {{else}}
                        Hue = {{Hue}};
            {{/if}}
                        Weight = {{Weight}};
                    }

            """ + Serial + """

                }
            }

            """,

            """
            @name Commodity deed box (reward)
            @group ServUO – Furniture & containers
            @output cs
            @file {{ClassName}}.cs
            @description Veteran reward box for commodity deeds (old tab Commodity Deed Box).
            @field ClassName | class | Class name | CommodityDeedBox
            @field BaseClass | combo:BaseContainer,LockableContainer | Base class | BaseContainer
            @field ItemID | itemid | Item ID | 0x9AA
            @field FlipID | itemid | Flip ID | 0xE7D
            @field Hue | hue | Hue | 0x47
            @field Weight | double | Weight | 4.0
            @field LabelNumber | int:0:9999999 | Label number | 1080523
            @field GumpID | int:0:65535 | Container gump | 0x43
            @field RewardCliloc | int:0:9999999 | Reward text cliloc | 1076217
            @template
            using System;
            using Server.Engines.VeteranRewards;

            namespace Server.Items
            {
                [Furniture]
                [Flipable({{ItemID}}, {{FlipID}})]
                public class {{ClassName}} : {{BaseClass}}, IRewardItem
                {
                    private bool m_IsRewardItem;

                    [Constructable]
                    public {{ClassName}}()
                        : base({{ItemID}})
                    {
                        Hue = {{Hue}};
                        Weight = {{Weight}};
                    }

                    public {{ClassName}}(Serial serial)
                        : base(serial)
                    {
                    }

                    public override int LabelNumber { get { return {{LabelNumber}}; } }
                    public override int DefaultGumpID { get { return {{GumpID}}; } }

                    [CommandProperty(AccessLevel.GameMaster)]
                    public bool IsRewardItem
                    {
                        get { return m_IsRewardItem; }
                        set { m_IsRewardItem = value; InvalidateProperties(); }
                    }

                    public static {{ClassName}} Find(Item deed)
                    {
                        Item parent = deed;
                        while (parent != null && !(parent is {{ClassName}}))
                            parent = parent.Parent as Item;
                        return parent as {{ClassName}};
                    }

                    public override void GetProperties(ObjectPropertyList list)
                    {
                        base.GetProperties(list);
                        if (m_IsRewardItem)
                            list.Add({{RewardCliloc}});
                    }

                    public override void Serialize(GenericWriter writer)
                    {
                        base.Serialize(writer);
                        writer.WriteEncodedInt(0); // version
                        writer.Write((bool)m_IsRewardItem);
                    }

                    public override void Deserialize(GenericReader reader)
                    {
                        base.Deserialize(reader);
                        int version = reader.ReadEncodedInt();
                        m_IsRewardItem = reader.ReadBool();
                    }
                }
            }

            """,

            """
            @name Bag with items
            @group ServUO – Furniture & containers
            @output cs
            @file {{ClassName}}.cs
            @description A bag filled with items, e.g. reagents (old tab Bag). Item = amount; empty amount = one item.
            @field ClassName | class | Class name | BagOfReagents
            @field BaseClass | combo:Bag,Backpack,Pouch | Base class | Bag
            @field Name | string | Name | a bag of reagents
            @field Hue | hue | Hue | 0
            @field Items | list:@ItemType | Items (type = amount) | BlackPearl=50;Bloodmoss=50;Garlic=50;Ginseng=50;MandrakeRoot=50;Nightshade=50;SulfurousAsh=50;SpidersSilk=50
            @template
            using System;

            namespace Server.Items
            {
                public class {{ClassName}} : {{BaseClass}}
                {
                    [Constructable]
                    public {{ClassName}}()
                    {
            {{#if Name}}
                        Name = "{{Name|cs}}";
            {{/if}}
                        Hue = {{Hue}};
            {{#each Items}}
            {{#if value}}
                        DropItem(new {{key}}({{value}}));
            {{else}}
                        DropItem(new {{key}}());
            {{/if}}
            {{/each}}
                    }

            """ + Serial + """

                }
            }

            """,

            // ---------------------------------------------------------------- deco
            """
            @name Decoration item
            @group ServUO – Decoration
            @output cs
            @file {{ClassName}}.cs
            @description Simple item: graphic, name, hue, weight, movable, optional light (needs the LightSource flag in tiledata).
            @field ClassName | class | Class name | GoldenStatue
            @field ItemID | itemid | Item ID | 0x139A
            @field Name | string | Name | a golden statue
            @field Hue | hue | Hue | 0x8A5
            @field Weight | double | Weight | 10.0
            @field Movable | bool | Movable | true
            @field LootType | choice:@LootType | Loot type | Regular
            @field Light | combo:@LightType | Light (empty = none) |
            @template
            using System;

            namespace Server.Items
            {
                public class {{ClassName}} : Item
                {
                    [Constructable]
                    public {{ClassName}}()
                        : base({{ItemID}})
                    {
            {{#if Name}}
                        Name = "{{Name|cs}}";
            {{/if}}
                        Hue = {{Hue}};
                        Weight = {{Weight}};
                        Movable = {{Movable}};
                        LootType = LootType.{{LootType}};
            {{#if Light}}
                        Light = LightType.{{Light}};
            {{/if}}
                    }

            """ + Serial + """

                }
            }

            """,

            """
            @name Damageable wall
            @group ServUO – Decoration
            @output cs
            @file {{ClassName}}.cs
            @description Wall that can be destroyed (old tab Wall / Sand). Destroyed ID = graphic of the rubble.
            @field ClassName | class | Class name | SandstoneWall
            @field BaseClass | string | Base class | DamageableItem
            @field ItemID | itemid | Item ID | 0x3C9
            @field DestroyedID | itemid | Destroyed ID | 0x277
            @field Name | string | Name | sandstone wall
            @field Hue | hue | Hue | 0
            @field Level | choice:@ItemLevel | Level | VeryEasy
            @field Movable | bool | Movable | false
            @template
            using System;

            namespace Server.Items
            {
                public class {{ClassName}} : {{BaseClass}}
                {
                    [Constructable]
                    public {{ClassName}}()
                        : base({{ItemID}}, {{DestroyedID}})
                    {
                        Name = "{{Name|cs}}";
                        Hue = {{Hue}};
                        Level = ItemLevel.{{Level}};
                        Movable = {{Movable}};
                    }

            """ + Serial + """

                }
            }

            """,

            """
            @name Door
            @group ServUO – Decoration
            @output cs
            @file {{ClassName}}.cs
            @description Door with closed / open graphic, sounds and the offset it moves when opened.
            @field ClassName | class | Class name | DarkWoodDoor
            @field ClosedID | itemid | Closed ID | 0x6A5
            @field OpenedID | itemid | Opened ID | 0x6A6
            @field OpenSound | sound | Open sound | 0xEA
            @field CloseSound | sound | Close sound | 0xF1
            @field OffsetX | int:-5:5 | Offset X | -1
            @field OffsetY | int:-5:5 | Offset Y | 1
            @field Hue | hue | Hue | 0
            @template
            using System;

            namespace Server.Items
            {
                public class {{ClassName}} : BaseDoor
                {
                    [Constructable]
                    public {{ClassName}}()
                        : base({{ClosedID}}, {{OpenedID}}, {{OpenSound}}, {{CloseSound}}, new Point3D({{OffsetX}}, {{OffsetY}}, 0))
                    {
                        Hue = {{Hue}};
                    }

            """ + Serial + """

                }
            }

            """,

            // ---------------------------------------------------------------- books
            """
            @name Book
            @group ServUO – Books
            @output cs
            @file {{ClassName}}.cs
            @description Book with a fixed text (old tab Book). Lines are wrapped at whole words, an empty line starts a new page.
            @field ClassName | class | Class name | TalesOfBritain
            @field Title | string | Title | Tales of Britain
            @field Author | string | Author | Nikodemus
            @field ItemID | itemid | Item ID (cover) | 0xFF2
            @field Hue | hue | Cover hue | 0
            @field Writable | bool | Writable | false
            @field Text | pages:8:22 | Text | Once upon a time in Britain …\n\nThe second page starts after an empty line.
            @template
            using System;
            using Server;

            namespace Server.Items
            {
                public class {{ClassName}} : BaseBook
                {
                    public static readonly BookContent Content = new BookContent
                    (
                        "{{Title|cs}}", "{{Author|cs}}",
            {{#each Text}}
                        new BookPageInfo({{value}}){{#if !last}},{{/if}}
            {{/each}}
                    );

                    public override BookContent DefaultContent { get { return Content; } }

                    [Constructable]
                    public {{ClassName}}()
                        : base({{ItemID}}, {{Writable}})
                    {
                        Hue = {{Hue}};
                    }

                    public {{ClassName}}(Serial serial)
                        : base(serial)
                    {
                    }

                    public override void Serialize(GenericWriter writer)
                    {
                        base.Serialize(writer);
                        writer.WriteEncodedInt(0); // version
                    }

                    public override void Deserialize(GenericReader reader)
                    {
                        base.Deserialize(reader);
                        int version = reader.ReadEncodedInt();
                    }
                }
            }

            """,

            // ---------------------------------------------------------------- Sphere
            """
            @name Sphere item (ITEMDEF)
            @group Sphere
            @output scp
            @file {{DefName}}.scp
            @description Item definition for SphereServer. IDs are written in Sphere hex (0abc).
            @field DefName | class | Defname | i_storm_bow
            @field ItemID | itemid | Item ID | 0x13B2
            @field Name | string | Name | storm bow
            @field Type | combo:t_normal,t_container,t_weapon_sword,t_weapon_fence,t_weapon_mace_sharp,t_weapon_axe,t_weapon_bow,t_weapon_xbow,t_armor,t_shield,t_clothing,t_jewelry,t_light_lit,t_book,t_food,t_potion | Type | t_weapon_bow
            @field Hue | hue | Color | 0
            @field Layer | int:0:30 | Layer (0 = none) | 2
            @field Damage | string | Damage (min,max) | 15, 19
            @field Armor | string | Armor (min,max) |
            @field Weight | double | Weight | 6
            @field Value | string | Value | 120
            @field Category | string | Category (Axis) | Custom
            @field Subsection | string | Subsection (Axis) | Weapons
            @template
            [ITEMDEF {{DefName}}]
            ID={{ItemID|scp}}
            NAME={{Name}}
            TYPE={{Type}}
            {{#if Layer}}
            LAYER={{Layer}}
            {{/if}}
            {{#if Damage}}
            DAM={{Damage}}
            {{/if}}
            {{#if Armor}}
            ARMOR={{Armor}}
            {{/if}}
            WEIGHT={{Weight}}
            VALUE={{Value}}
            CATEGORY={{Category}}
            SUBSECTION={{Subsection}}
            DESCRIPTION={{Name}}

            ON=@Create
            {{#if Hue}}
                COLOR={{Hue|scp}}
            {{/if}}

            [EOF]

            """,

            """
            @name Sphere character (CHARDEF)
            @group Sphere
            @output scp
            @file {{DefName}}.scp
            @description Character definition for SphereServer. Random values: {min max}.
            @field DefName | class | Defname | c_storm_drake
            @field Body | body | Body ID | 0x3C
            @field Name | string | Name | storm drake
            @field Sound | sound | Sound | 362
            @field Icon | itemid | Icon item ID | 0x20D6
            @field Damage | string | Damage (min,max) | 11, 17
            @field Armor | int:0:200 | Armor | 46
            @field Brain | choice:brain_monster,brain_animal,brain_human,brain_vendor,brain_healer,brain_guard,brain_beggar | Brain | brain_monster
            @field Str | string | Strength | {401 430}
            @field Dex | string | Dexterity | {133 152}
            @field Int | string | Intelligence | {101 140}
            @field Hue | hue | Color | 0
            @field Skills | list:@SkillName | Skills | Magery=40.0;MagicResist=80.0;Tactics=90.0;Wrestling=80.0
            @field Fame | int:-10000:10000 | Fame | 5500
            @field Karma | int:-10000:10000 | Karma | -5500
            @field Category | string | Category (Axis) | Custom
            @field Subsection | string | Subsection (Axis) | Monsters
            @template
            [CHARDEF {{DefName}}]
            ID={{Body|scp}}
            NAME={{Name}}
            SOUND={{Sound|scp}}
            ICON={{Icon|scp}}
            DAM={{Damage}}
            ARMOR={{Armor}}
            CATEGORY={{Category}}
            SUBSECTION={{Subsection}}
            DESCRIPTION={{Name}}

            ON=@Create
                NPC={{Brain}}
            {{#if Hue}}
                COLOR={{Hue|scp}}
            {{/if}}
                STR={{Str}}
                DEX={{Dex}}
                INT={{Int}}
                FAME={{Fame}}
                KARMA={{Karma}}
            {{#each Skills}}
                {{key|upper}}={{value}}
            {{/each}}

            [EOF]

            """
        };
    }
}
