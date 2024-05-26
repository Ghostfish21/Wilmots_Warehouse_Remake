using System.Collections;
using System.Collections.Concurrent;
using Guiyuu;
using UnityEngine;

public class PrefabParameters {
    
    private static PrefabParameters instance;
    
    private static int idCounter = 1;

    private readonly ConcurrentDictionary<int, object[]> parameters = new();

    public static GameObject initPrefab(GameObject prefab, Transform parent, params object[] param) {
        if (instance == null) instance = new PrefabParameters();
        GameObject go = GameObject.Instantiate(prefab, parent);
        instance.parameters.TryAdd(go.GetInstanceID(), param);
        return go;
    }
    
    public static object[] getParameters(GameObject go) {
        if (instance == null) instance = new PrefabParameters();
        instance.parameters.TryGetValue(go.GetInstanceID(), out object[] param);
        return param;
    }
    
}
