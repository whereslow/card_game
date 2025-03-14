using Godot;

public partial class CardScript : LifeNode2D
{
    //Export
    [Export]
    public bool is_static = false;

    // public
    public bool selecetd = false;

    public string name;

    public string symble;
    // 牌的id
    public uint id;
    // 数字
    public uint number;
    // 攻击力
    public uint attack;

    public string description;

    public bool is_front;

    public bool is_guard;

    public uint node_card_id;

    public string type;

    public bool visible;

    // private
    private Line2D line2d;
    private bool gurd_seen = false;
    private Vector2 card_center_offset;
    private AnimationPlayer animationPlayer;
    private Vector2 card_center;
    private Node2D arrow;
    private CardStateManager stateManager;
    private AnimationPlayer labelPlayer;
    private Label effectLabel;
    private Label numberLabel;
    private Label backNumberLabel;
    private Label nameLabel;
    private Label symbleLabel;
    private Label backSymbelLabel;
    private Label descriptionLabel;
    private bool visualAble;
    [Signal]
    public delegate void AttackEventHandler(uint source_id, uint target_id);

    public override void _Ready()
    {
        // 载入箭头
        PackedScene arrow_scene = (PackedScene)GD.Load("res://card/arrow.tscn");
        Node node = arrow_scene.Instantiate();
        arrow = node as Node2D;
        // 载入StateManager
        stateManager = GetNode<CardStateManager>("/root/CardStateManager");
        // 置于卡片背面
        GetNode<CanvasItem>("card/card_back").Visible = true;
        GetNode<CanvasItem>("card/card_front").Visible = false;
        is_front = false;
        // 载入动画
        animationPlayer = GetNode<AnimationPlayer>("card/AnimationPlayer");
        labelPlayer = GetNode<AnimationPlayer>("LabelPlayer");
        // 载入效果标签
        effectLabel = GetNode<Label>("EffectLabel");
        effectLabel.Hide();
        // 设置中心偏移量
        card_center_offset = GetNode<Control>("card/card_front").Size / (2 / GetNode<Control>("card/card_front").Scale.X);
        // 载入数字标签
        numberLabel = GetNode<Label>("card/card_front/number_label");
        backNumberLabel = GetNode<Label>("card/card_back/number_label");
        // 载入名称标签
        nameLabel = GetNode<Label>("card/card_front/name_label");
        // 载入符号标签
        symbleLabel = GetNode<Label>("card/card_front/symble_label");
        backSymbelLabel = GetNode<Label>("card/card_back/symble_label");
        // 载入描述标签
        descriptionLabel = GetNode<Label>("card/card_front/description_label");
        nameLabel.Text = name;
        symbleLabel.Text = symble;
        backSymbelLabel.Text = symble;
        descriptionLabel.Text = description;
        numberLabel.Text = number.ToString();
        backNumberLabel.Text = number.ToString();
        backNumberLabel.Visible = visualAble;
        backSymbelLabel.Visible = visualAble;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (gurd_seen)
        {
            // 直线绘制
            line2d.RemovePoint(line2d.GetPointCount() - 1);
            line2d.AddPoint(GetGlobalMousePosition());
            // 朝向
            arrow.LookAt(card_center);
            // 箭头位置
            arrow.Position = GetGlobalMousePosition();
        }
    }

    public void setBackVisual(bool visualAble)
    {
        this.visualAble = visualAble;
    }

    public void setNode(uint node_card_id, string type)
    {
        this.node_card_id = node_card_id;
        this.type = type;
    }

    public void setCard(uint id, uint number, string name, string symble, string description)
    {
        this.id = id;
        this.name = name;
        this.symble = symble;
        this.description = description;
        this.number = number;
    }


    public void Recover(uint life_value)
    {
        this.Addlife(life_value);
        // 设置名称
        effectLabel.Text = "HP+";
        // 设置颜色
        effectLabel.LabelSettings.FontColor = new Color("#49e6a5");
        // 播放动画
        labelPlayer.Play("label/rise_label");
        animationPlayer.Play("recover");
    }

    public void Hurt(uint life_value)
    {
        this.Subtractlife(life_value);
        // 设置名称
        effectLabel.Text = "HP-";
        // 设置颜色
        effectLabel.LabelSettings.FontColor = new Color("#ff0000");
        // 播放动画
        labelPlayer.Play("label/rise_label");
    }

    public void AddToGurd()
    {
        this.is_guard = true;
    }

    public void changeFace()
    {
        if (is_static)
        {
            return;
        }
        if (is_front)
        {
            animationPlayer.Play("front_to_back");
            GetNode<CanvasItem>("card/card_back").Visible = true;
            GetNode<CanvasItem>("card/card_front").Visible = false;
            is_front = !is_front;
        }
        else
        {
            animationPlayer.Play("back_to_front");
            GetNode<CanvasItem>("card/card_back").Visible = false;
            GetNode<CanvasItem>("card/card_front").Visible = true;
            is_front = !is_front;
        }
        GD.Print("change face front:" + is_front);
    }

    public void CardUp()
    {
        if (selecetd)
        {
            selecetd = false;
            animationPlayer.Play("mov_down");
        }
        else
        {
            selecetd = true;
            animationPlayer.Play("mov_up");
        }

    }
    public void lineDown()
    {
        if (is_guard)
        {
            GD.Print("line down");
            // 添加line2d
            Line2D line = new Line2D();
            card_center = this.GetNode<Control>("card/card_front").GlobalPosition + card_center_offset;
            line.AddPoint(card_center);
            line.DefaultColor = Colors.Red;
            line.Width = 5;
            line.Antialiased = true;
            line2d = line;
            line.AddPoint(GetGlobalMousePosition());
            GetParent().AddChild(line);
            // 添加箭头
            line.AddChild(arrow);
            gurd_seen = true;
            // 添加id
            stateManager.SetAttackSource(node_card_id);
        }
    }
    public void lineUp()
    {
        if (is_guard)
        {
            GD.Print("line up");
            line2d.RemoveChild(arrow);
            GetParent().RemoveChild(line2d);
            line2d.QueueFree();
            gurd_seen = false;
            // 发送信号
            EmitSignal("Attack", stateManager.GetAttackSource(), stateManager.GetAttackTarget());
        }
    }
    // 鼠标进入
    public void mouceEnter()
    {
        stateManager.SetAttackTarget(node_card_id);
    }
    // 鼠标退出
    public void mouceExit()
    {
        stateManager.SetAttackTarget(0);
    }
}
