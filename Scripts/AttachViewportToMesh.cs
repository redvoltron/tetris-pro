using Godot;
using System;

public partial class AttachViewportToMesh : SubViewport
{
	Speen mesh;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		mesh = (Speen)GetParent().GetParent();
		Material mat = mesh.Mesh.SurfaceGetMaterial(0);
		mat.Set(BaseMaterial3D.PropertyName.AlbedoTexture, GetTexture());
		GD.Print(mat.Get(BaseMaterial3D.PropertyName.AlbedoTexture));
		GD.Print(GetTexture());
	}
}
