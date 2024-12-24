using Godot;
using System;
public class SpringDamperModel
{
    private float spring, damper, maxAccel;
    protected Vector3 vel, offset;
    public Vector3 pos;
    public SpringDamperModel(float spring, float damper, float maxSpeed)
    {
        this.spring = spring;
        this.damper = damper;
        this.maxAccel = maxSpeed;
        this.vel = Vector3.Zero;
        this.pos = Vector3.Zero;
    }
    public void Tick(float delta)
    {
        Vector3 projectedForce = Vector3.Zero;
        projectedForce += (-pos - offset) * spring * delta;
        projectedForce += -(vel + projectedForce) * damper * delta;
        if (projectedForce.Length() > maxAccel * delta)
        {
            projectedForce = projectedForce.Normalized() * maxAccel * delta;
        }
        vel += projectedForce;
        pos += vel * delta;
    }
    public void ApplyImpulse(Vector3 impulse)
    {
        vel += impulse;
    }
    public void SetOffset(Vector3 offset)
    {
        this.offset = offset;
    }
    public void InstaDisplace(Vector3 offset)
    {
        this.pos += offset;
    }
}
public partial class Speen : MeshInstance3D
{
	SpringDamperModel springy, posspringy;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		springy = new(300, 10, float.MaxValue);
		posspringy = new(100, 10, float.MaxValue);
	}
	public void ApplyRotImpulse(Vector3 impulse)
	{
		springy.ApplyImpulse(impulse);
	}
	public void ApplyPosImpulse(Vector3 impulse)
	{
		posspringy.ApplyImpulse(impulse);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float dt = (float) delta * 2;
		springy.Tick(dt);
		posspringy.Tick(dt);
		Transform = Transform3D.Identity.Rotated(springy.pos.Normalized(), springy.pos.Length()).Translated(posspringy.pos);
	}
}
