// Author: Gabriel Armas

using System;
using System.Collections.Generic;

namespace PassthroughCameraSamples.StartScene
{
    // -------------------------------------------------------------------------
    // PROJECT DATA STRUCTURES
    // -------------------------------------------------------------------------

    /// <summary>
    /// Root container representing the full list of projects
    /// loaded from Resources/projects.json.
    /// </summary>
    [Serializable]
    public class ProjectList
    {
        public List<Project> projects;
    }

    /// <summary>
    /// Represents a single Arduino project idea.
    /// Includes:
    /// - name: project title
    /// - description: long text description
    /// - components: list of required electronic components
    /// </summary>
    [Serializable]
    public class Project
    {
        public string name;
        public string description;
        public List<Component> components;
    }


    // -------------------------------------------------------------------------
    // COMPONENT DATA STRUCTURES
    // -------------------------------------------------------------------------

    /// <summary>
    /// A standalone container used in places where
    /// only a component list is needed (inventory, scanning results, etc.).
    /// </summary>
    [Serializable]
    public class ComponentList
    {
        public List<Component> components;
    }

    /// <summary>
    /// Represents a single electronic component.
    ///
    /// Fields:
    /// - item: component name (matches filenames in 2dmod/ and 3DI/)
    /// - quantity: how many units of this component
    /// - value: optional numeric value (e.g., resistor in ohms)
    /// </summary>
    [Serializable]
    public class Component
    {
        public string item;
        public int quantity;

        /// <summary>
        /// Optional numeric value for the component.
        /// Example: 220 for a 220Ω resistor.
        /// If unused, defaults to 0.
        /// </summary>
        public int value;
    }


    // -------------------------------------------------------------------------
    // TIPS DATA STRUCTURES
    // -------------------------------------------------------------------------

    /// <summary>
    /// Root container for a list of helpful tips shown in menus.
    /// Loaded from Resources/tips.json.
    /// </summary>
    [Serializable]
    public class TipList
    {
        public List<Tip> tips;
    }

    /// <summary>
    /// Represents a single "friendly tip" message.
    /// - text: the actual tip shown to the user
    /// - category: optional categorization (e.g., beginner, safety, info)
    /// </summary>
    [Serializable]
    public class Tip
    {
        public string text;
        public string category;
    }
}
