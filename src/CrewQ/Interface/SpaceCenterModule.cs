﻿/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2015 Alexander Taylor
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using KSPPluginFramework;
using FingerboxLib;

namespace CrewQ.Interface
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class SpaceCenterModule : SceneModule
    {
        private bool ACSpawned;

        protected override void Awake()
        {
            GameEvents.onGUILaunchScreenVesselSelected.Add(onVesselSelected);
            GameEvents.onGUIAstronautComplexSpawn.Add(onGUIAstronautComplexSpawn);
            GameEvents.onGUIAstronautComplexDespawn.Add(onGUIAstronautComplexDespawn);
            GameEvents.onGUILaunchScreenSpawn.Add(onGUILaunchScreenSpawn);
            GameEvents.onGUILaunchScreenDespawn.Add(onGUILaunchScreenDespawn);
        }

        protected override void Update()
        {
            base.Update();

            if (ACSpawned)
            {
                Logging.Debug("AC is spawned...");
                IEnumerable<CrewItemContainer> CrewItemContainers = GameObject.FindObjectsOfType<CrewItemContainer>().Where(x => x.GetCrewRef().rosterStatus == ProtoCrewMember.RosterStatus.Available);
                IEnumerable<CrewNode> VacationNodes = CrewQDataStore.instance.CrewList.Where(x => x.vacation);

                foreach (CrewItemContainer container in CrewItemContainers)
                {
                    if (VacationNodes.Select(x => x.crewRef).Contains(container.GetCrewRef()))
                    {
                        Logging.Debug("relabeling: " + container.GetName());
                        string label;

                        if (CrewQDataStore.instance.settingVacationHardlock)
                        {
                            label = CrewQDataStore.VACATION_LABEL_HARD;
                        }
                        else
                        {
                            label = CrewQDataStore.VACATION_LABEL_SOFT;
                        }

                        label = label + "\t\t Ready In: " + Utilities.GetColonFormattedTime(VacationNodes.First(x => x.crewRef == container.GetCrewRef()).remaining);

                        container.SetLabel(label);
                    }
                }
            }
        }

        private void onGUIAstronautComplexDespawn()
        {
            ACSpawned = false;
        }

        private void onGUIAstronautComplexSpawn()
        {
            ACSpawned = true;
        }

        private void onGUILaunchScreenSpawn(GameEvents.VesselSpawnInfo info)
        {
            RemapCrew = true;
        }

        private void onGUILaunchScreenDespawn()
        {
            RemapCrew = false;
        }

        private void onVesselSelected(ShipTemplate shipTemplate)
        {
            CleanManifest();
            HijackUIElements();
        }
    }
}
