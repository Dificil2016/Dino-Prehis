using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialsPrefab;
    [SerializeField] Vector2 SpawnPos = Vector2.zero;

    private void Awake()
    {
       var existingEssentials = FindObjectsOfType<Essentials>();
        if (existingEssentials.Length <= 0) 
        {

            Instantiate(essentialsPrefab, new Vector3(SpawnPos.x ,SpawnPos.y ,0), Quaternion.identity);
        }
    }
}
