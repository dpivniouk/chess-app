using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    public string column;
    public int row;
    public GameObject pieceOnSpace = null;

    public GameObject enPassantPiece = null;
}
