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
	public bool t;
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
public struct Particle
{
	public float lifetime;
	public Vector2 pos;
}
public static class TetrisData
{
	public static float animspeed = 5f;
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
		pos = new(3, 17), t = true } );
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
			case 0xF0:
				return new(3, 1);
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
	public Speen parent;
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
	Node sounds;
	int hardlock = 0;
	int lowestpoint = 0;
	bool hardlockstarted = false;
	bool holdExhausted = false;
	bool ccw = false;
	int score = 0;
	int combo = 0;
	int b2b = 0;
	bool justrotated = false;
	int tspin = 0;
	Vector2I recentPiecePos;
	int recentPieceRot;
	bool holdingleft, holdingright, softdrop;
	bool gamestarted = false;
	Pcg16 rand, garbrand;
	List<KeyValuePair<int, KeyValuePair<int, float>>> garbagequeue; //yes
	Sprite2D particles;
	List<KVReference> linedropeffects;
	List<AnimatedSprite2D> generaleffects;
	float clearEffectStartTime;
	uint seed;
	public Playfield target;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playfield = new(10, 40, new byte[10 * 40], 0xFF);
		handling = new() {
			das = 0.14f,
			dcd = 0.1f,
			arr = 0,
			sdf = 9999
		};
		gravity = Mathf.Pow(0.8f-((this.level-1)*0.007f), this.level - 1);
		tiles = (TileMapLayer)GetChild(0);
		linedropeffects = new();
		generaleffects = new();
		garbagequeue = new();
		seed = (uint)Mathf.FloorToInt(Time.GetUnixTimeFromSystem());
		SetSeed(seed);
		StartGame();
		sounds = GetChild(5);
		parent = (Speen)GetParent().GetParent().GetParent();
		particles = (Sprite2D)GetChild(2);
		target = this;
	}
	public void SetSeed(uint seed)
	{
		rand = new(seed);
		garbrand = new(seed - 1);
		queue = new();
		bag = TetrisData.GetBag();
		UpdateQueue();
	}
	public void StartGame()
	{
		NewPiece();
		held = default;
		gamestarted = true;
	}
	public void Die()
	{
		playfield = new(10, 40, new byte[10 * 40], 0xFF);
		SetSeed(seed);
		StartGame();
	}
	private bool TryMovePiece(Vector2I dir)
	{
		if(piece.Collides(playfield, dir)) return false;
		else piece = piece with {pos = piece.pos + dir};
		UpdateGhost();
		justrotated = false;
		return true;
	}
	public Vector2 TilePosToScreenPos(Vector2 tilePos)
	{
		return new Vector2(tilePos.X * 8 - 36, tilePos.Y * 8 - 236);
	}
	private int CheckTSpin()
	{
		int nooks = 0;
		int corners = 0;
		if(!piece.t) tspin = 0;
		else
		{
			if(playfield.Get(piece.pos) != 0x00) 
			{
				corners += 1;
				if(piece.rotation == 0 || piece.rotation == 3) nooks += 1;
			}
			if(playfield.Get(piece.pos + new Vector2I(2, 0)) != 0x00) 
			{
				corners += 1;
				if(piece.rotation == 0 || piece.rotation == 1) nooks += 1;
			}
			if(playfield.Get(piece.pos + new Vector2I(0, 2)) != 0x00) 
			{
				corners += 1;
				if(piece.rotation == 2 || piece.rotation == 3) nooks += 1;
			}
			if(playfield.Get(piece.pos + new Vector2I(2, 2)) != 0x00) 
			{
				corners += 1;
				if(piece.rotation == 1 || piece.rotation == 2) nooks += 1;
			}
			GD.Print(nooks);
			GD.Print(corners);
			GD.Print(justrotated);
			GD.Print(tspin);
			if(corners > 2 && justrotated) if(nooks > 1) tspin = 2; else tspin = 1; else tspin = 0;
			
		}
		
		return tspin;
	}
	private void RotateCCW()
	{
		Piece newpiece = piece with {tiles = piece.tiles.RotateCCW(), rotation = piece.rotation - 1};
		foreach(Vector2I wallkick in TetrisData.WallkicksCCW(newpiece.rotation, piece.tiles.size.X))
		{
			if(!newpiece.Collides(playfield, wallkick))
			{
				piece = newpiece with {pos = newpiece.pos + wallkick};
				if(piece.rotation < 0) piece.rotation += 4;
				locktimer = 0;
				justrotated = true;
				recentPieceRot = piece.rotation;
				ccw = true;
				UpdateGhost();
				if(CheckTSpin() > 0) ((AudioStreamPlayer)sounds.GetChild(3)).Play(); else ((AudioStreamPlayer)sounds.GetChild(4)).Play();
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
				justrotated = true;
				recentPieceRot = piece.rotation;
				ccw = false;
				UpdateGhost();
				if(CheckTSpin() > 0) ((AudioStreamPlayer)sounds.GetChild(3)).Play(); else ((AudioStreamPlayer)sounds.GetChild(4)).Play();
				return;
			}
		}
	}
	private void LockPieceEffect()
	{
		for(int x = 0; x < piece.tiles.size.X; x++) for(int y = 0; y < piece.tiles.size.Y; y++)
		{
			byte data = piece.tiles.Get(new(x, y));
			if(data != 0)
			{
				playfield.Set(new Vector2I(x, y) + piece.pos, data);
				AnimatedSprite2D blockeffect = (AnimatedSprite2D)GetChild(4).Duplicate();
				blockeffect.Visible = true;
				blockeffect.Position = TilePosToScreenPos(new Vector2(x, y) + piece.pos);
				blockeffect.SpeedScale = TetrisData.animspeed;
				AddChild(blockeffect);
				generaleffects.Add(blockeffect);
			}
			
		}
	}
	private void LockPiece()
	{
		LockPieceEffect();
		for(int x = 0; x < piece.tiles.size.X; x++) for(int y = 0; y < piece.tiles.size.Y; y++)
		{
			byte data = piece.tiles.Get(new(x, y));
			if(data != 0)
			{
				playfield.Set(new Vector2I(x, y) + piece.pos, data);
			}
		}
		recentPiecePos = piece.pos;
		holdExhausted = false;
		CheckTSpin();
		if(!CheckForLineClears()) TryEmptyGarbageQueue();
		NewPiece();
	}
	private void TryEmptyGarbageQueue()
	{
		List<KeyValuePair<int, KeyValuePair<int, float>>> toremove = new();
		foreach(KeyValuePair<int, KeyValuePair<int, float>> garbage in garbagequeue)
		{
			if(garbage.Value.Value <= Time.GetTicksMsec())
			{
				ApplyGarbage(garbage);
				toremove.Add(garbage);
			}
		}
		foreach(var garbage in toremove)
		{
			garbagequeue.Remove(garbage);
		}
	}
	private void ApplyGarbage(KeyValuePair<int, KeyValuePair<int, float>> garbage)
	{
		for(int y = garbage.Value.Key; y < playfield.size.Y; y++)
		{
			for(int x = 0; x < playfield.size.X; x++)
			{
				playfield.Set(new(x, y - garbage.Value.Key), playfield.Get(new(x, y)));
			}
		}
		for(int y = (playfield.size.Y) - garbage.Value.Key; y < playfield.size.Y; y++)
		{
			for(int x = 0; x < playfield.size.X; x++)
			{
				playfield.Set(new(x, y), 0xF0);
			}
			playfield.Set(new(garbage.Key, y), 0x00);
		}
	}
	private bool CheckForLineClears()
	{
		int amtcleared = 0;
		for(int y = playfield.size.Y - 1; y > 0; y--) for(int x = 0; x < playfield.size.X; x++)
		{
			if(playfield.data[x + y * playfield.size.X] == 0x00) break;
			else if(x == playfield.size.X - 1) { ClearLine(y); y++; amtcleared += 1;}
		}
		AwardClears(amtcleared);
		return amtcleared > 0;
	}
	private void AdvanceCombo()
	{
		combo += 1;
	}
	private void BreakCombo()
	{
		combo = 0;
	}
	private void AdvanceB2B()
	{
		b2b++;
	}
	private void BreakB2B()
	{
		b2b = 0;
	}
	private void TSpinClear()
	{
		GD.Print("tpsin");
		AnimatedSprite2D spineffect = (AnimatedSprite2D)GetChild(3).Duplicate();
		spineffect.Visible = true;
		spineffect.Position = TilePosToScreenPos(recentPiecePos + new Vector2(1.5f, 1.5f));
		if(ccw)
			spineffect.FlipV = true;
		spineffect.RotationDegrees = (recentPieceRot) * 90;
		spineffect.SpeedScale = TetrisData.animspeed;
		AddChild(spineffect);
		generaleffects.Add(spineffect);
	}
	private void AwardClears(int clears)
	{
		switch(tspin)
		{
			case 0:
				switch(clears)
				{
					case 0:
						BreakCombo();
						return;
					case 1:
						score += 100 * level;
						BreakB2B();
						AdvanceCombo();
						SendGarbageFromLines(0);
						((AudioStreamPlayer)sounds.GetChild(5)).Play();
						return;
					case 2:
						score += 300 * level;
						BreakB2B();
						AdvanceCombo();
						SendGarbageFromLines(1);
						((AudioStreamPlayer)sounds.GetChild(5)).Play();
						return;
					case 3:
						score += 500 * level;
						BreakB2B();
						AdvanceCombo();
						SendGarbageFromLines(2);
						((AudioStreamPlayer)sounds.GetChild(5)).Play();
						return;
					case 4:
						score += 800 * level;
						AdvanceB2B();
						AdvanceCombo();
						SendGarbageFromLines(4);
						if(b2b < 2) ((AudioStreamPlayer)sounds.GetChild(6)).Play(); else ((AudioStreamPlayer)sounds.GetChild(7)).Play();
						return;
				}
				return;
			case 1:
				switch(clears)
				{
					case 0:
						score += 100 * level;
						GD.Print("T-Spin Mini");
						BreakCombo();
						return;
					case 1:
						score += 200 * level;
						GD.Print("T-Spin Mini Single");
						AdvanceCombo();
						AdvanceB2B();
						TSpinClear();
						SendGarbageFromLines(0);
						((AudioStreamPlayer)sounds.GetChild(8)).Play();
						return;
					case 2:
						score += 400 * level;
						GD.Print("T-Spin Mini Double");
						AdvanceCombo();
						AdvanceB2B();
						TSpinClear();
						SendGarbageFromLines(1);
						((AudioStreamPlayer)sounds.GetChild(8)).Play();
						return;
				}
				return;
			case 2:
				switch(clears)
				{
					case 0:
						score += 400 * level;
						GD.Print("T-Spin");
						BreakCombo();
						return;
					case 1:
						score += 800 * level;
						GD.Print("T-Spin Single");
						AdvanceCombo();
						AdvanceB2B();
						TSpinClear();
						SendGarbageFromLines(2);
						((AudioStreamPlayer)sounds.GetChild(8)).Play();
						return;
					case 2:
						score += 1200 * level;
						GD.Print("T-Spin Double");
						AdvanceCombo();
						AdvanceB2B();
						TSpinClear();
						SendGarbageFromLines(4);
						((AudioStreamPlayer)sounds.GetChild(8)).Play();
						return;
					case 3:
						score += 1600 * level;
						GD.Print("T-Spin Triple");
						AdvanceCombo();
						AdvanceB2B();
						TSpinClear();
						SendGarbageFromLines(6);
						((AudioStreamPlayer)sounds.GetChild(8)).Play();
						return;
				}
				return;
		}
	}
	private void ClearLineEffect(int which)
	{
		if(linedropeffects.Count > 0) linedropeffects.Add(new() {key = which == linedropeffects[linedropeffects.Count - 1].key - 1 ? which + 1 : which});
		else linedropeffects.Add(new() {key = which});
	}
	private void SendGarbageFromLines(int howmuch) //This function applies B2B & combo to your attack.
	{
		float g = howmuch + 1;
		float _b2b = 2f;
		while(_b2b <= b2b)
		{
			g += 1;
			_b2b *= 2f;
		}
		GD.Print("Sending " + howmuch.ToString() + " lines");
		GD.Print("...plus " + (g - 1 - howmuch).ToString() + " lines from B2B " + b2b);
		g *= (0.75f + 0.25f * combo);
		GD.Print("...times " + (0.75f + 0.25f * combo).ToString() + " from your " + combo + " combo");
		GD.Print("for a total of " + Mathf.FloorToInt(g - 1) + " lines sent");
		SendGarbage(Mathf.FloorToInt(g - 1));
	}
	private void SendGarbageEffect(int howmuch)
	{
		Vector2 startpos = TilePosToScreenPos((Vector2)recentPiecePos) * 0.0125f * new Vector2(1, -1);
		Vector3 startdir = new Vector3(-0.5f, Random.Shared.NextSingle() - 0.5f, 0).Normalized() * 0.5f;
		for(int i = 0; i < howmuch; i++)
		{
			clearEffectStartTime = Mathf.Max(clearEffectStartTime + 250 / TetrisData.animspeed, Time.GetTicksMsec());
			GarbageEffect effect = (GarbageEffect)parent.GetChild(1).Duplicate();
			effect.SetMeta("startTime", clearEffectStartTime);
			effect.SetMeta("startPos", new Vector3(startpos.X, startpos.Y, 0) + parent.Position);
			effect.SetMeta("startDir", startdir);
			effect.SetMeta("target", new Vector3(0.5f, 1f, 0) + target.parent.Position);
			effect.Visible = true;
			effect.ProcessMode = ProcessModeEnum.Inherit;
			target.parent.AddChild(effect);
		}
	}
	private void SendGarbage(int howmuch)
	{
		int _howmuch = howmuch;
		while(garbagequeue.Count > 0 && _howmuch > 0)
		{
			garbagequeue[0] = new(garbagequeue[0].Key, new(garbagequeue[0].Value.Key - 1, garbagequeue[0].Value.Value));
			_howmuch -= 1;
			if(garbagequeue[0].Value.Key <= 0) garbagequeue.RemoveAt(0);
		}
		if(target != null)
		{
			target.RecieveGarbage(_howmuch, garbrand.Next() % 10);
			SendGarbageEffect(_howmuch);
		}
	}
	public void RecieveGarbage(int howmuch, int column)
	{
		if(howmuch > 0) garbagequeue.Add(new(column, new(howmuch, Time.GetTicksMsec() + 500)));
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

		ClearLineEffect(which);
		
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
		recentPieceRot = piece.rotation;
		defaultPiece = piece;
		locktimer = 0;
		hardlock = 0;
		gravtimer = 0;
		dcdtimer = handling.dcd;
		lowestpoint = 0;
		hardlockstarted = false;
		holdExhausted = false;
		justrotated = false;
		if(piece.Collides(playfield, default)) Die();
	}
	private void HoldPiece()
	{
		if(holdExhausted) return;
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
		holdExhausted = true;
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
	public bool TryHardLock()
	{
		if(hardlock > 15 && piece.Collides(playfield, new(0, 1)))
		{
			LockPiece();
			return true;
		}
		return false;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	bool hitFloorLastFrame = false;
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
				if(TryMovePiece(new(0, 1))) ((AudioStreamPlayer)sounds.GetChild(1)).Play();
				gravtimer -= 1;
				
			}
			bool hitfloorThisFrame = piece.Collides(playfield, new(0, 1));
			if(hitfloorThisFrame) {
				if(!hitFloorLastFrame) ((AudioStreamPlayer)sounds.GetChild(9)).Play();
				locktimer += dt; 
				hardlockstarted = true;
				TryHardLock();
			}
			else locktimer = 0;
			hitFloorLastFrame = hitfloorThisFrame;
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
				if(TryMovePiece(Vector2I.Left)) {
					locktimer = 0;
					((AudioStreamPlayer)sounds.GetChild(0)).Play();
					hardlock += 1;
					if(TryHardLock()) break;
				}
				dastimer -= handling.arr;
			}
			while(dastimer > handling.das && !piece.Collides(playfield, new(1, 0)) && holdingright && dcdtimer <= 0)
			{
				tryingDas = true;
				if(TryMovePiece(Vector2I.Right)) {
					locktimer = 0;
					((AudioStreamPlayer)sounds.GetChild(0)).Play();
					hardlock += 1;
					if(TryHardLock()) break;
				}
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
		List<AnimatedSprite2D> toremove2 = new();
		foreach(AnimatedSprite2D effect in generaleffects)
		{
			if(effect.Frame == effect.SpriteFrames.GetFrameCount(effect.Animation) - 1)
			{
				effect.QueueFree();
				toremove2.Add(effect);
			}
		}
		foreach(AnimatedSprite2D effect in toremove2)
		{
			generaleffects.Remove(effect);
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
				//if(y == effect.key) offset += Mathf.Pow(Mathf.Clamp(1 - Mathf.Lerp(effect.value, 1, -0.5f), 0, 1), 2);
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
				float alpha = 1f;

				Vector2I atlascoords = TetrisData.ByteToAtlasCoord(tiles.Get(new(x, y)));
				if(atlascoords != new Vector2I(-1, -1))
				DrawTextureRectRegion(src.Texture, 
				new(new Vector2(
					x - tiles.size.X / 2f, 
					y - tiles.size.Y / 2f) * 8 + 
					TetrisData.heldpos,
				new(8, 8)), 
				new(atlascoords * 8, new(8, 8)), new(1, 1, 1, alpha));
			}
		}
		List<int> usedkeys = new();
		foreach(KVReference effect in linedropeffects)
		{
			float t = 1 - Mathf.Pow(1f - effect.value, 3);
			int y = effect.key;
			float multilineoffset = 0;
			foreach(int key in usedkeys) if(key == y) multilineoffset -= 8;
			usedkeys.Add(y);
			for(float x = Mathf.Lerp(0, -1, t); x < Mathf.Lerp(10, 10.9f, t); x += Mathf.Lerp(1, 1.2f, t))
			{
				Vector2 pos = new Vector2(x * 8 - 40, y * 8 - 240) - new Vector2(-2, -2);
			DrawTextureRectRegion(particles.Texture, 
				new(pos + new Vector2(0, Mathf.Lerp(multilineoffset, 3f, t)), new(Mathf.Lerp(8, 8 * 1.4f, t), Mathf.Lerp(7, 0, t))), 
				new(new(2, 2), new(8, 8)));
			}
		}
		float garbageoffset = 1;
		foreach(var garbage in garbagequeue)
		{
			garbageoffset -= 1;
			for(int i = 0; i < garbage.Value.Key; i++)
			{
				if(i % 2 == 0) DrawTexture(particles.Texture, TilePosToScreenPos(new(7.5f + garbageoffset, 18f)));
				else DrawTexture(particles.Texture, TilePosToScreenPos(new(7.5f + garbageoffset, 17.4f)));
				
				garbageoffset -= 0.6f;
			}
		}
        base._Draw();
    }
    private void HardDrop()
	{
		while(TryMovePiece(new(0, 1))) continue;
		LockPiece();
		((AudioStreamPlayer)sounds.GetChild(2)).Play();
		parent.ApplyRotImpulse(new(1.5f, 0, 0));
		parent.ApplyPosImpulse(new(0, -0.5f, 0));
	}
	private void MoveLeftRight(Vector2I where)
	{
		if(TryMovePiece(where)) {
			locktimer = 0; 
			if(hardlockstarted) 
			hardlock += 1; 
			TryHardLock();
			((AudioStreamPlayer)sounds.GetChild(0)).Play();
		}
	}
    public override void _Input(InputEvent @event)
    {
		if(@event.IsActionPressed("hold")) HoldPiece();
		if(@event.IsActionPressed("left")) MoveLeftRight(Vector2I.Left);
		if(@event.IsActionPressed("right")) MoveLeftRight(Vector2I.Right);
		if(@event.IsActionPressed("cw")) {RotateCW(); dcdtimer = 0; if(hardlockstarted) hardlock += 1;}
		if(@event.IsActionPressed("ccw")) {RotateCCW(); dcdtimer = 0; if(hardlockstarted) hardlock += 1;}
		softdrop = @event.IsActionPressed("sd") || (softdrop && !@event.IsActionReleased("sd"));
		holdingleft = @event.IsActionPressed("left") || (holdingleft && !@event.IsActionReleased("left"));
		holdingright = @event.IsActionPressed("right") || (holdingright && !@event.IsActionReleased("right"));
		if(@event.IsActionPressed("hd")) HardDrop();
        base._Input(@event);
	}
}
