using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromotionButton : MonoBehaviour
{
    public GameObject whitePromotionPrefab;
    public GameObject blackPromotionPrefab;

    public void Promote()
    {
        PieceController promotingPawnController = GameManager.instance.movedPiece.GetComponentInChildren<PieceController>();

        GameObject promotionPrefab;

        if(promotingPawnController.pieceColor == "w")
        {
            promotionPrefab = whitePromotionPrefab;
        }
        else
        {
            promotionPrefab = blackPromotionPrefab;
        }

        Vector2Int promotionCoords = GameManager.instance.finishingSpace;
        BoardSpace promotionSpace = GameManager.instance.boardSpaces[promotionCoords.y][promotionCoords.x].GetComponentInChildren<BoardSpace>();

        GameObject newPiece = Instantiate(promotionPrefab, promotionSpace.transform.position, promotionPrefab.transform.rotation);
        PieceController newPieceController = newPiece.GetComponentInChildren<PieceController>();
        newPieceController.hasMoved = true;
        string moveString = GameManager.instance.moveText.text;
        moveString = moveString.Trim() + GameManager.instance.pieceNotation[newPieceController.pieceType] + " ";
        GameManager.instance.moveText.text = moveString;

        promotionSpace.pieceOnSpace = newPiece;

        Destroy(promotingPawnController.gameObject);

        GameManager.instance.choosingPromotedUnit = false;
        GameManager.instance.pawnPromotionPanel.SetActive(false);
    }
}
