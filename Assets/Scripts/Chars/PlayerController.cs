using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour, ISavable
{
    
    Vector2 input;
    [SerializeField] Character character;
    public Character Character {  get { return character; } }
    public string trainerName;
    public Sprite trainerPortrait;
    [SerializeField] MonsterParty monsterParty;
    [SerializeField] Inventory inventory;
    Vector3 prevPos;

    public MonsterParty MonsterParty { get { return monsterParty; } }
    public Inventory Inventory { get { return inventory; } }
    
    public void HandleUpdate()
    {
        if(!character.isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input != Vector2.zero)
            {
                prevPos = transform.position;
                StartCoroutine(character.Move(input, CheckForTriggers));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Space))
        { Interact(); }
    }

    void Interact()
    {
        var facingDir = new Vector3(character.animator.moveX, character.animator.moveY, 0);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.2f, GameLayers.i.InteractableLayer);
        if ( collider != null && GameController.Instance.state == GameState.FreeRoaming)
        {
            collider.GetComponent<Interactables>()?.Interact(transform);
        }
    }

    private void CheckForTriggers()
    {
        if (prevPos != transform.position) 
        {
            var colliders = Physics2D.OverlapCircleAll(transform.position, 0.2f, GameLayers.i.TriggableLayers);

            foreach (var collider in colliders)
            {
                var triggereable = collider.GetComponent<PlayerTrigger>();
                if (triggereable != null)
                {
                    triggereable.OnPlayerTrigger(this);
                }
            }
        }
    }


    public void StopMovement(Transform targetTransform = null)
    { 
        character.animator.isMoving = false;
        input = Vector2.zero;

        if (targetTransform != null) 
        { character.LookTowards(targetTransform.position); }
    }

    public object CaptureState() 
    {
        var playerSaveData = new PlayerSaveData()
        { 
            position = new float[] { transform.position.x, transform.position.y }, 
            playerMonsters = monsterParty.Party.Select(m => m.GetSaveData()).ToList()
        };

        return playerSaveData;
    }

    public void RestoreState(object state) 
    {
        var saveData = (PlayerSaveData)state;
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);
        StopMovement();

        monsterParty.Party = saveData.playerMonsters.Select(m => new Monster(m)).ToList();
    }
}

[Serializable]
class PlayerSaveData
{
    public float[] position;
    public List<MonsterSaveData> playerMonsters;
}
