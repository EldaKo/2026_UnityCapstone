using UnityEngine;
using UnityEngine.EventSystems;

namespace NeoFPS
{
    public class InputManagerCheck : MonoBehaviour
    {
        void Start()
        {
            Validate();
        }

        private void Validate()
        {
#if !ENABLE_LEGACY_INPUT_MANAGER
            var legacyModule = GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
                Destroy(legacyModule);
#endif
        }
    }
}