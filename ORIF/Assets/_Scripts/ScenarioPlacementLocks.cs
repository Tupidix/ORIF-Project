using System.Collections.Generic;

/// <summary>Tracks finalized placement results for each menu of one session.</summary>
public sealed class ScenarioPlacementLocks
{
    private readonly List<Dictionary<string, bool>> _menus = new List<Dictionary<string, bool>>();
    public int MenuCount => _menus.Count;

    public void Reset() => _menus.Clear();
    public void AddMenu() => _menus.Add(new Dictionary<string, bool>());
    public bool IsLocked(int menuIndex, string tag) => _menus[menuIndex].ContainsKey(tag);

    public void LockCorrect(int menuIndex, string tag)
    {
        // Preserve duplicate detection rather than silently replacing an existing result.
        _menus[menuIndex].Add(tag, true);
    }

    public void FinalizeMissing(int menuIndex, IEnumerable<string> expectedTags)
    {
        foreach (string tag in expectedTags)
        {
            if (!_menus[menuIndex].ContainsKey(tag))
                _menus[menuIndex].Add(tag, false);
        }
    }

    public int TotalLockedCount
    {
        get
        {
            int count = 0;
            foreach (var menu in _menus)
                count += menu.Count;
            return count;
        }
    }

    public bool IsMenuCorrect(int menuIndex, int latestActionCount)
    {
        var menu = _menus[menuIndex];
        if (menu.Count != latestActionCount)
            return false;
        foreach (bool correct in menu.Values)
        {
            if (!correct)
                return false;
        }
        return true;
    }

    public IEnumerable<string> GetLockedTags(int menuIndex) => _menus[menuIndex].Keys;
}
