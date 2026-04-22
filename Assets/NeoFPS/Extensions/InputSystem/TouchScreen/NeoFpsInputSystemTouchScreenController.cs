using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
#endif


namespace NeoFPS
{
#if ENABLE_INPUT_SYSTEM
    using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#endif

    public class NeoFpsInputSystemTouchScreenController : FpsInputSystemHandler, IInputSystemVirtualInput
    {
#if ENABLE_INPUT_SYSTEM

        private const int k_InitialNumTouches = 4;
        private const int k_MaxHandlers = 4;

        protected List<INeoFpsInputSystemTouchControl> touchControls { get; } = new List<INeoFpsInputSystemTouchControl>();

        private List<float> m_Axes = new List<float>();
        private List<ButtonState> m_Buttons = new List<ButtonState>();
        private List<Vector2> m_Vectors = new List<Vector2>();
        private Dictionary<Guid, int> m_AxisMap = new Dictionary<Guid, int>();
        private Dictionary<Guid, int> m_ButtonMap = new Dictionary<Guid, int>();
        private Dictionary<Guid, int> m_VectorMap = new Dictionary<Guid, int>();
        private Stack<TouchProcessor> m_TouchPool = new Stack<TouchProcessor>(k_InitialNumTouches);
        private Dictionary<int, TouchProcessor> m_ActiveTouches = new Dictionary<int, TouchProcessor>(k_InitialNumTouches);
        private List<int> m_PendingTouches = new List<int>(k_InitialNumTouches);
        private bool m_ControlsAdded = false;

        enum ButtonState
        {
            Up,
            Pressed,
            Down,
            Released
        }

        public override FpsInputContext inputContext
        {
            get { return FpsInputContext.Character; }
        }

        protected override void OnAwake()
        {
            // Initialise touch processors
            for (int i = 0; i < k_InitialNumTouches; ++i)
                m_TouchPool.Push(new TouchProcessor());

            // Enable enhanced touch
            EnhancedTouchSupport.Enable();
            // Disable mouse emulation for proper touch handling
            TouchSimulation.Disable();
        }

        void TickInputValues()
        {
            for (int i = 0; i < m_Axes.Count; ++i)
                m_Axes[i] = 0f;
            for (int i = 0; i < m_Vectors.Count; ++i)
                m_Vectors[i] = Vector2.zero;
            for (int i = 0; i < m_Buttons.Count; ++i)
            {
                switch(m_Buttons[i])
                {
                    case ButtonState.Released:
                        m_Buttons[i] = ButtonState.Up;
                        break;
                    case ButtonState.Down:
                        m_Buttons[i] = ButtonState.Released;
                        break;
                    case ButtonState.Pressed:
                        m_Buttons[i] = ButtonState.Down;
                        break;
                }
            }
        }

        protected override void Update()
        {
            // Reset inputs
            TickInputValues();

            base.Update();
        }

        protected override void UpdateInput()
        {
            // Sort controls if changed
            if (m_ControlsAdded)
            {
                touchControls.Sort((x, y) => { return x.priority - y.priority; });
                m_ControlsAdded = false;
            }

            // Gather known touches
            foreach (var id in m_ActiveTouches.Keys)
                m_PendingTouches.Add(id);

            // Get active touches
            var touches = ETouch.activeTouches;

            // Iterate through
            for (int i = 0; i < touches.Count; ++i)
            {
                // Get the touch & id
                var touch = touches[i];
                int id = touch.touchId;

                // Check if touch is complete
                bool valid = 
                    touch.phase != UnityEngine.InputSystem.TouchPhase.Ended &&
                    touch.phase != UnityEngine.InputSystem.TouchPhase.Canceled &&
                    touch.phase != UnityEngine.InputSystem.TouchPhase.None;

                // Remove handled touch
                if (valid)
                    m_PendingTouches.Remove(id);

                // Check touch processors
                TouchProcessor processor = null;
                if (valid && !m_ActiveTouches.TryGetValue(id, out processor))
                {
                    // Get a free processor
                    if (m_TouchPool.Count == 0)
                        processor = new TouchProcessor();
                    else
                        processor = m_TouchPool.Pop();

                    // Add to active touches
                    m_ActiveTouches.Add(id, processor);

                    // Get controls under touch
                    for (int j = touchControls.Count - 1; j >= 0; --j)
                    {
                        var control = touchControls[j];

                        if (RectTransformUtility.RectangleContainsScreenPoint(control.rectTransform, touch.screenPosition, null))
                        {
                            control.AddTouch();
                            processor.controls.Add(control);
                        }
                    }
                }

                // Handle the touch input
                if (processor != null)
                    processor.HandleTouch(touch);
            }

            // Clean up touches that weren't handled
            for (int i = 0; i < m_PendingTouches.Count; ++i)
            {
                int id = m_PendingTouches[i];

                // Get the touch processor
                var processor = m_ActiveTouches[id];

                // rEturn to pool
                processor.Reset();
                m_TouchPool.Push(processor);

                // Remove the touch from active
                m_ActiveTouches.Remove(id);
            }
            m_PendingTouches.Clear();
        }

