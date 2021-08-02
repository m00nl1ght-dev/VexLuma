using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScoresUI : StateController.StateListener
{
    [SerializeField]
    private GameObject _scoreEntryPrefab;
    
    [SerializeField]
    private GameObject _scoreEntryEditablePrefab;
    
    private GameObject _scoresList;
    private List<Score> _scores = new List<Score>();
    
    private InputField _nameInput;
    private Score _currentScore;

    private string _savePath;
    
    public override void RegisterEvents()
    {
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

    private void OnScoresShow() 
    {
        gameObject.SetActive(true);

        if (StateController.PendingScore >= 0)
        {
            _scores.Add(new Score(null, StateController.PendingScore));
            transform.Find("Title").GetComponent<Text>().text = "Game Over";
        }
        else
        {
            transform.Find("Title").GetComponent<Text>().text = "Scores";
        }
        
        _scores.Sort((a, b) => b.Value - a.Value);
        
        foreach (var score in _scores)
        {
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

    private void OnScoresHide()
    {
        gameObject.SetActive(false);

        if (_currentScore != null)
        {
            _currentScore.Name = _nameInput.text;
            if (_currentScore.Name.Length == 0) _currentScore.Name = "Unnamed";
            Save();
        }

        _currentScore = null;
        _nameInput = null;
        
        for (int i = 0; i < _scoresList.transform.childCount; i++)
        {
            Destroy(_scoresList.transform.GetChild(i).gameObject);
        }
    }
    
    public void ToMenu()
    {
        StateController.PendingScore = -1;
        StateController.SwitchTo(StateController.State.Menu);
    }

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

    [Serializable]
    public class ScoreData
    {
        public List<Score> Scores;

        public ScoreData(List<Score> scores)
        {
            Scores = scores;
        }
    }

    private void Load()
    {
        if (!File.Exists(_savePath)) return;
        _scores = JsonUtility.FromJson<ScoreData>(File.ReadAllText(_savePath)).Scores;
    }

    private void Save()
    {
        File.WriteAllText(_savePath, JsonUtility.ToJson(new ScoreData(_scores)));
        // File.Delete(_savePath);
    }
}