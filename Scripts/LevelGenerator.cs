using Godot;
using System;

public partial class LevelGenerator : Node2D
{
	[Export] private int width = 200;
	[Export] private int height = 100;
	[Export] private Noise noise;

	[Export] private float verticalExponent = 2;
	[Export] private float edgeWidth = 0.2f;
	
	TileMap tilemap;

	public override void _Ready()
	{
		tilemap = GetNode<TileMap>("TileMap");

		tilemap.Clear();
		var heights = new int[width];
		var map = new byte[width,height];
		
		for (var x = 0; x < width; x++)
		for (var y = 0; y < height; y++)
		{
			var noise = this.noise.GetNoise2D(x, y) * 0.5 + 0.5;

			var threshold = y / (float)height;
			threshold = 1.0f - Mathf.Pow(1.0f - threshold, verticalExponent);

			var x0 = x / (float)width;
			var x1 = Mathf.Clamp(x0, edgeWidth, 1.0f - edgeWidth);
			var edge = 1.0f - Mathf.Abs(x0 - x1) / edgeWidth;

			noise *= edge;
			
			if (noise < threshold) continue;

			if (y > heights[x]) heights[x] = y;
			map[x, y] = 1;
		}

		for (var x = 0; x < width; x++)
		{
			heights[x] += (int)((noise.GetNoise1D(x + width * 10) * 0.5 + 0.5) * 20);
		}
		
		for (var i = 0; i < 12; i++)
		for (var x = 1; x < width - 1; x++)
		{
			var a = heights[x - 1];
			var b = heights[x];
			var c = heights[x + 1];
			heights[x] = Mathf.Max(b, (a + b + c) / 3);
		}
		

		for (var x = 0; x < width; x++)
		for (var y = 0; y < height; y++)
		{
			var c = map[x, y];
			if (c == 0) continue;
			tilemap.SetCell(0, new Vector2I(x - width / 2, -y), c - 1, new Vector2I(0, 0));
		}
		
		for (var x = 0; x < width; x++)
		for (var y = 0; y < heights[x]; y++)
		{
			tilemap.SetCell(1, new Vector2I(x - width / 2, -y), 0, new Vector2I(0, 0));
		}
	}
}
