﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using BepInEx.Bootstrap;
using EFT;
using EFT.InputSystem;
using SPTQuestingBots.Controllers;
using SPTQuestingBots.Helpers;

namespace SPTQuestingBots.Patches
{
    public class TarkovInitPatch : ModulePatch
    {
        public static string MinVersion { get; set; } = "0.0.0.0";
        public static string MaxVersion { get; set; } = "999999.999999.999999.999999";

        private static readonly string donutsGuid = "com.dvize.Donuts";
        private static readonly string sainGuid = "me.sol.sain";

        protected override MethodBase GetTargetMethod()
        {
            return typeof(TarkovApplication).GetMethod(nameof(TarkovApplication.Init), BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        protected static void PatchPostfix(IAssetsManager assetsManager, InputTree inputTree)
        {
            if (!Helpers.VersionCheckHelper.IsSPTWithinVersionRange(MinVersion, MaxVersion, out string currentVersion))
            {
                string errorMessage = "Could not load " + QuestingBotsPlugin.ModName + " because it requires SPT ";
                
                if (MinVersion == MaxVersion)
                {
                    errorMessage += MinVersion;
                }
                else if (MaxVersion == "999999.999999.999999.999999")
                {
                    errorMessage += MinVersion + " or later";
                }
                else if (MinVersion == "0.0.0.0")
                {
                    errorMessage += MaxVersion + " or older";
                }
                else
                {
                    errorMessage += "between versions " + MinVersion + " and " + MaxVersion;
                }

                errorMessage += ". The current version is " + currentVersion + ".";

                Chainloader.DependencyErrors.Add(errorMessage);
            }

            if (ConfigController.Config.BotSpawns.Enabled && Chainloader.PluginInfos.Any(p => p.Value.Metadata.GUID == donutsGuid))
            {
                Chainloader.DependencyErrors.Add("Using Questing Bots spawns with DONUTS may result in too many spawns. Use at your own risk.");
            }

            if (ConfigController.Config.Enabled && Chainloader.PluginInfos.Any(p => p.Value.Metadata.GUID == sainGuid))
            {
                LoggingController.LogInfo("SAIN detected. Adjusting Questing Bots brain layer priorities...");
                BotBrainHelpers.AddQuestingBotsBrainLayers(ConfigController.Config.Questing.BrainLayerPriorities.WithSAIN);
            }
            else
            {
                BotBrainHelpers.AddQuestingBotsBrainLayers(ConfigController.Config.Questing.BrainLayerPriorities.WithoutSAIN);
            }
        }
    }
}
