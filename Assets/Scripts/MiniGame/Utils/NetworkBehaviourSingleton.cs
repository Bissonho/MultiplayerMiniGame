using UnityEngine;
using Unity.Netcode;


namespace LobbyRelaySample.ngo
{
    public class NetworkBehaviourSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        private static readonly object lockObj = new object();
        private static T instance;

        public static T Instance
        {
            get
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<T>();
                        if (instance == null)
                        {
                            GameObject singleton = new GameObject(typeof(T).Name);
                            instance = singleton.AddComponent<T>();
                            DontDestroyOnLoad(singleton);
                        }
                    }
                    return instance;
                }
            }
        }
    }
}