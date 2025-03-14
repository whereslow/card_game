using Godot;
using System;
using System.Collections.Generic;

public class CardObject
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public string Symbol { get; set; }
    public string Description { get; set; }

    public uint life { get; set; }
    public uint attack { get; set; }
    public string type { get; set; }
}
public partial class CardLoader : Node
{
    

    public List<CardObject> cardList = new List<CardObject>();
    public List<Tuple<uint, string>> cardLibrary = new List<Tuple<uint, string>>(); // 序号 - id号 - 花色
    public Dictionary<uint, CardObject> cardDict = new Dictionary<uint, CardObject>();
    public Dictionary<string, HashSet<int>> cardNumberPool = new Dictionary<string, HashSet<int>>(); // 牌的序号池
    public int countOfOneSymbol;
    // 种子
    private int seed;
    private Random rng;
    public void syncSeed(int seed)
    {
        this.seed = seed;
        this.rng = new Random(seed);
    }
    public CardLoader()
    {
        LoadCardList("res://card_res/cards.json");
        // 初始化
        initNumberPool();
    }

    public CardObject GetCardById(uint id)
    {
        return cardDict[id];
    }
    // 发牌
    public Tuple<uint, uint, string> NextCard()
    {
        // 洗牌
        ShufflesLibrary();
        // 抽取牌库
        var card = cardLibrary[0];
        cardLibrary.RemoveAt(0);
        // 随机选择牌号
        var number = RandomNumberOnCard(card.Item2);
        // 记录牌号
        return new Tuple<uint, uint, string>(number, card.Item1, card.Item2);
    }
    private uint RandomNumberOnCard(string symbol)
    {
        var set = cardNumberPool[symbol];
        int randomNum;
        do
        {
            randomNum = rng.Next(1, countOfOneSymbol + 1);
            if (!set.Contains(randomNum))
            {
                cardNumberPool[symbol].Add(randomNum);
                break;
            }
        }
        while (true);
        return (uint)randomNum;
    }

    private void initNumberPool()
    {
        cardNumberPool["方片"] = new HashSet<int>();
        cardNumberPool["红桃"] = new HashSet<int>();
        cardNumberPool["黑桃"] = new HashSet<int>();
        cardNumberPool["梅花"] = new HashSet<int>();
        cardNumberPool["科技"] = new HashSet<int>();
        cardNumberPool["魔法"] = new HashSet<int>();
    }

    private void ShufflesLibrary()
    {
        int n = cardLibrary.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);  // 随机选择一个索引
            (cardLibrary[i], cardLibrary[j]) = (cardLibrary[j], cardLibrary[i]);  // 交换元素
        }
    }

    private void LoadCardList(String path)
    {
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var text = file.GetAsText();
        file.Close();
        var res = Json.ParseString(text);
        var Dictionary = res.AsGodotArray();

        foreach (var card in Dictionary)
        {
            Godot.Collections.Dictionary card_dict = card.AsGodotDictionary();

            var cardobj = new CardObject
            {
                Id = card_dict.ContainsKey("id") ? card_dict["id"].AsUInt32() : 0,
                Name = card_dict.ContainsKey("name") ? card_dict["name"].AsString() : "",
                Symbol = card_dict.ContainsKey("symbol") ? card_dict["symbol"].AsString() : "",
                Description = card_dict.ContainsKey("description") ? card_dict["description"].AsString() : "",
                life = card_dict.ContainsKey("life") ? card_dict["life"].AsUInt32() : 0,
                attack = card_dict.ContainsKey("attack") ? card_dict["attack"].AsUInt32() : 0,
                type = card_dict.ContainsKey("type") ? card_dict["type"].AsString() : ""
            };
            // 添加到字典映射
            cardDict[cardobj.Id] = cardobj;
            // 添加到牌面列表
            cardList.Add(cardobj);
            // 牌库相关逻辑
            {
                // 判断是否为基础牌
                if (cardobj.Symbol == "any" && cardobj.Name != "士兵" && cardobj.Name != "卫士")
                {
                    // 添加到牌库6种花色
                    foreach (var symbol in new string[] { "方片", "红桃", "梅花", "黑桃", "科技", "魔法" })
                    {
                        cardLibrary.Add(new Tuple<uint, string>(cardobj.Id, symbol));
                    }
                }
                else if (cardobj.Name == "士兵")
                {
                    // 添加到牌库6种花色 每个花色三张
                    for (int i = 0; i < 3; i++)
                    {
                        foreach (var symbol in new string[] { "方片", "红桃", "梅花", "黑桃", "科技", "魔法" })
                        {
                            cardLibrary.Add(new Tuple<uint, string>(cardobj.Id, symbol));
                        }
                    }

                }
                else if (cardobj.Name == "卫士")
                {
                    // 添加到牌库6种花色 每个花色两张
                    for (int i = 0; i < 2; i++)
                    {
                        foreach (var symbol in new string[] { "方片", "红桃", "梅花", "黑桃", "科技", "魔法" })
                        {
                            cardLibrary.Add(new Tuple<uint, string>(cardobj.Id, symbol));
                        }
                    }
                }
                else
                {
                    // 添加到牌库1种花色
                    cardLibrary.Add(new Tuple<uint, string>(cardobj.Id, cardobj.Symbol));
                }
            }
            countOfOneSymbol = cardLibrary.Count / 6;
        }
    }
}