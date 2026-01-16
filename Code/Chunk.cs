using Sandbox;
using Sandbox.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Chunk : Component
{
	[Property] public Material BlockMaterial { get; set; }
	public Vector2Int ChunkPos { get; set; }
	public int Size { get; set; } = 16;
	public float BlockSize { get; set; } = 48f;

	// Типы блоков
	const byte AIR = 0;
	const byte STONE = 1;
	const byte DIRT = 2;
	const byte GRASS = 3;
	const byte WOOD = 4;
	const byte LEAVES = 5;

	public void Initialize( Vector2Int pos, int size, float bSize, Material mat )
	{
		ChunkPos = pos;
		Size = size;
		BlockSize = bSize;
		BlockMaterial = mat;
		Generate();
	}

	void Generate()
	{
		var data = new byte[Size, Size, 32];
		var vertices = new List<Vertex>();
		var indices = new List<int>();

		// 1. Генерация рельефа
		for ( int x = 0; x < Size; x++ )
		{
			for ( int y = 0; y < Size; y++ )
			{
				float worldX = (ChunkPos.x * Size + x) * 1.5f;
				float worldY = (ChunkPos.y * Size + y) * 1.5f;
				float noiseValue = Noise.Perlin( worldX, worldY );
				int height = (int)(noiseValue * 12) + 5;

				for ( int z = 0; z < 32; z++ )
				{
					if ( z < height - 2 ) data[x, y, z] = STONE;
					else if ( z < height - 1 ) data[x, y, z] = DIRT;
					else if ( z < height ) data[x, y, z] = GRASS;
				}

				// 2. Шанс появления дерева (только на траве и подальше от краев чанка)
				if ( x > 2 && x < Size - 3 && y > 2 && y < Size - 3 )
				{
					// Используем Random основанный на позиции, чтобы деревья не менялись
					var seed = HashCode.Combine( ChunkPos, x, y );
					var random = new Random( seed );
					if ( random.NextDouble() > 0.98 ) // 2% шанс на блок
					{
						PlaceTree( data, x, y, height );
					}
				}
			}
		}

		// 3. Построение меша
		for ( int x = 0; x < Size; x++ )
		{
			for ( int y = 0; y < Size; y++ )
			{
				for ( int z = 0; z < 32; z++ )
				{
					byte block = data[x, y, z];
					if ( block == AIR ) continue;

					Color col = GetBlockColor( block );

					if ( IsAir( data, x, y, z + 1 ) ) AddFace( vertices, indices, x, y, z, Vector3.Up, col );
					if ( IsAir( data, x, y, z - 1 ) ) AddFace( vertices, indices, x, y, z, Vector3.Down, col );
					if ( IsAir( data, x + 1, y, z ) ) AddFace( vertices, indices, x, y, z, Vector3.Right, col );
					if ( IsAir( data, x - 1, y, z ) ) AddFace( vertices, indices, x, y, z, Vector3.Left, col );
					if ( IsAir( data, x, y + 1, z ) ) AddFace( vertices, indices, x, y, z, Vector3.Forward, col );
					if ( IsAir( data, x, y - 1, z ) ) AddFace( vertices, indices, x, y, z, Vector3.Backward, col );
				}
			}
		}

		if ( vertices.Count == 0 ) return;

		var mesh = new Mesh( BlockMaterial );
		mesh.CreateVertexBuffer<Vertex>( vertices.Count, vertices.ToArray() );
		mesh.CreateIndexBuffer( indices.Count, indices.ToArray() );

		var model = Model.Builder
			.AddMesh( mesh )
			.AddCollisionMesh( vertices.Select( v => v.Position ).ToArray(), indices.ToArray() )
			.Create();

		GameObject.Components.GetOrCreate<ModelRenderer>().Model = model;
		GameObject.Components.GetOrCreate<ModelCollider>().Model = model;
		GameObject.Tags.Add( "solid" );
	}

	void PlaceTree( byte[,,] data, int x, int y, int height )
	{
		int trunkHeight = 5;
		// Ствол
		for ( int i = 0; i < trunkHeight; i++ )
			if ( height + i < 32 ) data[x, y, height + i] = WOOD;

		// Листва (простая коробочка 3x3x3 наверху)
		for ( int lx = -2; lx <= 2; lx++ )
		{
			for ( int ly = -2; ly <= 2; ly++ )
			{
				for ( int lz = 0; lz <= 3; lz++ )
				{
					if ( MathF.Abs( lx ) + MathF.Abs( ly ) + MathF.Abs( lz - 1.5f ) < 3.5f ) // Скругление листвы
					{
						int nx = x + lx; int ny = y + ly; int nz = height + trunkHeight + lz - 1;
						if ( nx >= 0 && nx < Size && ny >= 0 && ny < Size && nz < 32 )
						{
							if ( data[nx, ny, nz] == AIR ) data[nx, ny, nz] = LEAVES;
						}
					}
				}
			}
		}
	}

	Color GetBlockColor( byte b ) => b switch
	{
		STONE => Color.Gray,
		DIRT => new Color( 0.4f, 0.25f, 0.15f ),
		GRASS => Color.Green,
		WOOD => new Color( 0.35f, 0.2f, 0.1f ),
		LEAVES => new Color( 0.1f, 0.5f, 0.1f ),
		_ => Color.White
	};

	bool IsAir( byte[,,] data, int x, int y, int z )
	{
		if ( x < 0 || x >= Size || y < 0 || y >= Size || z < 0 || z >= 32 ) return true;
		return data[x, y, z] == AIR;
	}

	void AddFace( List<Vertex> verts, List<int> indices, int x, int y, int z, Vector3 dir, Color col )
	{
		int count = verts.Count;
		var center = new Vector3( x, y, z ) * BlockSize;
		var h = BlockSize * 0.5f;

		Vector3 p0 = center + new Vector3( -h, -h, -h ); Vector3 p1 = center + new Vector3( h, -h, -h );
		Vector3 p2 = center + new Vector3( h, h, -h ); Vector3 p3 = center + new Vector3( -h, h, -h );
		Vector3 p4 = center + new Vector3( -h, -h, h ); Vector3 p5 = center + new Vector3( h, -h, h );
		Vector3 p6 = center + new Vector3( h, h, h ); Vector3 p7 = center + new Vector3( -h, h, h );

		Vector3 v1, v2, v3, v4;
		if ( dir == Vector3.Up ) { v1 = p4; v2 = p5; v3 = p6; v4 = p7; }
		else if ( dir == Vector3.Down ) { v1 = p0; v2 = p3; v3 = p2; v4 = p1; }
		else if ( dir == Vector3.Forward ) { v1 = p3; v2 = p7; v3 = p6; v4 = p2; }
		else if ( dir == Vector3.Backward ) { v1 = p0; v2 = p1; v3 = p5; v4 = p4; }
		else if ( dir == Vector3.Right ) { v1 = p1; v2 = p2; v3 = p6; v4 = p5; }
		else { v1 = p0; v2 = p4; v3 = p7; v4 = p3; }

		verts.Add( new Vertex( v1, dir, Vector3.Up, new Vector2( 0, 0 ) ) { Color = col } );
		verts.Add( new Vertex( v2, dir, Vector3.Up, new Vector2( 1, 0 ) ) { Color = col } );
		verts.Add( new Vertex( v3, dir, Vector3.Up, new Vector2( 1, 1 ) ) { Color = col } );
		verts.Add( new Vertex( v4, dir, Vector3.Up, new Vector2( 0, 1 ) ) { Color = col } );

		indices.Add( count + 0 ); indices.Add( count + 1 ); indices.Add( count + 2 );
		indices.Add( count + 2 ); indices.Add( count + 3 ); indices.Add( count + 0 );
	}
}
