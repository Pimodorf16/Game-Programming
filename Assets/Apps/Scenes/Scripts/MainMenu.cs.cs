using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;

    public LevelManager levelManager;
    
    private void Start()
    {
        if (!DataPersistenceManager.instance.HasGameData())
        {
            continueGameButton.interactable = false;
        }

        MusicManager.Instance.PlayMusic("MainMenu");
    }

    public void Play()
    {
        SceneManager.LoadScene("Game");
    }

    public void NewGame()
    {
        DisableMenuButtons();
        
        DataPersistenceManager.instance.NewGame();

        levelManager.LoadScene("Game");
    }

    public void ContinueGame()
    {
        DisableMenuButtons();

        levelManager.LoadScene("Game");
    }

    private void DisableMenuButtons()
    {
        newGameButton.interactable = false;
        continueGameButton.interactable = false;
    }

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
