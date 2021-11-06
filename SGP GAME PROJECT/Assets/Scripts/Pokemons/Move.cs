/*
    @author : SamirAli Mukhi
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move 
{
    public MoveBase Base {get; set;}
    
    public int PP {get; set;}
    
    public Move(MoveBase pBase)
    {
       Base = pBase;
       PP = pBase.PP;
    }

    public Move(MoveSaveData saveData)
    {
        Base = MoveDB.GetMoveByName(saveData.name);
        PP = saveData.pp;
    }

    public float CountMoveAccuracy(int sourceAccuracy, int targetEvasion)
    {
        float moveAccuracy = Base.Accuracy;

		var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f , 7f / 3f, 8f / 3f, 3f};
		//For Accuracy
		if(sourceAccuracy > 0)
		{
			moveAccuracy *= boostValues[sourceAccuracy];
		}
		else 
			moveAccuracy /= boostValues[-sourceAccuracy];

		//For Evasion
		if(targetEvasion > 0)
		{
			moveAccuracy /= boostValues[targetEvasion];
		}
		else 
			moveAccuracy *= boostValues[-targetEvasion];	

        return moveAccuracy;		
    }

    public void RestorePP(int amount)
    {
        PP += amount;
        if(PP > Base.PP)
            PP = Base.PP;
    }

    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData()
        {
            name = Base.Name,
            pp = PP
        };

        return saveData;
    }
}

[System.Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
}