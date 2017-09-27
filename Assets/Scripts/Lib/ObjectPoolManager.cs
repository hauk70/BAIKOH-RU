using System.Collections.Generic;
using UnityEngine;

namespace Lib.Util
{
    public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
    {

        protected Dictionary<GameObject, List<IPoolItem>> Pool;

        public override void Awake()
        {
            base.Awake();
            Pool = new Dictionary<GameObject, List<IPoolItem>>();
        }

        public IPoolItem Get(GameObject prefab)
        {
            if (!Pool.ContainsKey(prefab))
            {
                Pool.Add(prefab, new List<IPoolItem>());
            }

            var list = Pool[prefab];
            IPoolItem poolItem = null;

            foreach (var item in list)
            {
                if (!item.InUse)
                {
                    poolItem = item;
                    break;
                }
            }

            if (poolItem == null)
            {
                var obj = Instantiate(prefab);
                poolItem = obj.GetComponent<IPoolItem>();
                list.Add(poolItem);
            }

            poolItem.InUse = true;
            poolItem.OnCreate();
            return poolItem;
        }

        public void Push(GameObject prefab, IPoolItem item)
        {
            item.InUse = false;
            item.OnRemove();
        }

    }

    public interface IPoolItem
    {
        bool InUse { get; set; }

        void OnRemove();
        void OnCreate();
    }
}