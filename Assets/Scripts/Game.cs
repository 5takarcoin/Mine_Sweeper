using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public int width = 16;
    public int height = 16;

    public int mineCounts = 32;

    public Text text;

    private Board board;

    private Cell[,] state;

    private bool gameOver;
    private int count;

    private void OnValidate()
    {
        mineCounts = Mathf.Clamp(mineCounts, 0, width * height);
    }

    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        Camera.main.transform.position = new Vector3(width / 2, height / 2, -10);

        NewGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) NewGame();
        else if (!gameOver)
        {
            text.text = count.ToString();

            if (Input.GetKeyDown(KeyCode.Mouse1)) Flag();
            if (Input.GetKeyDown(KeyCode.Mouse0)) Reveal();
        }
    }

    public void NewGame()
    {
        gameOver = false;
        count = mineCounts;

        state = new Cell[width, height];

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        board.Draw(state);
    }

    private void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                //cell.revealed = true;
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for (int i = 0; i < mineCounts; i++)
        {
            int x = Random.Range(0, width * height);

            while (state[x / height, x % height].type == Cell.Type.Mine)
            {
                x++;
                if (x == width * height) x = 0;
            }

            state[x / height, x % height].type = Cell.Type.Mine;
            //state[x / height, x % height].revealed = true;
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine) continue;

                cell.number = CountMines(x, y);
                if (cell.number > 0) cell.type = Cell.Type.Number;
                //cell.revealed = true;
                state[x, y] = cell;
            }
        }
    }

    private int CountMines(int x, int y)
    {
        int count = 0;

        for (int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                if ((i==0 && j==0) || x + i < 0 || x + i >= width || y + j < 0 || y + j >= height) continue;

                if (state[x + i, y + j].type == Cell.Type.Mine) count++;
            }
        }

        return count;
    }

    private void Flag()
    {
        if(count > 0)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

            //Debug.Log(cellPosition);

            int x = cellPosition.x;
            int y = cellPosition.y;

            if (x >= 0 && x < width && y >= 0 && y < height && !state[x, y].revealed)
            {
                Cell cell = state[x, y];
                if (cell.flagged) count++;
                else count--;
                cell.flagged = !cell.flagged;
                state[x, y] = cell;
                board.Draw(state);
            }
        }
    }

    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

        //Debug.Log(cellPosition);

        int x = cellPosition.x;
        int y = cellPosition.y;

        if (x >= 0 && x < width && y >= 0 && y < height && !state[x, y].revealed && !state[x, y].flagged)
        {
            Cell cell = state[x, y];

            switch (cell.type)
            {
                case Cell.Type.Empty:
                    Flood(cell);
                    CheckWin();
                    break;
                case Cell.Type.Mine:
                    Explode(cell);
                    break;
                default:
                    cell.revealed = true;
                    state[x, y] = cell;
                    CheckWin();
                    break;
            }

            board.Draw(state);
        }
    }

    private void Flood(Cell cell)
    {
        if (cell.type == Cell.Type.Mine) return;
        else if (cell.revealed) return;

        int x = cell.position.x;
        int y = cell.position.y;

        cell.revealed = true;
        state[x, y] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            if (cell.flagged) count++;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if ((i == 0 && j == 0) || x + i < 0 || x + i >= width || y + j < 0 || y + j >= height) continue;

                    Flood(state[x + i, y + j]);
                }
            }
        }
    }

    private void Explode(Cell cell)
    {
        gameOver = true;
        text.text = "Sad!";

        cell.revealed = true;
        cell.exploded = true;
        state[cell.position.x, cell.position.y] = cell;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (state[i, j].type == Cell.Type.Mine) state[i, j].revealed = true;
            }
        }
    }

    private void CheckWin()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Cell cell = state[i, j];
                if (cell.type != Cell.Type.Mine && !cell.revealed) return;
            }
        }

        text.text = "Win!";
        gameOver = true;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (state[i, j].type == Cell.Type.Mine) state[i, j].flagged = true;
            }
        }
    }
}
