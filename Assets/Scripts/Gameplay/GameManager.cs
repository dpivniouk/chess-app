using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    //public GameObject interactionCanvas;

    public static GameManager instance;

    public GameObject[][] boardSpaces;
    public GameObject[] rankEight = new GameObject[8];
    public GameObject[] rankSeven = new GameObject[8];
    public GameObject[] rankSix = new GameObject[8];
    public GameObject[] rankFive = new GameObject[8];
    public GameObject[] rankFour = new GameObject[8];
    public GameObject[] rankThree = new GameObject[8];
    public GameObject[] rankTwo = new GameObject[8];
    public GameObject[] rankOne = new GameObject[8];

    public TextMeshProUGUI moveText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI colorToMoveText;

    //Pointers to UI elements
    //public GameObject unitPanel;

    //public GameObject unitInfo;

    //public GameObject alertPanel;
    public GameObject turnChangePanel;

    public GameObject selectedObject;

    private FENRecordData startingState;

    public string activeColor;
    public int halfmoveClock;
    private int fullmoveClock;

    private string[] boardStateRanks;

    private string enPassantLocation;
    private char[] castlingSides;
    

    //[HideInInspector]
    public bool enableInteraction = true;

    private Canvas mainCanvas;

    private PieceSet pieceSet;
    private Dictionary<char, GameObject> pieceIdentifiers;
    [HideInInspector]
    public Dictionary<PieceType, string> pieceNotation;

    public bool isActiveKingInCheck = false;

    public GameObject movedPiece;
    public GameObject capturedPiece;
    public Vector2Int startingSpace;
    public Vector2Int finishingSpace;
    public string castleSide = "";

    private GameMode currentGameMode;

    public Dictionary<string, Vector2Int> castlingVectors = new Dictionary<string, Vector2Int>
    {
        {"K", new Vector2Int(1,0) },
        {"Q", new Vector2Int(-1,0) },
        {"k", new Vector2Int(1,0) },
        {"q", new Vector2Int(-1,0) }
    };

    public Dictionary<string, Vector2Int> availableCastlingVectors = new Dictionary<string, Vector2Int>();

    public Dictionary<BoardSpace, int> availableEnPassantSpaces = new Dictionary<BoardSpace, int>();

    [HideInInspector]
    public bool choosingPromotedUnit = false;
    public GameObject pawnPromotionPanel;

    public GameObject menuPanel;

    public Button resetButton;

    public float mouseSensitivity = 2f;

    public GameObject endGamePanel;
    public TextMeshProUGUI gameWinnerText;
    public TMP_InputField gameSummaryText;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        pieceSet = DataManager.instance.selectedPieceSet;

        boardSpaces = new GameObject[8][] { rankOne, rankTwo, rankThree, rankFour, rankFive, rankSix, rankSeven, rankEight };

        pieceIdentifiers = new Dictionary<char, GameObject>
        {
            {'P',pieceSet.whitePawn },
            {'N',pieceSet.whiteKnight },
            {'B',pieceSet.whiteBishop },
            {'R',pieceSet.whiteRook },
            {'Q',pieceSet.whiteQueen },
            {'K',pieceSet.whiteKing },
            {'p',pieceSet.blackPawn },
            {'n',pieceSet.blackKnight },
            {'b',pieceSet.blackBishop },
            {'r',pieceSet.blackRook },
            {'q',pieceSet.blackQueen },
            {'k',pieceSet.blackKing }
        };

        pieceNotation = new Dictionary<PieceType, string>
        {
            {PieceType.WhitePawn,""},
            {PieceType.BlackPawn,"" },
            {PieceType.Bishop,"B" },
            {PieceType.Rook,"R" },
            {PieceType.Queen,"Q" },
            {PieceType.King,"K" },
            {PieceType.Knight,"N" }
        };

        currentGameMode = DataManager.instance.selectedGameMode;
        if(currentGameMode == GameMode.Puzzle)
        {
            startingState = DataManager.instance.selectedPuzzle;
        }
        else
        {
            startingState = DataManager.instance.standardGameMode;
        }

        mainCanvas = FindObjectOfType<Canvas>();

        InitGame();
	}

    void InitGame()
    {
        SetupBoard();

        activeColor = startingState.activeColor;
        halfmoveClock = int.Parse(startingState.halfmoveClock);
        fullmoveClock = int.Parse(startingState.fullmoveClock);

        castlingSides = startingState.castling.ToCharArray();

        foreach(char character in castlingSides)
        {
            if (castlingVectors.ContainsKey(character.ToString()))
            {
                availableCastlingVectors.Add(character.ToString(), castlingVectors[character.ToString()]);
            }
        }


        turnText.text = fullmoveClock.ToString();

        CheckAvailableMoves();
    }
	
    private void SetupBoard()
    {
        for(int i = 0; i < startingState.ranks.Length; i++)
        {
            char[] rankLayout = startingState.ranks[i].ToCharArray();
            int fileIndex = 0;
            for(int j = 0; j < rankLayout.Length; j++)
            {
                if (char.IsNumber(rankLayout[j]))
                {
                    fileIndex += (int)char.GetNumericValue(rankLayout[j]);
                }
                else
                {
                    GameObject targetSpace = boardSpaces[7-i][fileIndex];
                    Vector3 targetPosition = targetSpace.transform.position;
                    GameObject pieceToSpawn = pieceIdentifiers[rankLayout[j]];
                    GameObject newPiece = Instantiate(pieceToSpawn, targetPosition, pieceToSpawn.transform.rotation);
                    targetSpace.GetComponentInChildren<BoardSpace>().pieceOnSpace = newPiece;
                    fileIndex++;
                }
            }
        }
    }

	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenuPanel();
        }
        else if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            float yaw = Input.GetAxis("Mouse X");
            Vector3 rotationAngle = new Vector3(0f, -yaw * mouseSensitivity, 0f);
            CameraMover.instance.transform.Rotate(rotationAngle);
        }
	}

    public void ToggleMenuPanel()
    {
        enableInteraction = false;
        menuPanel.SetActive(!menuPanel.activeInHierarchy);

    }

    public void CheckValidMove()
    {
        if (castleSide != "")
        {
            StartCoroutine(ConcludeTurn());
            return;
        }

        isActiveKingInCheck = false;
        CheckAvailableMoves();

        if (isActiveKingInCheck)
        {
            ResetTurn();
        }
        else
        {
            StartCoroutine(ConcludeTurn());
        }
    }

    public IEnumerator ConcludeTurn()
    {
        enableInteraction = false;

        if(capturedPiece != null || movedPiece.GetComponentInChildren<PieceController>().pieceType == PieceType.WhitePawn || movedPiece.GetComponentInChildren<PieceController>().pieceType == PieceType.BlackPawn)
        {
            halfmoveClock = -1;
        }

        PieceController movedPieceController = movedPiece.GetComponentInChildren<PieceController>();

        if ((movedPieceController.pieceType == PieceType.Rook || movedPieceController.pieceType == PieceType.King) && !movedPieceController.hasMoved)
        {
            UpdateCastlingAvailability();
        }

        if (capturedPiece != null)
        {
            PieceController capturedPieceController = capturedPiece.GetComponentInChildren<PieceController>();
            if(capturedPieceController.pieceType == PieceType.Rook && !movedPieceController.hasMoved)
            {
                Debug.Log("Updating Castling Availability");
                UpdateCastlingAvailability();
            }
        }

        RecordMove();

        movedPieceController.hasMoved = true;

        if ((movedPieceController.pieceType == PieceType.WhitePawn && finishingSpace.y == 7) || (movedPieceController.pieceType == PieceType.BlackPawn && finishingSpace.y == 0))
        {
            resetButton.interactable = false;
            yield return StartCoroutine(PromotePawn());
            resetButton.interactable = true;
        }

        Destroy(capturedPiece);

        movedPiece = null;
        capturedPiece = null;
        startingSpace = default;
        finishingSpace = default;
        castleSide = "";

        List<BoardSpace> enPassantSpacesToRemove = new List<BoardSpace>();
        List<BoardSpace> keys = new List<BoardSpace>(availableEnPassantSpaces.Keys);
        foreach (BoardSpace key in keys)
        {
            if (availableEnPassantSpaces[key] == 1)
            {
                availableEnPassantSpaces[key]--;
            }
            else
            {
                enPassantSpacesToRemove.Add(key);
            }
        }
        foreach (BoardSpace space in enPassantSpacesToRemove)
        {
            space.enPassantPiece = null;
            availableEnPassantSpaces.Remove(space);
        }


        ChangeActiveColor();

    }

    private IEnumerator PromotePawn()
    {
        choosingPromotedUnit = true;
        pawnPromotionPanel.SetActive(true);

        while (choosingPromotedUnit)
        {
            yield return null;
        }

    }


    private void UpdateCastlingAvailability()
    {
        if((finishingSpace == new Vector2Int(0, 0) || startingSpace == new Vector2Int(0, 0)) && availableCastlingVectors.ContainsKey("Q"))
        {
            availableCastlingVectors.Remove("Q");
        }
        else if((finishingSpace == new Vector2Int(0, 7) || startingSpace == new Vector2Int(0, 7)) && availableCastlingVectors.ContainsKey("q"))
        {
            availableCastlingVectors.Remove("q");
        }
        else if((finishingSpace == new Vector2Int(7, 0) || startingSpace == new Vector2Int(7, 0)) && availableCastlingVectors.ContainsKey("K"))
        {
            availableCastlingVectors.Remove("K");
        }
        else if((finishingSpace == new Vector2Int(7, 7) || startingSpace == new Vector2Int(7, 7)) && availableCastlingVectors.ContainsKey("k"))
        {
            availableCastlingVectors.Remove("k");
        }
        else if(startingSpace == new Vector2Int(4, 0) && availableCastlingVectors.ContainsKey("Q") && availableCastlingVectors.ContainsKey("K"))
        {
            availableCastlingVectors.Remove("Q");
            availableCastlingVectors.Remove("K");
        }
        else if (startingSpace == new Vector2Int(4, 7) && availableCastlingVectors.ContainsKey("q") && availableCastlingVectors.ContainsKey("k"))
        {
            availableCastlingVectors.Remove("q");
            availableCastlingVectors.Remove("k");
        }
    }

    public void RecordMove()
    {
        string moveString = moveText.text;
        string initialString = " ";
        string piece = "";
        string targetSpace = "";
        string captureString = "";
        string check = "";

        if(castleSide == "Q")
        {
            moveString = moveString + initialString + "0-0-0 ";
            moveText.text = moveString;
            return;
        }
        else if(castleSide == "K")
        {
            moveString = moveString + initialString + "0-0 ";
            moveText.text = moveString;
            return;
        }

        if (activeColor == "w")
        {
            initialString = fullmoveClock.ToString() + ". ";
        }

        if (capturedPiece != null)
        {
            captureString = "x";
        }

        PieceController movedPieceController = movedPiece.GetComponentInChildren<PieceController>();
        foreach(Vector2Int allowedCaptureSpace in movedPieceController.allowedCaptureCoordinates)
        {
            GameObject targetPiece = boardSpaces[allowedCaptureSpace.y][allowedCaptureSpace.x].GetComponentInChildren<BoardSpace>().pieceOnSpace;
            if(targetPiece != null)
            {
                PieceController targetPieceController = targetPiece.GetComponentInChildren<PieceController>();
                if (targetPieceController.pieceType == PieceType.King)
                {
                    check = "+";
                    break;
                }

            }
        }

        piece = pieceNotation[movedPiece.GetComponentInChildren<PieceController>().pieceType];

        BoardSpace finishingBoardSpace = boardSpaces[finishingSpace.y][finishingSpace.x].GetComponentInChildren<BoardSpace>();

        targetSpace = finishingBoardSpace.column + finishingBoardSpace.row.ToString();

        moveString = moveString + initialString + piece + captureString + targetSpace + check + " ";

        moveText.text = moveString;
    }

    public void ResetTurn()
    {
        BoardSpace originalSpace = boardSpaces[startingSpace.y][startingSpace.x].GetComponentInChildren<BoardSpace>();

        BoardSpace finalSpace = boardSpaces[finishingSpace.y][finishingSpace.x].GetComponentInChildren<BoardSpace>();
        originalSpace.pieceOnSpace = movedPiece;

        if (capturedPiece == null)
        {
            finalSpace.pieceOnSpace = null;
        }
        else
        {
            capturedPiece.SetActive(true);
            finalSpace.pieceOnSpace = capturedPiece;
        }

        movedPiece.transform.position = originalSpace.transform.position;

        movedPiece = null;
        capturedPiece = null;
        startingSpace = default;
        finishingSpace = default;
        castleSide = "";

        List<BoardSpace> enPassantSpacesToRemove = new List<BoardSpace>();
        List<BoardSpace> keys = new List<BoardSpace>(availableEnPassantSpaces.Keys);
        foreach (BoardSpace key in keys)
        {
            if (availableEnPassantSpaces[key] == 1)
            {
                enPassantSpacesToRemove.Add(key);
            }
        }
        foreach (BoardSpace space in enPassantSpacesToRemove)
        {
            space.enPassantPiece = null;
            availableEnPassantSpaces.Remove(space);
        }


        CheckAvailableMoves();
    }

    public void ChangeActiveColor()
    {
        if(activeColor == "w")
        {
            activeColor = "b";
            colorToMoveText.text = "Black to move.";
        }
        else
        {
            activeColor = "w";
            colorToMoveText.text = "White to move.";
            fullmoveClock++;
        }

        turnText.text = fullmoveClock.ToString();

        CheckAvailableMoves();
        halfmoveClock++;
        StartCoroutine(RotateCamera());
    }

    private IEnumerator RotateCamera()
    {
        Quaternion startingRotation = CameraMover.instance.transform.rotation;

        enableInteraction = false;

        float timer = 0f;
        float timeToFinish = .4f;

        if (activeColor == "b")
        {
            while (Mathf.Abs(CameraMover.instance.transform.eulerAngles.y - 180f)>.01f)
            {
                CameraMover.instance.transform.rotation = Quaternion.Lerp(startingRotation, Quaternion.Euler(0f, 180f, 0f), timer / timeToFinish);

                timer += Time.unscaledDeltaTime;

                yield return null;
            }

            CameraMover.instance.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            while (Mathf.Abs(CameraMover.instance.transform.eulerAngles.y - 0f)>.01f)
            {
                CameraMover.instance.transform.rotation = Quaternion.Lerp(startingRotation, Quaternion.Euler(0f, 0f, 0f), timer / timeToFinish);

                timer += Time.unscaledDeltaTime;

                yield return null;
            }

            CameraMover.instance.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        }

        enableInteraction = true;
    }

    public void GameOver()
    {
        enabled = false;
    }

    public void CheckAvailableMoves()
    {
        PieceController[] allPieces = FindObjectsOfType<PieceController>();

        foreach (PieceController piece in allPieces)
        {
            piece.CheckAvailableMoves();
        }

    }

    public void FinalizeGame()
    {
        enableInteraction = false;
        string winner = "White";
        string winMoveString = "1-0";

        if (activeColor == "w")
        {
            winner = "Black";
            winMoveString = "0-1";
        }
        gameWinnerText.text = winner + " wins!";

        winMoveString = moveText.text + winMoveString;
        gameSummaryText.text = winMoveString;
    }
}
