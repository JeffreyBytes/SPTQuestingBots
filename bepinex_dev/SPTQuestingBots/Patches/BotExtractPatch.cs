﻿using System.Reflection;
using UnityEngine;
using SPT.Reflection.Patching;
using SPTQuestingBots.Controllers;
using EFT;

namespace SPTQuestingBots.Patches
{
    internal class BotExtractPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BaseLocalGame<EftGamePlayerOwner>).GetMethod(nameof(BaseLocalGame<EftGamePlayerOwner>.BotDespawn));
        }

        [PatchPrefix]
        protected static void PatchPrefix(BotOwner botOwner)
        {
            LoggingController.LogDebug($"{botOwner.GetText()} extracted.");

            botOwner.GetPlayer.gameObject.TryGetComponent<BotLogic.Objective.BotObjectiveManager>(out var objectiveManager);
            Object.Destroy(objectiveManager);
        }
    }
}
