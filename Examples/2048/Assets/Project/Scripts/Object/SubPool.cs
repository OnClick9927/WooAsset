using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPool 
{
    public GameObject objectPrefab;

    public List<GameObject> gameObjects = new List<GameObject>();

    public int size;


    public SubPool( GameObject gameObject,int size )
    {
        this.objectPrefab = gameObject;
        this.size = size;
    }

    public GameObject LoadObject() {

        GameObject go = null;
        // 判断当前这个池子里面有没有可用的物体
        foreach ( GameObject gameObject in gameObjects )
        {
            if ( gameObject.activeSelf == false  )
            {
                go = gameObject;
                break;
            }
        }

        if (go == null)
        {
            // 创建一个新的游戏物体
            go = GameObject.Instantiate(objectPrefab);
            gameObjects.Add(go);
        }

        go.GetComponent<IRecycleObject>().OnLoad();
        go.SetActive(true);   
        GameObject.DontDestroyOnLoad(go);
        return go;
    }

    public void RecoverObejct( GameObject gameObject ) {

        gameObject.GetComponent<IRecycleObject>().OnRecover();
        if (gameObjects.Count > this.size)
        {
            gameObjects.Remove(gameObject);
            // 把这个游戏物体给销毁掉
            GameObject.Destroy(gameObject);
        }
        else {
            gameObject.SetActive(false);
        }



    }


    public void RecoverAllObject() {
        foreach ( GameObject go in gameObjects )
        {
            go.GetComponent<IRecycleObject>().OnRecover();
        }
        gameObjects.Clear();
    }
}
