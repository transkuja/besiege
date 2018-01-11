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
public struct BlocData
{
    [SerializeField]
    public Vector3 position;
    [SerializeField]
    public Quaternion rotation;
    [SerializeField]
    public int blockType;

    public void SetData(BlocData _data)
    {
        //position = new Vector3(Mathf.Round(_data.position.x), Mathf.Round(_data.position.y), Mathf.Round(_data.position.z));
        //rotation = new Quaternion(Mathf.Round(_data.rotation.x), Mathf.Round(_data.rotation.y), Mathf.Round(_data.rotation.z), Mathf.Round(_data.rotation.w));
        position = _data.position;
        rotation = _data.rotation;
        blockType = _data.blockType;
    }

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

    public string vehicleName;

    public bool hasBeenLoaded = false;

    public Vehicle(int _size)
    {
        vehicleSize = _size;
        blocks = new BlocData[_size];
        vehicleName = "";
    }

    public Vehicle()
    {

    }

    public string Serialize()
    {
        string result = vehicleName + "[" + vehicleSize.ToString();
        for (int i = 0; i < vehicleSize; i++)
        {
            result += "[" + blocks[i].Serialize();
        }
        return result;
    }

    public void Deserialize(string _data)
    {
        string[] dataSplit = _data.Split('[');

        vehicleName = dataSplit[0];
        vehicleSize = int.Parse(dataSplit[1]);
        blocks = new BlocData[vehicleSize];

        for (int i = 2; i < dataSplit.Length; i++)
        {
            blocks[i - 2].Deserialize(dataSplit[i]);
        }
    }

    public void CreateVehicle(Transform _parent)
    {
        Creator creator = GameObject.FindObjectOfType<Creator>();

        for (int i = 0; i < vehicleSize; i++)
        {
            GameObject vehiclePart = GameObject.Instantiate(creator.prefabUtils.GetBlockFromType(blocks[i].blockType), _parent);
            vehiclePart.transform.localPosition = blocks[i].position;
            vehiclePart.transform.localRotation = blocks[i].rotation;
            vehiclePart.GetComponent<Bloc>().data.SetData(blocks[i]);
        }

    }
}

[System.Serializable]
public class Bloc : MonoBehaviour {

    [SerializeField]
    public BlocData data;
}
