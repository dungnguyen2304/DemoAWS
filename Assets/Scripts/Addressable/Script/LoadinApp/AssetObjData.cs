using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AssetObjData : MonoBehaviour
{
    [SerializeField] private AssetReference _sqrARef;
    [SerializeField] private List<AssetReference> _references = new List<AssetReference>();

    [SerializeField] private List<GameObject> _completedObj = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        _references.Add(_sqrARef);
        StartCoroutine(LoadAndWaitUntilComplete());
    }

    private IEnumerator LoadAndWaitUntilComplete()
    {
        yield return AssetRef.CreateAssetsAddToList(_references, _completedObj);
    }

    
}