        public float GetVirtualAxis(Guid id)
        {
            int index;
            if (m_AxisMap.TryGetValue(id, out index))
                return m_Axes[index];
            else
                return 0f;
        }

        public Vector2 GetVirtualVector2D(Guid id)
        {
            int index;
            if (m_VectorMap.TryGetValue(id, out index))
                return m_Vectors[index];
            else
                return Vector2.zero;
        }

        public bool GetVirtualButton(Guid id)
        {
            int index;
            if (m_ButtonMap.TryGetValue(id, out index))
                return m_Buttons[index] != ButtonState.Up;
            else
                return false;
        }

        public bool GetVirtualButtonDown(Guid id)
        {
            int index;
            if (m_ButtonMap.TryGetValue(id, out index))
                return m_Buttons[index] == ButtonState.Pressed;
            else
                return false;
        }

        public bool GetVirtualButtonUp(Guid id)
        {
            int index;
            if (m_ButtonMap.TryGetValue(id, out index))
                return m_Buttons[index] == ButtonState.Released;
            else
                return false;
        }

        public void SetVirtualAxis(Guid id, float value)
        {
            int index;
            if (m_AxisMap.TryGetValue(id, out index))
                m_Axes[index] += value;
            else
            {
                m_AxisMap.Add(id, m_Axes.Count);
                m_Axes.Add(value);
            }
        }

        public void SetVirtualVector2D(Guid id, Vector2 value)
        {
            int index;
            if (m_VectorMap.TryGetValue(id, out index))
                m_Vectors[index] += value;
            else
            {
                m_VectorMap.Add(id, m_Vectors.Count);
                m_Vectors.Add(value);
            }
        }

        public void SetVirtualButton(Guid id, bool value)
        {
            if (value)
            {
                int index;
                if (m_ButtonMap.TryGetValue(id, out index))
                {
                    switch (m_Buttons[index])
                    {
                        case ButtonState.Up:
                            m_Buttons[index] = ButtonState.Pressed;
                            break;
                        case ButtonState.Released:
                            m_Buttons[index] = ButtonState.Down;
                            break;
                    }
                }
                else
                {
                    m_ButtonMap.Add(id, m_Buttons.Count);
                    m_Buttons.Add(ButtonState.Pressed);
                }
            }
        }

        class TouchProcessor
        {
            public List<INeoFpsInputSystemTouchControl> controls = new List<INeoFpsInputSystemTouchControl>(k_MaxHandlers);

            public void Reset()
            {
                for (int i = 0; i < controls.Count; ++i)
                    controls[i].RemoveTouch();
                controls.Clear();
            }

            public void HandleTouch(ETouch touch)
            {
                // Iterate through controls until consumed
                for (int i = 0; i < controls.Count; ++i)
                {
                    if (controls[i] != null && controls[i].HandleTouch(touch))
                        break;
                }
            }
        }

        void ResetAllInputs()
        {
            // Clear active touches if the app is minimised
            foreach (var processor in m_ActiveTouches.Values)
            {
                processor.Reset();
                m_TouchPool.Push(processor);
            }
            m_ActiveTouches.Clear();

            // Reset inputs
            for (int i = 0; i < m_Axes.Count; ++i)
                m_Axes[i] = 0f;
            for (int i = 0; i < m_Vectors.Count; ++i)
                m_Vectors[i] = Vector2.zero;
            for (int i = 0; i < m_Buttons.Count; ++i)
                m_Buttons[i] = ButtonState.Up;
        }

        void OnApplicationFocus(bool focus)
        {
            ResetAllInputs();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ResetAllInputs();
        }

        public void RegisterTouchControl(INeoFpsInputSystemTouchControl control)
        {
            if (control != null)
            {
                touchControls.Add(control);
                m_ControlsAdded = true;
            }
        }

        public void UnregisterTouchControl(INeoFpsInputSystemTouchControl control)
        {
            touchControls.Remove(control);
        }

        protected override void OnGainFocus()
        {
            // Set as virtual input handler
            NeoFpsNewInputManager.virtualInput = this;
        }

        protected override void OnLoseFocus()
        {
            // Detach as virtual input handler
            NeoFpsNewInputManager.virtualInput = null;
        }

#endif
    }
}