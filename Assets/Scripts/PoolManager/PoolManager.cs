using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolManager : SingletonMonobehaviour<PoolManager>
{
    [SerializeField] private Pool[] pools = null;
    private Transform objectPoolTransform;
    private Dictionary<int, Queue<Component>> poolDictionary = new Dictionary<int, Queue<Component>>();
    
    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(pools), pools);
    }

#endif

    #endregion Validation
    private void Start()
    {
    
        objectPoolTransform = this.gameObject.transform;

        for (int i = 0; i < pools.Length; i++) {
            CreatePool(pools[i].prefab, pools[i].poolSize, pools[i].componentType);
        }
    }

    private void CreatePool(GameObject prefab, int poolSize, string componentType)
    {
        int poolKey = prefab.GetInstanceID();

        string prefabName = prefab.name;

        // 创建一个名为prefabName + "Anchor"的GameObject对象，并将其赋值给parentGameObject变量
        GameObject parentGameObject = new GameObject(prefabName + "Anchor");

        parentGameObject.transform.SetParent(objectPoolTransform);

        if (!poolDictionary.ContainsKey(poolKey)) {
            poolDictionary.Add(poolKey, new Queue<Component>());
            for (int i = 0; i < poolSize; i++) {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform) as GameObject;
                newObject.SetActive(false);
                poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(componentType)));
            }
        }
    }

    // 从预制件、位置和旋转创建一个可重用的组件
    public Component ReuseComponent(GameObject prefab, Vector3 position, Quaternion rotation) {
        // 获取预制件的实例ID
        int poolKey = prefab.GetInstanceID();

        // 如果池字典中包含该实例ID
        if (poolDictionary.ContainsKey(poolKey)) {
            // 从池中获取一个组件
            Component componentToReuse = GetComponentFromPool(poolKey);
            // 重置对象的位置和旋转
            ResetObject(prefab, position, rotation, componentToReuse);

            // 返回可重用的组件
            return componentToReuse;
        }
        else {
            // 如果池字典中不包含该实例ID，则输出日志
            Debug.Log("No object pool for" + prefab);
            // 返回null
            return null;
        }
    }

    private Component GetComponentFromPool(int poolKey)
    {
        Component componentToReuse = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(componentToReuse);
        if (componentToReuse.gameObject.activeSelf == true) {
            componentToReuse.gameObject.SetActive(false);
        }
        return componentToReuse;
    }

    private void ResetObject(GameObject prefab, Vector3 position, Quaternion rotation, Component componentToReuse)
    {
        componentToReuse.transform.position = position;
        componentToReuse.transform.rotation = rotation;
        componentToReuse.gameObject.transform.localScale = prefab.transform.localScale;
    }
}
