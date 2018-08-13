using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kiwi.JimmyGon
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public abstract class UIWrapDataContent : MonoBehaviour, IUIWrapDataContent
    {
        [NonSerialized]
        private UIWrapLayoutGroup m_UIWrapLayoutGroup;

        protected UIWrapLayoutGroup uiWrapLayoutGroup
        {
            get
            {
                if (!m_UIWrapLayoutGroup)
                    m_UIWrapLayoutGroup = GetComponent<UIWrapLayoutGroup>();
                return m_UIWrapLayoutGroup;
            }
        }

        public abstract int Length { get; }

        public abstract RectTransform Show(int index, Rect rect, bool force);

        public abstract void Release(int index);

        public virtual float GetSize(int index, int axis)
        {
            return 0;
        }

        public virtual void GetSizes(int index, int axis, out float min, out float preferred, out float flexible)
        {
            min = 0;
            preferred = 0;
            flexible = 0;
        }

        protected virtual void OnEnable()
        {
            SetDirty();
        }

        protected virtual void OnDisable()
        {
            SetDirty();
        }

        public void SetDirty()
        {
            uiWrapLayoutGroup?.SetDirty();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            SetDirty();
        }
#endif
    }

    public abstract class UIWrapDataContent<T, W> : UIWrapDataContent where W : UIWrapItem<T>
    {
        [SerializeField]
        protected UIWrapItem m_UIWrapItem;
        [SerializeField]
        protected bool m_AutoSize = false;
        [SerializeField]
        protected T[] m_Data;

        [NonSerialized]
        private W m_UIWrapItemMode;

        private Dictionary<int, W> m_VisibleItems = new Dictionary<int, W>();
        private Stack<W> m_InvisibleItems = new Stack<W>();

        public UIWrapItem uiWrapItem
        {
            get { return m_UIWrapItem; }
            set { SetProperty(ref m_UIWrapItem, value); }
        }

        public bool autoSize
        {
            get { return m_AutoSize; }
            set { SetProperty(ref m_AutoSize, value); }
        }

        public T[] data
        {
            get { return m_Data; }
            set { m_Data = value; SetDirty(); }
        }

        protected W uiWrapItemMode
        {
            get
            {
                if (!m_UIWrapItem || !(m_UIWrapItem is W)) return null;
                if (!m_UIWrapItemMode)
                {
                    m_UIWrapItemMode = Instantiate(m_UIWrapItem) as W;
                    m_UIWrapItemMode.gameObject.hideFlags = HideFlags.DontSave;
                    m_UIWrapItemMode.transform.SetParent(transform);
                    m_UIWrapItemMode.Hide();
                }
                return m_UIWrapItemMode;
            }
        }

        public override int Length
        {
            get { return m_Data != null ? m_Data.Length : 0; }
        }

        public override float GetSize(int index, int axis)
        {
            if (m_UIWrapItem)
                return m_UIWrapItem.rectTransform.sizeDelta[axis];
            return base.GetSize(index, axis);
        }

        public override void GetSizes(int index, int axis, out float min, out float preferred, out float flexible)
        {
            if (DetectLayoutElement(axis, out min, out preferred, out flexible)) return;
            if (autoSize) AutoGetSizes(index, axis, out min, out preferred, out flexible);
            else GetWrapSizes(index, axis, out min, out preferred, out flexible);
        }

        private bool DetectLayoutElement(int axis, out float min, out float preferred, out float flexible)
        {
            min = preferred = flexible = 0;
            if (m_UIWrapItem && m_UIWrapItem is W)
            {
                var wrapItem = m_UIWrapItem as W;
                var components = ListPool<Component>.Get();
                wrapItem.GetComponents(typeof(LayoutElement), components);
                bool result = false;
                int maxPriority = int.MinValue;
                foreach (var component in components)
                {
                    var layoutComp = component as LayoutElement;
                    if (!layoutComp.enabled)
                        continue;
                    result = true;
                    int priority = layoutComp.layoutPriority;
                    if (priority < maxPriority)
                        continue;
                    if (priority > maxPriority)
                    {
                        min = Mathf.Max(0, axis == 0 ? layoutComp.minWidth : layoutComp.minHeight);
                        preferred = Mathf.Max(0, axis == 0 ? layoutComp.preferredWidth : layoutComp.preferredHeight);
                        flexible = Mathf.Max(0, axis == 0 ? layoutComp.flexibleWidth : layoutComp.flexibleHeight);
                        maxPriority = priority;
                    }
                    else if (axis == 0)
                    {
                        if (layoutComp.minWidth > min) min = layoutComp.minWidth;
                        if (layoutComp.preferredWidth > preferred) preferred = layoutComp.preferredWidth;
                        if (layoutComp.flexibleWidth > flexible) flexible = layoutComp.flexibleWidth;
                    }
                    else if (axis == 1)
                    {
                        if (layoutComp.minHeight > min) min = layoutComp.minHeight;
                        if (layoutComp.preferredHeight > preferred) preferred = layoutComp.preferredHeight;
                        if (layoutComp.flexibleHeight > flexible) flexible = layoutComp.flexibleHeight;
                    }
                }
                ListPool<Component>.Release(components);
                return result;
            }
            return false;
        }

        protected virtual void GetWrapSizes(int index, int axis, out float min, out float preferred, out float flexible)
        {
            if (m_UIWrapItem && m_UIWrapItem is W && m_Data != null && m_Data.Length > index)
            {
                var wrapItem = m_UIWrapItem as W;
                wrapItem.GetSizes(m_Data[index], axis, out min, out preferred, out flexible);
            }
            else base.GetSizes(index, axis, out min, out preferred, out flexible);
        }

        private void AutoGetSizes(int index, int axis, out float min, out float preferred, out float flexible)
        {
            if (m_UIWrapItem && m_UIWrapItem is W && m_Data != null && m_Data.Length > index)
            {
                var wrapItem = m_UIWrapItem as W;
                wrapItem.SetData(m_Data[index], index);
                var components = ListPool<Component>.Get();
                wrapItem.GetComponents(typeof(ILayoutElement), components);
                foreach (var component in components)
                {
                    var layoutComp = component as ILayoutElement;
                    var element = component as ILayoutGroup;
                    layoutComp.CalculateLayoutInputHorizontal();
                    element?.SetLayoutHorizontal();
                    layoutComp.CalculateLayoutInputVertical();
                    element?.SetLayoutVertical();
                }
                ListPool<Component>.Release(components);
                min = LayoutUtility.GetMinSize(wrapItem.rectTransform, axis);
                preferred = LayoutUtility.GetPreferredSize(wrapItem.rectTransform, axis);
                flexible = LayoutUtility.GetFlexibleSize(wrapItem.rectTransform, axis);
            }
            else base.GetSizes(index, axis, out min, out preferred, out flexible);
        }

        protected virtual W GenerateItem()
        {
            if (!m_UIWrapItem || !(m_UIWrapItem is W)) return null;
            var item = Instantiate(m_UIWrapItem);
            item.gameObject.hideFlags = HideFlags.DontSave;
            item.transform.SetParent(transform, false);
            return item as W;
        }

        public override RectTransform Show(int index, Rect rect, bool force)
        {
            W item;
            if (!m_VisibleItems.TryGetValue(index, out item) || !item)
            {
                if (m_InvisibleItems.Count > 0)
                    item = m_InvisibleItems.Pop();
                else
                    item = GenerateItem();
                if (!item) return null;
                item.Show();
                m_VisibleItems[index] = item;
                force = true;
            }
            if (force)
            {
                try
                {
                    item.SetData(m_Data[index], index);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            return item.rectTransform;
        }

        public override void Release(int index)
        {
            W item;
            if (m_VisibleItems.TryGetValue(index, out item))
            {
                if (!item) return;
                item.Hide();
                m_VisibleItems.Remove(index);
                m_InvisibleItems.Push(item);
            }
        }

        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;
            currentValue = newValue;
            SetDirty();
        }

        protected virtual void OnDestroy()
        {
            DestroyItem(m_UIWrapItemMode);
            foreach (var item in m_InvisibleItems)
                DestroyItem(item);
            m_InvisibleItems.Clear();
            foreach (var item in m_VisibleItems.Values)
                DestroyItem(item);
            m_VisibleItems.Clear();
            m_Data = null;
        }

        private static void DestroyItem(W item)
        {
            if (!item) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(item.gameObject);
            else
#endif
                Destroy(item.gameObject);
        }
    }
}
