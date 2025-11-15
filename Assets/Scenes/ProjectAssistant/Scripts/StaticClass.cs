using System.Collections.Generic;
using PassthroughCameraSamples.StartScene;

public static class StaticClass
{
    public static int projectid = -1;
    public static bool RestartInventory = false;

    public static ComponentList Components = new ComponentList

    
    {
        components = new List<Component>()
    };
    
    public static void Reset()
    {
        projectid = -1;
        Components = new ComponentList
        {
            components = new List<Component>()
        };
    }

    public static void AddComponentQuantity(string item, int quantity)
    {
        
        var component = Components.components.Find(c => c.item == item);
        if (component != null)
        {
            component.quantity += quantity;
        }
        else
        {
            Components.components.Add(new Component { item = item, quantity = quantity });
        }
    }
}