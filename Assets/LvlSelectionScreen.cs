using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class LvlSelectionScreen : MonoBehaviour {
    public PrefabUtils prefabUtils;

    void Start () {

        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            GameObject button = Instantiate(prefabUtils.vehicleButton, transform);
            button.GetComponentInChildren<Text>().text = "Level " + i;
            int temp = i;
            button.GetComponent<Button>().onClick.AddListener(() => { LoadSceneFromButton(temp); });
            button.transform.localPosition = new Vector2(0.0f, 110 - 30.0f * i);
        }

        if (SceneManager.GetActiveScene().buildIndex != 0)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    }

    void LoadSceneFromButton(int _sceneIndex)
    {
        SceneManager.LoadScene(_sceneIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
