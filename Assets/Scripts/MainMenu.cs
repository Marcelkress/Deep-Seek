using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    public GameObject selectOnStart;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(selectOnStart);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
