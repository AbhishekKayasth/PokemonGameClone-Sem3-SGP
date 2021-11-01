using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneDetails : MonoBehaviour
{

    [SerializeField] List<SceneDetails> connectedScenes;

    public bool IsLoaded{get; private set; }

    List<SavableEntity> savableEntities;

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.tag == "Player")
        {
            Debug.Log($"Entered {gameObject.name}");
            
            LoadScene();
            //To set the current scene in GameController Script
            GameController.Instance.SetCurrentScene(this);

            //Loading all connected Scenes to prevent the black screen
            foreach(var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            //Unloading the scenes that are no longer connected
            var prevScene = GameController.Instance.PrevScene;
            if(prevScene != null)
            {
                var previouslyLoadedScenes = prevScene.connectedScenes;
                foreach (var scene in previouslyLoadedScenes)
                {
                    if(!connectedScenes.Contains(scene) && scene != this)
                    {
                        scene.UnloadScene();
                    }

                    if(!connectedScenes.Contains(prevScene))
                        prevScene.UnloadScene();
                }
            }
        }
    }

    //Dynamically loads different Scene in additive format
    public void LoadScene()
    {
        if(!IsLoaded)
        {
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnloadScene()
    {
        if(IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    } 

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();

        return savableEntities;
    }
}
