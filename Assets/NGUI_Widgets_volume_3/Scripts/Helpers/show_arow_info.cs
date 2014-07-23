using UnityEngine;
using System.Collections;

public class show_arow_info : MonoBehaviour 
{

	public void OnHold(Vector3 dir)
	{
		this.gameObject.GetComponent<UILabel>().text = "Direction: "+dir.ToString();
	}
}
