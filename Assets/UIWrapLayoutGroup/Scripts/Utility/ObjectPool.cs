using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kiwi.JimmyGon
{
    internal class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;

        public int countAll { get; private set; }
        public int countActive { get { return countAll - countInActive; } }
        public int countInActive { get { return m_Stack.Count; } }

        public ObjectPool(UnityAction<T> actionOnGet = null, UnityAction<T> actionOnRelease = null)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        public T Get()
        {
            T element;
            if (m_Stack.Count > 0) element = m_Stack.Pop();
            else
            {
                element = new T();
                countAll++;
            }
            m_ActionOnGet?.Invoke(element);
            return element;
        }

        public void Release(T element)
        {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            m_ActionOnRelease?.Invoke(element);
            m_Stack.Push(element);
        }
    }
}
