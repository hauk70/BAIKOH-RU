using UnityEngine;

namespace Lib.Util
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));
                    if (_instance == null)
                    {
                        GameObject GameSystem;
                        if (!(GameSystem = GameObject.Find("GameSystem")))
                        {
                            GameSystem = new GameObject("GameSystem");
                        }
                        _instance = GameSystem.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        public virtual void Awake()
        {
            if (Instance && this != Instance)
            {
                DestroyImmediate(gameObject);
                return;
            }
            DontDestroyOnLoad(transform.gameObject);
        }
    }
}