using UnityEngine;
using System.Collections;


public class item_rotate_self: MonoBehaviour 
{
	public float speed_x  	= 0;
	public float speed_y  	= 0;
	public float speed_z  	= 0;
	public bool worldSpace 	= false;
	
	
	
	// Update is called once per frame
	void Update () 
	{
		if (this.worldSpace)
		{
			if (this.speed_x!=0) 
			{
				
				transform.Rotate(new Vector3(1,0,0),speed_x*Time.deltaTime);
			}
		
			if (this.speed_y!=0) 
			{
				transform.Rotate(new Vector3(0,1,0),speed_y*Time.deltaTime);
			}
			
			if (this.speed_z!=0) 
			{
				transform.Rotate(new Vector3(0,0,1),speed_z*Time.deltaTime);
			}
		}
		else
		{
			if (this.speed_x!=0) 
			{
				transform.RotateAroundLocal(new Vector3(1,0,0),speed_x*Time.deltaTime);
			}
		
			if (this.speed_y!=0) 
			{
				transform.RotateAroundLocal(new Vector3(0,1,0),speed_y*Time.deltaTime);
			}
			
			if (this.speed_z!=0) 
			{
				transform.RotateAroundLocal(new Vector3(0,0,1),speed_z*Time.deltaTime);
			}
		}

	}
}
