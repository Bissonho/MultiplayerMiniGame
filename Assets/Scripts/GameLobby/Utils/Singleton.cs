using UnityEngine;

namespace LobbyRelaySample
{

    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly object lockObj = new object();
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        instance = FindObjectOfType<T>();
                        if (instance == null)
                        {
                            GameObject singleton = new GameObject(typeof(T).Name);
                            instance = singleton.AddComponent<T>();
                            DontDestroyOnLoad(singleton);
                        }
                    }
                }

                return instance;
            }
        }
    }

}