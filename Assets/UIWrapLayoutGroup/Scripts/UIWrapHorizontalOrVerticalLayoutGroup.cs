using UnityEngine;

namespace Kiwi.JimmyGon
{
    public abstract class UIWrapHorizontalOrVerticalLayoutGroup : UIWrapLayoutGroup
    {
        [SerializeField]
        protected float m_Spacing = 0;
        [SerializeField]
        protected bool m_ChildForceExpandWidth = true;
        [SerializeField]
        protected bool m_ChildForceExpandHeight = true;
        [SerializeField]
        protected bool m_ChildControlWidth = true;
        [SerializeField]
        protected bool m_ChildControlHeight = true;

        public float spacing
        {
            get { return m_Spacing; }
            set { SetProperty(ref m_Spacing, value); }
        }

        public bool childForceExpandWidth
        {
            get { return m_ChildForceExpandWidth; }
            set { SetProperty(ref m_ChildForceExpandWidth, value); }
        }

        public bool childForceExpandHeight
        {
            get { return m_ChildForceExpandHeight; }
            set { SetProperty(ref m_ChildForceExpandHeight, value); }
        }

        public bool childControlWidth
        {
            get { return m_ChildControlWidth; }
            set { SetProperty(ref m_ChildControlWidth, value); }
        }

        public bool childControlHeight
        {
            get { return m_ChildControlHeight; }
            set { SetProperty(ref m_ChildControlHeight, value); }
        }

        protected void CalcAlongAxis(int axis, bool isVertical)
        {
            float combinedPadding = (axis == 0 ? padding.horizontal : padding.vertical);
            bool controlSize = (axis == 0 ? m_ChildControlWidth : m_ChildControlHeight);
            bool childForceExpand = (axis == 0 ? m_ChildForceExpandWidth : m_ChildForceExpandHeight);

            float totalMin = combinedPadding;
            float totalPreferred = combinedPadding;
            float totalFlexible = 0;

            bool alongOtherAxis = isVertical ^ axis == 1;
            for (int i = 0; i < dataContent.Length; i++)
            {
                float min, preferred, flexible;
                GetChildSizes(i, axis, controlSize, childForceExpand, out min, out preferred, out flexible);
                if (alongOtherAxis)
                {
                    totalMin = Mathf.Max(min + combinedPadding, totalMin);
                    totalPreferred = Mathf.Max(preferred + combinedPadding, totalPreferred);
                    totalFlexible = Mathf.Max(flexible, totalFlexible);
                }
                else
                {
                    totalMin += min + spacing;
                    totalPreferred += preferred + spacing;
                    totalFlexible += flexible;
                }
            }
            if (!alongOtherAxis && dataContent.Length > 0)
            {
                totalMin -= spacing;
                totalPreferred -= spacing;
            }
            totalPreferred = Mathf.Max(totalMin, totalPreferred);
            SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
        }

        protected void SetChildrenAlongAxis(int axis, bool isVertical)
        {
            float size = rectTransform.rect.size[axis];
            bool controlSize = (axis == 0 ? m_ChildControlWidth : m_ChildControlHeight);
            bool childForceExpand = (axis == 0 ? m_ChildForceExpandWidth : m_ChildForceExpandHeight);

            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            bool alongOtherAxis = isVertical ^ axis == 1;
            if (alongOtherAxis)
            {
                float innerSize = size - (axis == 0 ? padding.horizontal : padding.vertical);
                for (int i = 0; i < dataContent.Length; i++)
                {
                    float min, preferred, flexible;
                    GetChildSizes(i, axis, controlSize, childForceExpand, out min, out preferred, out flexible);

                    float requiredSpace = Mathf.Clamp(innerSize, min, flexible > 0 ? size : preferred);
                    float startOffset = GetStartOffset(axis, requiredSpace);
                    if (controlSize)
                        SetChildRect(i, axis, startOffset, requiredSpace, true);
                    else
                    {
                        float sizeDelta = dataContent.GetSize(i, axis);
                        float offset = (requiredSpace - sizeDelta) * alignmentOnAxis;
                        SetChildRect(i, axis, startOffset + offset, sizeDelta, false);
                    }
                }
            }
            else
            {
                float pos = (axis == 0 ? padding.left : padding.top);
                if (GetTotalFlexibleSize(axis) == 0 && GetTotalPreferredSize(axis) < size)
                    pos = GetStartOffset(axis, GetTotalPreferredSize(axis) - (axis == 0 ? padding.horizontal : padding.vertical));

                float minMaxLerp = 0;
                if (GetTotalMinSize(axis) != GetTotalPreferredSize(axis))
                    minMaxLerp = Mathf.Clamp01((size - GetTotalMinSize(axis)) / (GetTotalPreferredSize(axis) - GetTotalMinSize(axis)));

                float itemFlexibleMultipllier = 0;
                if (size > GetTotalPreferredSize(axis))
                {
                    if (GetTotalFlexibleSize(axis) > 0)
                        itemFlexibleMultipllier = (size - GetTotalPreferredSize(axis)) / GetTotalFlexibleSize(axis);
                }

                for (int i = 0; i < dataContent.Length; i++)
                {
                    float min, preferred, flexible;
                    GetChildSizes(i, axis, controlSize, childForceExpand, out min, out preferred, out flexible);

                    float childSize = Mathf.Lerp(min, preferred, minMaxLerp);
                    childSize += flexible * itemFlexibleMultipllier;
                    if (controlSize)
                        SetChildRect(i, axis, pos, childSize, true);
                    else
                    {
                        float sizeDelta = dataContent.GetSize(i, axis);
                        float offset = (childSize - sizeDelta) * alignmentOnAxis;
                        SetChildRect(i, axis, pos + offset, sizeDelta, false);
                    }
                    pos += childSize + spacing;
                }
            }
        }

        private void GetChildSizes(int index, int axis, bool controlSize, bool childForceExpand, out float min, out float preferred, out float flexible)
        {
            if (!controlSize)
            {
                min = dataContent.GetSize(index, axis);
                preferred = min;
                flexible = 0;
            }
            else
            {
                dataContent.GetSizes(index, axis, out min, out preferred, out flexible);
            }
            if (childForceExpand) flexible = Mathf.Max(flexible, 1);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            m_ChildControlWidth = false;
            m_ChildControlHeight = false;
        }
#endif
    }
}
