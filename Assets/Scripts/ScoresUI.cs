using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script attached to the root RectTransform of the scores menu.
/// Handles UI events and State changes.
/// </summary>
public class ScoresUI : StateController.StateListener
{
    [SerializeField]
    [Tooltip("Prefab used to create normal score list entries.")]
    private GameObject _scoreEntryPrefab;
    
    [SerializeField]
    [Tooltip("Prefab used to create editable score list entries.")]
    private GameObject _scoreEntryEditablePrefab;
    
    private GameObject _scoresList;
    private List<Score> _scores = new List<Score>();
    
    private InputField _nameInput;
    private Score _currentScore;

    private string _savePath;
    
    /// <summary>
    /// Called during initialisation by the StateController.
    /// Scripts can subscribe to State change events in this method.
    /// </summary>
    public override void RegisterEvents()
    {
        // Init score list and data path here to ensure they are available when events are called
        _scoresList = transform.Find("ScoresList/Viewport/Content").gameObject;
        _savePath = Application.persistentDataPath + "/scores.json";
        Load();
        
        StateController.OnStateChange += (oldState, newState) =>
        {
            if (oldState == StateController.State.Scores) OnScoresHide();
            if (newState == StateController.State.Scores) OnScoresShow();
        };
    }

    private void Start()
    {
        if (StateController.CurrentState != StateController.State.Scores)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called on State change:
    /// Any -> Scores
    /// </summary>
    private void OnScoresShow() 
    {
        gameObject.SetActive(true);

        // If we got here from game end, add the new score entry
        if (StateController.PendingScore >= 0)
        {
            _scores.Add(new Score(null, StateController.PendingScore));
            transform.Find("Title").GetComponent<Text>().text = "Game Over";
        }
        else
        {
            transform.Find("Title").GetComponent<Text>().text = "Scores";
        }
        
        // Sort entries by score
        _scores.Sort((a, b) => b.Value - a.Value);
        
        // Init UI for all entries
        foreach (var score in _scores)
        {
            // If new score, make it editable
            if (score.Name == null)
            {
                var entry = Instantiate(_scoreEntryEditablePrefab, Vector3.zero, Quaternion.identity, _scoresList.transform);
                entry.transform.Find("ScoreText").GetComponent<Text>().text = score.Value.ToString();
                _nameInput = entry.transform.Find("NameInput").GetComponent<InputField>();
                _currentScore = score;
            }
            else
            {
                var entry = Instantiate(_scoreEntryPrefab, Vector3.zero, Quaternion.identity, _scoresList.transform);
                entry.transform.Find("NameText").GetComponent<Text>().text = score.Name;
                entry.transform.Find("ScoreText").GetComponent<Text>().text = score.Value.ToString();
            }
        }
    }

    /// <summary>
    /// Called on State change:
    /// Scores -> Any
    /// </summary>
    private void OnScoresHide()
    {
        gameObject.SetActive(false);

        // Apply new score if present
        if (_currentScore != null)
        {
            _currentScore.Name = _nameInput.text;
            if (_currentScore.Name.Length == 0) _currentScore.Name = "Unnamed";
            Save();
        }

        // Reset selection
        _currentScore = null;
        _nameInput = null;
        
        // Clear list UI
        for (int i = 0; i < _scoresList.transform.childCount; i++)
        {
            Destroy(_scoresList.transform.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// Event handler for "Back" button.
    /// </summary>
    public void ToMenu()
    {
        StateController.PendingScore = -1;
        StateController.SwitchTo(StateController.State.Menu);
    }

    /// <summary>
    /// Data container for a score entry.
    /// </summary>
    [Serializable]
    public class Score
    {
        public string Name;
        public int Value;

        public Score(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// Data container for a collection of score entries.
    /// </summary>
    [Serializable]
    public class ScoreData
    {
        public List<Score> Scores;

        public ScoreData(List<Score> scores)
        {
            Scores = scores;
        }
    }

    /// <summary>
    /// Load score data from local file.
    /// </summary>
    private void Load()
    {
        if (!File.Exists(_savePath)) return;
        _scores = JsonUtility.FromJson<ScoreData>(File.ReadAllText(_savePath)).Scores;
    }

    /// <summary>
    /// Save score data to local file.
    /// </summary>
    private void Save()
    {
        File.WriteAllText(_savePath, JsonUtility.ToJson(new ScoreData(_scores)));
    }
}