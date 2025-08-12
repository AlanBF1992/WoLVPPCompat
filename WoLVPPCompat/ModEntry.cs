using DaLion.Professions.Framework;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using WoLVPPCompat.Patches.VPP;
using WoLVPPCompat.Patches.WoL;

namespace WoLVPPCompat
{
    public class ModEntry : Mod
    {
        /// <summary>Monitoring and logging for the mod.</summary>
        public static IMonitor LogMonitor { get; internal set; } = null!;

        /// <summary>Simplified APIs for writing mods.</summary>
        public static IModHelper ModHelper { get; internal set; } = null!;

        /// <summary>Simplified APIs for writing mods.</summary>
        new public static IManifest ModManifest { get; internal set; } = null!;
        public override void Entry(IModHelper helper)
        {
            LogMonitor = Monitor;
            ModHelper = Helper;
            ModManifest = base.ModManifest;

            Harmony harmony = new(ModManifest.UniqueID);

            Patches(harmony);

            helper.Events.GameLoop.GameLaunched += VPPLvlExperience;
        }

        private static void VPPLvlExperience(object? sender, GameLaunchedEventArgs e)
        {
            var vppAPI = ModHelper.ModRegistry.GetApi<IVanillaPlusProfessions>("KediDili.VanillaPlusProfessions")!;
            var wolAPI = ModHelper.ModRegistry.GetApi<IProfessionsApi>("DaLion.Professions")!;

            for (int i = 1; i <= vppAPI.LevelExperiences.Length; i++)
            {
                vppAPI.LevelExperiences[i - 1] = (int)wolAPI.GetConfig().Masteries.ExpPerPrestigeLevel * i + ISkill.LEVEL_10_EXP;
            }
        }

        private static void Patches(Harmony harmony)
        {
            // WoL: Skip their profession being added at lvl 14 and 19
            harmony.Patch(
                original: AccessTools.Method(typeof(LevelUpMenu), nameof(LevelUpMenu.update)),
                transpiler: new HarmonyMethod(typeof(LevelUpMenuPatch), nameof(LevelUpMenuPatch.updateTranspiler))
            );

            // VPP: Profession are gained at lvl 14 and 19
            harmony.Patch(
                original: AccessTools.Method("VanillaPlusProfessions.DisplayHandler:HandleLevelUpMenu"),
                transpiler: new HarmonyMethod(typeof(DisplayHandlerPatch), nameof(DisplayHandlerPatch.HandleLevelUpMenuTranspiler))
            );

            // VPP: Change Icons to show on 14 and 19
            harmony.Patch(
                original: AccessTools.Method("VanillaPlusProfessions.Utilities.CoreUtility:GetProfessionIconImage"),
                transpiler: new HarmonyMethod(typeof(CoreUtilityPatch), nameof(CoreUtilityPatch.GetProfessionIconImageTranspiler))
            );
        }
    }
}
