using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NewControllerScript : MonoBehaviour
{
    public int numberOfBacksteps = 10;

    public Color onColor;
    public Color offColor;

    public int boardSize = 5;
    public float gap = 0.05f;
    public float edgeOffset = 0.1f;
    public float topOffsetPercentage = 0.1f;
    public float bottomOffsetPercentage = 0.2f;
    public GameObject cellObject;

    //public List<GameObject> buttonRows;
    public Text winText;

    private List<List<bool>> matrix;
    private List<List<GameObject>> buttons;
    private List<Vector2Int> hints;

    void Start() {
        DrawBoard();
    }

    public void RedrawBoard(Slider slider) {
        boardSize = (int)slider.value;
        Destroy(GameObject.Find("Board"));
        DrawBoard();
    }

    void DrawBoard() {
        matrix = new List<List<bool>>();
        buttons = new List<List<GameObject>>();
        hints = new List<Vector2Int>();
        float leftEdge = 0;
        float topEdge = 0;
        float cellWidth = (Camera.main.orthographicSize * Camera.main.aspect * 2 - edgeOffset * 2 - gap * (boardSize - 1)) / boardSize;
        if (cellWidth * boardSize + edgeOffset * 2 + gap * (boardSize - 1) > Camera.main.orthographicSize * 2 * (1 - topOffsetPercentage - bottomOffsetPercentage)) {
            cellWidth = (Camera.main.orthographicSize * 2 * (1 - topOffsetPercentage - bottomOffsetPercentage) - 2 * edgeOffset - gap * (boardSize - 1)) / boardSize;
            leftEdge = (Camera.main.orthographicSize * 2 * Camera.main.aspect - 2 * edgeOffset - (boardSize - 1) * gap - cellWidth * boardSize) * 0.5f;
        } else {
            topEdge = (Camera.main.orthographicSize * 2 * (1 - topOffsetPercentage - bottomOffsetPercentage) - 2 * edgeOffset - (boardSize - 1) * gap - cellWidth * boardSize) * 0.5f;
        }
        GameObject board = new GameObject("Board");
        for (int i = 0; i < boardSize; i++) {
            GameObject rowParent = new GameObject("Row " + (i + 1).ToString());
            rowParent.transform.parent = board.transform;
            List<GameObject> row = new List<GameObject>();
            for (int j = 0; j < boardSize; j++) {
                float buttonX = leftEdge + edgeOffset + j * gap + j * cellWidth + cellWidth * 0.5f - Camera.main.orthographicSize * Camera.main.aspect;
                float buttonY = Camera.main.orthographicSize - topEdge - topOffsetPercentage * (Camera.main.orthographicSize * 2) - i * cellWidth - i * gap - cellWidth * 0.5f - edgeOffset;
                Vector3 newPosition = new Vector3(buttonX, buttonY, 0);
                GameObject button = Instantiate(cellObject, newPosition, Quaternion.identity, rowParent.transform);
                button.transform.localScale = new Vector2(cellWidth, cellWidth);
                button.name = "Button " + (j + 1).ToString();
                row.Add(button);
            }
            buttons.Add(row);
        }
        NewGame();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }

        if (Application.isMobilePlatform) {
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) {
                Vector2 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, 100, LayerMask.GetMask("Clickable"));
                if (hit) {
                    TriggerBulb(hit.collider.gameObject);
                }
            }
        } else {
            if (Input.GetMouseButtonDown(0)) {
                Vector2 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, 100, LayerMask.GetMask("Clickable"));
                if (hit) {
                    TriggerBulb(hit.collider.gameObject);
                }
            }
        }
    }

    public void NewGame() {
        winText.enabled = false;
        matrix.Clear();
        hints.Clear();
        // Generate random matrix
        // GenerateRandomMatrix();
        // Generate winnable matrix
        GenerateWinnableMatrix();
        // Assign color to each button
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                if (matrix[i][j]) {
                    buttons[i][j].GetComponent<SpriteRenderer>().color = onColor;
                } else {
                    buttons[i][j].GetComponent<SpriteRenderer>().color = offColor;
                }
            }
        }
        if (GameWon()) {
            NewGame();
        }
    }

    private void GenerateWinnableMatrix() {
        // Start from win
        for (int i = 0; i < boardSize; i++) {
            List<bool> row = new List<bool>();
            for (int j = 0; j < boardSize; j++) {
                row.Add(true);
            }
            matrix.Add(row);
        }
        // Trigger random bulbs to go backwards and record these moves in hints
        for (int i = 0; i < numberOfBacksteps; i++) {
            // Generate random move
            int row = Random.Range(0, boardSize);
            int col = Random.Range(0, boardSize);
            Vector2Int move = new Vector2Int(row, col);

            // Trigger corresponding bulbs
            foreach (Vector2Int n in GetNeighbours(matrix, row, col)) {
                matrix[n.x][n.y] = !matrix[n.x][n.y];
            }

            // Update hints
            UpdateHints(move);
        }
    }

    public void PlayHint() {
        if (hints.Count == 0) {
            return;
        }
        // Get random move from hints
        Vector2Int move = hints[Random.Range(0, hints.Count)];
        // Trigger corresponding bulbs
        foreach (Vector2Int n in GetNeighbours(matrix, move.x, move.y)) {
            matrix[n.x][n.y] = !matrix[n.x][n.y];
        }
        // Remove played move from hints
        hints.Remove(move);
        // Update buttons on screen
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                if (matrix[i][j]) {
                    buttons[i][j].GetComponent<SpriteRenderer>().color = onColor;
                } else {
                    buttons[i][j].GetComponent<SpriteRenderer>().color = offColor;
                }
            }
        }
        // Update win text
        if (GameWon()) {
            winText.enabled = true;
        } else {
            winText.enabled = false;
        }
        // Print remaining moves
        Debug.Log(hints.Count);
    }

    private void UpdateHints(Vector2Int move) {
        if (hints.Contains(move)) {
            hints.Remove(move);
        } else {
            hints.Add(move);
        }
    }

    private void GenerateRandomMatrix() {
        for (int i = 0; i < boardSize; i++) {
            List<bool> row = new List<bool>();
            for (int j = 0; j < boardSize; j++) {
                if (Random.Range(0, 2) == 0) {
                    row.Add(true);
                } else {
                    row.Add(false);
                }
            }
            matrix.Add(row);
        }
    }

    public void TriggerBulb(GameObject button) {
        // Identify row and column
        int row = int.Parse(button.transform.parent.name.Substring(button.transform.parent.name.Length - 2, 2)) - 1;
        int col = int.Parse(button.name.Substring(button.name.Length - 2, 2)) - 1;
        foreach (Vector2Int n in GetNeighbours(matrix, row, col)) {
            if (matrix[n.x][n.y]) {
                buttons[n.x][n.y].GetComponent<SpriteRenderer>().color = offColor;
            } else {
                buttons[n.x][n.y].GetComponent<SpriteRenderer>().color = onColor;
            }
            matrix[n.x][n.y] = !matrix[n.x][n.y];
        }
        if (GameWon()) {
            winText.enabled = true;
        } else {
            winText.enabled = false;
        }
        // Update hints
        UpdateHints(new Vector2Int(row, col));
    }

    private bool GameWon() {
        for (int i = 0; i < boardSize; i++) {
            for (int j = 0; j < boardSize; j++) {
                if (!matrix[i][j]) {
                    return false;
                }
            }
        }
        return true;
    }

    private List<Vector2Int> GetNeighbours(List<List<bool>> matrix, int row, int col) {
        List<Vector2Int> neighboursList = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                // Ignore diagonals
                if (i != 0 && j != 0) {
                    continue;
                }
                int currentRow = row + i;
                int currentCol = col + j;
                // Check bounds
                if (currentRow >= 0 && currentRow < matrix.Count && currentCol >= 0 && currentCol < matrix[0].Count) {
                    neighboursList.Add(new Vector2Int(currentRow, currentCol));
                }
            }
        }
        return neighboursList;
    }
}
