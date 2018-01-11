using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class PrefabUtils : MonoBehaviour {

    public GameObject coreBlock;

    public List<GameObject> blocks;
    bool assetBundleLoaded = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (!assetBundleLoaded)
            StartCoroutine(LoadBlocks());
    }

    public GameObject vehicleButton;
    public GameObject blockButton;

    public Vector3 GetExtents(int index)
    {
        int _index = 0;

        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].GetComponent<Bloc>().data.blockType == index)
            {
                _index = i;
                break;
            }

        }

        return new Vector3(blocks[_index].GetComponent<BoxCollider>().size.x * blocks[_index].transform.localScale.x,
            blocks[_index].GetComponent<BoxCollider>().size.y * blocks[_index].transform.localScale.y,
            blocks[_index].GetComponent<BoxCollider>().size.z * blocks[_index].transform.localScale.z)/2.0f;
    }

    public IEnumerator LoadBlocks()
    {
        AssetBundle bundle = null;
            
        string uri = "file:///" + Application.dataPath + "/../AssetBundles/StandaloneWindows/vehicleblocks";
        UnityWebRequest request = UnityWebRequest.GetAssetBundle(uri, 0);
        yield return request.Send();

        try
        {
            bundle = DownloadHandlerAssetBundle.GetContent(request);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load blocks " + e.Message);
        }

        if (bundle == null)
        {
            Debug.LogError("Failed to load blocks");
            yield return null;
        }

        GameObject[] blocksFromBundle = bundle.LoadAllAssets<GameObject>();
        blocks = new List<GameObject>();
        blocks.AddRange(blocksFromBundle);
        assetBundleLoaded = true;
    }

    public GameObject GetBlockFromType(int _blockType)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].GetComponent<Bloc>().data.blockType == _blockType)
            {
                return blocks[i];
            }

        }

        return null;
    }
}
