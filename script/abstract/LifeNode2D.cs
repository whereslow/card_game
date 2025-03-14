using Godot;
using System;

public partial class LifeNode2D : Node2D
{
    public uint Life { get; set; } = 30;
    public void Addlife(uint value)
    {
        this.Life += value;
    }
    public void Subtractlife(uint value)
    {
        this.Life -= value;
    }
}
