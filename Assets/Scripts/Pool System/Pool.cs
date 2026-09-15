using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Pool<T> where T : MonoBehaviour
{
    Queue<T> availableObj = new Queue<T> ();
    T prefab;
    Transform parent;

    public Pool(T prefabToPool, int initialSize, Transform poolParent = null)
    {
        prefab = prefabToPool;
        parent = poolParent;

        //pre spawn objs
        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);

            //if its a network obj spawn it
            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }

            availableObj.Enqueue(obj);
        }
    }

    public T Get()
    {
        if (availableObj.Count > 0)
        {
            return availableObj.Dequeue ();
        }
        else
        {
            //pool empty, spawn new one
            T obj = Object.Instantiate (prefab, parent);

            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn ();
            }

            return obj;
        }
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        availableObj.Enqueue(obj);
    }
}
