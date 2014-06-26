﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace Kerbal_Construction_Time
{
    public class KCT_BuildListVessel : IKCTBuildItem
    {
        private ShipConstruct ship;
        public double progress, buildPoints;
        public String launchSite, flag, shipName;
        public ListType type;
        public enum ListType { VAB, SPH, TechNode };
        public List<string> InventoryParts;
        public ConfigNode shipNode;
        public Guid id;
        public bool cannotEarnScience = false;
        public double buildRate { get { return KCT_Utilities.GetBuildRate(this); } }
        public double timeLeft
        {
            get
            {
                if (buildRate > 0)
                    return (buildPoints-progress)/buildRate;
                else
                    return double.PositiveInfinity;
            }
        }
        public List<Part> ExtractedParts { 
            get 
            { 
                List<Part> temp = new List<Part>();
                foreach (string s in this.GetPartNames())
                {
                    temp.Add(KCT_Utilities.GetAvailablePartByName(s).partPrefab);
                }
                return temp;
            } 
        }
        public bool isFinished { get { return progress > buildPoints; } }

        public KCT_BuildListVessel(ShipConstruct s, String ls, double bP, String flagURL)
        {
            ship = s;
            shipNode = s.SaveShip();
            shipName = s.shipName;
            launchSite = ls;
            buildPoints = bP;
            progress = 0;
            flag = flagURL;
            if (launchSite == "LaunchPad")
                type = ListType.VAB;
            else
                type = ListType.SPH;
            InventoryParts = new List<string>();
            id = Guid.NewGuid();
        }

        public KCT_BuildListVessel(String name, String ls, double bP, String flagURL)
        {
            ship = new ShipConstruct();
            launchSite = ls;
            shipName = name;
            buildPoints = bP;
            progress = 0;
            flag = flagURL;
            if (launchSite == "LaunchPad")
                type = ListType.VAB;
            else
                type = ListType.SPH;
            InventoryParts = new List<string>();
        }

        public KCT_BuildListVessel NewCopy(bool RecalcTime)
        {
            KCT_BuildListVessel ret = new KCT_BuildListVessel(this.shipName, this.launchSite, this.buildPoints, this.flag);
            ret.shipNode = this.shipNode.CreateCopy();
            ret.id = Guid.NewGuid();
            if (RecalcTime)
            {
                ret.buildPoints = KCT_Utilities.GetBuildTime(ret.GetPartNames(), true, true);
            }
            return ret;
        }

        public ShipConstruct GetShip()
        {
            if (ship != null && ship.Parts != null && ship.Parts.Count > 0) //If the parts are there, then the ship is loaded
            {
                return ship;
            }
            else if (shipNode != null) //Otherwise load the ship from the ConfigNode
            {
                ship.LoadShip(shipNode);
                //ConfigNode.LoadObjectFromConfig(ship, shipNode);
            }
            return ship;
        }

        public void Launch()
        {
            KCT_GameStates.flightSimulated = false;
            string tempFile = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/Ships/temp.craft";
            shipNode.Save(tempFile);
            FlightDriver.StartWithNewLaunch(tempFile, flag, launchSite, new VesselCrewManifest());
        }

        public bool RemoveFromBuildList()
        {
            string typeName="";
            bool removed = false;
            if (type == ListType.SPH)
            {
                if (KCT_GameStates.SPHWarehouse.Contains(this))
                    removed = KCT_GameStates.SPHWarehouse.Remove(this);
                else if (KCT_GameStates.SPHList.Contains(this))
                    removed = KCT_GameStates.SPHList.Remove(this);
                typeName="SPH";
            }
            else if (type == ListType.VAB)
            {
                if (KCT_GameStates.VABWarehouse.Contains(this))
                    removed = KCT_GameStates.VABWarehouse.Remove(this);
                else if (KCT_GameStates.VABList.Contains(this))
                    removed = KCT_GameStates.VABList.Remove(this);
                typeName="VAB";
            }
            Debug.Log("[KCT] Removing " + shipName + " from "+ typeName +" storage/list.");
            if (!removed)
            {
                Debug.Log("[KCT] Failed to remove ship from list! Performing direct comparison of ids...");
                foreach (KCT_BuildListVessel blv in KCT_GameStates.SPHWarehouse)
                {
                    if (blv.id == this.id)
                    {
                        Debug.Log("[KCT] Ship found in SPH storage. Removing...");
                        removed = KCT_GameStates.SPHWarehouse.Remove(blv);
                    }
                }
                if (!removed)
                {
                    foreach (KCT_BuildListVessel blv in KCT_GameStates.VABWarehouse)
                    {
                        if (blv.id == this.id)
                        {
                            Debug.Log("[KCT] Ship found in VAB storage. Removing...");
                            removed = KCT_GameStates.VABWarehouse.Remove(blv);
                        }
                    }
                }
                if (!removed)
                {
                    foreach (KCT_BuildListVessel blv in KCT_GameStates.VABList)
                    {
                        if (blv.id == this.id)
                        {
                            Debug.Log("[KCT] Ship found in VAB List. Removing...");
                            removed = KCT_GameStates.VABList.Remove(blv);
                        }
                    }
                }
                if (!removed)
                {
                    foreach (KCT_BuildListVessel blv in KCT_GameStates.SPHList)
                    {
                        if (blv.id == this.id)
                        {
                            Debug.Log("[KCT] Ship found in SPH list. Removing...");
                            removed = KCT_GameStates.SPHList.Remove(blv);
                        }
                    }
                }
            }
            if (removed) Debug.Log("[KCT] Sucessfully removed ship from storage.");
            else Debug.Log("[KCT] Still couldn't remove ship!");
            return removed;
        }

        public List<String> GetPartNames()
        {
            List<String> retList = new List<String>();
            ConfigNode[] partNodes = shipNode.GetNodes("PART");
           // Debug.Log("[KCT] partNodes count: " + partNodes.Length);

            foreach (ConfigNode CN in partNodes)
            {
                FakePart p = new FakePart();
                ConfigNode.LoadObjectFromConfig(p, CN);
                string pName = "";
                for (int i = 0; i < p.part.Split('_').Length-1; i++ )
                    pName += p.part.Split('_')[i];
                retList.Add(pName);
                //Debug.Log("[KCT] " + pName);
            }
            return retList;
        }

        public double AddProgress(double toAdd)
        {
            progress+=toAdd;
            return progress;
        }

        public double ProgressPercent()
        {
            return 100 * (progress / buildPoints);
        }

        string IKCTBuildItem.GetItemName()
        {
            return this.shipName;
        }

        double IKCTBuildItem.GetBuildRate()
        {
            return this.buildRate;
        }

        double IKCTBuildItem.GetTimeLeft()
        {
            return this.timeLeft;
        }

        ListType IKCTBuildItem.GetListType()
        {
            return this.type;
        }

        bool IKCTBuildItem.IsComplete()
        {
            return (progress >= buildPoints);
        }

    }
}
