using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WooAsset;

public class ObjectPool
{


    Dictionary<string, SubPool> allPools = new Dictionary<string, SubPool>();

    private static ObjectPool _instance;

    public static ObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ObjectPool();
            }
            return _instance;
        }
    }

    public GameObject LoadObject(string name)
    {
        if (!allPools.ContainsKey(name))
        {
            var asset = Assets.LoadAsset(name);
            allPools.Add(name, new SubPool(asset.GetAsset<GameObject>(), 20));
        }

        return allPools[name].LoadObject();
    }

    public void RecoverObject(GameObject gameObject)
    {

        foreach (string key in allPools.Keys)
        {
            if (allPools[key].gameObjects.Contains(gameObject))
            {
                allPools[key].RecoverObejct(gameObject);
                break;
            }
        }

    }

    public SubPool GetSubPool(string name)
    {

        return allPools.ContainsKey(name) ? allPools[name] : null;
    }

}
