﻿using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QuestingBots.Models
{
    public class QuestZoneObjective : QuestObjective
    {
        public string ZoneID { get; set; } = null;

        public QuestZoneObjective() : base()
        {

        }

        public QuestZoneObjective(string zoneID) : this()
        {
            ZoneID = zoneID;
        }

        public QuestZoneObjective(string zoneID, Vector3 position) : this(zoneID)
        {
            Position = position;
        }

        public override void Clear()
        {
            ZoneID = null;
            base.Clear();
        }

        public override string ToString()
        {
            if (ZoneID != null)
            {
                return "Zone " + ZoneID;
            }

            return base.ToString();
        }
    }
}
