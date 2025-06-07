using System;
using System.Collections.Generic;

namespace amaz
{
    public class EventDispatcher : BaseService
    {
        // 定义事件处理函数的委托类型（支持参数）
        public delegate void EventHandler<T>(T args);
        public delegate void EventHandler();

        // 存储事件的字典（事件名 -> 处理函数列表）
        private readonly Dictionary<string, Delegate> _events = new Dictionary<string, Delegate>();

        #region 注册事件（支持带参数和不带参数的版本）

        // 注册带参数的事件
        public void AddListener<T>(string eventName, EventHandler<T> handler)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            lock (_events)
            {
                if (!_events.TryGetValue(eventName, out var existingHandlers))
                {
                    _events[eventName] = handler;
                }
                else
                {
                    _events[eventName] = Delegate.Combine(existingHandlers, handler);
                }
            }
        }

        // 注册不带参数的事件
        public void AddListener(string eventName, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            lock (_events)
            {
                if (!_events.TryGetValue(eventName, out var existingHandlers))
                {
                    _events[eventName] = handler;
                }
                else
                {
                    _events[eventName] = Delegate.Combine(existingHandlers, handler);
                }
            }
        }

        #endregion

        #region 移除事件

        // 移除带参数的事件监听者
        public void RemoveListener<T>(string eventName, EventHandler<T> handler)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            lock (_events)
            {
                if (_events.TryGetValue(eventName, out var existingHandlers))
                {
                    var newHandlers = Delegate.Remove(existingHandlers, handler);
                    if (newHandlers == null)
                        _events.Remove(eventName);
                    else
                        _events[eventName] = newHandlers;
                }
            }
        }

        // 移除不带参数的事件监听者
        public void RemoveListener(string eventName, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            lock (_events)
            {
                if (_events.TryGetValue(eventName, out var existingHandlers))
                {
                    var newHandlers = Delegate.Remove(existingHandlers, handler);
                    if (newHandlers == null)
                        _events.Remove(eventName);
                    else
                        _events[eventName] = newHandlers;
                }
            }
        }

        // 移除某个事件的所有监听者
        public void RemoveAllListeners(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            lock (_events)
            {
                _events.Remove(eventName);
            }
        }

        // 清除所有事件
        public void ClearAllEvents()
        {
            lock (_events)
            {
                _events.Clear();
            }
        }

        #endregion

        #region 触发事件

        // 触发带参数的事件
        public void Dispatch<T>(string eventName, T args)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            Delegate? handlers;
            lock (_events)
            {
                _events.TryGetValue(eventName, out handlers);
            }

            if (handlers != null)
            {
                if (handlers is EventHandler<T> typedHandler)
                {
                    typedHandler(args);
                }
                else
                {
                    throw new InvalidOperationException($"事件 {eventName} 的委托类型不匹配");
                }
            }
        }

        // 触发不带参数的事件
        public void Dispatch(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            Delegate? handlers;
            lock (_events)
            {
                _events.TryGetValue(eventName, out handlers);
            }

            if (handlers != null)
            {
                if (handlers is EventHandler typedHandler)
                {
                    typedHandler();
                }
                else
                {
                    throw new InvalidOperationException($"事件 {eventName} 的委托类型不匹配");
                }
            }
        }

        #endregion

        #region 查询事件

        // 检查事件是否存在
        public bool HasEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            lock (_events)
            {
                return _events.ContainsKey(eventName);
            }
        }

        // 获取事件的监听者数量
        public int GetListenerCount(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            lock (_events)
            {
                if (_events.TryGetValue(eventName, out var handlers))
                {
                    return handlers.GetInvocationList().Length;
                }
                return 0;
            }
        }

        #endregion

        public override void Dispose()
        {
            ClearAllEvents();
        }
    }
}
