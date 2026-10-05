using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Scenarios
{
    /// <summary>
    /// Loads scenario data from local resources or a remote server.
    /// </summary>
    public class JSONLoader : MonoBehaviour
    {
        public List<Scenario> _scenarios;

        private const string ScenariosResourceFolder = "Scenarios";

        void Awake()
        {
            LoadScenariosLocal();
        }

        /// <summary>
        /// Loads all scenario JSON files from the local Resources/Scenarios folder.
        /// </summary>
        public void LoadScenariosLocal()
        {
            _scenarios = new List<Scenario>();
            TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>(ScenariosResourceFolder);

            if (jsonFiles.Length == 0)
            {
                Debug.LogError("Failed to load JSON files from the Scenarios folder!");
                return;
            }

            var settings = CreateJsonSettings();
            foreach (var jsonFile in jsonFiles)
            {
                var scenario = DeserializeScenario(jsonFile.text, settings);
                if (scenario != null)
                    _scenarios.Add(scenario);
            }
        }

        /// <summary>
        /// Coroutine that fetches scenario data from a remote HTTP server.
        /// </summary>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator LoadScenarioServer()
        {
            _scenarios = new List<Scenario>();
            // VERY IMPORTANT IF YOU WANT TO USE THE WEBSERVER DATA !
            string ip = "SET_YOUR_IP_HERE";
            string url = $"http://{ip}:5000/api/unityScenarios";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error fetching scenarios from server: {webRequest.error}");
                    yield break;
                }

                ParseServerResponse(webRequest.downloadHandler.text);
            }
        }

        /// <summary>
        /// Parses a JSON response string into a list of scenarios and appends them.
        /// </summary>
        /// <param name="jsonResponse">The raw JSON string from the server.</param>
        private void ParseServerResponse(string jsonResponse)
        {
            print(jsonResponse);

            try
            {
                var settings = CreateJsonSettings();
                var fetchedScenarios = JsonConvert.DeserializeObject<List<Scenario>>(jsonResponse, settings);

                if (fetchedScenarios == null)
                {
                    Debug.LogError("Failed to parse scenarios from server.");
                    return;
                }

                foreach (var scenario in fetchedScenarios)
                    scenario.Print();

                _scenarios.AddRange(fetchedScenarios);
                Debug.Log("Scenarios loaded from server successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error deserializing server response: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns the list of loaded scenarios.
        /// </summary>
        /// <returns>The list of deserialized Scenario objects.</returns>
        public List<Scenario> GetLoadedScenarios()
        {
            return _scenarios;
        }

        /// <summary>
        /// Creates a JsonSerializerSettings instance with the custom dictionary converter.
        /// </summary>
        /// <returns>A configured JsonSerializerSettings.</returns>
        private JsonSerializerSettings CreateJsonSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new DictionaryConverter<string, Cutlery>());
            return settings;
        }

        /// <summary>
        /// Deserializes a single JSON string into a Scenario object.
        /// </summary>
        /// <param name="json">The JSON text to deserialize.</param>
        /// <param name="settings">The serializer settings to use.</param>
        /// <returns>The deserialized Scenario, or null on failure.</returns>
        private Scenario DeserializeScenario(string json, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<Scenario>(json, settings);
        }
    }
}