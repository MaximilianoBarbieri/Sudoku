using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;


public class Matrix<T> : IEnumerable<T>
{
    private T[] _data;

    private int _width;
    private int _height;
    private int _capacity;
    public int Width => _width;
    public int Height => _height;

    public Matrix(int width, int height)
    {
        _width = width;
        _height = height;
        _capacity = width * height;

        _data = new T[_capacity];
    }

    public Matrix(T[,] copyFrom)
    {
        _capacity = copyFrom.Length;
        _width = copyFrom.GetLength(0);
        _height = copyFrom.GetLength(1);

        _data = new T[_capacity];

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                this[x, y] = copyFrom[x, y];
            }
        }
    }

    public Matrix<T> Clone()
    {
        var aux = new Matrix<T>(Width, Height);

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                aux[x, y] = this[x, y];
            }
        }

        return aux;
    }

    public void SetRangeTo(int x0, int y0, int x1, int y1, T item)
    {
    }

    public List<T> GetRange(int x0, int y0, int x1, int y1)
    {
        var l = new List<T>();

        for (var x = x0; x < x1; x++)
        {
            for (var y = y0; y < y1; y++)
            {
                l.Add(this[x, y]);
            }
        }

        return l;
    }

    public T this[int x, int y]
    {
        get => _data[x + _height * y];
        set => _data[x + _height * y] = value;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)_data).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}