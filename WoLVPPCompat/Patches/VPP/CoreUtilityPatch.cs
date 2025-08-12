using HarmonyLib;
using StardewModdingAPI;
using System.Reflection.Emit;

namespace WoLVPPCompat.Patches.VPP
{
    internal static class CoreUtilityPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> GetProfessionIconImageTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)15)
                    )
                    .ThrowIfNotMatch("CoreUtilityPatch.GetProfessionIconImageTranspiler: IL code 1 not found")
                    .SetOperandAndAdvance(14)
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)20)
                    )
                    .ThrowIfNotMatch("CoreUtilityPatch.GetProfessionIconImageTranspiler: IL code 2 not found")
                    .SetOperandAndAdvance(19)
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(GetProfessionIconImageTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }
    }
}