using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;

namespace WoLVPPCompat.Patches.VPP
{
    internal static class DisplayHandlerPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> HandleLevelUpMenuTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                MethodInfo levelRequirementInfo = AccessTools.Method("VanillaPlusProfessions.Profession:get_LevelRequirement");

                //from: currentLevel is 15 or 20
                //to:   currentLevel is 14 or 19
                for (int i = 1; i <= 3; i++)
                {
                    matcher
                        .MatchStartForward(
                            new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15)
                        )
                        .ThrowIfNotMatch($"DisplayHandlerPatch.HandleLevelUpMenuTranspiler: IL code {i}a not found")
                        .SetOperandAndAdvance(14)
                        .MatchStartForward(
                            new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)20)
                        )
                        .ThrowIfNotMatch($"DisplayHandlerPatch.HandleLevelUpMenuTranspiler: IL code {i}b not found")
                        .SetOperandAndAdvance(19)
                    ;
                }

                //from: currentLevel == item.Value.LevelRequirement
                //to:   currentLevel == item.Value.LevelRequirement - 1
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Callvirt, levelRequirementInfo)
                    )
                    .ThrowIfNotMatch("DisplayHandlerPatch.HandleLevelUpMenuTranspiler: IL code 4 not found")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Sub)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(HandleLevelUpMenuTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }
    }
}