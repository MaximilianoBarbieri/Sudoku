using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Sudoku : MonoBehaviour
{
    [Range(1, 82)] public int difficulty = 40;

    public Cell prefabCell;
    public Canvas canvas;
    public Text feedback;

    public float stepDuration = 0.05f;

    private Matrix<Cell> _board;
    private Matrix<int> _createdMatrix;

    private List<int> _posibles = new List<int>();
    private List<int> _nums = new List<int>();

    private int _smallSide;
    private int _bigSide;
    private int _watchdog;

    private string _memory = "";
    private string _canSolve = "";

    private bool _canPlayMusic;

    private float _frequency = 440;
    private float _increment;
    private float _phase;

    private const float R = 1.0594f;
    private const float SamplingF = 48000;
    private const float Gain = 0.5f;


    private void Start()
    {
        long mem = System.GC.GetTotalMemory(true);

        feedback.text = $"MEM: {mem / (1024f * 1024f):f2}MB";
        _memory = feedback.text;
        _smallSide = 3;
        _bigSide = _smallSide * 3;
        _frequency *= Mathf.Pow(R, 2);

        CreateEmptyBoard();
        ClearBoard();
    }

    private void ClearBoard()
    {
        _createdMatrix = new Matrix<int>(_bigSide, _bigSide);

        foreach (var cell in _board)
        {
            cell.number = 0;
            cell.locked = cell.invalid = false;
        }
    }

    private void CreateEmptyBoard()
    {
        var spacing = 68f;
        var startX = -spacing * 4f;
        var startY = spacing * 4f;

        _board = new Matrix<Cell>(_bigSide, _bigSide);

        for (var x = 0; x < _board.Width; x++)
        {
            for (var y = 0; y < _board.Height; y++)
            {
                var cell = _board[x, y] = Instantiate(prefabCell);
                cell.transform.SetParent(canvas.transform, false);
                cell.transform.localPosition = new Vector3(startX + x * spacing, startY - y * spacing, 0);
            }
        }
    }

    private bool RecuSolve(Matrix<int> matrixParent, int x, int y, int protectMaxDepth, List<Matrix<int>> solution)
    {
        if (y >= matrixParent.Height) return true;
        
        protectMaxDepth--;
        
        if (protectMaxDepth <= 0) return false;

        if (matrixParent[x, y] != 0)
        {
            return RecuSolve(matrixParent.Clone(),
                (x == matrixParent.Width - 1) ? 0 : x + 1,
                (x == matrixParent.Width - 1) ? y + 1 : y,
                protectMaxDepth, solution);
        }

        for (var i = 1; i <= _smallSide * _smallSide; i++)
        {
            if (CanPlaceValue(matrixParent, i, x, y))
            {
                matrixParent[x, y] = i;
                var nextMatrix = matrixParent.Clone();
                solution.Add(nextMatrix);

                if (RecuSolve(nextMatrix,
                        (x == matrixParent.Width - 1) ? 0 : x + 1,
                        (x == matrixParent.Width - 1) ? y + 1 : y,
                        protectMaxDepth, solution))
                    return true;

                matrixParent[x, y] = 0;
            }
        }

        return false;
    }

    private void OnAudioFilterRead(float[] array, int channels)
    {
        if (_canPlayMusic)
        {
            _increment = _frequency * Mathf.PI / SamplingF;

            for (var i = 0; i < array.Length; i++)
            {
                _phase += _increment;
                array[i] = (Gain * Mathf.Sin(_phase));
            }
        }
    }

    private void changeFreq(int num)
    {
        _frequency = 440 + num * 80;
    }

    private IEnumerator ShowSequence(List<Matrix<int>> seq)
    {
        for (int i = 0; i < seq.Count; i++)
        {
            TranslateAllValues(seq[i]);
            feedback.text = "Pasos: " + (i + 1) + "/" + seq.Count + " - " + _memory + " - " + _canSolve;

            yield return new WaitForSeconds(stepDuration);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(1))
            SolvedSudoku();

        if (Input.GetKeyDown(KeyCode.C) || Input.GetMouseButtonDown(0))
            CreateNew();
    }

    private void SolvedSudoku()
    {
        StopAllCoroutines();

        _nums = new List<int>();
        _watchdog = 100000;

        var solution = new List<Matrix<int>>();
        var result = RecuSolve(_createdMatrix, 0, 0, _watchdog, solution); //TODO: MODIFICAR
        long mem = System.GC.GetTotalMemory(true);

        _memory = $"MEM: {mem / (1024f * 1024f):f2}MB";
        _canSolve = result ? " VALID" : " INVALID";

        StartCoroutine(ShowSequence(solution));
    }

    private void CreateSudoku()
    {
        StopAllCoroutines();

        _nums = new List<int>();
        _canPlayMusic = false;

        ClearBoard();

        List<Matrix<int>> l = new List<Matrix<int>>();

        _watchdog = 100000;

        GenerateValidLine(_createdMatrix, 0, 0);

        var result = false;

        _createdMatrix = l[0].Clone();

        LockRandomCells();
        ClearUnlocked(_createdMatrix);
        TranslateAllValues(_createdMatrix);

        var mem = System.GC.GetTotalMemory(true);

        _memory = $"MEM: {mem / (1024f * 1024f):f2}MB";
        _canSolve = result ? " VALID" : " INVALID";
        feedback.text = "Pasos: " + l.Count + "/" + l.Count + " - " + _memory + " - " + _canSolve;
    }

    private void GenerateValidLine(Matrix<int> mtx, int x, int y)
    {
        int[] aux = new int[9];
        for (int i = 0; i < 9; i++)
        {
            aux[i] = i + 1;
        }

        int numAux = 0;
        for (int j = 0; j < aux.Length; j++)
        {
            int r = 1 + Random.Range(j, aux.Length);
            numAux = aux[r - 1];
            aux[r - 1] = aux[j];
            aux[j] = numAux;
        }

        for (int k = 0; k < aux.Length; k++)
        {
            mtx[k, 0] = aux[k];
        }
    }

    private void ClearUnlocked(Matrix<int> mtx)
    {
        for (int i = 0; i < _board.Height; i++)
        {
            for (int j = 0; j < _board.Width; j++)
            {
                if (!_board[j, i].locked)
                    mtx[j, i] = Cell.EMPTY;
            }
        }
    }

    private void LockRandomCells()
    {
        List<Vector2> posibles = new List<Vector2>();
        for (int i = 0; i < _board.Height; i++)
        {
            for (int j = 0; j < _board.Width; j++)
            {
                if (!_board[j, i].locked)
                    posibles.Add(new Vector2(j, i));
            }
        }

        for (int k = 0; k < 82 - difficulty; k++)
        {
            int r = Random.Range(0, posibles.Count);
            _board[(int)posibles[r].x, (int)posibles[r].y].locked = true;
            posibles.RemoveAt(r);
        }
    }

    private void TranslateAllValues(Matrix<int> matrix)
    {
        for (var y = 0; y < _board.Height; y++)
        {
            for (var x = 0; x < _board.Width; x++)
            {
                _board[x, y].number = matrix[x, y];
            }
        }
    }

    private void TranslateSpecific(int value, int x, int y)
    {
        _board[x, y].number = value;
    }

    private void TranslateRange(int x0, int y0, int xf, int yf)
    {
        for (int x = x0; x < xf; x++)
        {
            for (int y = y0; y < yf; y++)
            {
                _board[x, y].number = _createdMatrix[x, y];
            }
        }
    }

    private void CreateNew()
    {
        CreateEmptyBoard();
        ClearBoard();

        _createdMatrix =
            new Matrix<int>(
                Tests.validBoards
                    [Tests.validBoards.Length - 1]);

        LockRandomCells();

        ClearUnlocked(_createdMatrix);

        TranslateAllValues(_createdMatrix);
    }

    private bool CanPlaceValue(Matrix<int> mtx, int value, int x, int y)
    {
        List<int> fila = new List<int>();
        List<int> columna = new List<int>();
        List<int> area = new List<int>();
        List<int> total = new List<int>();

        Vector2 cuadrante = Vector2.zero;

        for (int i = 0; i < mtx.Height; i++)
        {
            for (int j = 0; j < mtx.Width; j++)
            {
                if (i != y && j == x) columna.Add(mtx[j, i]);
                else if (i == y && j != x) fila.Add(mtx[j, i]);
            }
        }


        cuadrante.x = (int)(x / 3);

        if (x < 3)
            cuadrante.x = 0;
        else if (x < 6)
            cuadrante.x = 3;
        else
            cuadrante.x = 6;

        if (y < 3)
            cuadrante.y = 0;
        else if (y < 6)
            cuadrante.y = 3;
        else
            cuadrante.y = 6;

        area = mtx.GetRange((int)cuadrante.x, (int)cuadrante.y, (int)cuadrante.x + 3, (int)cuadrante.y + 3);
        total.AddRange(fila);
        total.AddRange(columna);
        total.AddRange(area);
        total = FilterZeros(total);

        if (total.Contains(value))
            return false;
        else
            return true;
    }

    private List<int> FilterZeros(List<int> list)
    {
        List<int> aux = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != 0) aux.Add(list[i]);
        }

        return aux;
    }
}