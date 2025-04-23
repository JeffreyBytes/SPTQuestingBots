﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using SPTQuestingBots.Controllers;
using SPTQuestingBots.Models.Debug;
using SPTQuestingBots.Models.Questing;
using UnityEngine;

namespace SPTQuestingBots.Components
{
    public class DebugData : MonoBehaviour
    {
        private readonly static float markerRadius = 0.5f;

        private List<AbstractDebugGizmo> gizmos = new List<AbstractDebugGizmo>();

        private int jobAssignmentGizmoCount => gizmos.Count(gizmo => gizmo is Models.Debug.JobAssignmentGizmo);

        public void RegisterBot(BotOwner bot)
        {
            gizmos.Add(new Models.Debug.BotInfoGizmo(bot));
            gizmos.Add(new Models.Debug.BotPathInfoGizmo(bot, markerRadius));
        }

        protected void Awake()
        {
            QuestingBotsPluginConfig.QuestOverlayFontSize.SettingChanged += (object sender, EventArgs e) => { updateGuiStyles(); };

            gizmos.Add(new PlayerCoordinatesGizmo());
        }

        private void updateGuiStyles() => gizmos.ForEach(gizmo => gizmo.UpdateGUIStyle());

        protected void Update()
        {
            if (!Singleton<GameWorld>.Instance.GetComponent<BotQuestBuilder>().HaveQuestsBeenBuilt)
            {
                return;
            }

            if (QuestingBotsPluginConfig.ShowQuestInfoOverlays.Value && (jobAssignmentGizmoCount == 0))
            {
                loadAllPossibleJobAssignments();
            }

            removeUnneededGizmos();

            foreach (AbstractDebugGizmo gizmo in gizmos)
            {
                gizmo.Update();
            }
        }

        protected void OnGUI()
        {
            if ((!Singleton<GameWorld>.Instantiated) || (Camera.main == null))
            {
                return;
            }

            foreach (AbstractDebugGizmo gizmo in gizmos)
            {
                gizmo.Draw();
            }
        }

        private void removeUnneededGizmos()
        {
            foreach (AbstractDebugGizmo gizmo in gizmos.Where(gizmo => gizmo.ReadyToDispose()))
            {
                gizmo.Disable();
            }

            gizmos.RemoveAll(gizmo => gizmo.ReadyToDispose());
        }

        private void loadAllPossibleJobAssignments()
        {
            LoggingController.LogInfo("Loading all possible job assignments...");

            IEnumerable<JobAssignment> jobAssignments = BotJobAssignmentFactory.CreateAllPossibleJobAssignments();

            Vector3 lastPosition = Vector3.positiveInfinity;
            Quest lastQuest = null;
            foreach (JobAssignment jobAssignment in jobAssignments)
            {
                // Ensure the position is valid and isn't the same as the previous step in the quest objective
                Vector3? stepPosition = jobAssignment.Position;
                if (!stepPosition.HasValue || (stepPosition == lastPosition))
                {
                    continue;
                }

                addGizmosForQuestStep(jobAssignment, stepPosition.Value);

                if (lastQuest != jobAssignment.QuestAssignment)
                {
                    IList<Vector3> waypoints = jobAssignment.QuestAssignment.GetWaypointPositions();
                    for (int w = 0; w < waypoints.Count; w++)
                    {
                        addGizmosForQuestWaypoint(jobAssignment, waypoints[w], w + 1);
                    }
                }

                lastPosition = stepPosition.Value;
                lastQuest = jobAssignment.QuestAssignment;
            }

            LoggingController.LogInfo("Loading all possible job assignments...done (Created " + jobAssignmentGizmoCount + " markers).");
        }

        private void addGizmosForQuestStep(JobAssignment jobAssignment, Vector3 position)
        {
            string questText = "Quest: " + jobAssignment.QuestAssignment.ToString();
            questText += "\nObjective: " + jobAssignment.QuestObjectiveAssignment.ToString();
            questText += "\nStep: " + jobAssignment.QuestObjectiveStepAssignment.ToString();
            questText += "\nDistance: ";

            gizmos.Add(new JobAssignmentGizmo(jobAssignment, position, questText, markerRadius, Color.red));
        }

        private void addGizmosForQuestWaypoint(JobAssignment jobAssignment, Vector3 position, int waypointNumber)
        {
            string questText = "Quest: " + jobAssignment.QuestAssignment.ToString();
            questText += "\nWaypoint #" + waypointNumber + ": " + position;
            questText += "\nDistance: ";

            gizmos.Add(new JobAssignmentGizmo(jobAssignment, position, questText, markerRadius, Color.blue));
        }
    }
}
