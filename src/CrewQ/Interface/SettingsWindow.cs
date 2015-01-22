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
using KSPPluginFramework;
using UnityEngine;
using FingerboxLib;

namespace CrewQ.Interface
{
    class SettingsWindow : MonoBehaviourWindow
    {
        private const string WINDOW_TITLE = "CrewQ(ueue) Settings";
        private const int BORDER_SIZE = 5;

        private const int WINDOW_WIDTH = 330, WINDOW_HEIGHT = 175;
        private const int COLUMN_A = BORDER_SIZE;
        private const int COLUMN_B = COLUMN_A + (WINDOW_WIDTH / 2);

        private string[] toggleCaptions = { "Automatically select crew", "Crew on vacation are ineligible for missions", "Use module type crew compositions", "<color=orange>Permanently hide this menu</color>" };

        private bool toggleDoCustomAssignment, toggleVacationHardlock, toggleUseCrewCompositions, toggleHideSettingsIcon;

        private bool pauseSync = false, popupArmed = true;

        private PopupDialog popup;

        protected override void Awake()
        {
            ClampToScreenOffset = AppLauncher.instance.DefaultOffset;
            WindowRect.Set(AppLauncher.instance.ScreenPosition.x, AppLauncher.instance.ScreenPosition.y, WINDOW_WIDTH, WINDOW_HEIGHT);
            WindowCaption = WINDOW_TITLE;
        }

        protected override void DrawWindow(int id)
        {
            if (!pauseSync)
            {
                PreSync();
            }           

            GUI.Box(new Rect(BORDER_SIZE, 30, WindowRect.width - (BORDER_SIZE * 2), WindowRect.height - (BORDER_SIZE * 2) - 30), "");

            toggleDoCustomAssignment = GUI.Toggle(new Rect(COLUMN_A, WINDOW_HEIGHT - 135, WINDOW_WIDTH - (BORDER_SIZE * 2), 30), toggleDoCustomAssignment, toggleCaptions[0]);
            toggleVacationHardlock = GUI.Toggle(new Rect(COLUMN_A, WINDOW_HEIGHT - 105, WINDOW_WIDTH - (BORDER_SIZE * 2), 30), toggleVacationHardlock, toggleCaptions[1]);
            toggleUseCrewCompositions = GUI.Toggle(new Rect(COLUMN_A, WINDOW_HEIGHT - 75, WINDOW_WIDTH - (BORDER_SIZE * 2), 30), toggleUseCrewCompositions, toggleCaptions[2]);
            toggleHideSettingsIcon = GUI.Toggle(new Rect(COLUMN_A, WINDOW_HEIGHT - 45, WINDOW_WIDTH - (BORDER_SIZE * 2), 30), toggleHideSettingsIcon, toggleCaptions[3]);

            if (toggleHideSettingsIcon && popupArmed)
            {
                ConfirmHide();
            }

            if (!popupArmed && !toggleHideSettingsIcon)
            {
                popupArmed = true;
            }

            if (!pauseSync)
            {
                Sync();
            }            
        }

        public void PreSync()
        {
            CrewQDataStore settings = CrewQDataStore.instance;

            if (settings != null)
            {
                toggleDoCustomAssignment = settings.settingDoCustomAssignment;
                toggleUseCrewCompositions = settings.settingUseCrewCompositions;
                toggleVacationHardlock = settings.settingVacationHardlock;
            }
        }

        public void Sync()
        {
            CrewQDataStore settings = CrewQDataStore.instance;

            if (settings != null)
            {
                settings.settingDoCustomAssignment = toggleDoCustomAssignment;
                settings.settingUseCrewCompositions = toggleUseCrewCompositions;
                settings.settingVacationHardlock = toggleVacationHardlock;
                settings.settingHideSettingsIcon = toggleHideSettingsIcon;
            }
        }

        public void ConfirmHide()
        {
            pauseSync = true;

            InputLockManager.SetControlLock(ControlTypes.All, "CrewQSettingsWindow");

            string dialogMsg = "<color=white>Are you sure that you want to <color=red>permanently remove</color> the icon for this settings menu?\n\r\nYou will only be to recover it by <color=orange>manually editing</color> your save file.</color>";
            string windowTitle = "WARNING";

            DialogOption[] options = { new DialogOption("Cancel", HideCancel), new DialogOption("<color=orange>Confirm</color>", HideConfirm) };

            MultiOptionDialog confirmationBox = new MultiOptionDialog(dialogMsg, windowTitle, HighLogic.Skin, options);

            popup = PopupDialog.SpawnPopupDialog(confirmationBox, false, HighLogic.Skin);
        }

        public void HideCancel()
        {
            Invoke("Unlock", 0.5f);
            toggleHideSettingsIcon = false;
            pauseSync = false;
        }

        public void HideConfirm()
        {
            Invoke("Unlock", 0.5f);
            toggleHideSettingsIcon = true;
            Sync();
            pauseSync = false;
            popupArmed = false;
        }

        private void Unlock()
        {
            InputLockManager.RemoveControlLock("CrewQSettingsWindow");
        }
    }
}