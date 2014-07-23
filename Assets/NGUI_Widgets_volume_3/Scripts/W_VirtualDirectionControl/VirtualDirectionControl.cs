using UnityEngine;
using System.Collections;

public class VirtualDirectionControl : MonoBehaviour 
{
	
	public GameObject targetReceiver;
	public string receiverMessageName = "OnDirection";
	
	public Vector3 direction;
	public UITexture cursor;
	
	
	private bool isDown = false;
	private Vector3 screenControlPivot = Vector3.zero;
	private Vector3 mousePos;
	private Vector3 CursorPos;
	
	void OnPress(bool active)
	{
		this.isDown = active;
	}
	
	
	void Start()
	{
		this.CursorPos = this.transform.localPosition;	
		
	}
	
	// Use this for initialization
	void Update () 
	{
		this.CursorPos = this.transform.parent.transform.localPosition;	
		this.screenControlPivot.x = ( Screen.width / 2 ) + this.CursorPos.x;
		this.screenControlPivot.y = ( Screen.height / 2 ) + this.CursorPos.y;
		
		if (this.isDown)
		{
			this.mousePos = Input.mousePosition;
			this.direction = -this.screenControlPivot + this.mousePos;
			this.direction.Normalize();
			float angle_Z  = Vector3.Angle(this.direction, new Vector3(0,1,0));
			
			if (this.direction.x>0)
			{
				this.cursor.transform.rotation = Quaternion.Euler(0,0, -angle_Z);
			}
			else
			{
				this.cursor.transform.rotation = Quaternion.Euler(0,0, angle_Z);
			}
			
			if (this.targetReceiver != null) this.targetReceiver.SendMessage(this.receiverMessageName,this.direction,SendMessageOptions.DontRequireReceiver);
		}
	}
	
}
