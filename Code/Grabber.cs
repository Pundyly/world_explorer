using Sandbox;

public sealed class Grabber : Component
{
	[Property] public GameObject helper {  get; set; }
	[Property] public PlayerController Controller;
	[Property] public Item ItemInHands;
	[Property] public float PickupRange;
	[Property] public float dist;

	private Item lastHighlightedItem;

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "Slot0" ) )
			helper.Enabled = !helper.Enabled;
		if ( Input.Pressed( "attack2" ) )
			ThrowItem( false );
		if ( Input.Pressed( "Drop" ) )
			ThrowItem( true );

		if ( !ItemInHands.IsValid() && Input.Pressed( "Use" ) )
			PickupItem( CastTrace() );
		HighlightItem( CastTrace() );
	}

	protected override void OnFixedUpdate()
	{
		if ( ItemInHands.IsValid() )
		{
			var rotation = Controller.EyeAngles.ToRotation();
			ItemInHands.WorldPosition = Controller.EyePosition + rotation.Forward * dist + rotation.Right * 25f - rotation.Up * 20f;
			ItemInHands.WorldRotation = rotation;
		}
	}

	private Item CastTrace()
	{
		var End = Controller.EyePosition + Controller.EyeAngles.Forward * PickupRange;
		var Trace = Scene.Trace.Ray( Controller.EyePosition, End ).Radius( 2f ).WithoutTags("fence").IgnoreGameObjectHierarchy( GameObject ).Run();
		if ( Trace.GameObject?.Components.Get<Item>() is not { } item) return null;
		return item;
	}

	private void PickupItem( Item item )
	{
		if ( !item.IsValid() ) return;
		ItemInHands = item;
		ItemInHands.GetCollider().Enabled = false;
		ItemInHands.GetRigidbody().Enabled = false;
	}

	private void HighlightItem( Item item )
	{
		if ( lastHighlightedItem.IsValid() && lastHighlightedItem != item ) 
			lastHighlightedItem.HideLine(); 
		if ( ItemInHands.IsValid()) return;
		if ( item.IsValid() ) 
			item.ShowLine(); 
		lastHighlightedItem = item;
	}

	private void ThrowItem( bool Lower)
	{
		if ( !ItemInHands.IsValid() ) return;
		var throwDirection = Controller.EyeAngles.Forward;

		ItemInHands.GetRigidbody().Enabled = true;
		ItemInHands.GetCollider().Enabled = true;
		if ( !Lower )
			ItemInHands.ApplyForce( throwDirection * 10000000 * ( ItemInHands.GetRigidbody().Mass / 434 ) * 3);
		ItemInHands = null;
	}
}
