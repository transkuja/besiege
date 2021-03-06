﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Cinemachine;

public class Creator : MonoBehaviour {

    public PrefabUtils prefabUtils;

    public GameObject vehicle;
    public CinemachineFreeLook freelookCameraCreator;
    public CinemachineFreeLook freelookCameraVehicle;

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

    bool hasTheVehicleBeenLoaded = false;
    int vehicleSelectedIndex = -1;
    string vehicleSelectedName = "";

    public GameObject saveScreen;
    public GameObject loadScreen;
    public GameObject lvlSelectionScreen;
    public GameObject blocksPanel;

    Vehicle vehicleToLoad = new Vehicle();

    bool saveNeeded = false;

    void Start () {
        prefabUtils = GameObject.FindObjectOfType<PrefabUtils>();

        int nbButtons = 0;
        foreach (GameObject block in prefabUtils.blocks)
        {
            GameObject blockButton = Instantiate(prefabUtils.blockButton, blocksPanel.transform);
            int temp = block.GetComponent<Bloc>().data.blockType;
            blockButton.GetComponent<Button>().onClick.AddListener(() => { ChangeBlock(temp); });

            if (block.GetComponentInChildren<ZeroGBlock>() != null)
            {
                blockButton.GetComponentInChildren<Text>().text = "ZeroG Module";
                blockButton.transform.localPosition = new Vector2(40.0f, 90.0f);
            }
            else if (block.GetComponentInChildren<BoostBlock>() != null)
            {
                blockButton.GetComponentInChildren<Text>().text = "Boost Module";
                blockButton.transform.localPosition = new Vector2(110.0f, 90.0f);
            }
            else
            {
                blockButton.GetComponentInChildren<Text>().text = "Block" + nbButtons;
                blockButton.transform.localPosition = new Vector2(40.0f + nbButtons * 70.0f, 30.0f);
                nbButtons++;
            }
        }

        Instantiate(prefabUtils.coreBlock, vehicle.transform);
        CreatePreviewBlock();

        LoadVehicleOnStart();
    }

