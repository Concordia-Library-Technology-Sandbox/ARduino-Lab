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
}