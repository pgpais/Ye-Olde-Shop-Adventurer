using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static string diseasedItemReferenceName = "diseasedItems";
    public static string difficultyReferenceName = "difficulty";
    public static UnityEvent NoMissionExists = new UnityEvent();
    public static UnityEvent NewMissionAdded = new UnityEvent();
    public static int maxDifficulty = 40;
    public static int minDifficulty = 0;
    public static int difficultyWinModifier = 3;
    public static int difficultyDeathModifier = -1;

    public static GameManager instance;

    public FamilyStats stats;

    public CharacterClassData currentSelectedClass;

    public int BaseDifficulty { get; private set; }
    public MissionDifficulty DifficultyRank
    {
        get
        {
            if (BaseDifficulty <= 10)
            {
                return MissionDifficulty.Easy;
            }
            else if (BaseDifficulty >= 11 && BaseDifficulty <= 20)
            {
                return MissionDifficulty.Medium;
            }
            else
            {
                return MissionDifficulty.Hard;
            }
        }
    }
    public int NumberOfMissions => stats.stats.numberOfMissions;

    public Mission CurrentMission => currentMission;
    public string DiseasedItemName => diseasedItemName;
    public bool JustFinishedMission => justFinishedMission; //TODO: use this for showing the remaining unlocks popup

    private Mission currentMission;
    [SerializeField] string diseasedItemName;

    LogsManager logsManager;


    private bool performingMission;
    private bool justFinishedMission = false;

    //Don't look here! I was just lazy to find a better way
    #region Silly variables
    private bool alreadyDetectedPause = false;
    #endregion

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        logsManager = LogsManager.instance;


        DontDestroyOnLoad(this);

        LoadSceneAdditivelyAndSetActive(1);

        FirebaseCommunicator.LoggedIn.AddListener(OnLoggedIn);
        FirebaseCommunicator.GameStarted.AddListener(OnGameStarted);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        Localisation.SetLanguage((Language)PlayerPrefs.GetInt("Language", 0));

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneUnloaded(Scene arg0)
    {
        LogsManager.SendLogDirectly(new Log(
            LogType.SceneUnloaded,
            new Dictionary<string, string>(){
                {"scene", arg0.name}
            }
        ));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        LogsManager.SendLogDirectly(new Log(
            LogType.SceneLoaded,
            new Dictionary<string, string>(){
                {"scene", scene.name}
            }
        ));

        if (scene.buildIndex == 1)
        {
            if (justFinishedMission)
            {
                Debug.Log("Arrived from mission");
                justFinishedMission = false;
                MenuManager.instance.ShowUnlocksProgessPopup();
            }
        }
    }

    private void Start()
    {
        LogsManager.SendLogDirectly(new Log(
            LogType.GameStarted,
            new Dictionary<string, string>(),
            DateTime.Now
        ));

        // Setup listener for new missions
        FirebaseCommunicator.instance.SetupListenForChildAddedEvents(new string[] { Mission.firebaseReferenceName, FirebaseCommunicator.instance.FamilyId.ToString() }, OnMissionAdded);
    }

    private void OnLoggedIn()
    {
        stats = new FamilyStats();
        // GetDifficulty();
        SetupDifficultyListener();
    }

    private void OnGameStarted()
    {
        GetMissions();
    }

    void GetMissions()
    {
        FirebaseCommunicator.instance.GetObject(MissionManager.referenceName, (task) =>
              {
                  if (task.IsFaulted)
                  {
                      Debug.LogError("smth went wrong. " + task.Exception.ToString());
                  }

                  if (task.IsCompleted)
                  {
                      Debug.Log("yey got mission");
                      string json = task.Result.GetRawJsonValue();
                      if (json == null)
                      {
                          OnNoMissionExists();
                          return;
                      }

                      currentMission = JsonConvert.DeserializeObject<Mission>(json);
                      NewMissionAdded.Invoke();
                  }
              });
    }

    void GetDiseasedItem()
    {
        FirebaseCommunicator.instance.GetObject(new string[] { diseasedItemReferenceName, DateTime.Today.ToString("yyyyMMdd") }, (task) =>
              {
                  if (task.IsFaulted)
                  {
                      Debug.LogError("smth went wrong. " + task.Exception.ToString());
                      return;
                  }

                  if (task.IsCompleted)
                  {
                      Debug.Log("yey got diseased");
                      string json = task.Result.GetRawJsonValue();
                      if (json == null)
                      {
                          Debug.LogError("No Diseased Item today!");
                          return;
                      }

                      diseasedItemName = JsonConvert.DeserializeObject<string>(json);

                  }
              });
    }

    private void SetupDifficultyListener()
    {
        FirebaseCommunicator.instance.SetupListenForValueChangedEvents(difficultyReferenceName, (obj, args) =>
        {
            string json = args.Snapshot.GetRawJsonValue();
            if (string.IsNullOrEmpty(json))
            {
                Debug.Log("No difficulty found!");
                BaseDifficulty = 0;
            }
            else
            {
                BaseDifficulty = JsonConvert.DeserializeObject<int>(json);
                Debug.Log($"Difficulty set to {BaseDifficulty}");
            }
        });
    }

    void GetDifficulty()
    {
        FirebaseCommunicator.instance.GetObject(difficultyReferenceName, (task) =>
              {
                  if (task.IsFaulted)
                  {
                      Debug.LogError("smth went wrong. " + task.Exception.ToString());
                      return;
                  }

                  if (task.IsCompleted)
                  {
                      Debug.Log("yey got difficulty");
                      string json = task.Result.GetRawJsonValue();
                      if (json == null)
                      {
                          Debug.LogError("No difficulty found!");
                          BaseDifficulty = 0;
                          return;
                      }

                      BaseDifficulty = JsonConvert.DeserializeObject<int>(json);

                  }
              });
    }

    void UploadDifficulty()
    {
        string json = JsonConvert.SerializeObject(BaseDifficulty);
        FirebaseCommunicator.instance.SendObject(json, difficultyReferenceName, (task) =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("smth went wrong. " + task.Exception.ToString());
                return;
            }

            if (task.IsCompleted)
            {
                Debug.Log("CLOUD: Updated Difficulty");
            }
        });
    }

    public void StartRun()
    {
        performingMission = true;

        SceneManager.UnloadSceneAsync(1);

        LoadSceneAdditivelyAndSetActive(2);
    }

    public void FinishLevel(bool playerDied)
    {
        justFinishedMission = true;

        InventoryManager.instance.AddInventoryToGlobalItems();

        if (!playerDied)
        {

            if (QuestManager.instance != null)
            {
                QuestManager.instance.OnLevelFinished();
            }

            if (OfferingManager.instance != null)
            {
                OfferingManager.instance.OnLevelFinished();
            }
        }

        SceneManager.UnloadSceneAsync(2);
        LoadSceneAdditivelyAndSetActive(1);

        if (playerDied)
        {
            FailedMission();
        }
        else
        {
            SuccessfulMission();
        }
    }

    void LoadSceneAdditivelyAndSetActive(int buildIndex)
    {
        var parameters = new LoadSceneParameters();
        parameters.loadSceneMode = LoadSceneMode.Additive;

        SceneManager.LoadScene(buildIndex, parameters);

        SceneManager.sceneLoaded += SetActiveScene;
    }

    void SetActiveScene(Scene scene, LoadSceneMode mode)
    {
        SceneManager.SetActiveScene(scene);
        SceneManager.sceneLoaded -= SetActiveScene;
    }

    void OnNoMissionExists()
    {
        Debug.Log("Mission is null! Disable gameplay until we get a new mission");
        NoMissionExists.Invoke();
    }

    void OnMissionAdded(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("Mission added!");
        GetMissions();
    }

    public void FailedMission()
    {
        LogsManager.SendLogDirectly(new Log(LogType.MissionFail, null));
        UpdateBaseDifficulty(difficultyDeathModifier);

        stats.stats.numberOfMissions++;
        stats.stats.numberOfDeaths++;

        UpdateStats();
    }

    public void SuccessfulMission()
    {
        LogsManager.SendLogDirectly(new Log(LogType.MissionSuccess, null));
        UpdateBaseDifficulty(difficultyWinModifier);

        stats.stats.numberOfMissions++;
        stats.stats.numberOfSuccessfulMissions++;

        UpdateStats();
    }

    private void UpdateStats()
    {
        stats.UpdateStats();
    }

    void UpdateBaseDifficulty(int amount)
    {
        BaseDifficulty += amount;

        BaseDifficulty = Mathf.Clamp(BaseDifficulty, minDifficulty, maxDifficulty);

        UploadDifficulty();
    }

    private void Update()
    {
        // very lazy way to do this.
        if (MoreMountains.TopDownEngine.GameManager.Instance.Paused && !alreadyDetectedPause)
        {
            alreadyDetectedPause = true;
            LogsManager.SendLogDirectly(new Log(
                LogType.Paused
            ));
        }
        else if (!MoreMountains.TopDownEngine.GameManager.Instance.Paused)
        {
            alreadyDetectedPause = false;
        }
    }

    public static void ExitGame()
    {
        Application.Quit();
    }
}

public enum MissionDifficulty
{
    Easy,
    Medium,
    Hard,
}
