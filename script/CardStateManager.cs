using Godot;
using System;

public partial class CardStateManager : Node
{
    private uint attack_source_id;

    private uint attack_target_id;

    public void SetAttackSource(uint source_id)
    {
        attack_source_id = source_id;
    }

    public uint GetAttackSource()
    {
        var value= this.attack_source_id;
        this.attack_source_id = 0;
        return value;
    }

    public void SetAttackTarget(uint target_id)
    {
        attack_target_id = target_id;
    }

    public uint GetAttackTarget()
    {
        var value= this.attack_target_id;
        this.attack_target_id = 0;
        return value;
    }
}
