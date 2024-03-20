using HarmonyLib;
using BoplFixedMath

namespace Splotch.Patches
{
    public static class SplotchPatches
    {
        public static void ApplyPatches()
        {
        
            // Harmony Harmony = new Harmony("Harmony") - Melon, 2024
            Harmony harmony = new Harmony("com.Splotch.splotchpatches");


            // epic patching
            MethodInfo originalfix = AccessTools.Method(typeof(Client), "Awake");
            MethodInfo splotchfix = AccessTools.Method(typeof(SplotchFixes), "BugFixPatch");
            harmony.Patch(originalfix, new HarmonyMethod(splotchfix));
            
        }
        public class SplotchFixes
        {
            // Code from Client - 306-334 - Need to patch this but eh, will do that later.
            public static bool UpdateInputHistoryReplacement(Client __instance, InputPacketUpdate update)
            {
                uint num = this.inputHistoryHeadSeqNum;
            int num2 = (int)(update.seqNumber - num);
            if (num2 > 32)
            {
                Debug.LogError("Discarded a packet because it was too new");
                return true;
            }
            for (int i = num2 - 1; i >= 0; i--)
            {
                uint num = this.inputHistoryHeadSeqNum;
            int num2 = (int)(update.seqNumber - num);
            if (num2 > 32)
            {
                Debug.LogError("Discarded a packet because it was too new");
                return true;
            }
            for (int i = num2 - 1; i >= 0; i--)
            {
                uint num3 = 1U << i;
                InputPacket inputPacket = default(InputPacket);
                inputPacket.jump = ((update.jump & num3) > 0U);
                inputPacket.ab1 = ((update.ab1 & num3) > 0U);
                inputPacket.ab2 = ((update.ab2 & num3) > 0U);
                inputPacket.ab3 = ((update.ab3 & num3) > 0U);
                inputPacket.select = ((update.select & num3) > 0U);
                inputPacket.start = ((update.start & num3) > 0U);
                inputPacket.w = ((update.w & num3) > 0U);
                inputPacket.a = ((update.a & num3) > 0U);
                inputPacket.s = ((update.s & num3) > 0U);
                inputPacket.d = ((update.d & num3) > 0U);
                inputPacket.joystickAngle = update.joystickAngle[i];
                inputPacket.targetDelayBufferSize = update.targetDelayBufferSize;
                inputPacket.seqNumber = num + 1U;
                num += 1U;
                this.inputHistory.Enqueue(inputPacket);
                this.inputHistoryHeadSeqNum = inputPacket.seqNumber;
            }
            return false;
            }
        }
    }
    
}
