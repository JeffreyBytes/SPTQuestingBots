﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTQuestingBots.Controllers;
using SPTQuestingBots.Helpers;

namespace SPTQuestingBots.Patches
{
    public class MenuShowPatch : ModulePatch
    {
        private static bool _displayedReflexWarning = false;

        protected override MethodBase GetTargetMethod()
        {
            // Same as SPT method to display plugin errors
            return typeof(MenuScreen).GetMethods().First(m => m.Name == nameof(MenuScreen.Show));
        }

        [PatchPostfix]
        protected static void PatchPostfix()
        {
            checkNvidiaReflex();
        }

        private static void checkNvidiaReflex()
        {
            if (_displayedReflexWarning)
            {
                return;
            }

            if (!ConfigController.Config.Enabled || !ConfigController.Config.BotSpawns.Enabled)
            {
                return;
            }

            if (!GameCompatibilityCheckHelper.IsNvidiaReflexEnabled())
            {
                _displayedReflexWarning = false;
                return;
            }

            showNvidiaReflexWarning();
        }

        private static void showNvidiaReflexWarning()
        {
            string profileWarningMessage = "Using nVidia Reflex may result in long raid loading times";
            NotificationManagerClass.DisplayWarningNotification(profileWarningMessage, EFT.Communications.ENotificationDurationType.Long);
            LoggingController.LogWarningToServerConsole(profileWarningMessage);

            _displayedReflexWarning = true;
        }
    }
}
