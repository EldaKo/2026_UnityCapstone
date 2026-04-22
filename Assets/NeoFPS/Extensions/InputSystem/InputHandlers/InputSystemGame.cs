using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoSaveGames;
using NeoFPS.Samples;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-inputsystemgame.html")]
    public class InputSystemGame : FpsInputSystemHandler
    {
#if ENABLE_INPUT_SYSTEM

        protected override void UpdateInput()
        {
            // Main controls
            var controls = NeoFpsNewInputManager.controls;

            if (GetButtonDown(controls.Miscellaneous.QuickSave))
                SaveGameManager.QuickSave();

            if (GetButtonDown(controls.Miscellaneous.QuickLoad))
                SaveGameManager.QuickLoad();

            if (GetButtonDown(controls.Miscellaneous.QuickMenu))
                QuickOptionsPopup.ToggleVisible();
        }

#endif
    }
}