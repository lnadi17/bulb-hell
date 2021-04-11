using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ControllerScript : MonoBehaviour {
    # region Definitions

    [MinValue(0)] [SerializeField] private int numberOfBacksteps = 10;

    [ColorPalette("Light Cell Colors")] [SerializeField]
    private Color onColor;

    [ColorPalette("Dark Cell Colors")] [SerializeField]
    private Color offColor;


    [Range(0, 1f)] [SerializeField] private float edgeOffset;
    [Range(0, 0.5f)] [SerializeField] private float gap;
    [Range(0, 1f)] [SerializeField] private float topOffsetPercentage = 0.2f;
    [Range(0, 1f)] [SerializeField] private float bottomOffsetPercentage = 0.1f;
    [AssetsOnly] [SerializeField] private GameObject cellObject;
    [SceneObjectsOnly] [SerializeField] private GameObject winText;

    [ShowInInspector] [Sirenix.OdinInspector.ReadOnly]
    private int _boardSize = 5;

    [ShowInInspector] [TableList(ShowIndexLabels = true)] [Sirenix.OdinInspector.ReadOnly]
    private List<List<bool>> _matrix;

    private List<List<GameObject>> _buttons;
    private List<Vector2Int> _hints;
    private Camera _camera;

    #endregion

    private void Awake() {
        _camera = Camera.main;
    }

    private void Start() {
        DrawBoard();
    }

    public void RedrawBoard(Slider slider) {
        _boardSize = (int) slider.value;
        Destroy(GameObject.Find("Board"));
        DrawBoard();
    }

    public void RedrawBoard() {
        Destroy(GameObject.Find("Board"));
        DrawBoard();
    }

    private void DrawBoard() {
        _matrix = new List<List<bool>>();
        _buttons = new List<List<GameObject>>();
        _hints = new List<Vector2Int>();

        float leftEdge = 0;
        float topEdge = 0;
        var orthographicSize = _camera.orthographicSize;
        var cellWidth = (orthographicSize * _camera.aspect * 2 - edgeOffset * 2 - gap * (_boardSize - 1)) / _boardSize;

        if (cellWidth * _boardSize + edgeOffset * 2 + gap * (_boardSize - 1) >
            orthographicSize * 2 * (1 - topOffsetPercentage - bottomOffsetPercentage)) {
            cellWidth = (orthographicSize * 2 * (1 - topOffsetPercentage - bottomOffsetPercentage) - 2 * edgeOffset -
                         gap * (_boardSize - 1)) / _boardSize;
            leftEdge = (orthographicSize * 2 * _camera.aspect - 2 * edgeOffset - (_boardSize - 1) * gap -
                        cellWidth * _boardSize) * 0.5f;
        } else {
            topEdge = (orthographicSize * 2 * (1 - topOffsetPercentage - bottomOffsetPercentage) - 2 * edgeOffset -
                       (_boardSize - 1) * gap - cellWidth * _boardSize) * 0.5f;
        }

        var board = new GameObject("Board");
        for (var i = 0; i < _boardSize; i++) {
            var row = new List<GameObject>();
            var rowParent = new GameObject("Row " + (i + 1));
            rowParent.transform.parent = board.transform;
            for (var j = 0; j < _boardSize; j++) {
                var buttonX = leftEdge + edgeOffset + j * gap + j * cellWidth + cellWidth * 0.5f -
                              orthographicSize * _camera.aspect;
                var buttonY = orthographicSize - topEdge - topOffsetPercentage * (orthographicSize * 2) -
                              i * cellWidth - i * gap - cellWidth * 0.5f - edgeOffset;
                var newPosition = new Vector3(buttonX, buttonY, 0);
                var button = Instantiate(cellObject, newPosition, Quaternion.identity, rowParent.transform);
                button.transform.localScale = new Vector2(cellWidth, cellWidth);
                button.name = "Button " + (j + 1);
                row.Add(button);
            }

            _buttons.Add(row);
        }

        NewGame();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }

        if (Application.isMobilePlatform) {
            if (Input.touchCount != 1 || Input.GetTouch(0).phase != TouchPhase.Began) return;
            Vector2 origin = _camera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(origin, Vector2.zero, 100, LayerMask.GetMask("Clickable"));
            if (hit) {
                TriggerBulb(hit.collider.gameObject);
            }
        } else {
            if (!Input.GetMouseButtonDown(0)) return;
            Vector2 origin = _camera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(origin, Vector2.zero, 100, LayerMask.GetMask("Clickable"));
            if (hit) {
                TriggerBulb(hit.collider.gameObject);
            }
        }
    }

    public void NewGame() {
        winText.SetActive(false);
        _matrix.Clear();
        _hints.Clear();
        GenerateWinnableMatrix();
        // Assign color to each button
        for (var i = 0; i < _boardSize; i++) {
            for (var j = 0; j < _boardSize; j++) {
                _buttons[i][j].GetComponent<SpriteRenderer>().color = _matrix[i][j] ? onColor : offColor;
            }
        }

        if (GameWon()) {
            NewGame();
        }
    }

    private void GenerateWinnableMatrix() {
        // Start from win
        for (var i = 0; i < _boardSize; i++) {
            var row = new List<bool>();
            for (var j = 0; j < _boardSize; j++) {
                row.Add(true);
            }

            _matrix.Add(row);
        }

        // Trigger random bulbs to go backwards and record these moves in hints
        for (var i = 0; i < numberOfBacksteps; i++) {
            // Generate random move
            var row = Random.Range(0, _boardSize);
            var col = Random.Range(0, _boardSize);
            var move = new Vector2Int(row, col);

            // Trigger corresponding bulbs
            foreach (var n in GetNeighbours(_matrix, row, col)) {
                _matrix[n.x][n.y] = !_matrix[n.x][n.y];
            }

            // Update hints
            UpdateHints(move);
        }
    }

    public void PlayHint() {
        if (_hints.Count == 0) {
            return;
        }

        // Get random move from hints
        Vector2Int move = _hints[Random.Range(0, _hints.Count)];
        // Trigger corresponding bulbs
        foreach (var n in GetNeighbours(_matrix, move.x, move.y)) {
            _matrix[n.x][n.y] = !_matrix[n.x][n.y];
        }

        // Remove played move from hints
        _hints.Remove(move);
        // Update buttons on screen
        for (var i = 0; i < _boardSize; i++) {
            for (var j = 0; j < _boardSize; j++) {
                _buttons[i][j].GetComponent<SpriteRenderer>().color = _matrix[i][j] ? onColor : offColor;
            }
        }

        // Update win text
        winText.SetActive(GameWon());

        // Print remaining moves
        Debug.Log(_hints.Count);
    }

    private void UpdateHints(Vector2Int move) {
        if (_hints.Contains(move)) {
            _hints.Remove(move);
        } else {
            _hints.Add(move);
        }
    }

    public void TriggerBulb(GameObject button) {
        // Identify row and column
        var parent = button.transform.parent;
        var row = int.Parse(parent.name.Substring(parent.name.Length - 2, 2)) - 1;
        var col = int.Parse(button.name.Substring(button.name.Length - 2, 2)) - 1;
        foreach (var n in GetNeighbours(_matrix, row, col)) {
            _buttons[n.x][n.y].GetComponent<SpriteRenderer>().color = _matrix[n.x][n.y] ? offColor : onColor;

            _matrix[n.x][n.y] = !_matrix[n.x][n.y];
        }

        winText.SetActive(GameWon());
        // Update hints
        UpdateHints(new Vector2Int(row, col));
    }

    private bool GameWon() {
        for (var i = 0; i < _boardSize; i++) {
            for (var j = 0; j < _boardSize; j++) {
                if (!_matrix[i][j]) {
                    return false;
                }
            }
        }

        return true;
    }

    private IEnumerable<Vector2Int> GetNeighbours(IReadOnlyList<List<bool>> matrix, int row, int col) {
        var neighboursList = new List<Vector2Int>();
        for (var i = -1; i <= 1; i++) {
            for (var j = -1; j <= 1; j++) {
                // Ignore diagonals
                if (i != 0 && j != 0) {
                    continue;
                }

                var currentRow = row + i;
                var currentCol = col + j;
                // Check bounds
                if (currentRow >= 0 && currentRow < matrix.Count && currentCol >= 0 && currentCol < matrix[0].Count) {
                    neighboursList.Add(new Vector2Int(currentRow, currentCol));
                }
            }
        }

        return neighboursList;
    }
}