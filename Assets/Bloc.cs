using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Anchors
{
    Top,
    Bottom,
    Left,
    Right,
    Forward,
    Back
}

[System.Serializable]
public class BlocData
{
    [SerializeField]
    public Vector3 position;
    [SerializeField]
    public Quaternion rotation;
    [SerializeField]
    public int blockType;

    public string Serialize()
    {
        return position.x + "|" + position.y + "|" + position.z + "|"
            + rotation.x + "|" + rotation.y + "|" + rotation.z + "|" + rotation.w + "|"
            + blockType;
    }

    public void Deserialize(string _data)
    {
        string[] dataSplit = _data.Split('|');

        position = new Vector3(float.Parse(dataSplit[0]), float.Parse(dataSplit[1]), float.Parse(dataSplit[2]));
        rotation = new Quaternion(float.Parse(dataSplit[3]), float.Parse(dataSplit[4]), float.Parse(dataSplit[5]), float.Parse(dataSplit[6]));
        blockType = int.Parse(dataSplit[7]);
    }
}

[System.Serializable]
public class Vehicle
{
    [SerializeField]
    public int vehicleSize;
    [SerializeField]
    public BlocData[] blocks;

    public Vehicle(int _size)
    {
        vehicleSize = _size;
        blocks = new BlocData[_size];
    }

    public Vehicle()
    {

    }

    public string Serialize()
    {
        string result = vehicleSize.ToString();
        for (int i = 0; i < vehicleSize; i++)
        {
            Debug.Log(i);
            result += "[" + blocks[i].Serialize();
        }
        return result;
    }

    public void Deserialize(string _data)
    {
        string[] dataSplit = _data.Split('[');

        vehicleSize = int.Parse(dataSplit[0]);
        blocks = new BlocData[vehicleSize];

        for (int i = 1; i < dataSplit.Length; i++)
            blocks[i - 1].Deserialize(dataSplit[i]);
    }

    public void CreateVehicle(Transform _parent)
    {
        Creator creator = GameObject.FindObjectOfType<Creator>();
        //GameObject.Instantiate(creator.prefabUtils.coreBlock, _parent);

        for (int i = 0; i < vehicleSize; i++)
            GameObject.Instantiate(creator.prefabUtils.blocks[blocks[i].blockType], _parent);

    }
}

public class Bloc : MonoBehaviour {

    [SerializeField]
    public BlocData data;
}
