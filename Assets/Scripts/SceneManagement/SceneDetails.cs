using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> conectedScenes;

    [SerializeField] bool isLoaded;

    List<SavableEntity> savableEntities;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            LoadScene(); 
            GameController.Instance.SetCurrentScene(this);

            foreach (var scene in conectedScenes)
            { scene.LoadScene(); }

            var prevScene = GameController.Instance.prevScene;
            if (prevScene != null)
            {
                var prevLoadScenes = prevScene.conectedScenes;
                foreach (var scene in prevScene.conectedScenes)
                {
                    if (!conectedScenes.Contains(scene) && scene != this)
                    { scene.UnloadScene(); }
                }
                if (!conectedScenes.Contains(prevScene))
                { prevScene.UnloadScene(); }
            }
            GameController.Instance.SetMapArea();
        }
    }

    public void LoadScene()
    {
        if (!isLoaded)
        {
            var Operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            isLoaded = true;

            Operation.completed += (AsyncOperation op) => 
            {
                savableEntities = GetSavableEntities();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
        else { savableEntities = GetSavableEntities(); }
    }

    public void UnloadScene()
    {
        if (isLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);
            SceneManager.UnloadSceneAsync(gameObject.name);
            isLoaded = false;
        }
    }

    List<SavableEntity> GetSavableEntities()
    {
        var currentScene = SceneManager.GetSceneByName(gameObject.name);
        return FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currentScene).ToList();
    }
}
