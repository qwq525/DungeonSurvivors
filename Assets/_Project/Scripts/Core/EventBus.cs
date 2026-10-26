using System;
using System.Collections.Generic;

namespace DungeonSurvivors.Core
{
    /// <summary>
    /// 全局事件总线：让 UI / 音效 / 统计等模块订阅游戏事件，逻辑层不反向依赖 UI。
    /// 面试可讲点：
    /// 1) 为什么用事件而不是直接引用（解耦、可测试）；
    /// 2) 订阅和退订必须成对，否则静态字典会持有已销毁对象的委托（内存泄漏/空引用）；
    /// 3) Publish 时不要做重逻辑，避免一帧内回调过长。
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            Type type = typeof(T);
            Handlers.TryGetValue(type, out Delegate existing);
            Handlers[type] = Delegate.Combine(existing, handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            Type type = typeof(T);
            if (!Handlers.TryGetValue(type, out Delegate existing)) return;

            Delegate combined = Delegate.Remove(existing, handler);
            if (combined == null)
            {
                Handlers.Remove(type);
            }
            else
            {
                Handlers[type] = combined;
            }
        }

        public static void Publish<T>(T evt)
        {
            if (Handlers.TryGetValue(typeof(T), out Delegate handler))
            {
                Action<T> callback = handler as Action<T>;
                callback?.Invoke(evt);
            }
        }

        /// <summary>场景切换/退出游戏时清理，避免残留订阅。</summary>
        public static void Clear()
        {
            Handlers.Clear();
        }
    }
}
