using System.Collections;
using Sandbox;

public enum ItemType{
	None,
	FireExtinguiser
}

public sealed class Item : Component
{
	[Property] public ItemType Type;

	private Rigidbody _rb;
	private ModelCollider _col;
	private HighlightOutline _ho;
	
	protected override void OnStart()
	{
		_rb = GameObject.Components.GetOrCreate<Rigidbody>();
		_col = GameObject.Components.GetOrCreate<ModelCollider>();
		_ho = GameObject.Components.GetOrCreate<HighlightOutline>();

		HideLine();
	}

	public Rigidbody GetRigidbody()
	{
		return _rb;
	}

	public ModelCollider GetCollider()
	{
		return _col;
	}

	public void ApplyForce( Vector3 vec)
	{
		_rb.ApplyForce(vec);
	}

	public void ShowLine()
	{
		_ho.Enabled = true;
	}

	public void HideLine()
	{
		_ho.Enabled = false;
	}

	public void Use()
	{
		switch ( Type )
		{
			case ItemType.None:
				break;
			case ItemType.FireExtinguiser:
				break;
			default:
				break;
		}
	}
}
