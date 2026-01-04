using Sandbox;
using Sandbox.Utility;
using System;
using System.Collections.Generic;

public sealed class WorldMaker : Component
{
	[Property] public Material MeshMaterial { get; set; }
	[Property] public int GridSize { get; set; } = 64;
	[Property] public float CellSize { get; set; } = 64f;
	[Property] public float Scale { get; set; } = 0.03f; // Масштаб (0.01 - огромные горы)
	[Property] public float Power { get; set; } = 3.0f;  // Экспонента (чем выше, тем резче горы)
	[Property] public int MaxHeight { get; set; } = 25;  // Макс. высота в блоках
	[Property] public SceneFile Fe { get; set; }
	[Property] public GameObject models { get; set; }

	protected override void OnStart()
	{
		BuildMinecraftWorld();
	}

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "reload" ) )
		{
			Scene.Load( Fe );
		}
	}

	private void BuildMinecraftWorld()
	{
		if ( MeshMaterial == null ) return;

		var vertices = new List<Vertex>();
		var indices = new List<int>();
		var seed = Game.Random.Float( 0, 100000 );

		for ( int x = 0; x < GridSize; x++ )
		{
			for ( int y = 0; y < GridSize; y++ )
			{
				// 1. Получаем базовый шум 0...1
				float rawNoise = Noise.Perlin( (x + seed) * Scale, (y + seed) * Scale );

				// 2. ЭКСПОНЕНЦИАЛЬНЫЙ МЕТОД: делаем горы крутыми
				// Возведение в степень убирает "плоскость"
				float shapedNoise = MathF.Pow( rawNoise, Power );

				// 3. Дискретизация (Minecraft-ступеньки)
				int blocks = MathX.FloorToInt( shapedNoise * MaxHeight ) + 1;
				float height = blocks * CellSize;

				Vector3 pos = new Vector3( x * CellSize, y * CellSize, height / 2f );
				Vector3 size = new Vector3( CellSize, CellSize, height );

				// Цвет в зависимости от крутизны/высоты
				Color color = GetVoxelColor( blocks );

				AddCubeFaces( vertices, indices, pos, size, color );
				CreateCollider( pos, size );
			}
		}

		var mesh = new Mesh( MeshMaterial );
		mesh.CreateVertexBuffer( vertices.Count, Vertex.Layout, vertices.ToArray() );
		mesh.CreateIndexBuffer( indices.Count, indices.ToArray() );

		var model = Model.Builder.AddMesh( mesh ).Create();
		Components.GetOrCreate<ModelRenderer>().Model = model;


		//entities
		var rand = new Random();

		for ( int x = 0; x < 150; x++ )
		{
			GameObject bgo = models.Clone();
			bgo.WorldPosition = new Vector3( rand.Next( 0, 3980 ), rand.Next( 0, 3980 ), rand.Next( 1000, 2000 ) );
		}
	}

	private void AddCubeFaces( List<Vertex> verts, List<int> inds, Vector3 pos, Vector3 size, Color col )
	{
		Vector3 half = size * 0.5f;

		// Направления сторон
		Vector3[] normals = { Vector3.Up, Vector3.Down, Vector3.Forward, Vector3.Backward, Vector3.Left, Vector3.Right };

		foreach ( var n in normals )
		{
			int v = verts.Count;
			Vector3 s1 = new Vector3( n.z, n.x, n.y );
			Vector3 s2 = Vector3.Cross( n, s1 );

			// ПРАВИЛЬНЫЙ ПОРЯДОК (Clockwise относительно нормали), чтобы фейсы не были инвертированы
			verts.Add( new Vertex( pos + (n - s1 - s2) * half, n, s1, new Vector2( 0, 0 ) ) { Color = col } );
			verts.Add( new Vertex( pos + (n - s1 + s2) * half, n, s1, new Vector2( 0, 1 ) ) { Color = col } );
			verts.Add( new Vertex( pos + (n + s1 + s2) * half, n, s1, new Vector2( 1, 1 ) ) { Color = col } );
			verts.Add( new Vertex( pos + (n + s1 - s2) * half, n, s1, new Vector2( 1, 0 ) ) { Color = col } );

			// Индексы треугольников
			inds.Add( v + 0 ); inds.Add( v + 2 ); inds.Add( v + 1 );
			inds.Add( v + 0 ); inds.Add( v + 3 ); inds.Add( v + 2 );
		}
	}

	private Color GetVoxelColor( int h )
	{
		if ( h <= 2 ) return "#E2C391"; // Песок/Дно
		if ( h < MaxHeight * 0.5f ) return "#4CAF50"; // Трава
		if ( h < MaxHeight * 0.8f ) return "#757575"; // Камень
		return "#FFFFFF"; // Снег на пиках
	}

	private void CreateCollider( Vector3 pos, Vector3 size )
	{
		var go = new GameObject( true, "VoxelPhys" );
		go.Parent = GameObject;
		go.WorldPosition = pos;
		go.Components.Create<BoxCollider>().Scale = size;
		go.Flags = GameObjectFlags.NotSaved | GameObjectFlags.Hidden;
	}
}
