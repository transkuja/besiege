using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableWinText : MonoBehaviour {

    public GameObject winText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.GetComponent<VehicleController>())
            winText.SetActive(true);
    }
}
