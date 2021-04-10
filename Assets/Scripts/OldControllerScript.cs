using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OldControllerScript : MonoBehaviour {
    public int numberOfBacksteps = 10;

    public List<GameObject> buttonRows;
    public Text winText;

    private List<List<bool>> matrix;
    private List<List<GameObject>> buttons;
    private List<Vector2Int> hints;

    // Start is called before the first frame update
    void Start() {
        matrix = new List<List<bool>>();
        buttons = new List<List<GameObject>>();
        hints = new List<Vector2Int>();
        for (int i = 0; i < 5; i++) {
            List<GameObject> row = new List<GameObject>();
            for (int j = 0; j < 5; j++) {
                row.Add(buttonRows[i].transform.GetChild(j).gameObject);
            }
            buttons.Add(row);
        }
        NewGame();
    }

    void Update() {
	if (Input.GetKeyDown(KeyCode.Escape)) {
	    Application.Quit();
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
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
                if (matrix[i][j]) {
                    buttons[i][j].transform.GetComponent<Image>().color = Color.yellow;
                } else {
                    buttons[i][j].transform.GetComponent<Image>().color = Color.black;
                }
            }
        }
    }

    private void GenerateWinnableMatrix() {
        // Start from win
        for (int i = 0; i < 5; i++) {
            List<bool> row = new List<bool>();
            for (int j = 0; j < 5; j++) {
                row.Add(true);
            }
            matrix.Add(row);
        }
        // Trigger random bulbs to go backwards and record these moves in hints
        for (int i = 0; i < numberOfBacksteps; i++) {
            // Generate random move
            int row = Random.Range(0, 5);
            int col = Random.Range(0, 5);
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
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
                if (matrix[i][j]) {
                    buttons[i][j].transform.GetComponent<Image>().color = Color.yellow;
                } else {
                    buttons[i][j].transform.GetComponent<Image>().color = Color.black;
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
        for (int i = 0; i < 5; i++) {
            List<bool> row = new List<bool>();
            for (int j = 0; j < 5; j++) {
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
        int row = int.Parse(button.transform.parent.name[button.transform.parent.name.Length - 1].ToString()) - 1;
        int col = int.Parse(button.name[button.name.Length - 1].ToString()) - 1;
        foreach (Vector2Int n in GetNeighbours(matrix, row, col)) {
            if (matrix[n.x][n.y]) {
                buttons[n.x][n.y].GetComponent<Image>().color = Color.black;
            } else {
                buttons[n.x][n.y].GetComponent<Image>().color = Color.yellow;
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
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
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
