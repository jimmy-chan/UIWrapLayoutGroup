using UnityEngine;

namespace Kiwi.JimmyGon
{
    internal sealed class UIWrapDataContentHandler : IUIWrapDataContent
    {
        public int Length
        {
            get { return 0; }
        }

        public float GetSize(int index, int axis)
        {
            return 0;
        }

        public void GetSizes(int index, int axis, out float min, out float preferred, out float flexible)
        {
            min = 0;
            preferred = 0;
            flexible = 0;
        }

        public void Release(int index) { }

        public RectTransform Show(int index, Rect rect, bool force)
        {
            return null;
        }
    }
}
