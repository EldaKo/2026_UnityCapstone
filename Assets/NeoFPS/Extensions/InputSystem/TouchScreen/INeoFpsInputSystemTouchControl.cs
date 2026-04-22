using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public interface INeoFpsInputSystemTouchControl
    {
#if ENABLE_INPUT_SYSTEM

        int priority { get; }
        RectTransform rectTransform { get; }

        void AddTouch();
        void RemoveTouch();
        bool HandleTouch(UnityEngine.InputSystem.EnhancedTouch.Touch touch);

#endif
    }
}
