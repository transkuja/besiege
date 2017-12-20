using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Cinemachine;

public class Creator : MonoBehaviour {

    public PrefabUtils prefabUtils;

    public GameObject vehicle;
    public CinemachineFreeLook freelookCamera;

    GameObject core;
    Ray ray;
    bool drawGizmos = false;
    RaycastHit hit;

    // TODO: implement this
    int currentlySelectedBlock = 0;

    public float cameraSpeed = 50.0f;

    Vector3 instantiateCenter;

    string savefileName = "/save.txt";

    void Start () {
        vehicle = new GameObject("Vehicle");
        core = Instantiate(prefabUtils.coreBlock, vehicle.transform);
	}
	
	void Update () {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        drawGizmos = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Block"));

        if (Input.GetMouseButtonDown(0) && drawGizmos)
        {
            if (prefabUtils.blocks[currentlySelectedBlock].GetComponent<BoxCollider>() != null)
            {
                instantiateCenter = GetPosition();
                Collider[] overlapColliders = Physics.OverlapBox(instantiateCenter, prefabUtils.GetExtents(currentlySelectedBlock) * 0.95f);
                if (overlapColliders.Length == 0)
                {
                    GameObject block = Instantiate(prefabUtils.blocks[currentlySelectedBlock], GetPosition(), Quaternion.identity, vehicle.transform);
                    block.GetComponent<Bloc>().data.position = transform.position;
                    block.GetComponent<Bloc>().data.rotation = transform.rotation;
                }
            }
        }

        if (Input.GetMouseButtonDown(1) && drawGizmos)
        {
            if (hit.transform.tag != "Core")
                Destroy(hit.transform.gameObject);
        }

        Camera.main.transform.position +=
            (Camera.main.transform.right * Input.GetAxisRaw("Horizontal") +
            Camera.main.transform.forward * Input.GetAxisRaw("Vertical"))
            * Time.deltaTime * cameraSpeed;

        Camera.main.transform.LookAt(vehicle.transform);

        if (Input.GetKey(KeyCode.LeftControl))
        {
            // TODO: Gimbal lock
            Camera.main.transform.Rotate(Camera.main.transform.right, Input.GetAxisRaw("Mouse Y"));
            Camera.main.transform.Rotate(Vector3.up, Input.GetAxisRaw("Mouse X"));

        }
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(hit.point, Vector3.one);
        }
    }

    Vector3 GetPosition()
    {
        Vector3 newPos = new Vector3(
                hit.normal.x * (hit.collider.bounds.extents.x + prefabUtils.GetExtents(currentlySelectedBlock).x), 
                hit.normal.y * (hit.collider.bounds.extents.y + prefabUtils.GetExtents(currentlySelectedBlock).y), 
                hit.normal.z * (hit.collider.bounds.extents.z + prefabUtils.GetExtents(currentlySelectedBlock).z)) 
            + hit.transform.position;

        return newPos;

    }

    public void ChangeBlock(int _newBlockIndex)
    {
        currentlySelectedBlock = _newBlockIndex;
    }

    public void Build()
    {
        vehicle.AddComponent<Rigidbody>();
        vehicle.AddComponent<VehicleController>();
        enabled = false;
    }

    public void ResetButton()
    {
        if (enabled)
        {
            for (int i = 1; i < vehicle.transform.childCount; i++)
                Destroy(vehicle.transform.GetChild(i).gameObject);
        }
    }

    public void BackToCreation()
    {
        enabled = true;
        Destroy(vehicle.GetComponent<Rigidbody>());
        Destroy(vehicle.GetComponent<VehicleController>());
        vehicle.transform.localPosition = Vector3.zero;
    }

    public void SaveButton()
    {
        Vehicle vehicleToSave = new Vehicle(vehicle.transform.childCount - 1);

        for (int i = 1; i < vehicle.transform.childCount; i++)
        {
            vehicleToSave.blocks[i - 1] = vehicle.transform.GetChild(i).GetComponent<Bloc>().data;
        }

        StreamWriter sw = File.CreateText(Application.persistentDataPath + savefileName);
        Debug.Log(Application.persistentDataPath + savefileName);
        sw.WriteLine(vehicleToSave.Serialize());
        sw.Close();
    }

    public void LoadButton()
    {
        if (!File.Exists(Application.persistentDataPath + savefileName))
            return;
        
        
        Vehicle vehicleToLoad = new Vehicle();
        vehicleToLoad.Deserialize(File.ReadAllText(Application.persistentDataPath + savefileName));

        ResetButton();
        vehicleToLoad.CreateVehicle(vehicle.transform);
    }
}
