using System;
using Newtonsoft.Json;
using UnityEngine;

public class DiseasedManager : MonoBehaviour
{
    public static string referenceName = "diseasedItems";
    public static DiseasedManager instance;

    public string DiseasedItemName => diseased.diseasedItemName;


    [SerializeField] bool testUpload = false;
    [SerializeField] int uploadAmount = 1;
    DiseasedTime diseased;

    DateTime currentDate;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        FirebaseCommunicator.LoggedIn.AddListener(OnLoggedIn);

        DateTime now = DateTime.Now;
        for (int i = 24; i >= 0; i--)
        {
            if (i <= now.Hour)
            {
                currentDate = new DateTime(now.Year, now.Month, now.Day, i, 00, 00);
                break;
            }
        }
    }

    void OnLoggedIn()
    {
        GetDiseasedItem();
    }

    void GetDiseasedItem()
    {
        FirebaseCommunicator.instance.GetObject(new string[] { referenceName, FirebaseCommunicator.instance.FamilyId.ToString(), currentDate.ToString("yyyyMMdd HH") }, (task) =>
              {
                  if (task.IsFaulted)
                  {
                      Debug.LogError("smth went wrong. " + task.Exception.ToString());
                      return;
                  }

                  if (task.IsCompleted)
                  {
                      string json = task.Result.GetRawJsonValue();
                      if (json == null)
                      {
                          Debug.Log("yey got diseased");
                          Debug.LogError("No Diseased Item today! creating a new one");
                          CreateDiseasedItem(currentDate);
                          return;
                      }

                      diseased = JsonConvert.DeserializeObject<DiseasedTime>(json);
                      Debug.Log("yey got diseased - " + diseased.diseasedItemName);

                  }
              });
    }

    private void Update()
    {
        DateTime now = DateTime.Now;
        if (now.Hour >= currentDate.Hour + 8 || now.DayOfYear > currentDate.DayOfYear)
        {
            currentDate = currentDate.AddHours(8);
            CreateDiseasedItem(currentDate);
        }


        if (testUpload)
        {
            DateTime dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            for (int i = 0; i < uploadAmount; i++)
            {
                CreateDiseasedItem(dateTime + new TimeSpan(8 * i, 0, 0));
            }

            testUpload = false;
        }
    }

    public void CreateDiseasedItem(DateTime diseaseDate)
    {
        var item = ItemManager.instance.itemsData.GetRandomItem((item) => item.Type == Item.ItemType.Gatherable && item.Unlocked);
        DiseasedTime newDiseased = new DiseasedTime(item.ItemName, diseaseDate.ToString("yyyyMMdd HH"));

        string json = JsonConvert.SerializeObject(newDiseased);

        FirebaseCommunicator.instance.SendObject(json, new string[] { referenceName, FirebaseCommunicator.instance.FamilyId.ToString(), diseaseDate.ToString("yyyyMMdd HH") }, (task) =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to create new diseased item! Message:" + task.Exception.Message);
                return;
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Created new Diseased item!");
                GetDiseasedItem();
            }
        });
    }

    struct DiseasedTime
    {
        public string diseasedItemName;
        public string diseaseDate;

        public DiseasedTime(string diseasedItemName, string diseaseDate)
        {
            this.diseasedItemName = diseasedItemName;
            this.diseaseDate = diseaseDate;
        }
    }
}