using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerScript : MonoBehaviour {
    [SerializeField] private int numberOfBacksteps = 10;

    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;

    [SerializeField] private int boardSize = 5;

    [SerializeField] private Text winText;

    private List<List<bool>> _matrix;
    private List<List<GameObject>> _buttons;
    private List<Vector2Int> _hints;
    private Camera _camera;

    private void Awake() {
        _camera = Camera.main;
    }

    private void Start() {
        DrawBoard();
    }

    public void RedrawBoard(Slider slider) {
        boardSize = (int) slider.value;
        Destroy(GameObject.Find("Board"));
        DrawBoard();
    }

    private void DrawBoard() {
        _matrix = new List<List<bool>>();
        _buttons = new List<List<GameObject>>();
        _hints = new List<Vector2Int>();

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
        }
        else {
            if (!Input.GetMouseButtonDown(0)) return;
            Vector2 origin = _camera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(origin, Vector2.zero, 100, LayerMask.GetMask("Clickable"));
            if (hit) {
                TriggerBulb(hit.collider.gameObject);
            }
        }
    }

    public void NewGame() {
        winText.enabled = false;
        _matrix.Clear();
        _hints.Clear();
        GenerateWinnableMatrix();
        // Assign color to each button
        for (var i = 0; i < boardSize; i++) {
            for (var j = 0; j < boardSize; j++) {
                _buttons[i][j].GetComponent<SpriteRenderer>().color = _matrix[i][j] ? onColor : offColor;
            }
        }

        if (GameWon()) {
            NewGame();
        }
    }

    private void GenerateWinnableMatrix() {
        // Start from win
        for (var i = 0; i < boardSize; i++) {
            var row = new List<bool>();
            for (var j = 0; j < boardSize; j++) {
                row.Add(true);
            }

            _matrix.Add(row);
        }

        // Trigger random bulbs to go backwards and record these moves in hints
        for (var i = 0; i < numberOfBacksteps; i++) {
            // Generate random move
            var row = Random.Range(0, boardSize);
            var col = Random.Range(0, boardSize);
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
        for (var i = 0; i < boardSize; i++) {
            for (var j = 0; j < boardSize; j++) {
                _buttons[i][j].GetComponent<SpriteRenderer>().color = _matrix[i][j] ? onColor : offColor;
            }
        }

        // Update win text
        winText.enabled = GameWon();

        // Print remaining moves
        Debug.Log(_hints.Count);
    }

    private void UpdateHints(Vector2Int move) {
        if (_hints.Contains(move)) {
            _hints.Remove(move);
        }
        else {
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

        winText.enabled = GameWon();
        // Update hints
        UpdateHints(new Vector2Int(row, col));
    }

    private bool GameWon() {
        for (var i = 0; i < boardSize; i++) {
            for (var j = 0; j < boardSize; j++) {
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