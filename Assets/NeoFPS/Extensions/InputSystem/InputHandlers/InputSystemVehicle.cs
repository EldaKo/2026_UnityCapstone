using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoFPS.Constants;
using NeoFPS.WieldableTools;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/inputref-mb-inputsystemvehicle.html")]
    public class InputSystemVehicle : FpsInputSystemHandler
    {
#if ENABLE_INPUT_SYSTEM

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Vehicle; }
        }

        protected override void OnGainFocus()
        {
            base.OnGainFocus();

            // Capture mouse cursor
            NeoFpsInputManagerBase.captureMouseCursor = true;
        }

        protected override void OnEnable()
        {
            //base.OnEnable();

            // Attach this component to the vehicle
            // Call PushContext manually (or via unity events) when entering a vehicle
            // and PopContext when exiting the vehicle
        }

        protected override void OnLoseFocus()
        {
            // Release mouse cursor
            NeoFpsInputManagerBase.captureMouseCursor = false;
        }

        protected override void UpdateInput()
        {
            // Inherit from this class and override this function to add vehicle inputs
        }

#endif
    }
}