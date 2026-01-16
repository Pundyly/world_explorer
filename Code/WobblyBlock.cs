using Sandbox;
using System;

public sealed class WobblyBlock : Component
{
	[Property] public float Intensity { get; set; }
	[Property] public float Speed { get; set; }

	private Vector3 _baseScale;

	protected override void OnStart()
	{
		_baseScale = Transform.LocalScale;
		Intensity = Random.Shared.Float( 0.2f, 2 );
		Speed = Random.Shared.Float( 25, 30 );
	}

	protected override void OnUpdate()
	{
		float noiseX = (Random.Shared.Float( -1, 1 ) * Intensity);
		float noiseY = (Random.Shared.Float( -1, 1 ) * Intensity);
		float noiseZ = (Random.Shared.Float( -1, 1 ) * Intensity);

		Vector3 targetScale = _baseScale + new Vector3( noiseX, noiseY, noiseZ );

		Transform.LocalScale = Vector3.Lerp( Transform.LocalScale, targetScale, Time.Delta * Speed );
	}
}
