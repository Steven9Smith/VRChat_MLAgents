using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UdonQueue<T> : UdonSharpBehaviour
{
    private T _defaultValue;
    private T[] _array;
    private int _head;
    private int _tail;
    private int _size;
    private const int _defaultCapacity = 4;

    public int Count => _size;
/*    public UdonQueue()
    {
        _array = new T[_defaultCapacity];
        _head = 0;
        _tail = 0;
        _size = 0;
    }

    public UdonQueue(int capacity)
    {
        if (capacity < 0)
        {
            Debug.LogError("Capacity must be a non-negative number");
            return;
        }

        _array = new T[capacity];
        _head = 0;
        _tail = 0;
        _size = 0;
    }*/
    public void InitializeUdonQueue(T defaultValue)
    {
        _array = new T[_defaultCapacity];
        _head = 0;
        _tail = 0;
        _size = 0;
        _defaultValue = defaultValue;
    }

    public void InititalizeUdonQueue(T defaultValue,int capacity)
    {
        if (capacity < 0)
        {
            Debug.LogError("Capacity must be a non-negative number");
            return;
        }

        _array = new T[capacity];
        _head = 0;
        _tail = 0;
        _size = 0;
        _defaultValue = defaultValue;
    }

    public void Enqueue(T item)
    {
        if (_size == _array.Length)
        {
            int newCapacity = _array.Length * 2;
            if (newCapacity < _array.Length + _defaultCapacity)
            {
                newCapacity = _array.Length + _defaultCapacity;
            }
            SetCapacity(newCapacity);
        }

        _array[_tail] = item;
        _tail = (_tail + 1) % _array.Length;
        _size++;
    }

    public T Dequeue()
    {
        if (_size == 0)
        {
            Debug.LogError("Queue is empty");
            return _defaultValue;
        }

        T removed = _array[_head];
        _array[_head] = _defaultValue;
        _head = (_head + 1) % _array.Length;
        _size--;

        return removed;
    }

    public T Peek()
    {
        if (_size == 0)
        {
            Debug.LogError("Queue is empty");
            return _defaultValue;
        }

        return _array[_head];
    }

    public bool Contains(T item)
    {
        int index = _head;
        int size = _size;

        while (size-- > 0)
        {
            if (_array[index] == null)
            {
                if (item == null)
                {
                    return true;
                }
            }
            else if (_array[index].Equals(item))
            {
                return true;
            }

            index = (index + 1) % _array.Length;
        }

        return false;
    }

    public T[] ToArray()
    {
        T[] result = new T[_size];
        if (_size == 0)
        {
            return result;
        }

        if (_head < _tail)
        {
            System.Array.Copy(_array, _head, result, 0, _size);
        }
        else
        {
            System.Array.Copy(_array, _head, result, 0, _array.Length - _head);
            System.Array.Copy(_array, 0, result, _array.Length - _head, _tail);
        }

        return result;
    }

    private void SetCapacity(int capacity)
    {
        T[] newArray = new T[capacity];
        if (_size > 0)
        {
            if (_head < _tail)
            {
                System.Array.Copy(_array, _head, newArray, 0, _size);
            }
            else
            {
                System.Array.Copy(_array, _head, newArray, 0, _array.Length - _head);
                System.Array.Copy(_array, 0, newArray, _array.Length - _head, _tail);
            }
        }

        _array = newArray;
        _head = 0;
        _tail = _size == capacity ? 0 : _size;
    }
}
