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

    public Component ReuseComponent(GameObject prefab, Vector3 position, Quaternion rotation) {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey)) {
            Component componentToReuse = GetComponentFromPool(poolKey);
            ResetObject(prefab, position, rotation, componentToReuse);

            return componentToReuse;
        }
        else {
            Debug.Log("No object pool for" + prefab);
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
