using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadAssetFromCloud : MonoBehaviour
{
    [SerializeField]
    private string label;

    [SerializeField]
    private AssetReferenceAudioClip musicAssetReference;
    
    // Start is called before the first frame update
    void Start()
    {
        Get(label);
    }

    private async void Get(string label)
    {
        var locations = await Addressables.LoadResourceLocationsAsync(label).Task;

        foreach (var location in locations)
        {
            await Addressables.InstantiateAsync(location).Task;
        }
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
