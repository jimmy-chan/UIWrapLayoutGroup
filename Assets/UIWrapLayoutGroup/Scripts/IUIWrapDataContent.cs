using UnityEngine;

namespace Kiwi.JimmyGon
{
    internal interface IUIWrapDataContent
    {
        int Length { get; }
        RectTransform Show(int index, Rect rect, bool force);
        void Release(int index);
        float GetSize(int index, int axis);
        void GetSizes(int index, int axis, out float min, out float preferred, out float flexible);
    }
}
