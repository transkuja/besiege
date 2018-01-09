using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabUtils : MonoBehaviour {

    public GameObject coreBlock;

    public List<GameObject> blocks;
    //public List<GameObject> zeroGBlocks;

    private void Awake()
    {
        int index = 0;
        foreach (GameObject go in blocks)
        {
            go.GetComponent<Bloc>().data.blockType = index;
            index++;
        }
        //foreach (GameObject go in zeroGBlocks)
        //{
        //    go.GetComponent<Bloc>().data.blockType = index;
        //    index++;
        //}
    }

    public GameObject vehicleButton;

    public Vector3 GetExtents(int _index)
    {
        return new Vector3(blocks[_index].GetComponent<BoxCollider>().size.x * blocks[_index].transform.localScale.x,
            blocks[_index].GetComponent<BoxCollider>().size.y * blocks[_index].transform.localScale.y,
            blocks[_index].GetComponent<BoxCollider>().size.z * blocks[_index].transform.localScale.z)/2.0f;
    }
}
