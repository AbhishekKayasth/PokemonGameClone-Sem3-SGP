/*
	@author - SamirAli Mukhi
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
// For Diaogues
public class Dialogue 
{
    [SerializeField] List<string> lines;

    public List<string> Lines 
    {
        get { return lines; }
    }
}
