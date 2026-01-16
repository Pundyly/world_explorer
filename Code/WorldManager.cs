using Sandbox;
using System.Collections.Generic;

public sealed class WorldManager : Component
{
	[Property] public Material BlockMat { get; set; }
	[Property] public int RenderDistance { get; set; } = 3;
	[Property] public float BlockScale { get; set; } = 32f;

	Dictionary<Vector2Int, GameObject> Chunks = new();

	protected override void OnUpdate()
	{
		if ( Scene.Camera == null ) return;
		var playerPos = Scene.Camera.Transform.Position;

		// Размер чанка в мировых координатах
		float chunkSizeUnits = 16 * BlockScale;

		int currentChunkX = MathX.FloorToInt( playerPos.x / chunkSizeUnits );
		int currentChunkY = MathX.FloorToInt( playerPos.y / chunkSizeUnits );

		for ( int x = -RenderDistance; x <= RenderDistance; x++ )
		{
			for ( int y = -RenderDistance; y <= RenderDistance; y++ )
			{
				var pos = new Vector2Int( currentChunkX + x, currentChunkY + y );
				if ( !Chunks.ContainsKey( pos ) )
				{
					var go = new GameObject( true, $"Chunk {pos.x} {pos.y}" );
					go.Parent = GameObject;
					go.Transform.Position = new Vector3( pos.x * chunkSizeUnits, pos.y * chunkSizeUnits, 0 );

					var chunk = go.Components.Create<Chunk>();
					chunk.Initialize( pos, 16, BlockScale, BlockMat );
					Chunks.Add( pos, go );
				}
			}
		}
	}
}