    void Update () {
        if (!GameState.isInCreatorMode)
            return;
            
        if (saveScreen.activeInHierarchy)
            return;


        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        hasHitAPotentialTarget = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Block"));
        if (Input.GetMouseButtonDown(1) && hasHitAPotentialTarget)
        {
            if (hit.collider.transform.tag != "Core")
                Destroy(hit.collider.transform.gameObject);
        }

        if (currentlySelectedBlock == null)
            return;

        if (hasHitAPotentialTarget)
        {
            if (prefabUtils.blocks[currentlySelectedBlockIndex].GetComponent<BoxCollider>() != null)
            {
                Vector3 newPosition = GetPosition();
                if (prefabUtils.blocks[currentlySelectedBlockIndex].transform.GetComponent<ZeroGBlock>() != null)
                {
                    // Check if there's nothing under the zero gravity module we want to put
                    if (Physics.Raycast(newPosition, Vector3.down, prefabUtils.GetExtents(currentlySelectedBlockIndex).y + 0.5f))
                    {
                        currentlySelectedBlock.SetActive(false);
                        return;
                    }
                }

                if (prefabUtils.blocks[currentlySelectedBlockIndex].transform.GetComponent<BoostBlock>() != null)
                {
                    // Check if there's nothing behind the boost module we want to put
                    if (Physics.Raycast(newPosition, -transform.forward, prefabUtils.GetExtents(currentlySelectedBlockIndex).z + 0.5f))
                    {
                        currentlySelectedBlock.SetActive(false);
                        return;
                    }
                }

                // Makes sure we do not something under a zero gravity module
                if (hit.collider.GetComponentInChildren<ZeroGBlock>() != null || hit.collider.GetComponentInChildren<BoostBlock>() != null)
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
                    currentlySelectedBlock.GetComponent<Bloc>().data.position = currentlySelectedBlock.transform.position;
                    currentlySelectedBlock.GetComponent<Bloc>().data.rotation = currentlySelectedBlock.transform.rotation;

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
        vehicle.GetComponent<VehicleController>().InitController(freelookCameraVehicle, this);
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

            hasTheVehicleBeenLoaded = false;
        }
    }

    public void BackToCreation()
    {
        GameState.isInCreatorMode = true;
        enabled = true;
        vehicle.GetComponent<Rigidbody>().useGravity = false;
        vehicle.GetComponent<Rigidbody>().velocity = Vector3.zero;
        vehicle.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        Destroy(vehicle.GetComponent<VehicleController>());
        vehicle.transform.localPosition = Vector3.zero;
        vehicle.transform.localRotation = Quaternion.identity;
        freelookCameraCreator.gameObject.SetActive(true);
        freelookCameraVehicle.gameObject.SetActive(false);
    }

    public void SaveButton()
    {
        Destroy(currentlySelectedBlock);
        if (!hasTheVehicleBeenLoaded)
        {
            // open save screen
            Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = false;
            saveScreen.SetActive(true);
        }
        else
        {
            SaveValidated();
        }

    }

    public void SaveValidated(string forcedName = "")
    {
        if (vehicle == null)
            return;

        Vehicle vehicleToSave = new Vehicle(vehicle.transform.childCount - 1);

        vehicleToSave.vehicleName = saveScreen.transform.GetChild(saveScreen.transform.childCount - 1).GetComponent<Text>().text;

        for (int i = 1; i < vehicle.transform.childCount; i++)
        {
            vehicleToSave.blocks[i - 1].SetData(vehicle.transform.GetChild(i).GetComponent<Bloc>().data);
        }

        if (hasTheVehicleBeenLoaded)
        {
            string[] vehiclesLoaded = File.ReadAllLines(Application.persistentDataPath + savefileName);
            vehicleToSave.vehicleName = vehicleSelectedName;
            vehiclesLoaded[vehicleSelectedIndex] = vehicleToSave.Serialize();

            StreamWriter sw = File.CreateText(Application.persistentDataPath + savefileName);
            for (int i = 0; i < vehiclesLoaded.Length; i++)
                sw.WriteLine(vehiclesLoaded[i]);
            sw.Close();
        }
        else
        {
            StreamWriter sw = File.AppendText(Application.persistentDataPath + savefileName);
            sw.WriteLine(forcedName + vehicleToSave.Serialize());
            sw.Close();
        }
       
        if (saveScreen.activeInHierarchy)
        {
            Camera.main.GetComponent<Cinemachine.CinemachineBrain>().enabled = true;
            saveScreen.SetActive(false);
        }
    }

    // Open load screen
    public void LoadButton()
    {
        Destroy(currentlySelectedBlock);
        for (int i = 1; i < loadScreen.transform.childCount; i++)
            Destroy(loadScreen.transform.GetChild(i).gameObject);
        loadScreen.SetActive(true);

        // read all file, creates buttons accordingly
        string[] vehiclesLoaded;
        if (!File.Exists(Application.persistentDataPath + savefileName))
        {
            vehiclesLoaded = new string[1];
            vehiclesLoaded[0] = "Default[8[1|0|0|0|0|0|1|4[0|1|0|0|0|0|1|4[-1.000004|-1.519918E-06|0|0|0|0|1|4[-2.000004|-1.519918E-06|0|0|0|0|1|0[1.999999|-3.248453E-06|0|0|0|0|1|0[-1.400709E-06|0.9999968|-1.5|0|0|0|1|1[0.9999986|-1.329184E-05|-1.499993|0|0|0|1|1[-1.000004|-1.871726E-05|-1.499988|0|0|7.047312E-18|1|1";
        }
        else
            vehiclesLoaded = File.ReadAllLines(Application.persistentDataPath + savefileName);

        for (int i = 0; i < vehiclesLoaded.Length; i++)
        {
            GameObject button = Instantiate(prefabUtils.vehicleButton, loadScreen.transform);
            vehicleToLoad.Deserialize(vehiclesLoaded[i]);
            button.GetComponentInChildren<Text>().text = vehicleToLoad.vehicleName;
            int temp = i;
            button.GetComponent<Button>().onClick.AddListener(() => { LoadValidated(temp); });
            if (i < 8)
                button.transform.localPosition = new Vector2(-100.0f, 110 - 30.0f*i);
            else
                button.transform.localPosition = new Vector2(100.0f, 110 - 30.0f * (i - 8));
        }
    }

    // Load the selected vehicle
    void LoadValidated(int _indexButton)
    {
        string[] vehiclesLoaded = File.ReadAllLines(Application.persistentDataPath + savefileName);

        vehicleToLoad.Deserialize(vehiclesLoaded[_indexButton]);

        ResetButton();
        vehicleToLoad.CreateVehicle(vehicle.transform);
        hasTheVehicleBeenLoaded = true;
        vehicleSelectedIndex = _indexButton;
        vehicleSelectedName = vehicleToLoad.vehicleName;
        loadScreen.SetActive(false);
    }

    // Load a vehicle at start while the player has not saved his/her own
    void LoadVehicleOnStart()
    {
        string[] vehiclesLoaded;
        if (!File.Exists(Application.persistentDataPath + savefileName))
        {
            vehiclesLoaded = new string[1];
            vehiclesLoaded[0] = "Default[8[1|0|0|0|0|0|1|4[0|1|0|0|0|0|1|4[-1.000004|-1.519918E-06|0|0|0|0|1|4[-2.000004|-1.519918E-06|0|0|0|0|1|0[1.999999|-3.248453E-06|0|0|0|0|1|0[-1.400709E-06|0.9999968|-1.5|0|0|0|1|1[0.9999986|-1.329184E-05|-1.499993|0|0|0|1|1[-1.000004|-1.871726E-05|-1.499988|0|0|7.047312E-18|1|1";
            saveNeeded = true;
        }
        else
            vehiclesLoaded = File.ReadAllLines(Application.persistentDataPath + savefileName);

        // Create vehicle at start if there's only the one by default saved
        if (vehiclesLoaded.Length != 1)
            return;

        vehicleToLoad.Deserialize(vehiclesLoaded[0]);

        ResetButton();
        vehicleToLoad.CreateVehicle(vehicle.transform);
        hasTheVehicleBeenLoaded = true;
        vehicleSelectedIndex = 0;
        vehicleSelectedName = vehicleToLoad.vehicleName;

        if (saveNeeded)
        {
            hasTheVehicleBeenLoaded = false;
            SaveValidated("Default");
        }
    }

    private void CreatePreviewBlock()
     {
        foreach (GameObject go in prefabUtils.blocks)
        {
            if (go.GetComponent<Bloc>().data.blockType == currentlySelectedBlockIndex)
            {
                currentlySelectedBlock = Instantiate(go, Vector3.zero, Quaternion.identity, vehicle.transform);
                break;
            }

        }

        if (currentlySelectedBlock == null)
            return;

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
