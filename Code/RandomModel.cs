using Sandbox;
using System;

public sealed class RandomModel : Component
{
	[Property] public Model[] models {  get; set; }
	protected override void OnAwake()
	{
		var rand = new Random();
		var mdl = Components.GetOrCreate<ModelRenderer>();
		//AddComponent<Rigidbody>();
		mdl.Model = models[rand.Next(0, models.Length)];
		//AddComponent<ModelCollider>();
	}
}
