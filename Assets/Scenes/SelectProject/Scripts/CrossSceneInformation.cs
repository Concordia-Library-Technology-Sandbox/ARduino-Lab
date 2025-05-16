using System;
using System.Collections.Generic;
public static class StaticClass
{
    public static int projectid { get; set; }

    public static (string name, int quantity, int value)[] components { get; set; }


    private static bool[] _objectsFound;
    public static event Action<bool[]> OnObjectsFoundChanged;

    public static bool[] ObjectsFound
    {
        get => _objectsFound;
        set
        {
            if (!AreListsEqual(_objectsFound, value))
            {
                _objectsFound = (bool[])value.Clone();
                OnObjectsFoundChanged?.Invoke(_objectsFound);
            }
        }
    }

    private static bool AreListsEqual(bool[] a, bool[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    // Your other fields like `components`, `projectid`, etc.
}



public static class DynamicProjectStaticClass
{
    private static List<string> _components = new List<string>();
    public static event Action<List<string>> OnComponentsChanged;

    public static List<string> components
    {
        get => _components;
        set
        {
            _components = new List<string>(value); // defensive copy
            OnComponentsChanged?.Invoke(_components);
        }
    }

    public static void SetComponents(List<string> newComponents)
    {
        components = newComponents;
    }
}


