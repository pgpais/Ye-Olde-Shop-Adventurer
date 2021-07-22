using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class OracleManager : MonoBehaviour
{
    public static string oracleReferenceName = "oracle";
    public static string marketReferenceName = "marketPrices";
    public static string dateFormat = "yyyyMMdd";
    public static OracleManager Instance;

    private MarketPrices marketPrices;
    private OracleData oracleData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        FirebaseCommunicator.LoggedIn.AddListener(OnLoggedIn);
    }

    internal string GetItemName()
    {
        return oracleData.itemName;
    }

    internal Sprite GetItemIcon()
    {
        Item item = ItemManager.instance.itemsData.GetItemByName(oracleData.itemName);
        return item.sprite;
    }

    internal int GetHour()
    {
        return oracleData.bestPriceIndex * 3;
    }

    private void OnLoggedIn()
    {
        GetMarketPrices();

    }

    // Get the current oracle information
    public OracleData GetOracleData()
    {
        string itemName = ItemManager.instance.itemsData.GetRandomUnlockedItem().ItemName;
        int bestPriceIndex = marketPrices.GetBestPriceIndex(itemName);

        oracleData = new OracleData(bestPriceIndex, itemName);
        Debug.Log("Got oracle data, Best price: " + oracleData.bestPriceIndex + " for item: " + oracleData.itemName);

        return oracleData;
    }

    // Get Market Prices for today
    public void GetMarketPrices()
    {
        DateTime today = DateTime.Today;

        FirebaseCommunicator.instance.GetObject(new string[] { marketReferenceName, today.ToString(dateFormat) }, (task) =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to get market prices: " + task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                Debug.Log("yey got oracle data");
                string json = task.Result.GetRawJsonValue();

                marketPrices = new MarketPrices(JsonConvert.DeserializeObject<Dictionary<string, int>[]>(json));
            }
        });
    }


}

[System.Serializable]
public struct OracleData
{
    public int bestPriceIndex;
    public string itemName;

    public OracleData(int bestPriceIndex, string itemName)
    {
        this.bestPriceIndex = bestPriceIndex;
        this.itemName = itemName;
    }
}

[System.Serializable]
public struct MarketPrices
{
    public Dictionary<string, int>[] prices;

    public MarketPrices(Dictionary<string, int>[] prices)
    {
        this.prices = prices;
    }

    public int GetBestPriceIndex(string itemName)
    {
        int bestIndex = -1;
        int bestPrice = int.MinValue;

        for (int i = 0; i < prices.Length; i++)
        {
            if (prices[i].ContainsKey(itemName))
            {
                if (prices[i][itemName] > bestPrice)
                {
                    bestIndex = i;
                    bestPrice = prices[i][itemName];
                }
            }
        }
        return bestIndex;
    }
}

public struct ItemPrice
{
    public string itemName;
    public int priceModifier;
}