using UnityEngine;

namespace UGUIGen
{
    [ExecuteInEditMode]
    public sealed class UGUIBaker : MonoBehaviour
    {
        public RectTransform trans;

        private void OnEnable()
        {
            if (trans == null)
            {
                trans = GetComponent<RectTransform>();
            }
        }
    }
}
