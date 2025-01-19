﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Reflection.Patching;
using EFT;
using SPTQuestingBots.Controllers;

namespace SPTQuestingBots.Patches
{
    public class IsFollowerSuitableForBossPatch : ModulePatch
    {
        private static readonly IReadOnlyCollection<Profile> emptyProfileCollection = new Profile[0];

        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotBoss).GetMethod("OfferSelf", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPrefix]
        protected static bool PatchPrefix(ref bool __result, BotBoss __instance, BotOwner offer)
        {
            // EFT sometimes instructs bots ask themselves to be followers of themselves. I guess they're really lonely, so we'll allow it.  
            if (__instance.Owner.Profile.Id == offer.Profile.Id)
            {
                return true;
            }

            IReadOnlyCollection<Profile> bossGroupMemberProfiles = emptyProfileCollection;
            if (Components.Spawning.BotGenerator.TryGetBotGroupFromAnyGenerator(__instance.Owner, out Models.BotSpawnInfo botSpawnInfo))
            {
                bossGroupMemberProfiles = botSpawnInfo.GetGeneratedProfiles();
            }

            IReadOnlyCollection<Profile> offerGroupMemberProfiles = emptyProfileCollection;
            if (Components.Spawning.BotGenerator.TryGetBotGroupFromAnyGenerator(offer, out botSpawnInfo))
            {
                offerGroupMemberProfiles = botSpawnInfo.GetGeneratedProfiles();
            }

            Controllers.LoggingController.LogInfo(__instance.Owner.GetText() + "'s spawn group contains: " + string.Join(",", bossGroupMemberProfiles.Select(m => m.Nickname)));
            Controllers.LoggingController.LogInfo(offer.GetText() + "'s spawn group contains: " + string.Join(",", offerGroupMemberProfiles.Select(m => m.Nickname)));

            List<BotOwner> bossGroupMembers = SPT.Custom.CustomAI.AiHelpers.GetAllMembers(__instance.Owner.BotsGroup);
            List<BotOwner> offerGroupMembers = SPT.Custom.CustomAI.AiHelpers.GetAllMembers(offer.BotsGroup);
            Controllers.LoggingController.LogInfo(__instance.Owner.GetText() + "'s group contains: " + string.Join(",", bossGroupMembers.Select(m => m.GetText())));
            Controllers.LoggingController.LogInfo(offer.GetText() + "'s group contains: " + string.Join(",", offerGroupMembers.Select(m => m.GetText())));

            // If neither the boss nor the offer were spawned in the bot groups generated by this mod, run the EFT code
            if ((bossGroupMemberProfiles.Count == 0) && (offerGroupMemberProfiles.Count == 0))
            {
                return true;
            }

            // Allow the offer to join the boss's group if it spawned in that group
            if (bossGroupMemberProfiles.Any(m => m.Id == offer.Profile.Id))
            {
                return true;
            }

            Controllers.LoggingController.LogInfo("Preventing " + offer.GetText() + " from becoming a follower for " + __instance.Owner.GetText());

            __result = false;
            return false;
        }

        [PatchPostfix]
        protected static void PatchPostfix(bool __result, BotBoss __instance, BotOwner offer, GClass430 ____followers)
        {
            Controllers.LoggingController.LogInfo("Checking if " + offer.GetText() + " can follow " + __instance.Owner.GetText() + ": " + __result);

            if (____followers.Followers.Count >= ____followers.TargetFollowersCount)
            {
                Controllers.LoggingController.LogWarning(__instance.Owner.GetText() + " already has enough followers");
            }
            if (!BotBoss.IsFollowerSuitableForBoss(offer.Profile.Info.Settings.Role, __instance.Owner.Profile.Info.Settings.Role))
            {
                Controllers.LoggingController.LogWarning(offer.GetText() + " is not a suitable follower for " + __instance.Owner.GetText());
            }
            if (offer.BotFollower.HaveBoss)
            {
                Controllers.LoggingController.LogWarning(offer.GetText() + " already has a boss: " + offer.BotFollower.BossToFollow.Player().GetText());
            }
            if (offer.Boss.IamBoss)
            {
                Controllers.LoggingController.LogWarning(offer.GetText() + " is a boss");
            }
        }
    }
}
