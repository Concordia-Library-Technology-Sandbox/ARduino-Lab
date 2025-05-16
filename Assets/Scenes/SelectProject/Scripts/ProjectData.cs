using System;
using System.Collections.Generic;

namespace PassthroughCameraSamples.StartScene
{
    [Serializable]
    public class ProjectList
    {
        public List<Project> projects;
    }

    [Serializable]
    public class Project
    {
        public string name;
        public string description;
        public List<Component> components;
    }

    [Serializable]
    public class Component
    {
        public string item;
        public int quantity;

        public int value;
    }

    [Serializable]
    public class TipList
    {
        public List<Tip> tips;
    }
    [Serializable]
    public class Tip{
        public string text;
        public string category;
    }
}