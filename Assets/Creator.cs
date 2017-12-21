using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Cinemachine;

public class Creator : MonoBehaviour {

    public PrefabUtils prefabUtils;

    public GameObject vehicle;
    public CinemachineFreeLook freelookCameraCreator;
    public CinemachineFreeLook freelookCameraVehicle;

    GameObject core;
    Ray ray;
    bool drawGizmos = false;
    RaycastHit hit;

    // TODO: implement this
    int currentlySelectedBlock = 0;

    public float cameraSpeed = 50.0f;

    Vector3 instantiateCenter;

    string savefileName = "/save.txt";
    public float mouseScrollSensitivity;

    void Start () {
        vehicle = new GameObject("Vehicle");
        core = Instantiate(prefabUtils.coreBlock, vehicle.transform);
	}
	
	void Update () {
        if (!GameState.isInCreatorMode)
            return;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        drawGizmos = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Block"));

        if (Input.GetMouseButtonDown(0) && drawGizmos)
        {
            if (prefabUtils.blocks[currentlySelectedBlock].GetComponent<BoxCollider>() != null)
            {
                Vector3 newPosition = GetPosition();
                if (prefabUtils.blocks[currentlySelectedBlock].transform.childCount > 0) // Ugly, need a better handling of reactors post 1st proto
                {
                    if (Physics.Raycast(newPosition, Vector3.down, prefabUtils.GetExtents(currentlySelectedBlock).y +0.5f))
                        return;
                }

                instantiateCenter = GetPosition();
                Collider[] overlapColliders = Physics.OverlapBox(instantiateCenter, prefabUtils.GetExtents(currentlySelectedBlock) * 0.95f);
                if (overlapColliders.Length == 0)
                {
                    GameObject block = Instantiate(prefabUtils.blocks[currentlySelectedBlock], newPosition, Quaternion.identity, vehicle.transform);
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

        freelookCameraCreator.m_XAxis.Value += -Input.GetAxisRaw("Horizontal");

        //Camera.main.transform.position +=
        //    (Camera.main.transform.right * Input.GetAxisRaw("Horizontal") +
        //    Camera.main.transform.forward * Input.GetAxisRaw("Vertical"))
        //    * Time.deltaTime * cameraSpeed;

        Camera.main.transform.LookAt(vehicle.transform);

        for (int i = 0; i < 3; i++)
        {
            freelookCameraCreator.m_Orbits[i].m_Radius = Mathf.Clamp(freelookCameraCreator.m_Orbits[i].m_Radius - Input.GetAxisRaw("Mouse ScrollWheel") * mouseScrollSensitivity, 7, 40);
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
        vehicle.GetComponent<VehicleController>().InitController(freelookCameraVehicle);
        freelookCameraCreator.enabled = false;
        freelookCameraVehicle.enabled = true;
        GameState.isInCreatorMode = false;
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
        GameState.isInCreatorMode = true;
        enabled = true;
        Destroy(vehicle.GetComponent<Rigidbody>());
        Destroy(vehicle.GetComponent<VehicleController>());
        vehicle.transform.localPosition = Vector3.zero;
        vehicle.transform.localRotation = Quaternion.identity;
        freelookCameraCreator.enabled = true;
        freelookCameraVehicle.enabled = false;
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
