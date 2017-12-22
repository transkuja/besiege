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
    bool hasHitAPotentialTarget = false;

    RaycastHit hit;

    // TODO: implement this
    int currentlySelectedBlockIndex = 0;
    GameObject currentlySelectedBlock;

    public float cameraSpeed = 50.0f;

    Vector3 instantiateCenter;

    string savefileName = "/save.txt";
    public float mouseScrollSensitivity;

    void Start () {
        core = Instantiate(prefabUtils.coreBlock, vehicle.transform);
        CreatePreviewBlock();
    }

    void Update () {
        if (!GameState.isInCreatorMode)
            return;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        hasHitAPotentialTarget = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Block"));

        if (hasHitAPotentialTarget)
        {
            if (prefabUtils.blocks[currentlySelectedBlockIndex].GetComponent<BoxCollider>() != null)
            {
                Vector3 newPosition = GetPosition();
                if (prefabUtils.blocks[currentlySelectedBlockIndex].transform.childCount > 0) // Ugly, need a better handling of reactors post 1st proto
                {
                    // Check if there's nothing under the zero gravity module we want to put
                    if (Physics.Raycast(newPosition, Vector3.down, prefabUtils.GetExtents(currentlySelectedBlockIndex).y + 0.5f))
                    {
                        currentlySelectedBlock.SetActive(false);
                        return;
                    }
                }

                // Makes sure we do not something under a zero gravity module
                if (hit.collider.GetComponentInChildren<ZeroGBlock>() != null)
                {
                    currentlySelectedBlock.SetActive(false);
                    return;
                }

                instantiateCenter = GetPosition();
                Collider[] overlapColliders = Physics.OverlapBox(instantiateCenter, prefabUtils.GetExtents(currentlySelectedBlockIndex) * 0.95f);
                if (overlapColliders.Length > 0)
                {
                    currentlySelectedBlock.SetActive(false);
                    return;

                }

                // Show the position where the block will be put
                currentlySelectedBlock.transform.position = newPosition;
                currentlySelectedBlock.transform.rotation = Quaternion.identity;
                currentlySelectedBlock.SetActive(true);

                if (Input.GetMouseButtonDown(0))
                {
                    // Put the selected block on the vehicle
                    currentlySelectedBlock.GetComponent<BoxCollider>().enabled = true;
                    Color oldColor = currentlySelectedBlock.GetComponent<MeshRenderer>().material.color;
                    currentlySelectedBlock.GetComponent<MeshRenderer>().material.color = new Color(oldColor.r, oldColor.g, oldColor.b, 1.0f);
                    currentlySelectedBlock.GetComponent<Bloc>().data.position = transform.position;
                    currentlySelectedBlock.GetComponent<Bloc>().data.rotation = transform.rotation;

                    // Create a new preview block
                    CreatePreviewBlock();
                }
            }
        }
        else
        {
            // Deactivate preview if no potential target
            currentlySelectedBlock.SetActive(false);
        }

        if (Input.GetMouseButtonDown(1) && hasHitAPotentialTarget)
        {
            if (hit.collider.transform.tag != "Core")
                Destroy(hit.collider.transform.gameObject);
        }

        freelookCameraCreator.m_XAxis.Value += -Input.GetAxisRaw("Horizontal");

        Camera.main.transform.LookAt(vehicle.transform);

        for (int i = 0; i < 3; i++)
        {
            freelookCameraCreator.m_Orbits[i].m_Radius = Mathf.Clamp(freelookCameraCreator.m_Orbits[i].m_Radius - Input.GetAxisRaw("Mouse ScrollWheel") * mouseScrollSensitivity, 7, 40);
        }

    }

    Vector3 GetPosition()
    {
        Vector3 newPos = new Vector3(
                hit.normal.x * (hit.collider.bounds.extents.x + prefabUtils.GetExtents(currentlySelectedBlockIndex).x), 
                hit.normal.y * (hit.collider.bounds.extents.y + prefabUtils.GetExtents(currentlySelectedBlockIndex).y), 
                hit.normal.z * (hit.collider.bounds.extents.z + prefabUtils.GetExtents(currentlySelectedBlockIndex).z)) 
            + hit.collider.transform.position;

        return newPos;

    }

    public void ChangeBlock(int _newBlockIndex)
    {
        currentlySelectedBlockIndex = _newBlockIndex;

        if (currentlySelectedBlock != null) DestroyImmediate(currentlySelectedBlock);
        CreatePreviewBlock();
    }

    public void Build()
    {
        vehicle.GetComponent<Rigidbody>().useGravity = true;
        vehicle.AddComponent<VehicleController>();
        vehicle.GetComponent<VehicleController>().InitController(freelookCameraVehicle);
        freelookCameraCreator.gameObject.SetActive(false);
        freelookCameraVehicle.gameObject.SetActive(true);
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
        vehicle.GetComponent<Rigidbody>().useGravity = false;
        Destroy(vehicle.GetComponent<VehicleController>());
        vehicle.transform.localPosition = Vector3.zero;
        vehicle.transform.localRotation = Quaternion.identity;
        freelookCameraCreator.gameObject.SetActive(true);
        freelookCameraVehicle.gameObject.SetActive(false);
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

    private void CreatePreviewBlock()
    {
        currentlySelectedBlock = Instantiate(prefabUtils.blocks[currentlySelectedBlockIndex], Vector3.zero, Quaternion.identity, vehicle.transform);
        foreach (Collider c in currentlySelectedBlock.GetComponentsInChildren<Collider>())
            c.enabled = false;
        Color oldColor = currentlySelectedBlock.GetComponent<MeshRenderer>().material.color;
        currentlySelectedBlock.GetComponent<MeshRenderer>().material.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.3f);
        currentlySelectedBlock.SetActive(false);
    }

    public void InvertXAxis()
    {
        freelookCameraCreator.m_XAxis.m_InvertAxis = !freelookCameraCreator.m_XAxis.m_InvertAxis;
        freelookCameraVehicle.m_XAxis.m_InvertAxis = !freelookCameraVehicle.m_XAxis.m_InvertAxis;
    }

    public void InvertYAxis()
    {
        freelookCameraCreator.m_YAxis.m_InvertAxis = !freelookCameraCreator.m_YAxis.m_InvertAxis;
        freelookCameraVehicle.m_YAxis.m_InvertAxis = !freelookCameraVehicle.m_YAxis.m_InvertAxis;
    }
}
