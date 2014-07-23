using UnityEngine;
using System.Collections;

public class show_dire_info : MonoBehaviour 
{
	
	public void OnDirection(Vector3 dir)
	{
		this.gameObject.GetComponent<UILabel>().text = "Direction: "+dir.ToString();
	}
}
