using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JobSheetUserInterfaceUpdater : MonoBehaviour
{
    [SerializeField] private Scenario _scenario;
    [SerializeField] private TextMeshProUGUI txtClientName;
    [SerializeField] private TextMeshProUGUI txtClientPhone;
    [SerializeField] private TextMeshProUGUI txtClientEmail;
    [SerializeField] private TextMeshProUGUI txtDate;
    [SerializeField] private TextMeshProUGUI txtWeek;
    [SerializeField] private TextMeshProUGUI txtPlace;
    [SerializeField] private TextMeshProUGUI txtPersonCount;
    [SerializeField] private GameObject prefabMenuJobSheet;
    [SerializeField] private Transform menuParent;
    [SerializeField] private Transform drinksParent;
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private TextMeshProUGUI txtExtras;

    private List<GameObject> _menuInstances = new List<GameObject>();
    private List<GameObject> drinkInstances = new List<GameObject>();

    private const float MenuInitialOffset = -0.0152f;
    private const float MenuOffsetStep = -0.0807f;

    public Scenario Scenario { get => _scenario; set => _scenario = value; }

    /// <summary>
    /// Refreshes all job sheet UI sections from the current Scenario data.
    /// </summary>
    public void UpdateUI()
    {
        print("JobSheetUserInterfaceUpdater : UpdateUI");
        UpdateClientInfo();
        UpdateDateInfo();
        UpdatePlaceInfo();
        UpdatePersonCount();
        RecreateMenuInstances();
        UpdateDrinks();
    }

    /// <summary>
    /// Fills the client information fields with placeholder data.
    /// </summary>
    private void UpdateClientInfo()
    {
        txtClientName.text = "John Doe";
        txtClientPhone.text = "022.000.00.00";
        txtClientEmail.text = "john.doe@mail.com";
    }

    /// <summary>
    /// Updates the date and week number fields with the current date.
    /// </summary>
    private void UpdateDateInfo()
    {
        var currentDate = System.DateTime.Now;
        txtDate.text = currentDate.ToString("dd.MM.yyyy");
        txtWeek.text = GetWeekNumber(currentDate).ToString();
    }

    /// <summary>
    /// Returns the ISO week number for the given date.
    /// </summary>
    /// <param name="date">The date to compute the week number for.</param>
    /// <returns>The week number of the year.</returns>
    private int GetWeekNumber(System.DateTime date)
    {
        return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            date,
            System.Globalization.CalendarWeekRule.FirstFourDayWeek,
            System.DayOfWeek.Monday);
    }

    /// <summary>
    /// Updates the place field with a random table number.
    /// </summary>
    private void UpdatePlaceInfo()
    {
        int tableNumber = Random.Range(1, 51);
        txtPlace.text = $"Table {tableNumber} au restaurant";
    }

    /// <summary>
    /// Updates the person count field from the number of menus.
    /// </summary>
    private void UpdatePersonCount()
    {
        txtPersonCount.text = Scenario.menus.Length.ToString();
    }

    /// <summary>
    /// Destroys existing menu instances and recreates them from the scenario data.
    /// </summary>
    private void RecreateMenuInstances()
    {
        DestroyInstances(_menuInstances);
        float offset = MenuInitialOffset;
        foreach (var menu in Scenario.menus)
        {
            var instance = CreateMenuInstance(menu, offset);
            _menuInstances.Add(instance);
            offset += MenuOffsetStep;
        }
    }

    /// <summary>
    /// Instantiates a single menu entry and positions it in the job sheet.
    /// </summary>
    /// <param name="menu">The menu data to display.</param>
    /// <param name="offset">The local Z offset for positioning.</param>
    /// <returns>The instantiated menu GameObject.</returns>
    private GameObject CreateMenuInstance(Menu menu, float offset)
    {
        var menuInstance = Instantiate(prefabMenuJobSheet, menuParent);
        menuInstance.transform.localPosition = new Vector3(0, 0, offset);
        FillMenuTexts(menuInstance, menu);
        return menuInstance;
    }

    /// <summary>
    /// Fills the text fields of a menu instance from the menu data.
    /// </summary>
    /// <param name="menuInstance">The instantiated menu prefab.</param>
    /// <param name="menu">The source menu data.</param>
    private void FillMenuTexts(GameObject menuInstance, Menu menu)
    {
        foreach (var text in menuInstance.GetComponentsInChildren<TextMeshProUGUI>())
        {
            switch (text.name)
            {
                case "txtMenu":           text.text = menu.name;       break;
                case "txtMenuAppetizer":  text.text = menu.appetizer;  break;
                case "txtMenuMainCourse": text.text = menu.mainCourse; break;
                case "txtMenuDessert":    text.text = menu.dessert;    break;
            }
        }
    }

    /// <summary>
    /// Destroys existing drink elements and recreates them from the scenario data.
    /// </summary>
    private void UpdateDrinks()
    {
        print("UpdateDrinks");
        DestroyInstances(drinkInstances);
        if (!HasDrinks())
        {
            Debug.LogWarning("Aucune boisson n'est définie dans le scénario.");
            CreateTextElement(drinksParent, "Aucune boisson n'est présente dans le scénario.", drinkInstances);
            return;
        }
        foreach (var drink in Scenario.drinks)
        {
            if (drink == null) continue;
            string label = FormatDrinkLabel(drink);
            CreateTextElement(drinksParent, label, drinkInstances);
        }
    }

    /// <summary>
    /// Returns whether the scenario has any drinks defined.
    /// </summary>
    /// <returns>True if drinks are present and non-empty.</returns>
    private bool HasDrinks()
    {
        return Scenario.drinks != null && Scenario.drinks.Length > 0;
    }

    /// <summary>
    /// Formats a drink into a display label based on its type.
    /// </summary>
    /// <param name="drink">The drink to format.</param>
    /// <returns>A formatted label string.</returns>
    private string FormatDrinkLabel(Drink drink)
    {
        return drink.type switch
        {
            "Eau minérale" => "Eau minérale",
            "Vin blanc"    => $"Vin blanc, {drink.name}",
            "Vin rouge"    => $"Vin rouge, {drink.name}",
            _              => drink.name
        };
    }

    /// <summary>
    /// Updates the extras text field from the scenario data.
    /// </summary>
    private void UpdateExtras()
    {
        print("UpdateExtras");
        if (txtExtras == null)
        {
            Debug.LogError("Le champ texte pour les extras n'est pas assigné dans l'inspecteur.");
            return;
        }
        if (!HasExtras())
        {
            Debug.LogWarning("Aucun extra n'est défini dans le scénario.");
            txtExtras.text = "Aucun n'extra n'est présent dans le scénario.";
            return;
        }
        txtExtras.text = BuildExtrasText();
    }

    /// <summary>
    /// Returns whether the scenario has any extras defined.
    /// </summary>
    /// <returns>True if extras are present and non-empty.</returns>
    private bool HasExtras()
    {
        return Scenario.extras != null && Scenario.extras.Length > 0;
    }

    /// <summary>
    /// Builds a comma-separated string of all extra names.
    /// </summary>
    /// <returns>A formatted string listing all extras.</returns>
    private string BuildExtrasText()
    {
        var names = new List<string>();
        foreach (var extra in Scenario.extras)
        {
            if (extra != null)
                names.Add(extra.name);
        }
        return string.Join(", ", names);
    }

    /// <summary>
    /// Destroys all GameObjects in the list and clears it.
    /// </summary>
    /// <param name="instances">The list of instances to destroy.</param>
    private void DestroyInstances(List<GameObject> instances)
    {
        foreach (var instance in instances)
        {
            Destroy(instance);
        }
        instances.Clear();
    }

    /// <summary>
    /// Instantiates a text prefab, sets its content, and tracks it in the instance list.
    /// </summary>
    /// <param name="parent">The parent transform for the new text element.</param>
    /// <param name="text">The text content to display.</param>
    /// <param name="instanceList">The list to track the created instance.</param>
    private void CreateTextElement(Transform parent, string text, List<GameObject> instanceList)
    {
        var newText = Instantiate(textPrefab, parent);
        var tmp = newText.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
            tmp.text = text;
        instanceList.Add(newText);
    }
}
