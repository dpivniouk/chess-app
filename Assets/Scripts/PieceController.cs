using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    WhitePawn,
    BlackPawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}

public class PieceController : MonoBehaviour
{
    public string pieceColor;
    public PieceType pieceType;

    private BoxCollider boxCollider;
    private Rigidbody rb;

    //Movement setup//
    public GameObject targetObject;
    public GameObject movementTarget;

    public List<Vector2Int> allowedMovementCoordinates = new List<Vector2Int>();
    public List<Vector2Int> allowedCaptureCoordinates = new List<Vector2Int>();

    public List<Vector2Int> allowedCastleCoordinates = new List<Vector2Int>();
    public Dictionary<Vector2Int, (GameObject, Vector2Int)> castlingPartners = new Dictionary<Vector2Int, (GameObject, Vector2Int)>();

    public PieceBaseState currentState;
    public List<PieceBaseState> stateQueue = new List<PieceBaseState>();
    public readonly PieceIdleState IdleState = new PieceIdleState();
    public readonly PieceSelectedState SelectedState = new PieceSelectedState();
    public readonly PieceCaptureTargetState CaptureTargetState = new PieceCaptureTargetState();

    public bool hasMoved;

    private Dictionary<PieceType, Vector2Int[]> movementVectors = new Dictionary<PieceType, Vector2Int[]>
    {
        {PieceType.WhitePawn,new Vector2Int[]{new Vector2Int(0,1) } },
        {PieceType.BlackPawn,new Vector2Int[]{new Vector2Int(0,-1) } },
        {PieceType.Knight,new Vector2Int[]{new Vector2Int(-2,-1), new Vector2Int(-2, 1), new Vector2Int(-1, -2), new Vector2Int(-1, 2), new Vector2Int(1, -2), new Vector2Int(1, 2), new Vector2Int(2, -1), new Vector2Int(2, 1) } },
        {PieceType.Bishop,new Vector2Int[]{new Vector2Int(-1,-1), new Vector2Int(-1, 1) , new Vector2Int(1, -1) , new Vector2Int(1, 1) } },
        {PieceType.Rook,new Vector2Int[]{new Vector2Int(-1,0), new Vector2Int(0, 1) , new Vector2Int(1, 0) , new Vector2Int(0, -1) } },
        {PieceType.Queen,new Vector2Int[]{new Vector2Int(-1,0), new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1), new Vector2Int(-1, -1) } },
        {PieceType.King,new Vector2Int[]{new Vector2Int(-1,0), new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1), new Vector2Int(-1, -1) } },
    };
    private Dictionary<PieceType, Vector2Int[]> captureVectors = new Dictionary<PieceType, Vector2Int[]>
    {
        {PieceType.WhitePawn,new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(1, 1) } },
        {PieceType.BlackPawn,new Vector2Int[] { new Vector2Int(-1, -1), new Vector2Int(1, -1) } }
    };

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>(); ;

        TransitionToState(IdleState);
    }

    void Update()
    {
        if (GameManager.instance.activeColor != pieceColor || GameManager.instance.choosingPromotedUnit) return;

        currentState.Update(this);
    }

    public void TransitionToState(PieceBaseState state)
    {
        currentState = state;
        currentState.OnStateEnter(this);
    }

    void OnMouseEnter()
    {
        if (GameManager.instance.choosingPromotedUnit) return;

        currentState.OnMouseEnter(this);
    }

    void OnMouseExit()
    {
        if (GameManager.instance.choosingPromotedUnit) return;

        currentState.OnMouseExit(this);
    }

    void OnMouseDown()
    {
        if (GameManager.instance.activeColor != pieceColor || GameManager.instance.choosingPromotedUnit) return;

        currentState.OnMouseDown(this);
    }

    public void InitiatePositionCheck()
    {
        BoardSpace currentSpace;
        int xPos = 0;
        int yPos = 0;

        for (int i = 0; i < GameManager.instance.boardSpaces.Length; i++)
        {
            for(int j = 0; j < GameManager.instance.boardSpaces[i].Length; j++)
            {
                currentSpace = GameManager.instance.boardSpaces[i][j].GetComponentInChildren<BoardSpace>();
                if(currentSpace.pieceOnSpace == this.gameObject)
                {
                    xPos = j;
                    yPos = i;
                    break;
                }
            }
        }

        Vector2Int initialPosition = new Vector2Int(xPos, yPos);

        CheckAllowedMovement(initialPosition);
    }

    private void CheckAllowedMovement(Vector2Int initialSpace)
    {
        Vector2Int[] availableMovementVectors = movementVectors[pieceType];

        if(captureVectors.ContainsKey(pieceType))
        {
            Vector2Int[] availableCaptureVectors = captureVectors[pieceType];
            for(int i = 0; i < availableCaptureVectors.Length; i++)
            {
                Vector2Int spaceToCheck = initialSpace + availableCaptureVectors[i];

                if (spaceToCheck.x >= 0 && spaceToCheck.x < 8 && spaceToCheck.y >= 0 && spaceToCheck.y < 8)
                {
                    BoardSpace boardSpaceToCheck = GameManager.instance.boardSpaces[spaceToCheck.y][spaceToCheck.x].GetComponentInChildren<BoardSpace>();
                    if (boardSpaceToCheck.pieceOnSpace != null)
                    {
                        PieceController targetPiece = boardSpaceToCheck.pieceOnSpace.GetComponent<PieceController>();

                        if (targetPiece.pieceColor != this.pieceColor)
                        {
                            allowedCaptureCoordinates.Add(spaceToCheck);
                            if (targetPiece.pieceType == PieceType.King && targetPiece.pieceColor == GameManager.instance.activeColor)
                            {
                                GameManager.instance.isActiveKingInCheck = true;
                            }
                        }
                    }
                    else if(boardSpaceToCheck.enPassantPiece != null)
                    {
                        PieceController targetPiece = boardSpaceToCheck.enPassantPiece.GetComponent<PieceController>();

                        if (targetPiece.pieceColor != this.pieceColor)
                        {
                            allowedCaptureCoordinates.Add(spaceToCheck);
                            if (targetPiece.pieceType == PieceType.King && targetPiece.pieceColor == GameManager.instance.activeColor)
                            {
                                GameManager.instance.isActiveKingInCheck = true;
                            }
                        }

                    }
                }
            }
        }

        if (pieceType == PieceType.King && !GameManager.instance.isActiveKingInCheck)
        {
            string[] possibleCastlingDirections = new string[] { "k", "q" };
            if (pieceColor == "w")
            {
                for (int i = 0; i < possibleCastlingDirections.Length; i++)
                {
                    possibleCastlingDirections[i] = possibleCastlingDirections[i].ToUpper();
                }
            }

            foreach (string direction in possibleCastlingDirections)
            {
                if (GameManager.instance.availableCastlingVectors.ContainsKey(direction))
                {
                    Vector2Int spaceToCheck = initialSpace;
                    bool checkNextSpace = true;

                    Vector2Int currentCastlingVector = GameManager.instance.availableCastlingVectors[direction];

                    while (checkNextSpace)
                    {
                        spaceToCheck = spaceToCheck + currentCastlingVector;
                        if (spaceToCheck.x >= 0 && spaceToCheck.x < 8 && spaceToCheck.y >= 0 && spaceToCheck.y < 8)
                        {
                            BoardSpace boardSpaceToCheck = GameManager.instance.boardSpaces[spaceToCheck.y][spaceToCheck.x].GetComponentInChildren<BoardSpace>();
                            if (boardSpaceToCheck.pieceOnSpace == null)
                            {
                                PieceController[] allPieces = FindObjectsOfType<PieceController>();

                                foreach (PieceController piece in allPieces)
                                {
                                    if (piece.allowedMovementCoordinates.Contains(spaceToCheck) && piece.pieceColor != pieceColor)
                                    {
                                        checkNextSpace = false;
                                        break;
                                    }
                                }
                            }
                            else if (boardSpaceToCheck.pieceOnSpace.GetComponentInChildren<PieceController>().pieceType == PieceType.Rook)
                            {
                                allowedCastleCoordinates.Add(initialSpace+2*currentCastlingVector);
                                (GameObject, Vector2Int) newCastlePartner = (boardSpaceToCheck.pieceOnSpace, initialSpace + currentCastlingVector);

                                castlingPartners.Add(initialSpace + 2 * currentCastlingVector, newCastlePartner);

                                checkNextSpace = false;
                            }
                            else
                            {
                                checkNextSpace = false;
                            }
                        }
                    }
                }
            }

        }

        for (int i = 0; i < availableMovementVectors.Length; i++)
        {
            Vector2Int spaceToCheck = initialSpace;
            bool checkNextSpace = true;

            while (checkNextSpace)
            {
                spaceToCheck = spaceToCheck + availableMovementVectors[i];

                if (spaceToCheck.x>=0 && spaceToCheck.x < 8 && spaceToCheck.y >= 0 && spaceToCheck.y < 8)
                {
                    BoardSpace boardSpaceToCheck = GameManager.instance.boardSpaces[spaceToCheck.y][spaceToCheck.x].GetComponentInChildren<BoardSpace>();
                    if (boardSpaceToCheck.pieceOnSpace == null)
                    {
                        allowedMovementCoordinates.Add(spaceToCheck);
                        if (!hasMoved && (pieceType == PieceType.WhitePawn || pieceType == PieceType.BlackPawn))
                        {
                            spaceToCheck = spaceToCheck + availableMovementVectors[i];

                            if (spaceToCheck.x >= 0 && spaceToCheck.x < 8 && spaceToCheck.y >= 0 && spaceToCheck.y < 8)
                            {
                                boardSpaceToCheck = GameManager.instance.boardSpaces[spaceToCheck.y][spaceToCheck.x].GetComponentInChildren<BoardSpace>();
                                if (boardSpaceToCheck.pieceOnSpace == null)
                                {
                                    allowedMovementCoordinates.Add(spaceToCheck);
                                }
                                checkNextSpace = false;
                            }
                        }
                        else if (pieceType == PieceType.King || pieceType == PieceType.Knight || pieceType == PieceType.WhitePawn || pieceType == PieceType.BlackPawn)
                        {
                            checkNextSpace = false;
                        }
                    }
                    else
                    {
                        PieceController targetPiece = boardSpaceToCheck.pieceOnSpace.GetComponent<PieceController>();
                        if (targetPiece.pieceColor == this.pieceColor)
                        {
                        }
                        else if(pieceType != PieceType.WhitePawn && pieceType != PieceType.BlackPawn)
                        {
                            allowedCaptureCoordinates.Add(spaceToCheck);
                            if(targetPiece.pieceType == PieceType.King && targetPiece.pieceColor == GameManager.instance.activeColor)
                            {
                                GameManager.instance.isActiveKingInCheck = true;
                            }
                        }
                        checkNextSpace = false;

                    }
                }
                else
                {
                    checkNextSpace = false;
                }
            }
        }
    }

    public void BuildMovementGrid()
    {
        List<GameObject> allowedMovementSpaces = new List<GameObject>();
        List<GameObject> allowedCaptureSpaces = new List<GameObject>();
        List<GameObject> allowedCastleSpaces = new List<GameObject>();

        foreach (Vector2Int spaceCoordinate in allowedMovementCoordinates)
        {
            allowedMovementSpaces.Add(GameManager.instance.boardSpaces[spaceCoordinate.y][spaceCoordinate.x]);
        }
        foreach (Vector2Int spaceCoordinate in allowedCaptureCoordinates)
        {
            allowedCaptureSpaces.Add(GameManager.instance.boardSpaces[spaceCoordinate.y][spaceCoordinate.x]);
        }
        foreach(Vector2Int spaceCoordinate in allowedCastleCoordinates)
        {
            allowedCastleSpaces.Add(GameManager.instance.boardSpaces[spaceCoordinate.y][spaceCoordinate.x]);
        }

        foreach (GameObject boardSpace in allowedMovementSpaces)
        {
            BoardSpaceExtensions boardSpaceExtensions = boardSpace.GetComponentInChildren<BoardSpaceExtensions>();
            boardSpaceExtensions.TransitionToState(boardSpaceExtensions.MovementTargetState);
        }
        foreach (GameObject boardSpace in allowedCaptureSpaces)
        {
            BoardSpaceExtensions space = boardSpace.GetComponentInChildren<BoardSpaceExtensions>();
            space.TransitionToState(space.CaptureTargetState);
            if (boardSpace.GetComponentInChildren<BoardSpace>().pieceOnSpace != null)
            {
                PieceController pieceController = boardSpace.GetComponentInChildren<BoardSpace>().pieceOnSpace.GetComponentInChildren<PieceController>();
                pieceController.TransitionToState(pieceController.CaptureTargetState);

            }
        }
        foreach (GameObject boardSpace in allowedCastleSpaces)
        {
            BoardSpaceExtensions boardSpaceExtensions = boardSpace.GetComponentInChildren<BoardSpaceExtensions>();
            boardSpaceExtensions.TransitionToState(boardSpaceExtensions.CastleTargetState);
        }
    }

    public void ClearMovementGrid()
    {
        List<GameObject> allowedMovementSpaces = new List<GameObject>();
        List<GameObject> allowedCaptureSpaces = new List<GameObject>();
        List<GameObject> allowedCastleSpaces = new List<GameObject>();

        foreach (Vector2Int spaceCoordinate in allowedMovementCoordinates)
        {
            allowedMovementSpaces.Add(GameManager.instance.boardSpaces[spaceCoordinate.y][spaceCoordinate.x]);
        }
        foreach (Vector2Int spaceCoordinate in allowedCaptureCoordinates)
        {
            allowedCaptureSpaces.Add(GameManager.instance.boardSpaces[spaceCoordinate.y][spaceCoordinate.x]);
        }
        foreach(Vector2Int spaceCoordinate in allowedCastleCoordinates)
        {
            allowedCastleSpaces.Add(GameManager.instance.boardSpaces[spaceCoordinate.y][spaceCoordinate.x]);
        }

        foreach (GameObject boardSpace in allowedMovementSpaces)
        {
            BoardSpaceExtensions boardSpaceExtensions = boardSpace.GetComponentInChildren<BoardSpaceExtensions>();
            boardSpaceExtensions.TransitionToState(boardSpaceExtensions.IdleState);
        }
        foreach (GameObject boardSpace in allowedCaptureSpaces)
        {
            BoardSpaceExtensions space = boardSpace.GetComponentInChildren<BoardSpaceExtensions>();
            space.TransitionToState(space.IdleState);
            PieceController pieceController;
            if (boardSpace.GetComponentInChildren<BoardSpace>().enPassantPiece != null)
            {
                pieceController = boardSpace.GetComponentInChildren<BoardSpace>().enPassantPiece.GetComponentInChildren<PieceController>();
            }
            else
            {
                pieceController = boardSpace.GetComponentInChildren<BoardSpace>().pieceOnSpace.GetComponentInChildren<PieceController>();

            }
            pieceController.TransitionToState(pieceController.IdleState);
        }
        foreach (GameObject boardSpace in allowedCastleSpaces)
        {
            BoardSpaceExtensions boardSpaceExtensions = boardSpace.GetComponentInChildren<BoardSpaceExtensions>();
            boardSpaceExtensions.TransitionToState(boardSpaceExtensions.IdleState);
        }
    }

    public void Lift()
    {
        StartCoroutine(LiftPiece());
    }

    private IEnumerator LiftPiece()
    {
        GameManager.instance.enableInteraction = false;
        Vector3 startingPosition = transform.position;

        float timer = 0f;
        float timeToFinish = .1f;

        while(transform.position.y < 1f)
        {
            transform.position = Vector3.Lerp(startingPosition, new Vector3(transform.position.x, 1f, transform.position.z), timer / timeToFinish);

            timer += Time.deltaTime;
            yield return null;
        }

        GameManager.instance.enableInteraction = true;

    }

    public void SetDown(bool turnTaken = false)
    {
        StartCoroutine(SetDownPiece(turnTaken));
    }

    private IEnumerator SetDownPiece(bool turnTaken=false)
    {
        GameManager.instance.enableInteraction = false;
        Vector3 startingPosition = transform.position;

        float timer = 0f;
        float timeToFinish = .1f;

        while (transform.position.y > 0f)
        {
            transform.position = Vector3.Lerp(startingPosition, new Vector3(transform.position.x, 0f, transform.position.z), timer / timeToFinish);

            timer += Time.deltaTime;
            yield return null;

        }

        GameManager.instance.enableInteraction = true;
        ClearMovementGrid();
        TransitionToState(IdleState);

        if (turnTaken)
        {
            GameManager.instance.CheckValidMove();

        }
    }

    public void Move(Vector3 targetPosition)
    {
        BoardSpace currentSpace;

        for (int i = 0; i < GameManager.instance.boardSpaces.Length; i++)
        {
            for (int j = 0; j < GameManager.instance.boardSpaces[i].Length; j++)
            {
                currentSpace = GameManager.instance.boardSpaces[i][j].GetComponentInChildren<BoardSpace>();
                if (currentSpace.pieceOnSpace == this.gameObject)
                {
                    int xPos = j;
                    int yPos = i;

                    Vector2Int initialPosition = new Vector2Int(xPos, yPos);
                    GameManager.instance.startingSpace = initialPosition;

                    currentSpace.pieceOnSpace = null;
                    break;
                }
            }
        }

        ClearMovementGrid();
        StartCoroutine(MovePiece(targetPosition));
    }

    private IEnumerator MovePiece(Vector3 targetPosition)
    {
        GameManager.instance.enableInteraction = false;
        Vector3 startingPosition = transform.position;
        Vector3 endingPosition = new Vector3(targetPosition.x, 1f, targetPosition.z);
        float timer = 0f;
        float timeToFinish = .5f;

        while (Vector3.Magnitude(endingPosition - transform.position) > 0)
        {
            transform.position = Vector3.Lerp(startingPosition, endingPosition, timer / timeToFinish);

            timer += Time.deltaTime;
            yield return null;
        }

        SetDown(true);
    }

    public void CapturePiece(Vector2Int currentPosition)
    {
        GameManager.instance.capturedPiece = this.gameObject;
        PieceController[] allPieces = FindObjectsOfType<PieceController>();

        foreach(PieceController piece in allPieces)
        {
            if (piece.allowedCaptureCoordinates.Contains(currentPosition))
            {
                piece.allowedCaptureCoordinates.Remove(currentPosition);
            }
        }

        this.gameObject.SetActive(false);
    }

    public void CheckAvailableMoves()
    {
        allowedMovementCoordinates.Clear();
        allowedCaptureCoordinates.Clear();
        allowedCastleCoordinates.Clear();
        castlingPartners.Clear();

        InitiatePositionCheck();
    }

}
