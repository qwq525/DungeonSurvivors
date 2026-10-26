using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonSurvivors.Core
{
    /// <summary>需要池化管理的对象实现此接口，在取出/回收时重置自身状态。</summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }

    /// <summary>
    /// 通用对象池：敌人 / 子弹 / 经验宝石 / 伤害数字 / 特效统一走这里，避免 Instantiate/Destroy 造成 GC 与卡顿。
    /// 面试可讲点：
    /// 1) 预热与容量上限策略；
    /// 2) 回收时必须重置哪些状态（血量/速度/协程/事件订阅/父节点/激活态）；
    /// 3) 为什么池根节点要独立（Hierarchy 整洁，方便整体禁用）。
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly Func<T> createFunc;
        private readonly Transform root;
        private readonly int maxSize;
        private readonly Stack<T> items = new Stack<T>();

        public int CountInactive => items.Count;

        public ObjectPool(Func<T> createFunc, Transform root = null, int prewarmCount = 0, int maxSize = 256)
        {
            this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            this.root = root;
            this.maxSize = Mathf.Max(1, maxSize);

            for (int i = 0; i < prewarmCount; i++)
            {
                T item = CreateNew();
                item.gameObject.SetActive(false);
                items.Push(item);
            }
        }

        public T Get(Transform parent = null)
        {
            T item = items.Count > 0 ? items.Pop() : CreateNew();
            Transform t = item.transform;
            t.SetParent(parent != null ? parent : root, false);
            item.gameObject.SetActive(true);

            if (item is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;

            if (item is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            item.gameObject.SetActive(false);
            item.transform.SetParent(root, false);

            if (items.Count < maxSize)
            {
                items.Push(item);
            }
            else
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
        }

        private T CreateNew()
        {
            T item = createFunc();
            item.transform.SetParent(root, false);
            return item;
        }
    }
}
