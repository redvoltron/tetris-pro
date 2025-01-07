using Godot;
using System;

public partial class GarbageEffect : Sprite3D
{
	float time_start = 0;
	float time_end = 0;
	Vector3 startpos;
	Vector3 startdir;
	Vector3 target;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		time_start = (float)GetMeta("startTime");
		startpos = (Vector3)GetMeta("startPos");
		startdir = (Vector3)GetMeta("startDir");
		target = (Vector3)GetMeta("target");
		time_end = time_start + 1000;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float t = (Time.GetTicksMsec() - time_start) / 500;
		Rotation = new(0, 0, -t * Mathf.Pi * TetrisData.animspeed * 0.25f);
		if(t <= 0) Position = startpos;
		else Position = (startpos + startdir * t * TetrisData.animspeed).Lerp(target, Mathf.Clamp(t, 0, 1));
		if(t > 1) QueueFree();
	}
}
