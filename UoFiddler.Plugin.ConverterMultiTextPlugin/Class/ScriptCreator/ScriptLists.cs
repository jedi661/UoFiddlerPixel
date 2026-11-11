using System;
using System.Collections.Generic;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ScriptCreator
{
    /// <summary>
    /// Named lists for choice / combo / list fields ("choice:@SkillName"). Names as in ServUO.
    /// Own lists: a text file per list in ScriptTemplates\Lists (one entry per line) – it replaces or adds a list.
    /// </summary>
    internal static class ScriptLists
    {
        private static readonly Dictionary<string, string[]> BuiltIn = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["AosAttribute"] = Split("RegenHits RegenStam RegenMana DefendChance AttackChance BonusStr BonusDex BonusInt BonusHits BonusStam BonusMana WeaponDamage WeaponSpeed SpellDamage CastRecovery CastSpeed LowerManaCost LowerRegCost ReflectPhysical EnhancePotions Luck SpellChanneling NightSight IncreasedKarmaLoss Brittle LowerAmmoCost BalancedWeapon"),
            ["AosWeaponAttribute"] = Split("LowerStatReq SelfRepair HitLeechHits HitLeechStam HitLeechMana HitLowerAttack HitLowerDefend HitMagicArrow HitHarm HitFireball HitLightning HitDispel HitColdArea HitFireArea HitPoisonArea HitEnergyArea HitPhysicalArea ResistPhysicalBonus ResistFireBonus ResistColdBonus ResistPoisonBonus ResistEnergyBonus UseBestSkill MageWeapon DurabilityBonus BloodDrinker BattleLust HitCurse HitFatigue HitManaDrain SplinteringWeapon ReactiveParalyze"),
            ["AosArmorAttribute"] = Split("LowerStatReq SelfRepair MageArmor DurabilityBonus ReactiveParalyze SoulCharge"),
            ["SkillName"] = Split("Alchemy Anatomy AnimalLore ItemID ArmsLore Parry Begging Blacksmith Fletching Peacemaking Camping Carpentry Cartography Cooking DetectHidden Discordance EvalInt Healing Fishing Forensics Herding Hiding Provocation Inscribe Lockpicking Magery MagicResist Tactics Snooping Musicianship Poisoning Archery SpiritSpeak Stealing Tailoring AnimalTaming TasteID Tinkering Tracking Veterinary Swords Macing Fencing Wrestling Lumberjacking Mining Meditation Stealth RemoveTrap Necromancy Focus Chivalry Bushido Ninjitsu Spellweaving Mysticism Imbuing Throwing"),
            ["LootPack"] = Split("Poor Meager Average Rich FilthyRich UltraRich SuperBoss LowScrolls MedScrolls HighScrolls Gems Potions"),
            ["AIType"] = Split("AI_Melee AI_Animal AI_Archer AI_Healer AI_Vendor AI_Mage AI_Berserk AI_Predator AI_Thief AI_NecroMage AI_Paladin AI_Spellweaving AI_Mystic AI_Ninja AI_Samurai"),
            ["FightMode"] = Split("Closest Aggressor Strongest Weakest Evil Good None"),
            ["LootType"] = Split("Regular Blessed Newbied Cursed"),
            ["ItemLevel"] = Split("VeryEasy Easy Average Hard VeryHard"),
            ["HideType"] = Split("Regular Spined Horned Barbed"),
            ["FoodType"] = Split("None Meat FruitsAndVegies GrainsAndHay Fish Eggs Gold Metal"),
            ["Bool"] = Split("true false"),
            ["Layer"] = Split("OneHanded TwoHanded Shoes Pants Shirt Helm Gloves Ring Talisman Neck Hair Waist InnerTorso Bracelet Face FacialHair MiddleTorso Earrings Arms Cloak Backpack OuterTorso OuterLegs InnerLegs Mount"),
            ["LightType"] = Split("Circle300 Circle225 Circle150 ArchedWindowEast WestBig NorthBig Empty"),
            ["Reagent"] = Split("BlackPearl Bloodmoss Garlic Ginseng MandrakeRoot Nightshade SulfurousAsh SpidersSilk BatWing GraveDust DaemonBlood NoxCrystal PigIron"),
            ["ItemType"] = Split("Gold BlackPearl Bloodmoss Garlic Ginseng MandrakeRoot Nightshade SulfurousAsh SpidersSilk Bandage Arrow Bolt IronIngot Board Log Cloth Leather Bottle GreaterHealPotion GreaterCurePotion GreaterStrengthPotion TotalRefreshPotion RecallScroll BlankScroll Torch Apple BreadLoaf"),
            ["SBInfo"] = Split("SBBlacksmith SBWeaponSmith SBArmorSmith SBTailor SBProvisioner SBMage SBAlchemist SBHealer SBBaker SBButcher SBCarpenter SBTinker SBJewel SBScribe SBInnKeeper SBFisherman SBBowyer SBHerbalist SBVeterinarian SBShipwright SBMiner SBFarmer SBCook SBLeatherWorker SBHairStylist SBBard SBRealEstateBroker SBArchitect"),
            ["RangedBase"] = Split("Bow Crossbow HeavyCrossbow CompositeBow RepeatingCrossbow Yumi ElvenCompositeLongbow MagicalShortbow"),
            ["MeleeBase"] = Split("Longsword Broadsword Katana Scimitar VikingSword Cutlass Kryss Dagger WarFork Spear ShortSpear Pike Halberd Bardiche Scythe WarAxe BattleAxe DoubleAxe LargeBattleAxe TwoHandedAxe ExecutionersAxe Axe Hatchet WarHammer WarMace Mace Maul HammerPick QuarterStaff GnarledStaff BlackStaff ShepherdsCrook"),
            ["ArmorBase"] = Split("PlateChest PlateArms PlateLegs PlateGloves PlateGorget PlateHelm ChainChest ChainLegs ChainCoif RingmailChest RingmailArms RingmailLegs RingmailGloves LeatherChest LeatherArms LeatherLegs LeatherGloves LeatherGorget LeatherCap StuddedChest StuddedArms StuddedLegs StuddedGloves StuddedGorget BoneChest BoneArms BoneLegs BoneGloves BoneHelm Bascinet CloseHelm Helmet NorseHelm OrcHelm HeaterShield MetalKiteShield Buckler BronzeShield WoodenShield MetalShield WoodenKiteShield ChaosShield OrderShield"),
            ["ClothingBase"] = Split("BaseShirt BasePants BaseOuterTorso BaseMiddleTorso BaseHat BaseShoes BaseWaist BaseCloak BaseOuterLegs"),
            ["ContainerBase"] = Split("BaseContainer LockableContainer TrapableContainer FillableContainer Bag Backpack Pouch WoodenBox MetalBox"),
            ["FurnitureBase"] = Split("CraftableFurniture Item")
        };

        private static Dictionary<string, string[]> _user;

        private static string[] Split(string s) => s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        public static IEnumerable<string> Names => BuiltIn.Keys.Concat(UserLists.Keys).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n);

        private static Dictionary<string, string[]> UserLists
        {
            get
            {
                if (_user == null)
                {
                    _user = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                    try
                    {
                        string dir = System.IO.Path.Combine(ScriptTemplateLibrary.UserFolder, "Lists");
                        if (System.IO.Directory.Exists(dir))
                        {
                            foreach (string f in System.IO.Directory.GetFiles(dir, "*.txt"))
                            {
                                _user[System.IO.Path.GetFileNameWithoutExtension(f)] = System.IO.File.ReadAllLines(f)
                                    .Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith("#", StringComparison.Ordinal)).ToArray();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // unreadable lists are ignored
                    }
                }
                return _user;
            }
        }

        public static void Reload() => _user = null;

        /// <summary>Items of "a,b,c" or "@ListName".</summary>
        public static string[] Resolve(string options)
        {
            options = (options ?? string.Empty).Trim();
            if (options.StartsWith("@", StringComparison.Ordinal))
            {
                string name = options.Substring(1);
                if (UserLists.TryGetValue(name, out string[] u))
                {
                    return u;
                }
                return BuiltIn.TryGetValue(name, out string[] b) ? b : Array.Empty<string>();
            }
            return options.Split(',').Select(o => o.Trim()).Where(o => o.Length > 0).ToArray();
        }
    }
}
