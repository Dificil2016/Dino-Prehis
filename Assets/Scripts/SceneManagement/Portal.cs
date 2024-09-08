using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, PlayerTrigger
{
    [SerializeField] PlayerController player;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Fader fader;
    [SerializeField] int destinationPortal;
    bool outTransition;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
        outTransition = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
        {
            outTransition = false;
            StartCoroutine(FadeIn());
        }
    }

    IEnumerator FadeIn()
    {
        yield return fader.FadeIn(0.4f);
        outTransition = true;
    }

    public void OnPlayerTrigger(PlayerController player)
    {
        this.player = player;
        player.StopMovement();
        StartCoroutine(SwitchScene());
    }

    IEnumerator SwitchScene()
    {
        GameController.Instance.PauseGame(true);
        yield return new WaitUntil(() => outTransition);

        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);

        player.Character.SetPositionAndSnap(destPortal.spawnPoint.transform.position);

        yield return fader.FadeOut(0.4f);
        GameController.Instance.PauseGame(false);
    }
}
