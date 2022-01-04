using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
{
    public AssetReferenceAudioClip(string guid): base(guid) { }
}

public class AddressablesManager : MonoBehaviour
{
    [SerializeField]
    private AssetReference playerArmatureAssetReference;

    [SerializeField]
    private AssetReferenceAudioClip musicAssetReference;

   
    private GameObject playerController;

    
    // Start is called before the first frame update
    void Start()
    {
        // start the loader

        Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
    }

    private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
    {

        playerArmatureAssetReference.LoadAssetAsync<GameObject>().Completed += (playerAsset) =>
        {

            playerArmatureAssetReference.InstantiateAsync().Completed += (playerGameObject) =>
            {

                playerController = playerGameObject.Result;

            };
        };

        musicAssetReference.LoadAssetAsync<AudioClip>().Completed += (clip) =>
        {
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip.Result;
            audioSource.volume = 0.05f;
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.Play();

        };


    }

    
  
}
