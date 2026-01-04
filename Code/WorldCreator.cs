using Sandbox;
using System;

public sealed class WorldCreator : Component
{
	[Property] public GameObject prefab { get; set; }
	[Property] public GameObject empty { get; set; }
	[Property] public GameObject models {  get; set; }
	[Property] public Material[] Skys { get; set; }
	[Property] public SceneFile Fe {  get; set; }

	protected override void OnStart()
	{
		CreateRandomWorld();
	}

	private Color GetSoftColor()
	{
		return new ColorHsv( Game.Random.Float( 0, 360 ), 0.3f, 0.9f );
	}

	private void CreateRandomWorld()
	{
		var rand = new Random();

		//direction light
		GameObject light = empty.Clone();
		light.Name = "Directional light";
		light.Components.GetOrCreate<DirectionalLight>().LightColor = GetSoftColor();
		light.WorldRotation = new Angles( rand.Next( 0, 360 ), rand.Next( -180, 0 ), 0 );

		//skybox
		GameObject sky = empty.Clone();
		sky.Name = "Skybox";
		var skybox = sky.Components.GetOrCreate<SkyBox2D>();
		skybox.SkyMaterial = Skys[rand.Next(0, Skys.Length)];
		skybox.Tint = GetSoftColor();

		//waterlevel
		GameObject water = prefab.Clone();
		water.Name = "Ocean";
		water.WorldPosition = new Vector3( 0, 0, rand.Next( -5625, 172 ));
		water.WorldScale = new Vector3( 1000, 1000, 100);
		water.GetComponent<ModelRenderer>().Tint = "#003BFF3F";
		water.GetComponent<BoxCollider>().IsTrigger = true;
		water.Tags.Add("water");

		//small fishes
		for ( int x = 0; x < 501; x++ )
		{
			GameObject go = prefab.Clone();
			go.WorldPosition = new Vector3( rand.Next(-4000, 4000 ), rand.Next( -4000, 4000 ), rand.Next( -4000, 4000 ) );
			go.WorldRotation = new Angles( rand.Next( 0, 360 ), rand.Next( -359, 359 ), rand.Next( -359, 359 ) );
			go.WorldScale = new Vector3( rand.Next(0, 10), rand.Next( 0, 10 ), rand.Next( 0, 10 ) );
			go.GetComponent<ModelRenderer>().Tint = Color.Random;
		}

		//big fishes
		for ( int x = 0; x < 15; x++ )
		{
			GameObject bgo = prefab.Clone();
			bgo.WorldPosition = new Vector3( rand.Next( -4000, 4000 ), rand.Next( -4000, 4000 ), rand.Next( -4000, 4000 ) );
			bgo.WorldScale = new Vector3( rand.Next( 10, 100 ), rand.Next( 10, 100 ), rand.Next( 10, 100 ) );
			bgo.GetComponent<ModelRenderer>().Tint = Color.Random;
		}

		//entities
		for ( int x = 0; x < 150; x++ )
		{
			GameObject bgo = models.Clone();
			bgo.WorldPosition = new Vector3( rand.Next( -3000, 3000 ), rand.Next( -3000, 3000 ), rand.Next( -3000, 3000 ) );
		}
	}

	protected override void OnUpdate()
	{
		if (Input.Pressed("reload"))
		{
			Scene.Load( Fe ); 
		}
	}
}
