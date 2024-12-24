using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Zanaptak.PcgRandom;
public struct Handling
{
	public float das, dcd, arr, sdf;
}
public struct TileStruct
{
	public byte[] data;
	public byte oobtile;
	public Vector2I size;
	public byte Get(Vector2I point)
	{
		if(point.X >= size.X || point.X < 0 || point.Y >= size.Y || point.Y < 0) return oobtile;
		return data[point.X + point.Y * size.X];
	}
	public void Set(Vector2I point, byte data)
	{
		if(point.X >= size.X || point.X < 0 || point.Y >= size.Y || point.Y < 0) return;
		this.data[point.X + point.Y * size.X] = data;
	}
	public bool Collides(TileStruct other, Vector2I offset)
	{
		for(int x = 0; x < size.X; x++) for(int y = 0; y < size.Y; y++)
		{
			if(Get(new(x, y)) != 0 && other.Get(new Vector2I(x, y) + offset) != 0)
			{
				return true;
			}
		}
		return false;
	}
	public TileStruct(int w, int h, byte[] data, byte oobtile)
	{
		this.data = data;
		size = new(w, h);
		this.oobtile = oobtile;
	}
	public void Render(TileMapLayer tiles, Vector2I pos)
	{
		for(int x = -1; x <= size.X; x++) for(int y = -1; y <= size.Y; y++)
		{
			byte tile = Get(new(x,y));
			if(tile != 0x00)
				tiles.SetCell(new(pos.X + x, pos.Y + y), 0, TetrisData.ByteToAtlasCoord(tile));
		}
	}
	public TileStruct RotateCW()
	{
		byte[] newdata = new byte[size.X * size.Y];
		for(int y = size.Y - 1; y >= 0; y--) for(int x = 0; x < size.X; x++)
		{
			newdata[size.Y - y - 1 + x * size.X] = data[x + y * size.X];
		}
		return new(size.X, size.Y, newdata, oobtile);
	}
	public TileStruct RotateCCW()
	{
		return RotateCW().RotateCW().RotateCW(); //masterful gambit sir
	}
}
public struct Piece
{
	public TileStruct tiles;
	public Vector2I pos;
	public int rotation;
	public bool Collides(TileStruct other, Vector2I offset)
	{
		return tiles.Collides(other, pos + offset);
	}
}
public class KVReference
{
	public int key;
	public float value;
}
public static class TetrisData
{
	public static float animspeed = 5;
	public static Vector2 heldpos = new(-60, -64);
	public static Vector2 queuepos = new(60, -64);
	public static Vector2 queueoffset = new(0, 32);
	public static List<Piece> GetBag()
	{
		List<Piece> pieces = new();
		pieces.Add(new() { tiles = new(3, 3, [
			0, 1, 1,
			1, 1, 0,
			0, 0, 0
		], 0x00),
		pos = new(3, 17) } );
		pieces.Add(new() { tiles = new(3, 3, [
			2, 2, 0,
			0, 2, 2,
			0, 0, 0
		], 0x00),
		pos = new(3, 17) } );
		pieces.Add(new() { tiles = new(3, 3, [
			0, 3, 0,
			3, 3, 3,
			0, 0, 0
		], 0x00),
		pos = new(3, 17) } );
		pieces.Add(new() { tiles = new(4, 4, [
			0, 0, 0, 0,
			4, 4, 4, 4,
			0, 0, 0, 0,
			0, 0, 0, 0
		], 0x00),
		pos = new(3, 18) } );
		pieces.Add(new() { tiles = new(4, 4, [
			0, 0, 0, 0,
			0, 5, 5, 0,
			0, 5, 5, 0,
			0, 0, 0, 0
		], 0x00),
		pos = new(3, 16) } );
		pieces.Add(new() { tiles = new(3, 3, [
			0, 0, 6,
			6, 6, 6,
			0, 0, 0
		], 0x00),
		pos = new(3, 17) } );
		pieces.Add(new() { tiles = new(3, 3, [
			7, 0, 0,
			7, 7, 7,
			0, 0, 0
		], 0x00),
		pos = new(3, 17) } );
		return pieces;
	}
	public static Vector2I ByteToAtlasCoord(byte data)
	{
		switch(data)
		{
			case 0x00:
				return new(-1, -1);
			case 0x01:
				return new(0, 0);
			case 0x02:
				return new(1, 0);
			case 0x03:
				return new(2, 0);
			case 0x04:
				return new(3, 0);
			case 0x05:
				return new(0, 1);
			case 0x06:
				return new(1, 1);
			case 0x07:
				return new(2, 1);
			default:
				return new(-1, -1);
		}
	}
	public static List<Vector2I> WallkicksCCW(int rot, int tilewidth)
	{
		List<Vector2I> list = new();
		foreach(Vector2I kick in Wallkicks(rot, tilewidth))
		{
			list.Add(-kick);
		}
		return list;
	}
	public static List<Vector2I> Wallkicks(int rot, int tilewidth)
	{
		int rotation = rot % 4;
		if(rotation < 0) rotation += 4;
		switch(tilewidth)
		{
			case 3:
				switch(rotation)
				{
					case 1: 
						return new([
							new(0, 0),
							new(1, 0),
							new(1, 1),
							new(0, -2),
							new(1, -2)
						]);
					case 2: 
						return new([
							new(0, 0),
							new(1, 0),
							new(1, -1),
							new(0, 2),
							new(1, 2)
						]);
					case 3: 
						return new([
							new(0, 0),
							new(-1, 0),
							new(-1, 1),
							new(0, -2),
							new(-1, -2)
						]);
					default:
						return new([
							new(0, 0),
							new(-1, 0),
							new(-1, -1),
							new(0, 2),
							new(-1, 2)
						]);
				}
			default:
				switch(rotation)
				{
					case 1:
						return new([
							new(0, 0),
							new(-1, 0),
							new(2, 0),
							new(-1, -2),
							new(2, 1)
						]);
					case 2:
						return new([
							new(0, 0),
							new(2, 0),
							new(-1, 0),
							new(2, -1),
							new(-1, 1)
						]);
					case 3:
						return new([
							new(0, 0),
							new(-2, 0),
							new(1, 0),
							new(-2, -1),
							new(1, 2)
						]);
					default:
						return new([
							new(0, 0),
							new(-2, 0),
							new(1, 0),
							new(1, -2),
							new(-2, 1)
						]);
				}
		}
	}
}
public partial class Playfield : Node2D
{
	private TileMapLayer tiles;
	Speen parent;
	Handling handling;
	TileStruct playfield;
	Piece piece, held, ghostPiece;
	bool holdQueueFull;
	Queue<Piece> queue;
	List<Piece> bag;
	float dastimer = 0;
	float dcdtimer = 0;
	int goalpoints = 0;
	int level = 1;
	Piece defaultPiece;
	int queuelength = 5;
	float gravity = 0;
	float gravtimer = -1;
	float locktimer = 0;
	int hardlock = 0;
	int lowestpoint = 0;
	bool hardlockstarted = false;
	int score = 0;
	int b2b = 0;
	bool justrotated = false;
	int tspin = 0;
	Vector2I recentPiecePos;
	bool holdingleft, holdingright, softdrop;
	bool gamestarted = false;
	Pcg16 rand;

