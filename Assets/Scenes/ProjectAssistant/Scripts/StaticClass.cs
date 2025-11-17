using System.Collections.Generic;
using PassthroughCameraSamples.StartScene;

public static class StaticClass
{
    public static int projectid = -1;
    public static bool RestartInventory = false;

    public static string projectTitle = "";

    public static string projectDescription = "";
    
    public static ComponentList Components = new ComponentList

    
    {
        components = new List<Component>()
    };


    
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

    public static string generateCompoundStringOfComponents()
    {
        string result = "";
        foreach (var component in Components.components)
        {
            if (component.quantity > 0){
             result += component.item + " x" + component.quantity + "\n";
            }
        }

        return result;
    }
}