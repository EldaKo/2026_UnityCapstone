
using System;
using UnityEngine;
namespace NeoFPS
{
    public interface IInputSystemVirtualInput
    {
#if ENABLE_INPUT_SYSTEM

        float GetVirtualAxis(Guid id);
        Vector2 GetVirtualVector2D(Guid id);
        bool GetVirtualButton(Guid id);
        bool GetVirtualButtonDown(Guid id);
        bool GetVirtualButtonUp(Guid id);

#endif
    }
}
