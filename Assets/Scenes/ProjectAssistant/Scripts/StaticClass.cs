// Author: Gabriel Armas

using System.Collections.Generic;
using PassthroughCameraSamples.StartScene;

public static class StaticClass
{
    // -------------------------------------------------------------------------
    // PROJECT STATE
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tracks the current project ID. -1 means unset / no project selected.
    /// </summary>
    public static int projectid = -1;

    /// <summary>
    /// Indicates whether the inventory should be cleared when loading the next scene.
    /// </summary>
    public static bool RestartInventory = false;

    /// <summary>
    /// The selected project title stored temporarily between scenes.
    /// </summary>
    public static string projectTitle = "";

    /// <summary>
    /// The selected project description stored temporarily between scenes.
    /// </summary>
    public static string projectDescription = "";

    // -------------------------------------------------------------------------
    // INVENTORY DATA
    // -------------------------------------------------------------------------

    /// <summary>
    /// Stores the global Arduino component inventory.
    /// Initialized with an empty component list.
    /// </summary>
    public static ComponentList Components = new ComponentList
    {
        components = new List<Component>()
    };

    // -------------------------------------------------------------------------
    // RESETTERS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Completely clears all stored project metadata and resets the inventory.
    /// Does NOT set RestartInventory automatically.
    /// </summary>
    public static void Reset()
    {
        projectid = -1;
        projectTitle = "";
        projectDescription = "";

        Components = new ComponentList
        {
            components = new List<Component>()
        };
    }

    // -------------------------------------------------------------------------
    // INVENTORY MANAGEMENT
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds quantity to an existing component.  
    /// If the component doesn't exist yet, it is created.
    /// </summary>
    /// <param name="item">Component identifier string (e.g., "led").</param>
    /// <param name="quantity">Quantity to be added.</param>
    public static void AddComponentQuantity(string item, int quantity)
    {
        if (Components.components == null)
            Components.components = new List<Component>();

        var component = Components.components.Find(c => c.item == item);

        if (component != null)
        {
            component.quantity += quantity;
        }
        else
        {
            Components.components.Add(new Component
            {
                item = item,
                quantity = quantity
            });
        }
    }

    // -------------------------------------------------------------------------
    // STRING BUILDING HELPERS
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a human-readable list of components with quantity > 0.
    /// Example output:
    ///   arduino x1
    ///   led x3
    ///   diode x2
    /// </summary>
    public static string generateCompoundStringOfComponents()
    {
        if (Components.components == null || Components.components.Count == 0)
            return "";

        string result = "";

        foreach (var component in Components.components)
        {
            if (component.quantity > 0)
            {
                result += $"{component.item} x{component.quantity}\n";
            }
        }

        return result;
    }
}
