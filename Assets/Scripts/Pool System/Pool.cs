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

        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(prefab, parent);

            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(); // Spawn while active (default state after Instantiate)
            }

            obj.gameObject.SetActive(false); // then deactivate

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

            obj.gameObject.SetActive(false);
            return obj;
        }
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        availableObj.Enqueue(obj);
    }
}
