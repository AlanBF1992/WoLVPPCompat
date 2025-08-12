using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Reflection.Emit;

namespace WoLVPPCompat.Patches.WoL
{
    internal static class LevelUpMenuPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> updateTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                var currentLvlInfo = AccessTools.Field(typeof(LevelUpMenu), "currentLevel");
                var originalAddInfo = AccessTools.Method(typeof(LevelUpMenuPatch), nameof(originalAdd));

                //from: just adding things
                //to:   check if lvl is 14 or 19
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Ldarg_0)
                    )
                ;

                matcher.CreateLabelWithOffsets(23, out Label skipAddingWoL);

                var checkInstructions = matcher.InstructionsWithOffsets(2, 5);

                checkInstructions.AddRange([
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, currentLvlInfo),
                    new CodeInstruction(OpCodes.Call, originalAddInfo),
                    new CodeInstruction(OpCodes.Brtrue, skipAddingWoL)
                ]);

                matcher.Insert(checkInstructions);

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(updateTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static bool originalAdd(int professionToAdd, int currentLevel)
        {
            if (currentLevel is 14 or 19)
            {
                Game1.player.professions.Add(professionToAdd);
                return true;
            }
            return false;
        }
    }
}
