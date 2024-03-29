﻿/*
    @author : SamirAli Mukhi
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;

    public event Action<int> onMenuSelected;
    public event Action onBack;
    
    List<Text> menuItems;

    int selectedItem = 0;

    private void Awake()
    {
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
    }
    // For Game Menu
    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMenu()
    {
        menu.gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        int prevSelection = selectedItem;

        if(Input.GetKeyDown(KeyCode.DownArrow))
            ++selectedItem;
        else if(Input.GetKeyDown(KeyCode.UpArrow))  
            --selectedItem;

        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count - 1);

        if(prevSelection != selectedItem)
            UpdateItemSelection();  

        if(Input.GetKeyDown(KeyCode.Z))
        {
            onMenuSelected?.Invoke(selectedItem);
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            CloseMenu();
            onBack?.Invoke();
        }
    }

    void UpdateItemSelection()
    {
        for(int i = 0; i < menuItems.Count; i++)
        {
            if(i == selectedItem)
                menuItems[i].color = GlobalSettings.i.HighlightedColor;
            else
                menuItems[i].color = Color.black;
        }
    }
}
