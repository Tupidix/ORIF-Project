using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a tag-to-prefab dictionary and instantiates cutlery prefabs by tag.
/// </summary>
public class PrefabCutleryManager : MonoBehaviour
{
    [SerializeField] private List<PrefabEntry> prefabEntries;

    public IReadOnlyList<PrefabEntry> PrefabEntries => prefabEntries.AsReadOnly();

    private Dictionary<string, GameObject> _prefabDictionary;

    /// <summary>
    /// Represents a mapping between a tag and its associated prefab.
    /// </summary>
    [System.Serializable]
    public class PrefabEntry
    {
        public string tag;
        public GameObject prefab;
    }

    void Awake()
    {
        BuildPrefabDictionary();
    }

    /// <summary>
    /// Instantiates a prefab matching the given tag at the specified position and rotation.
    /// </summary>
    /// <param name="tag">The tag identifying which prefab to instantiate.</param>
    /// <param name="position">The world position for the new instance.</param>
    /// <param name="rotation">The rotation for the new instance.</param>
    /// <returns>The instantiated GameObject, or null if the tag is not found.</returns>
    public GameObject InstantiatePrefabByTag(string tag, Vector3 position, Quaternion rotation)
    {
        if (_prefabDictionary.TryGetValue(tag, out var prefab))
            return Instantiate(prefab, position, rotation);

        Debug.LogError($"Prefab with tag '{tag}' not found.");
        return null;
    }

    /// <summary>
    /// Builds the internal dictionary from the serialized prefab entries list.
    /// Duplicate tags are silently ignored.
    /// </summary>
    private void BuildPrefabDictionary()
    {
        _prefabDictionary = new Dictionary<string, GameObject>();
        foreach (var entry in prefabEntries)
        {
            if (!_prefabDictionary.ContainsKey(entry.tag))
                _prefabDictionary.Add(entry.tag, entry.prefab);
        }
    }
}