using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Represents a complete training scenario including menus, validators and timing.
/// </summary>
[System.Serializable]
public class Scenario
{
    public string name;
    public string description;
    public Mode mode;
    public Level level;
    public Menu[] menus;
    public Validator validator;
    public float allowedTime;
    public Drink[] drinks;
    public Extra[] extras;

    public enum Mode { Learning, Training, Exam }
    public enum Level { Begginner, Medium, Expert }

    /// <summary>
    /// Prints the full scenario contents to the Unity console.
    /// </summary>
    public void Print()
    {
        var sb = new StringBuilder();
        AppendHeader(sb);
        AppendDrinks(sb);
        AppendExtras(sb);
        AppendMenus(sb);
        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// Appends the scenario header information to the StringBuilder.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    private void AppendHeader(StringBuilder sb)
    {
        sb.AppendLine($"Scenario Name: {name}");
        sb.AppendLine($"Description: {description}");
        sb.AppendLine($"Mode: {mode}");
        sb.AppendLine($"Level: {level}");
        sb.AppendLine("Validator:");
        sb.AppendLine($"  Position: {validator.position}");
        sb.AppendLine($"  Rotation: {validator.rotation}");
        sb.AppendLine($"  Order: {validator.order}");
        sb.AppendLine($"Allowed time : {allowedTime}");
    }

    /// <summary>
    /// Appends the drinks section to the StringBuilder.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    private void AppendDrinks(StringBuilder sb)
    {
        sb.AppendLine("Drinks:");
        foreach (var drink in drinks)
        {
            sb.AppendLine($"  Type: {drink.type}");
            if (!string.IsNullOrEmpty(drink.name))
                sb.AppendLine($"  Name: {drink.name}");
        }
    }

    /// <summary>
    /// Appends the extras section to the StringBuilder.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    private void AppendExtras(StringBuilder sb)
    {
        sb.AppendLine("Extras:");
        foreach (var extra in extras)
        {
            sb.AppendLine($"  Tag: {extra.tag}");
            sb.AppendLine($"  Name: {extra.name}");
        }
    }

    /// <summary>
    /// Appends the menus section to the StringBuilder.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    private void AppendMenus(StringBuilder sb)
    {
        sb.AppendLine("Menus:");
        foreach (var menu in menus)
        {
            sb.AppendLine($"  Menu Name: {menu.name}");
            sb.AppendLine($"  Description: {menu.description}");
            sb.AppendLine($"  Appetizer: {menu.appetizer}");
            sb.AppendLine($"  Main course: {menu.mainCourse}");
            sb.AppendLine($"  Dessert: {menu.dessert}");
            AppendCutleryDetails(sb, menu);
        }
    }

    /// <summary>
    /// Appends the cutlery details of a single menu to the StringBuilder.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    /// <param name="menu">The menu whose cutlery details to append.</param>
    private void AppendCutleryDetails(StringBuilder sb, Menu menu)
    {
        sb.AppendLine($"  Cutlery to Place:");
        foreach (var cutleryEntry in menu.cutleryToPlace)
        {
            var cutlery = cutleryEntry.Value;
            sb.AppendLine($"    Tag: {cutlery.tag}");
            sb.AppendLine($"    Parent: {cutlery.parent}");
            sb.AppendLine($"    Placement Order Index: {cutlery.placementOrderIndex}");
            sb.AppendLine($"    Offset Position: {cutlery.offset.position}");
            sb.AppendLine($"    Offset Rotation: {cutlery.offset.rotation}");
        }
    }
}

/// <summary>
/// Represents a drink with a type and optional name.
/// </summary>
[System.Serializable]
public class Drink
{
    public string type;
    public string name;
}

/// <summary>
/// Represents an extra item with a name and tag.
/// </summary>
[System.Serializable]
public class Extra
{
    public string name;
    public string tag;
}

/// <summary>
/// Represents a menu containing courses and the cutlery to place.
/// </summary>
[System.Serializable]
public class Menu
{
    public string name;
    public string description;
    public string appetizer;
    public string mainCourse;
    public string dessert;

