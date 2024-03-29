﻿/*
	@author - SamirAli Mukhi
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDB : MonoBehaviour
{
    static Dictionary<string, MoveBase> moves;
    // New Moves Initiator
    public static void Init()
    {
        moves = new Dictionary<string, MoveBase>();

        var moveArray = Resources.LoadAll<MoveBase>("");
        foreach(var move in moveArray)
        {
            if(moves.ContainsKey(move.Name))
            {
                Debug.LogError($"There are 2 moves with name {move.Name}");
                continue;
            }
            moves[move.Name] = move;
        }
    }
    // Get Moves Check Move in Database
    public static MoveBase GetMoveByName(string name)
    {
        if(!moves.ContainsKey(name))
        {
            Debug.LogError($"Move with name {name} not found in the database");
            return null;
        }

        return moves[name];
    }
}
