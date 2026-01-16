using Sandbox;
using System;

public sealed class RandomText : Component
{
	[Property] public TextRenderer text {  get; set; }

	private string GenerateRandomString( )
	{
		var rand = new Random();
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789#&                           ❤🍆";
		Random random = new Random();

		char[] result = Enumerable.Repeat( chars, rand.Next(5,50) ).Select( s => s[random.Next( s.Length )] ).ToArray();

		return new string( result );
	}

	protected override void OnStart()
	{
		var rand = new Random();
		text.Text = GenerateRandomString();
		text.Scale = rand.Next( 1, 2 );
	}
}