    [JsonConverter(typeof(DictionaryConverter<string, Cutlery>))]
    public Dictionary<string, Cutlery> cutleryToPlace;
}

/// <summary>
/// Represents a single cutlery item with its parent, placement order and offset.
/// </summary>
[System.Serializable]
public class Cutlery
{
    public string tag;
    public string parent;
    public int placementOrderIndex;
    public Offset offset;
}

/// <summary>
/// Represents a serializable position and rotation offset.
/// </summary>
[System.Serializable]
public class Offset
{
    public Vector3Serializable position;
    public QuaternionSerializable rotation;
}

/// <summary>
/// Defines which validation criteria are enabled for a scenario.
/// </summary>
[System.Serializable]
public class Validator
{
    public bool position;
    public bool rotation;
    public bool order;
}

/// <summary>
/// JSON-serializable wrapper for Vector3 with implicit conversion operators.
/// </summary>
[System.Serializable]
public class Vector3Serializable
{
    public float x;
    public float y;
    public float z;

    /// <summary>
    /// Converts a Vector3Serializable to a Unity Vector3.
    /// </summary>
    /// <param name="v">The serializable vector.</param>
    public static implicit operator Vector3(Vector3Serializable v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    /// <summary>
    /// Converts a Unity Vector3 to a Vector3Serializable.
    /// </summary>
    /// <param name="v">The Unity vector.</param>
    public static implicit operator Vector3Serializable(Vector3 v)
    {
        return new Vector3Serializable { x = v.x, y = v.y, z = v.z };
    }
}

/// <summary>
/// JSON-serializable wrapper for Quaternion with implicit conversion operators.
/// </summary>
[System.Serializable]
public class QuaternionSerializable
{
    public float x;
    public float y;
    public float z;
    public float w;

    /// <summary>
    /// Converts a QuaternionSerializable to a Unity Quaternion.
    /// </summary>
    /// <param name="q">The serializable quaternion.</param>
    public static implicit operator Quaternion(QuaternionSerializable q)
    {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }

    /// <summary>
    /// Converts a Unity Quaternion to a QuaternionSerializable.
    /// </summary>
    /// <param name="q">The Unity quaternion.</param>
    public static implicit operator QuaternionSerializable(Quaternion q)
    {
        return new QuaternionSerializable { x = q.x, y = q.y, z = q.z, w = q.w };
    }
}

/// <summary>
/// Records the result of a single cutlery placement action during a scenario.
/// </summary>
public class ScenarioAction
{
    public bool isPositionCorrect;
    public bool isRotationCorrect;
    public bool isOrderCorrect;
    public string tag;
    public Vector3 position;
    public Quaternion rotation;
    public int order;

    /// <summary>
    /// Creates a new ScenarioAction with all validation results.
    /// </summary>
    /// <param name="positionCorrect">Whether the position was correct.</param>
    /// <param name="rotationCorrect">Whether the rotation was correct.</param>
    /// <param name="orderCorrect">Whether the placement order was correct.</param>
    /// <param name="tag">The cutlery tag identifier.</param>
    /// <param name="position">The world position where the item was placed.</param>
    /// <param name="rotation">The rotation of the placed item.</param>
    /// <param name="order">The order index of the placed item.</param>
    public ScenarioAction(bool positionCorrect, bool rotationCorrect, bool orderCorrect, string tag, Vector3 position, Quaternion rotation, int order)
    {
        isPositionCorrect = positionCorrect;
        isRotationCorrect = rotationCorrect;
        isOrderCorrect = orderCorrect;
        this.tag = tag;
        this.position = position;
        this.rotation = rotation;
        this.order = order;
    }
}

/// <summary>
/// Immutable snapshot of a Transform's position, rotation and scale.
/// </summary>
[System.Serializable]
public class TransformSnapshot
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    /// <summary>
    /// Captures a snapshot from an existing Transform.
    /// </summary>
    /// <param name="transform">The source Transform to capture.</param>
    public TransformSnapshot(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
    }

    /// <summary>
    /// Creates a snapshot from explicit values.
    /// </summary>
    /// <param name="position">The world position.</param>
    /// <param name="rotation">The rotation.</param>
    /// <param name="scale">The local scale.</param>
    public TransformSnapshot(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }

    /// <summary>
    /// Applies this snapshot's values to a target Transform.
    /// </summary>
    /// <param name="transform">The Transform to overwrite.</param>
    public void ApplyToTransform(Transform transform)
    {
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
    }
}



