﻿/*
	@author - Taufik Mansuri
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grassLayer;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask fovLayer;
    [SerializeField] LayerMask portalLayer; 
public static GameLayers i { get; set;}
    private void Awake()
    {
        i = this;
    }
    
    // All Game Layers Declaration  
    public LayerMask SolidLayer
    {
        get => solidObjectsLayer;
    }

    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }

    public LayerMask GrassLayer{
        get => grassLayer;
    }

    public LayerMask PlayerLayer{
        get => playerLayer;
    }

    public LayerMask FovLayer{
        get => fovLayer;
    }

    public LayerMask PortalLayer{
        get => portalLayer;
    }

    public LayerMask TrigerrableLayers{
        get => grassLayer | fovLayer | portalLayer;
    }
}
