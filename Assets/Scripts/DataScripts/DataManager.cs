using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameMode
{
    Puzzle,
    SingleScreen,
    Network
}

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public GameMode selectedGameMode;

    public PieceSet defaultPieceSet;
    public PieceSet[] allPieceSets;

    [HideInInspector]
    public PieceSet selectedPieceSet;

    public FENRecordData[] availablePuzzles;

    public FENRecordData selectedPuzzle;
    public FENRecordData standardGameMode;

    public bool hasTurnLimit;
    public int turnLimit;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
        }

        selectedPieceSet = defaultPieceSet;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.LoadScene(1);
    }

}
