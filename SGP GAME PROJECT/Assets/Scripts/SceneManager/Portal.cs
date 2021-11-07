/*
    @author : Mitren Kadiwala
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifier destinationPortal ;
    [SerializeField] Transform spawnPoint;

    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        StartCoroutine(SwitchScene());
    }
    Fader fader;
    private void Start() 
    {
        fader = FindObjectOfType<Fader>();    
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);
        //To fade the screen on scene switching
        yield return fader.FadeIn(0.5f);
        yield return SceneManager.LoadSceneAsync(sceneToLoad);
        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);//To prevent this portal to cause problem while specifying the location of portal in other scen
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        //To fade out the screen on scene loading
        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
        Destroy(gameObject);       
    }
    public Transform SpawnPoint => spawnPoint;
}
public enum DestinationIdentifier { A,B,C,D,E }





