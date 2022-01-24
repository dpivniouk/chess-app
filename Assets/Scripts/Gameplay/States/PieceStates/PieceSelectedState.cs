using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PieceSelectedState : PieceBaseState
{
    public override void OnMouseDown(PieceController piece)
    {
    }

    public override void OnMouseEnter(PieceController piece)
    {
    }

    public override void OnMouseExit(PieceController piece)
    {
    }

    public override void OnStateEnter(PieceController piece)
    {
        GameManager.instance.selectedObject = piece.gameObject;
        piece.Lift();
        piece.ClearMovementGrid();
        piece.BuildMovementGrid();
    }

    public override void Update(PieceController piece)
    {
        if (Input.GetMouseButtonDown(0) && GameManager.instance.enableInteraction && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if(hit.collider.gameObject.TryGetComponent(out PieceController pieceController))
                {
                    if(pieceController.currentState == pieceController.CaptureTargetState)
                    {
                        piece.Move(hit.collider.transform.position);
                        GameManager.instance.movedPiece = piece.gameObject;

                        BoardSpace targetSpace;
                        int xPos = 0;
                        int yPos = 0;

                        for (int i = 0; i < GameManager.instance.boardSpaces.Length; i++)
                        {
                            for (int j = 0; j < GameManager.instance.boardSpaces[i].Length; j++)
                            {
                                targetSpace = GameManager.instance.boardSpaces[i][j].GetComponentInChildren<BoardSpace>();
                                if (targetSpace.pieceOnSpace == pieceController.gameObject)
                                {
                                    targetSpace.pieceOnSpace = piece.gameObject;
                                    xPos = j;
                                    yPos = i;
                                    break;
                                }
                            }
                        }

                        Vector2Int targetPosition = new Vector2Int(xPos, yPos);
                        GameManager.instance.finishingSpace = targetPosition;

                        hit.collider.gameObject.GetComponentInChildren<PieceController>().CapturePiece(targetPosition);

                    }
                }
                else if (hit.collider.gameObject.TryGetComponent(out BoardSpaceExtensions boardSpaceExtensions))
                {
                    if(boardSpaceExtensions.currentState == boardSpaceExtensions.MovementTargetState)
                    {
                        piece.Move(hit.collider.transform.position);
                        GameManager.instance.movedPiece = piece.gameObject;

                        BoardSpace targetSpace = hit.collider.gameObject.GetComponent<BoardSpace>();

                        for (int i = 0; i < GameManager.instance.boardSpaces.Length; i++)
                        {
                            for(int j = 0; j < GameManager.instance.boardSpaces[i].Length; j++)
                            {
                                if(GameManager.instance.boardSpaces[i][j] == targetSpace.gameObject)
                                {
                                    Vector2Int targetPosition = new Vector2Int(j, i);
                                    GameManager.instance.finishingSpace = targetPosition;
                                    break;
                                }

                            }
                        }

                        targetSpace.pieceOnSpace = piece.gameObject;

                        if ((piece.pieceType == PieceType.WhitePawn || piece.pieceType == PieceType.BlackPawn) && Mathf.Abs(GameManager.instance.finishingSpace.y - GameManager.instance.startingSpace.y) > 1)
                        {
                            int enPassantCoordinate = (GameManager.instance.finishingSpace.y + GameManager.instance.startingSpace.y) / 2;
                            int xCoordinate = GameManager.instance.startingSpace.x;

                            BoardSpace enPassantSpace = GameManager.instance.boardSpaces[enPassantCoordinate][xCoordinate].GetComponentInChildren<BoardSpace>();

                            enPassantSpace.enPassantPiece = piece.gameObject;
                            GameManager.instance.availableEnPassantSpaces.Add(enPassantSpace, 1);
                        }
                    }
                    else if(boardSpaceExtensions.currentState == boardSpaceExtensions.CaptureTargetState)
                    {
                        piece.Move(hit.collider.transform.position);
                        GameManager.instance.movedPiece = piece.gameObject;

                        BoardSpace targetSpace = hit.collider.gameObject.GetComponentInChildren<BoardSpace>();

                        PieceController targetPieceController;
                        if(targetSpace.enPassantPiece != null)
                        {
                            targetPieceController = targetSpace.enPassantPiece.GetComponentInChildren<PieceController>();
                        }
                        else
                        {
                            targetPieceController = targetSpace.pieceOnSpace.GetComponentInChildren<PieceController>();
                        }

                        int xPos = 0;
                        int yPos = 0;

                        for (int i = 0; i < GameManager.instance.boardSpaces.Length; i++)
                        {
                            for (int j = 0; j < GameManager.instance.boardSpaces[i].Length; j++)
                            {
                                if (GameManager.instance.boardSpaces[i][j] == targetSpace.gameObject)
                                {
                                    targetSpace.pieceOnSpace = piece.gameObject;
                                    xPos = j;
                                    yPos = i;
                                    break;
                                }

                            }
                        }

                        Vector2Int targetPosition = new Vector2Int(xPos,yPos);
                        GameManager.instance.finishingSpace = targetPosition;

                        targetPieceController.CapturePiece(targetPosition);

                    }
                    else if(boardSpaceExtensions.currentState == boardSpaceExtensions.CastleTargetState)
                    {
                        BoardSpace targetSpace = hit.collider.gameObject.GetComponent<BoardSpace>();
                        Vector2Int targetPosition = new Vector2Int(0, 0);

                        for (int i = 0; i < GameManager.instance.boardSpaces.Length; i++)
                        {
                            for (int j = 0; j < GameManager.instance.boardSpaces[i].Length; j++)
                            {
                                if (GameManager.instance.boardSpaces[i][j] == targetSpace.gameObject)
                                {
                                    targetPosition = new Vector2Int(j, i);
                                    GameManager.instance.finishingSpace = targetPosition;
                                    break;
                                }
                            }
                        }

                        (GameObject, Vector2Int) partnerPieceTarget = piece.castlingPartners[targetPosition];
                        PieceController partnerPieceController = partnerPieceTarget.Item1.GetComponentInChildren<PieceController>();
                        BoardSpace partnerTargetSpace = GameManager.instance.boardSpaces[partnerPieceTarget.Item2.y][partnerPieceTarget.Item2.x].GetComponentInChildren<BoardSpace>();

                        BoardSpace[] allSpaces = UnityEngine.Object.FindObjectsOfType<BoardSpace>();

                        foreach (BoardSpace space in allSpaces)
                        {
                            if (space.pieceOnSpace == partnerPieceController.gameObject)
                            {
                                space.pieceOnSpace = null;
                                break;
                            }
                        }

                        partnerPieceController.transform.position = partnerTargetSpace.transform.position;
                        partnerPieceController.hasMoved = true;
                        partnerTargetSpace.pieceOnSpace = partnerPieceController.gameObject;

                        if(partnerPieceTarget.Item2.x == 5)
                        {
                            GameManager.instance.castleSide = "K";
                        }
                        else
                        {
                            GameManager.instance.castleSide = "Q";
                        }

                        piece.Move(hit.collider.transform.position);
                        GameManager.instance.movedPiece = piece.gameObject;
                        targetSpace.pieceOnSpace = piece.gameObject;

                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1) && GameManager.instance.enableInteraction)
        {
            piece.SetDown();
        }
    }
}
