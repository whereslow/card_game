using Godot;
using System;
using System.Collections.Generic;
using System.Xml.Schema;


public class CardStruct : CardObject
{
    public uint number { get; set; }
}
public partial class ListScript : Node2D
{
    [Export]
    public Vector2 StartPos;

    [Export]
    public int offsetY;

    [Export]
    public int card_count;

    [Export]
    public int card_symbol_and_number_count;

    [Export]
    public Vector2 card_from_pos;
    [Export]
    public Vector2 card_from_scale;
    [Export]
    public float speed;

    [Export]
    public int visual_count;
    //public
    public uint count = 0;
    public List<CardScript> cards;
    //private
    private PackedScene cardPref;
    private CardLoader cardLoader;
    private List<CardStruct> cardStructs = new List<CardStruct>();
    private Node2D movingCard;
    private bool moving = false;
    private List<Node2D> card_stack_list = new List<Node2D>();
    private bool mov_stac_offset = false;
    private List<Vector2> distPos = new List<Vector2>();
    private int stack_count = 0;
    private int visual_current_count = 0;
    private Random rng;
    private List<bool> visual_able_list = new(){false,false,true,true,false};
    public override void _Ready()
    {
        // 设置随机种子
        rng = new Random(1);
        // 设置子卡牌背面图片不可视
        var exam1 = GetNode<Node2D>("card_example1");
        var exam2 = GetNode<Node2D>("card_example2");
        var label1 = exam1.GetNode<Label>("card/card_back/number_label");
        var label2 = exam2.GetNode<Label>("card/card_back/number_label");
        label1.Text = "?";
        label2.Text = "?";
        // 载入卡牌加载器
        cardLoader = GetNode<CardLoader>("/root/CardLoader");
        // 同步种子
        cardLoader.syncSeed(1);
        // 初始化卡片列表
        cards = new List<CardScript>();
        // 载入{card_count}张卡牌
        for (int i = 0; i < card_count; i++)
        {
            var card_st = getNextCard();
            if (card_st == null)
            {
                return;
            }
            cardStructs.Add(card_st);
        }
        // 初始化预制体
        cardPref = (PackedScene)GD.Load("res://card/card.tscn");
        // 对背面可视序列洗牌
        ShuffleList(visual_able_list);
        // 添加初始卡牌
        int _card_count = 0;
        foreach (var cardStruct in cardStructs)
        {
            var visualAble = visual_able_list[_card_count];
            _card_count++;
            GD.Print("visualAble:" + visualAble);
            if (visualAble)
            {
                visual_current_count++;
            }
            var card = GenerateCard(GenNextPos(), cardStruct, visualAble);
            this.AddChild(card);
            card_stack_list.Add(card);
        }
    }
    // By Sonnet3.7的洗牌算法
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;

        // Fisher-Yates 洗牌算法
        for (int i = n - 1; i > 0; i--)
        {
            // 随机选择一个索引 j，范围是 [0, i]
            int j = rng.Next(0, i + 1);

            // 交换 i 和 j 位置的元素
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    private CardStruct getNextCard()
    {
        var card = cardLoader.NextCard();
        if (card == null)
        {
            return null;
        }
        var cardObj = cardLoader.GetCardById(card.Item2);
        var cardStruct = new CardStruct
        {
            number = card.Item1,
            Symbol = card.Item3,
            Id = cardObj.Id,
            Description = cardObj.Description,
            Name = cardObj.Name,
            life = cardObj.life,
            attack = cardObj.attack,
            type = cardObj.type
        };
        return cardStruct;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        // 发卡插值
        if (moving)
        {
            // 速度和缩放插值
            movingCard.Position = movingCard.Position.Lerp(StartPos, (float)(speed * delta));
            movingCard.Scale = movingCard.Scale.Lerp(new Vector2(1, 1), (float)(speed * delta));
            if (movingCard.Position.DistanceTo(StartPos) <= 0.6f)
            {
                moving = false;
            }
        }
        stack_count = 0;
        // 卡牌堆移动插值
        if (mov_stac_offset)
        {
            // 确保distPos数组初始化
            if (distPos == null)
            {
                distPos = new List<Vector2>();
            }

            // 预先计算所有目标位置
            if (distPos.Count == 0)
            {
                foreach (var card in card_stack_list)
                {
                    var dist = card.Position + new Vector2(0, offsetY);
                    distPos.Add(dist);
                }
            }

            // 确保distPos和card_stack_list长度一致
            if (distPos.Count != card_stack_list.Count)
            {
                // 调整distPos大小以匹配card_stack_list
                while (distPos.Count < card_stack_list.Count)
                {
                    var lastCard = card_stack_list[distPos.Count];
                    distPos.Add(lastCard.Position + new Vector2(0, offsetY));
                }
                while (distPos.Count > card_stack_list.Count)
                {
                    distPos.RemoveAt(distPos.Count - 1);
                }
            }

            foreach (var card in card_stack_list)
            {
                if (stack_count < distPos.Count)
                {

                    card.Position = card.Position.Lerp(distPos[stack_count], (float)(speed * delta));

                    if (card.Position.DistanceTo(distPos[stack_count]) <= 0.6f && stack_count >= card_count - 1)
                    {
                        distPos.Clear();
                        mov_stac_offset = false;
                    }
                }
                stack_count++;
            }
        }
        if (Input.IsActionJustPressed("ui_accept"))
        {
            addCardToPosition(GenNextPos());
        }
        else if (Input.IsActionJustPressed("ui_cancel"))
        {
            stack_moving_offset();
        }
    }
    private void stack_moving_offset()
    {
        mov_stac_offset = true;
    }
    private void addCardToPosition(Vector2 position)
    {
        var cardStruct = getNextCard();
        if (cardStruct == null)
        {
            return;
        }
        // 保证场上具有指定数量的背面明牌
        bool visualAble = false;
        if (visual_current_count < visual_count)
        {
            visualAble = true;
        }
        if (visualAble)
        {
            visual_current_count++;
        }
        var card = GenerateCard(card_from_pos, cardStruct, visualAble);
        card.Scale = card_from_scale;
        movingCard = card;
        moving = true;
        this.AddChild(card);
        card_stack_list.Add(card);
        // 当前牌库z轴 + 1
        foreach (var item in card_stack_list)
        {
            item.ZIndex += 1;
        }
        // 当前牌库z轴置0
        card.ZIndex = 0;
    }
    private Vector2 GenNextPos()
    {
        // 偏离位置
        return new Vector2(StartPos.X, StartPos.Y + (offsetY * count));
    }



    private Node2D GenerateCard(
        Vector2 position,
        CardStruct cardStruct,
        bool visual_able)
    {
        // 计数加1
        this.count++;
        // 生成卡片
        var card = (Node2D)cardPref.Instantiate();
        // 获取卡片脚本
        CardScript cardScript = card.GetNode<CardScript>("./");
        // 卡牌相关设置
        cardScript.is_static = false;
        cardScript.setCard(cardStruct.Id, cardStruct.number, cardStruct.Name, cardStruct.Symbol, cardStruct.Description);
        cardScript.setNode(count, cardStruct.type);
        cardScript.Life = cardStruct.life;
        cardScript.attack = cardStruct.attack;
        // 添加到卡牌列表
        cards.Add(cardScript);
        // 设置卡片位置
        card.Position = position;
        cardScript.setBackVisual(visual_able);
        return card;
    }
}
