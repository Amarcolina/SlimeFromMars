using UnityEngine;
using System.Collections;

public class ObjectInfoCursor : MonoBehaviour 
{
	
	public GameObject target;
	public string textLine1 ="Line 1";
	public string textLine2 ="Line 2";
	
	
	private UILabel lbl1;
	private UILabel lbl2;
	
	// Use this for initialization
	void Start () 
	{
		
		this.lbl1 = (UILabel)this.transform.FindChild("Label_1").gameObject.GetComponent<UILabel>();	
		this.lbl2 = (UILabel)this.transform.FindChild("Label_2").gameObject.GetComponent<UILabel>();	
		
		this.SetTextLine(1,target.name);
		this.SetTextLine(2,textLine2);
	}
	
	public void SetTextLine(int id, string text)
	{
		if (id==1) this.lbl1.text = text;
		if (id==2) this.lbl2.text = text;
		
		
	}
	
	void Update()
	{
		
		Vector3 viewpos = Camera.main.WorldToViewportPoint(target.transform.position);
		Vector3 screenpos = UICamera.currentCamera.ViewportToScreenPoint(viewpos);
		
		float multiplier = 1f;
		
		//spos.z = this.transform.position.z;	
		this.transform.localPosition = new Vector3 (screenpos.x - Screen.width/2, screenpos.y - Screen.height/2, 0);
		
	}
}
