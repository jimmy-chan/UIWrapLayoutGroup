using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kiwi.JimmyGon
{
    [DisallowMultipleComponent, ExecuteInEditMode, RequireComponent(typeof(RectTransform))]
    public abstract class UIWrapLayoutGroup : UIBehaviour, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        internal static IUIWrapDataContent s_DataContent = new UIWrapDataContentHandler();

        private struct ChildRect
        {
            public Rect rect;
            public DrivenTransformProperties properties;
        }

        [SerializeField]
        protected RectOffset m_Padding = new RectOffset();
        [SerializeField, FormerlySerializedAs("m_Alignment")]
        protected TextAnchor m_ChildAlignment = TextAnchor.UpperLeft;
        [SerializeField]
        protected RectTransform m_Viewport;

        [NonSerialized]
        private RectTransform m_Rect;
        [NonSerialized]
        private RectTransform m_ViewRect;
        [NonSerialized]
        private IUIWrapDataContent m_DataContent;
        [NonSerialized]
        private ChildRect[] m_ChildArray;
        [NonSerialized]
        private ArraySegment<ChildRect> m_RectChildren;
        [NonSerialized]
        private Rect m_ViewportRect;
        [NonSerialized]
        private int m_PreRectChildrenCount;
        [NonSerialized]
        private bool m_PerformingUpdateItems;
        [NonSerialized]
        private bool m_IsLayoutDirty;

        protected DrivenRectTransformTracker m_Tracker;
        private Vector2 m_TotalMinSize = Vector2.zero;
        private Vector2 m_TotalPreferredSize = Vector2.zero;
        private Vector2 m_TotalFlexibleSize = Vector2.zero;

        public RectOffset padding
        {
            get { return m_Padding; }
            set { SetProperty(ref m_Padding, value); }
        }

        public TextAnchor childAlignment
        {
            get { return m_ChildAlignment; }
            set { SetProperty(ref m_ChildAlignment, value); }
        }

        public RectTransform viewport
        {
            get { return m_Viewport; }
            set
            {
                if (m_Viewport == value) return;
                m_Viewport = value;
                SetDirty();
            }
        }

        protected RectTransform rectTransform
        {
            get
            {
                if (!m_Rect)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        protected RectTransform viewRect
        {
            get
            {
                if (!m_ViewRect)
                    m_ViewRect = m_Viewport;
                if (!m_ViewRect)
                    m_ViewRect = rectTransform.parent as RectTransform ?? rectTransform;
                return m_ViewRect;
            }
        }

        internal IUIWrapDataContent dataContent
        {
            get { return m_DataContent ?? s_DataContent; }
        }

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }
            if (executing == CanvasUpdate.PostLayout)
            {
                m_IsLayoutDirty = true;
            }
        }

        public virtual void LayoutComplete() { }

        public virtual void GraphicUpdateComplete() { }

        private void UpdateCachedData()
        {
            m_PreRectChildrenCount = m_RectChildren != null ? m_RectChildren.Count : 0;
        }

        public virtual void CalculateLayoutInputHorizontal()
        {
            var dataContent = GetComponent<UIWrapDataContent>();
            m_DataContent = dataContent && dataContent.enabled ? dataContent : null;
            if (m_ChildArray == null)
                m_ChildArray = new ChildRect[this.dataContent.Length];
            else if (m_ChildArray.Length < this.dataContent.Length)
                Array.Resize(ref m_ChildArray, this.dataContent.Length);
            m_RectChildren = new ArraySegment<ChildRect>(m_ChildArray, 0, this.dataContent.Length);
            m_Tracker.Clear();
        }

        public abstract void CalculateLayoutInputVertical();

        public virtual float minWidth => GetTotalMinSize(0);

        public virtual float preferredWidth => GetTotalPreferredSize(0);

        public virtual float flexibleWidth => GetTotalFlexibleSize(0);

        public virtual float minHeight => GetTotalMinSize(1);

        public virtual float preferredHeight => GetTotalPreferredSize(1);

        public virtual float flexibleHeight => GetTotalFlexibleSize(1);

        public int layoutPriority => 0;

        public abstract void SetLayoutHorizontal();

        public abstract void SetLayoutVertical();

        protected float GetTotalMinSize(int axis)
        {
            return m_TotalMinSize[axis];
        }

        protected float GetTotalPreferredSize(int axis)
        {
            return m_TotalPreferredSize[axis];
        }

        protected float GetTotalFlexibleSize(int axis)
        {
            return m_TotalFlexibleSize[axis];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
            Canvas.willRenderCanvases += OnWillRenderCanvases;
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            Canvas.willRenderCanvases -= OnWillRenderCanvases;
            base.OnDisable();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            SetDirty();
        }

        protected virtual void OnWillRenderCanvases()
        {
            UpdateItems();
        }

        private void UpdateItems()
        {
            if (m_ChildArray == null || m_PerformingUpdateItems) return;
            m_PerformingUpdateItems = true;
            bool isDirty = false;
            Rect clipRect = GetViewRect();
            if (clipRect != m_ViewportRect || m_IsLayoutDirty)
            {
                m_ViewportRect = clipRect;
                List<int> visibleIndexes = ListPool<int>.Get();
                if (m_PreRectChildrenCount > m_RectChildren.Count)
                {
                    for (int i = m_RectChildren.Count; i < m_PreRectChildrenCount; i++)
                        dataContent.Release(i);
                    m_PreRectChildrenCount = m_RectChildren.Count;
                }
                for (int i = m_RectChildren.Offset; i < m_RectChildren.Count; i++)
                {
                    if (m_ViewportRect.Overlaps(m_RectChildren.Array[i].rect))
                        visibleIndexes.Add(i);
                    else
                        dataContent.Release(i);
                }
                foreach (var index in visibleIndexes)
                {
                    ChildRect child = m_RectChildren.Array[index];
                    Rect rect = new Rect(child.rect.min - rectTransform.rect.min, child.rect.size);
                    var childRect = dataContent.Show(index, rect, m_IsLayoutDirty);
                    if (!childRect) continue;
                    SetChildAlongAxis(childRect, 0, rect.xMin, rect.width, child.properties);
                    SetChildAlongAxis(childRect, 1, rect.yMin, rect.height, child.properties);
                    isDirty = true;
                }
                ListPool<int>.Release(visibleIndexes);
                m_IsLayoutDirty = false;
            }
            if (isDirty && !CanvasUpdateRegistry.IsRebuildingGraphics() && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
            m_PerformingUpdateItems = false;
        }

        private readonly Vector3[] m_Corners = new Vector3[4];
        private Rect GetViewRect()
        {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var toLocal = rectTransform.worldToLocalMatrix;
            viewRect.GetWorldCorners(m_Corners);
            for (int i = 0; i < 4; i++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[i]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }
            return Rect.MinMaxRect(vMin.x, vMin.y, vMax.x, vMax.y);
        }

        protected float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
        {
            float requiredSpace = requiredSpaceWithoutPadding + (axis == 0 ? padding.horizontal : padding.vertical);
            float availableSpace = rectTransform.rect.size[axis];
            float surplusSpcae = availableSpace - requiredSpace;
            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            return (axis == 0 ? padding.left : padding.top) + surplusSpcae * alignmentOnAxis;
        }

        protected float GetAlignmentOnAxis(int axis)
        {
            float alignmentOnAxis;
            if (axis == 0)
                alignmentOnAxis = ((int)childAlignment % 3) * 0.5f;
            else
                alignmentOnAxis = ((int)childAlignment / 3) * 0.5f;
            return alignmentOnAxis;
        }

        protected void SetLayoutInputForAxis(float totalMin, float totalPreferred, float totalFlexible, int axis)
        {
            m_TotalMinSize[axis] = totalMin;
            m_TotalPreferredSize[axis] = totalPreferred;
            m_TotalFlexibleSize[axis] = totalFlexible;
        }

        protected void SetChildRect(int index, int axis, float pos, float size, bool sizeDrive)
        {
            ChildRect childRect = m_RectChildren.Array[index];
            childRect.properties |= DrivenTransformProperties.Anchors;
            if (axis == 0)
            {
                childRect.rect.xMin = pos + rectTransform.rect.xMin;
                childRect.rect.width = size;
                childRect.properties |= DrivenTransformProperties.AnchoredPositionX;
                if (sizeDrive) childRect.properties |= DrivenTransformProperties.SizeDeltaX;
            }
            else
            {
                childRect.rect.yMin = rectTransform.rect.yMin + rectTransform.rect.height - pos - size;
                childRect.rect.height = size;
                childRect.properties |= DrivenTransformProperties.AnchoredPositionY;
                if (sizeDrive) childRect.properties |= DrivenTransformProperties.SizeDeltaY;
            }
            m_RectChildren.Array[index] = childRect;
        }

        protected void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size, DrivenTransformProperties properties)
        {
            if (!rect) return;
            m_Tracker.Add(this, rect, properties);
            rect.SetInsetAndSizeFromParentEdge(axis != 0 ? RectTransform.Edge.Bottom : RectTransform.Edge.Left, pos, size);
        }

        private bool isRootLayoutGroup
        {
            get
            {
                Transform parent = transform.parent;
                if (!parent) return true;
                return parent.GetComponent<ILayoutGroup>() == null;
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (isRootLayoutGroup) SetDirty();
        }

        private IEnumerator DelayedSetDirty(RectTransform rectTransform)
        {
            yield return null;
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        public void SetDirty()
        {
            if (!IsActive()) return;
            if (CanvasUpdateRegistry.IsRebuildingLayout())
                StartCoroutine(DelayedSetDirty(rectTransform));
            else
            {
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            }
        }

        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;
            currentValue = newValue;
            SetDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif
    }
}