	List<KVReference> linedropeffects;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playfield = new(10, 40, new byte[10 * 40], 0xFF);
		handling = new() {
			das = 0.14f,
			dcd = 0.1f,
			arr = 0,
			sdf = 20
		};
		gravity = Mathf.Pow(0.8f-((this.level-1)*0.007f), this.level - 1);
		tiles = (TileMapLayer)GetChild(0);
		linedropeffects = new();

		SetSeed(1);
		StartGame();
		parent = (Speen)GetParent().GetParent().GetParent();
	}
	public void SetSeed(uint seed)
	{
		rand = new(seed);
		queue = new();
		bag = TetrisData.GetBag();
		UpdateQueue();
	}
	public void StartGame()
	{
		NewPiece();
		gamestarted = true;
	}
	private bool TryMovePiece(Vector2I dir)
	{
		if(piece.Collides(playfield, dir)) return false;
		else piece = piece with {pos = piece.pos + dir};
		UpdateGhost();
		return true;
	}
	private void RotateCCW()
	{
		Piece newpiece = piece with {tiles = piece.tiles.RotateCCW(), rotation = piece.rotation - 1};
		foreach(Vector2I wallkick in TetrisData.WallkicksCCW(newpiece.rotation, piece.tiles.size.X))
		{
			if(!newpiece.Collides(playfield, wallkick))
			{
				piece = newpiece with {pos = newpiece.pos + wallkick};
				locktimer = 0;
				UpdateGhost();
				return;
			}
		}
	}
	private void RotateCW()
	{
		Piece newpiece = piece with {tiles = piece.tiles.RotateCW(), rotation = piece.rotation + 1};
		foreach(Vector2I wallkick in TetrisData.Wallkicks(piece.rotation, piece.tiles.size.X))
		{
			if(!newpiece.Collides(playfield, wallkick))
			{
				piece = newpiece with {pos = newpiece.pos + wallkick};
				locktimer = 0;
				UpdateGhost();
				return;
			}
		}
	}
	private void LockPiece()
	{
		for(int x = 0; x < piece.tiles.size.X; x++) for(int y = 0; y < piece.tiles.size.Y; y++)
		{
			byte data = piece.tiles.Get(new(x, y));
			if(data != 0)
			{
				playfield.Set(new Vector2I(x, y) + piece.pos, data);
			}
		}
		recentPiecePos = piece.pos;
		CheckForLineClears();
		NewPiece();
	}
	private void CheckForLineClears()
	{
		for(int y = playfield.size.Y - 1; y > 0; y--) for(int x = 0; x < playfield.size.X; x++)
		{
			if(playfield.data[x + y * playfield.size.X] == 0x00) break;
			else if(x == playfield.size.X - 1) { ClearLine(y); y++;}
		}
	}
	private void ClearLine(int which)
	{
		for(int y = which; y > 1; y--) for(int x = 0; x < playfield.size.X; x++)
		{
			playfield.data[x + y * playfield.size.X] = playfield.data[x + (y - 1) * playfield.size.X];
		}
		for(int x = 0; x < playfield.size.X; x++)
		{
			playfield.data[x] = 0;
		}
		if(linedropeffects.Count > 0) linedropeffects.Add(new() {key = which == linedropeffects[linedropeffects.Count - 1].key - 1 ? which + 1 : which});
		else linedropeffects.Add(new() {key = which});
	}
	private void NewPiece()
	{
		piece = queue.Dequeue();
		ResetPieceRelatedStuff();
		UpdateQueue();
	}
	private void UpdateGhost()
	{
		ghostPiece = piece;
		while(!ghostPiece.Collides(playfield, Vector2I.Down)) ghostPiece = ghostPiece with {pos = ghostPiece.pos + Vector2I.Down};
	}
	private void ResetPieceRelatedStuff()
	{
		UpdateGhost();
		defaultPiece = piece;
		locktimer = 0;
		hardlock = 0;
		gravtimer = 0;
		dcdtimer = handling.dcd;
		lowestpoint = 0;
		hardlockstarted = false;
		justrotated = false;
	}
	private void HoldPiece()
	{
		if(!holdQueueFull)
		{
			held = defaultPiece;
			holdQueueFull = true;
			NewPiece();
		} else
		{
			Piece _ = held;
			held = defaultPiece;
			piece = _;
			ResetPieceRelatedStuff();
		}
	}
    private void UpdateQueue()
	{
		while(queue.Count < queuelength)
		{
			if(bag.Count == 0)
			{
				bag = TetrisData.GetBag();
			}
			int index = rand.Next() % bag.Count;
			queue.Enqueue(bag[index]);
			bag.RemoveAt(index);
		}
		QueueRedraw();
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float dt = (float)delta;

		if(gamestarted)
		{
			float effectivegrav = gravity * dt;
			if(softdrop) effectivegrav *= handling.sdf;
			gravtimer += effectivegrav;
			while(gravtimer >= 1)
			{
				TryMovePiece(new(0, 1));
				gravtimer -= 1;
			}
			if(piece.Collides(playfield, new(0, 1))) locktimer += dt;
			else locktimer = 0;
			if(locktimer > 0.5f)
			{
				LockPiece();
			}
			dcdtimer -= dt;
			if(holdingleft ^ holdingright) dastimer += dt; else dastimer = 0;
			bool tryingDas = false;
			while(dastimer > handling.das && !piece.Collides(playfield, new(-1, 0)) && holdingleft && dcdtimer <= 0)
			{
				tryingDas = true;
				if(TryMovePiece(Vector2I.Left)) locktimer = 0;
				dastimer -= handling.arr;
			}
			while(dastimer > handling.das && !piece.Collides(playfield, new(1, 0)) && holdingright && dcdtimer <= 0)
			{
				tryingDas = true;
				if(TryMovePiece(Vector2I.Right)) locktimer = 0;
				dastimer -= handling.arr;
			}
			if(dastimer > handling.das && piece.Collides(playfield, new(-1, 0)) && tryingDas)
			{
				parent.ApplyRotImpulse(new(0, -0.75f, 0));
				parent.ApplyPosImpulse(new(-0.25f, 0, 0));
			}
			if(dastimer > handling.das && piece.Collides(playfield, new(1, 0)) && tryingDas)
			{
				parent.ApplyRotImpulse(new(0, 0.75f, 0));
				parent.ApplyPosImpulse(new(0.25f, 0, 0));
			}
		}
		List<KVReference> toremove = new();
		foreach(KVReference effect in linedropeffects)
		{
			if(effect.value > 1) toremove.Add(effect);
			else effect.value += dt * TetrisData.animspeed;
		}
		foreach(KVReference effect in toremove)
		{
			linedropeffects.Remove(effect);
		}

		tiles.Clear();
		
		playfield.Render(tiles, new(0, 0));
		if(gamestarted && !piece.Equals(default)) piece.tiles.Render(tiles, piece.pos );
		QueueRedraw();
	}
    public override void _Draw()
    {
		TileSetAtlasSource src = (TileSetAtlasSource)tiles.TileSet.GetSource(0);

		for(int x = 0; x < 10; x++) for(int y = 20; y < 40; y++)
		{
			Vector2 pos = new(x * 8 - 40, y * 8 - 240);
			DrawTextureRectRegion(src.Texture, 
				new(pos, new(8, 8)), 
				new(new(0, 16), new(8, 8)));
		}
		float offset = 0;
		for(int y = 39; y >= 0; y--) {
			foreach(KVReference effect in linedropeffects)
			{
				if(y == effect.key) offset += Mathf.Pow(1 - effect.value, 2);
			}
			for(int x = 0; x < 10; x++)
			{
				Vector2I atlascoords = tiles.GetCellAtlasCoords(new(x, y));
				Vector2 pos = new(x * 8 - 40, y * 8 - 240 - offset * 8);
				if(atlascoords != new Vector2I(-1, -1))
				DrawTextureRectRegion(src.Texture, 
				new(pos, new(8, 8)), 
				new(atlascoords * 8, new(8, 8)));
			}
		}
		for(int i = 0; i < queue.Count; i++)
		{
			TileStruct tiles = queue.ToArray()[i].tiles;
			for(int x = 0; x < tiles.size.X; x++) for(int y = 0; y < tiles.size.Y; y++)
			{
				Vector2I atlascoords = TetrisData.ByteToAtlasCoord(tiles.Get(new(x, y)));
				if(atlascoords != new Vector2I(-1, -1))
				DrawTextureRectRegion(src.Texture, 
				new(new Vector2(
					x - tiles.size.X / 2f, 
					y - tiles.size.Y / 2f) * 8 + 
					TetrisData.queuepos + 
					TetrisData.queueoffset * i, 
				new(8, 8)), 
				new(atlascoords * 8, new(8, 8)));
			}
		}
		TileStruct _tiles = ghostPiece.tiles;
		for(int x = 0; x < _tiles.size.X; x++) for(int y = 0; y < _tiles.size.Y; y++)
		{
			Vector2I atlascoords = TetrisData.ByteToAtlasCoord(_tiles.Get(new(x, y)));
			if(atlascoords != new Vector2I(-1, -1))
			DrawTextureRectRegion(src.Texture, 
			new(new Vector2(
				x * 8 - 40, y * 8 - 240) + ghostPiece.pos * 8,
			new(8, 8)), 
			new(atlascoords * 8, new(8, 8)), new(1, 1, 1, 0.5f));
		}
		if(holdQueueFull) 
		{
			TileStruct tiles = held.tiles;
			for(int x = 0; x < tiles.size.X; x++) for(int y = 0; y < tiles.size.Y; y++)
			{
				Vector2I atlascoords = TetrisData.ByteToAtlasCoord(tiles.Get(new(x, y)));
				if(atlascoords != new Vector2I(-1, -1))
				DrawTextureRectRegion(src.Texture, 
				new(new Vector2(
					x - tiles.size.X / 2f, 
					y - tiles.size.Y / 2f) * 8 + 
					TetrisData.heldpos,
				new(8, 8)), 
				new(atlascoords * 8, new(8, 8)), new(1, 1, 1, 0.5f));
			}
		}
        base._Draw();
    }
    private void HardDrop()
	{
		while(TryMovePiece(new(0, 1))) continue;
		parent.ApplyRotImpulse(new(1.5f, 0, 0));
		parent.ApplyPosImpulse(new(0, -0.5f, 0));
		LockPiece();
	}
    public override void _Input(InputEvent @event)
    {
		GD.Print("help");
		if(@event.IsActionPressed("hold")) HoldPiece();
		if(@event.IsActionPressed("left")) if(TryMovePiece(Vector2I.Left)) locktimer = 0;
		if(@event.IsActionPressed("right")) if(TryMovePiece(Vector2I.Right)) locktimer = 0;
		if(@event.IsActionPressed("cw")) {RotateCW(); dcdtimer = 0;}
		if(@event.IsActionPressed("ccw")) {RotateCCW(); dcdtimer = 0;}
		softdrop = @event.IsActionPressed("sd") || (softdrop && !@event.IsActionReleased("sd"));
		holdingleft = @event.IsActionPressed("left") || (holdingleft && !@event.IsActionReleased("left"));
		holdingright = @event.IsActionPressed("right") || (holdingright && !@event.IsActionReleased("right"));
		if(@event.IsActionPressed("hd")) HardDrop();
        base._Input(@event);
	}
}
