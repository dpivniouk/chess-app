using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FENRecord")]
public class FENRecordData : ScriptableObject
{
    public string[] ranks = new string[8];

    public string activeColor;

    public string castling;

    public string enPassant;

    public string halfmoveClock;

    public string fullmoveClock;

    public GameMode gameMode;

    public bool hasTurnLimit;
}
