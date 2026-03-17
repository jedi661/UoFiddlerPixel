namespace UoFiddler.Controls.Uop
{
    public static class UopConstants
    {
        public const int MAX_ANIMATIONS_DATA_INDEX_COUNT = 0x10000; // 65536
        public const int ANIMATION_UOP_GROUPS_COUNT = 200; 
        public const int ANIMATION_GROUPS_COUNT = 35; 
        public const int MAX_ANIMATION_FRAME_UOP_FILES = 15; // Increased to match my previous change

        public static class ActionNames
        {
            public static readonly string[] MonsterActions = new[]
            {
                "Walk", "Idle", "Die1", "Die2", "Attack1", "Attack2", "Attack3", "AttackBow", "AttackCrossBow",
                "AttackThrow", "GetHit", "Pillage", "Stomp", "Cast2", "Cast3", "BlockRight", "BlockLeft", "Idle",
                "Fidget", "Fly", "TakeOff", "GetHitInAir"
            };

            public static readonly string[] AnimalActions = new[]
            {
                "Walk", "Run", "Idle", "Eat", "Alert", "Attack1", "Attack2", "GetHit", "Die1", "Idle",
                "Fidget", "LieDown", "Die2"
            };

            public static readonly string[] HumanActions = new[]
            {
                "Walk_01", "WalkStaff_01", "Run_01", "RunStaff_01", "Idle_01", "Idle_01", "Fidget_Yawn_Stretch_01",
                "CombatIdle1H_01", "CombatIdle1H_01", "AttackSlash1H_01", "AttackPierce1H_01", "AttackBash1H_01",
                "AttackBash2H_01", "AttackSlash2H_01", "AttackPierce2H_01", "CombatAdvance_1H_01", "Spell1",
                "Spell2", "AttackBow_01", "AttackCrossbow_01", "GetHit_Fr_Hi_01", "Die_Hard_Fwd_01",
                "Die_Hard_Back_01", "Horse_Walk_01", "Horse_Run_01", "Horse_Idle_01", "Horse_Attack1H_SlashRight_01",
                "Horse_AttackBow_01", "Horse_AttackCrossbow_01", "Horse_Attack2H_SlashRight_01",
                "Block_Shield_Hard_01", "Punch_Punch_Jab_01", "Bow_Lesser_01", "Salute_Armed1h_01", "Ingest_Eat_01"
            };

            public static readonly string[] CharActions = new[]
            {
                "Walk", "Walk (With Weapon)", "Run", "Run (With Weapon)", "Idle", "Idle (With Weapon)", "Fidget",
                "Idle - Combat (1H Weapon)", "Idle - Combat (2H Weapon)", "Slash Attack (1H Weapon)", "Pierce Attack (1H Weapon)",
                "Bash Attack (1H Weapon)", "Bash Attack (2H Weapon)", "Slash Attack (2H Weapon)", "Pierce Attack (2H Weapon)",
                "Combat Walk (2H Weapon)", "Spell 1", "Spell 2", "Bow Attack", "Crossbow Attack", "Get Hit", "Die Backward",
                "Die Forward", "Walk Mounted", "Run Mounted", "Idle Mounted", "Bash Attack Mounted", "Bow Attack Mounted",
                "Crossbow Attack Mounted", "Slash Attack Mounted", "Shield Block", "Punch", "Bowing", "Salute (Armed)",
                "Drinking", "Combat Walk (1H Weapon)", "Combat Walk (Unarmed)", "Idle (Shield)", "Sitting",
                "Get Hit (2H Weapon)", "Mining", "Idle - Combat (Shield)", "Drinking (Sat Down)", "", "", "", "",
                "Idle (2H Weapon) Mounted", "Get Hit Mounted", "Spell Cast Mounted", "Get Hit (Shield) Mounted", "Drinking Mounted",
                "", "", "", "", "", "", "", "", "Take off", "Land", "Fly Forward (Slow)", "Fly Forward (Fast)",
                "Fly Idle", "Fly Idle Combat", "Fly Fidget", "Fly Fidget 2", "Fly Get Hit", "Fly Die Backward",
                "Fly Die Forward", "Fly Attack (1H Weapon)", "Fly Attack (2H Weapon)", "Fly Attack (Boomerang)",
                "Fly Get Hit (Shield)", "Fly Spell 1", "Fly Spell 2", "Fly Get Hit", "Fly Drinking"
            };
        }
    }
}
