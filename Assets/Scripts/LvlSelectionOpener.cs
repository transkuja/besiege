using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LvlSelectionOpener : MonoBehaviour {

    public GameObject lvlSelectionScreen;

    void Update () {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                lvlSelectionScreen.SetActive(!lvlSelectionScreen.activeInHierarchy);
            }
        }
	}

}
