using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Effect : Node2D
{
    [Export]
    public float duration;

    [Export]
    public Vector2 moveToPositionOffset;

    private bool isPlaying;
    private GpuParticles2D blackMagic;
    private GpuParticles2D whiteMagic;
    private double timer;
    private GpuParticles2D PlayingPartic;
    public override void _Ready()
    {
        base._Ready();
        this.blackMagic = GetNode<GpuParticles2D>("Node2D/BlackMagic");
        this.whiteMagic = GetNode<GpuParticles2D>("Node2D/WhiteMagic");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        // 测试
        if (Input.IsActionJustPressed("ui_accept"))
        {
            PlayWhiteMagicEffect();
        }
        if (!isPlaying)
        {
            return;
        }
        // 播放粒子
        if (timer >= duration && PlayingPartic.Emitting)
        {
            timer = 0;
            PlayingPartic.Emitting = false;
            // 复位
            PlayingPartic.Position = Vector2.Zero;
        }
        else if (timer < duration)
        {
            timer += delta;
            // 对于移动类效果,插值移动
            if (PlayingPartic.Emitting && (new List<String> { "WhiteMagic" }.Contains(PlayingPartic.Name)))
            {
                // 解除hide
                if (timer > 0.6)
                {
                    this.PlayingPartic.Show();
                }
                PlayingPartic.Position = PlayingPartic.Position.MoveToward(moveToPositionOffset, (float)(50 * delta));
            }
        }
    }

    public void PlayBlackMagicEffect()
    {
        isPlaying = true;
        blackMagic.Emitting = true;
        this.PlayingPartic = blackMagic;
        timer = 0;
    }
    public void PlayWhiteMagicEffect()
    {
        isPlaying = true;
        whiteMagic.Emitting = true;
        whiteMagic.Hide();
        this.PlayingPartic = whiteMagic;
        timer = 0;
    }
}
