/*
    @author : Abhishek  Kayasth
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject loadScreen;
    [SerializeField] List<Text> options; 

    int selection = 0;
    bool loadMenuActive;

    // Start is called before the first frame update
    void Start()
    {
        AudioManager.i.PlayMusic(SoundLibrary.GetClipFromName("Game Title"));
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            OpenMenu();
        }

        if(loadMenuActive)
        {
            HandleUpdate();
        }
    }

    void NewGame()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    void LoadData()
    {
        SavingSystem.i.Load("saveSlot1");
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void OpenMenu()
    {
        loadScreen.SetActive(true);
        loadMenuActive = true;
        UpdateItemSelection();
    }

    public void CloseMenu()
    {
        loadScreen.gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        int prevSelection = selection;

        if(Input.GetKeyDown(KeyCode.DownArrow))
            ++selection;
        else if(Input.GetKeyDown(KeyCode.UpArrow))  
            --selection;

        selection = Mathf.Clamp(selection, 0, options.Count - 1);

        if(prevSelection != selection)
            UpdateItemSelection();  

        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(options[selection] == options[0])
                NewGame();
            else if(options[selection] == options[1])
                LoadData();
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            CloseMenu();
        }
    }

    void UpdateItemSelection()
    {
        for(int i = 0; i < options.Count; i++)
        {
            if(i == selection)
                options[i].color = GlobalSettings.i.HighlightedColor;
            else
                options[i].color = Color.black;
        }
    }
}
