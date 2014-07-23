using UnityEngine;
using System.Collections;

public class VirtualArrowControl : MonoBehaviour 
{

	public GameObject targetReceiver;
	public string receiverMessageName = "OnHold";
	public Vector3 direction = Vector3.up;
	private bool isDown = false;
	
	void OnPress(bool active)
	{
		this.isDown = active;
	}
	
	
	// Use this for initialization
	void Update () 
	{
		
		if (this.isDown)
		{
			if (this.targetReceiver != null) this.targetReceiver.SendMessage(this.receiverMessageName,this.direction,SendMessageOptions.DontRequireReceiver);
		}
	}
}
