using UnityEngine;

namespace UGUIGen
{
    [ExecuteInEditMode]
    public sealed class UGUIGenerater : MonoBehaviour
    {
        public float width, height;
        public TextAsset data;
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
