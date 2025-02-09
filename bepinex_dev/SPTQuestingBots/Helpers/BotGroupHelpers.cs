﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPTQuestingBots.Controllers;
using SPTQuestingBots.Models;

namespace SPTQuestingBots.Helpers
{
    public static class BotGroupHelpers
    {
        private static FieldInfo botSpawnerDeadBodiesControllerFieldInfo = null;
        private static FieldInfo botSpawnerAllPlayersFieldInfo = null;

        public static BotsGroup CreateGroup(BotOwner initialBot, BotZone zone, int targetMembersCount)
        {
            BotSpawner botSpawner = Singleton<IBotGame>.Instance.BotsController.BotSpawner;

            // --- From BotsGroup.GetGroupAndSetEnemies ---
            EPlayerSide side = initialBot.Profile.Info.Side;

            List<BotOwner> list = new List<BotOwner>();
            foreach (BotOwner botOwner in botSpawner.method_4(initialBot))
            {
                list.Add(botOwner);
            }

            BotsGroup group = new BotsGroup(zone, botSpawner.BotGame, initialBot, list, GetDeadBodiesController(), GetAllPlayersList(), true);
            group.TargetMembersCount = targetMembersCount;
            botSpawner.Groups.Add(zone, side, group, true);
            // ------------------------------------------

            group.Lock();

            return group;
        }

        private static DeadBodiesController GetDeadBodiesController()
        {
            BotSpawner botSpawner = Singleton<IBotGame>.Instance.BotsController.BotSpawner;

            if (botSpawnerDeadBodiesControllerFieldInfo == null)
            {
                botSpawnerDeadBodiesControllerFieldInfo = AccessTools.Field(typeof(BotSpawner), "_deadBodiesController");
            }

            return (DeadBodiesController)botSpawnerDeadBodiesControllerFieldInfo.GetValue(botSpawner);
        }

        private static List<Player> GetAllPlayersList()
        {
            BotSpawner botSpawner = Singleton<IBotGame>.Instance.BotsController.BotSpawner;

            if (botSpawnerAllPlayersFieldInfo == null)
            {
                botSpawnerAllPlayersFieldInfo = AccessTools.Field(typeof(BotSpawner), "_allPlayers");
            }

            return (List<Player>)botSpawnerAllPlayersFieldInfo.GetValue(botSpawner);
        }

        public static IEnumerable<BotOwner> FindZryachiyAndFollowers()
        {
            if (!Singleton<IBotGame>.Instantiated || (Singleton<IBotGame>.Instance.BotsController?.Bots?.BotOwners == null))
            {
                return Enumerable.Empty<BotOwner>();
            }

            return Singleton<IBotGame>.Instance.BotsController.Bots.BotOwners
                .Where(b => b.IsZryachiyOrFollower())
                .Where(b => !b.IsDead);
        }

        public static Player GetPlayer(this IPlayer player)
        {
            IEnumerable<Player> matchingPlayers = Singleton<GameWorld>.Instance.AllAlivePlayersList
                .Where(p => p.ProfileId == player?.ProfileId);

            if (matchingPlayers.Count() == 1)
            {
                return matchingPlayers.First();
            }

            return null;
        }

        public static BotOwner GetBotOwner(this IPlayer player)
        {
            IEnumerable<BotOwner> matchingOwners = Singleton<IBotGame>.Instance.BotsController.Bots.BotOwners
                .Where(b => b.Profile.Id == player?.Profile?.Id);

            if (matchingOwners.Count() == 1)
            {
                return matchingOwners.First();
            }

            return null;
        }

        public static void FormAlliance(this IPlayer iplayer, IPlayer playerToAlly)
        {
            if (iplayer == null)
            {
                return;
            }

            iplayer.GetBotOwner()?.BotsGroup?.FormAlliance(playerToAlly);
        }

        public static void FormAlliance(this BotsGroup playerGroup, IPlayer playerToAlly)
        {
            if ((playerGroup == null) || (playerToAlly == null))
            {
                return;
            }

            IPlayer[] enemyMatches = playerGroup.Enemies
                    .Where(e => e.Key.Profile.Id == playerToAlly.Profile.Id)
                    .Select(e => e.Key)
                    .ToArray();

            List<BotOwner> groupMemberList = SPT.Custom.CustomAI.AiHelpers.GetAllMembers(playerGroup);
            string groupMembersText = string.Join(", ", groupMemberList.Select(m => m.GetText()));

            foreach (IPlayer remainingEnemy in enemyMatches)
            {
                LoggingController.LogInfo("Group containing " + groupMembersText + " has paused their hostility with " + remainingEnemy.GetText());
                playerGroup.RemoveEnemy(remainingEnemy);
            }

            Player otherPlayer = playerToAlly.GetPlayer();
            if (!playerGroup.Allies.Contains(otherPlayer))
            {
                LoggingController.LogInfo("Group containing " + groupMembersText + " is temporarily allied with " + otherPlayer.GetText());
                playerGroup.AddAlly(otherPlayer);
            }
        }
    }
}
