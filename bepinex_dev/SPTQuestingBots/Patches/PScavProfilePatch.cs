﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT;
using SPT.Reflection.Patching;
using SPTQuestingBots.Controllers;

namespace SPTQuestingBots.Patches
{
    internal class PScavProfilePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // BotsPresets : GClass658
            return typeof(GClass658).GetMethod("GetNewProfile", new Type[] { typeof(BotCreationDataClass), typeof(bool) });
        }

        [PatchPrefix]
        protected static bool PatchPrefix(GClass658 __instance, ref Profile __result, BotCreationDataClass data, bool withDelete)
        {
            if (__instance.list_0.Count > 0)
            {
                if (ServerRequestPatch.ForcePScavCount > 0)
                {
                    // Filter list to only pscavs, if none - the client requests from the server
                    // Use Info.Settings.Role instead of Side. Server always return x.Side == EPlayerSide.Savage
                    List<Profile> filteredProfiles = __instance.list_0.ApplyFilter(x => x.Info.Settings.Role == WildSpawnType.assault && x.Nickname.Contains("("));
                    __result = data.ChooseProfile(filteredProfiles, withDelete);

                    return false;
                }
                else
                {
                    // Filter list to only scavs (No pmc nickname)
                    List<Profile> filteredProfiles = __instance.list_0.ApplyFilter(x => (x.Info.Settings.Role == WildSpawnType.assault && !x.Nickname.Contains("(") || x.Info.Settings.Role != WildSpawnType.assault));
                    __result = data.ChooseProfile(filteredProfiles, withDelete);

                    return false;
                }
            }
            __result = null;
            return false;
        }
    }
}
