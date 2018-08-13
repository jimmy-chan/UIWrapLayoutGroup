using UnityEngine;
using System;

namespace Kiwi.JimmyGon
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIWrapItem : MonoBehaviour
    {
        [NonSerialized]
        private RectTransform m_Rect;
        [NonSerialized]
        private CanvasGroup m_CanvasGroup;

        public RectTransform rectTransform
        {
            get
            {
                if (!m_Rect)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        public CanvasGroup canvasGroup
        {
            get
            {
                if (!m_CanvasGroup)
                    m_CanvasGroup = GetComponent<CanvasGroup>();
                return m_CanvasGroup;
            }
        }

        public virtual void Show()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void Hide()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public abstract class UIWrapItem<T> : UIWrapItem
    {
        public abstract void SetData(T data, int index);

        public void GetSizes(T data, int axis, out float min, out float preferred, out float flexible)
        {
            if (axis == 0)
            {
                min = GetMinWidth(data);
                preferred = GetPreferredWidth(data);
                flexible = GetFlexibleWidth(data);
            }
            else
            {
                min = GetMinHeight(data);
                preferred = GetPreferredHeight(data);
                flexible = GetFlexibleHeight(data);
            }
        }

        public virtual float GetMinWidth(T data)
        {
            return 0;
        }

        public virtual float GetMinHeight(T data)
        {
            return 0;
        }

        public virtual float GetPreferredWidth(T data)
        {
            return 0;
        }

        public virtual float GetPreferredHeight(T data)
        {
            return 0;
        }

        public virtual float GetFlexibleWidth(T data)
        {
            return 0;
        }

        public virtual float GetFlexibleHeight(T data)
        {
            return 0;
        }
    }
}
