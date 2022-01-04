using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class AssetRef : MonoBehaviour
{
     public static async Task CreateAssetAddToList<T>(AssetReference reference,List<T> compleredObjs)
        where T: Object
    {
        compleredObjs.Add(await reference.InstantiateAsync().Task as T);

    }
    public static async Task CreateAssetsAddToList<T>(List<AssetReference> references, List<T> completedObjs)
     where T : Object
    {
        foreach (var reference in references)
            completedObjs.Add(await reference.InstantiateAsync().Task as T);
    }

}
