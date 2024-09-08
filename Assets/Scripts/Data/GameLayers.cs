using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour 
{
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask wallsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask triggerLayer;
    [SerializeField] LayerMask portalLayer;

    public static GameLayers i;
    private void Awake()
    {
        i = this;
    }

    public LayerMask GrassLayer {  get => grassLayer;  }
    public LayerMask WallLayer { get => wallsLayer;  }
    public LayerMask InteractableLayer { get => interactableLayer;  }
    public LayerMask PlayerLayer { get => playerLayer; }
    public LayerMask TriggerLayer { get => triggerLayer; }
    public LayerMask TriggableLayers { get => portalLayer | triggerLayer | grassLayer; }
}
