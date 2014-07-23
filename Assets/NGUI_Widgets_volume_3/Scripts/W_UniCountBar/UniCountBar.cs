using UnityEngine;
using System.Collections;

public class UniCountBar : MonoBehaviour 
{
	public GameObject iconPrefab;	
	public Color colorForFill=Color.white;
	public Color colorForEmpty=Color.black;
	
	public Vector2 offset = new Vector2(30,0);
	public int maxCount = 5;
	public int count=0;
	public UISprite[] items;
	
	private UILabel label;
	
	// Use this for initialization
	void Start () 
	{
		this.label = this.GetComponentInChildren<UILabel>();
		
		this.items = new UISprite[this.maxCount];
		for(int i=0;i<this.maxCount;i++)
		{	
			GameObject star = NGUITools.AddChild(this.gameObject,this.iconPrefab);			
			
			this.items[i] = (UISprite)star.gameObject.GetComponent<UISprite>();
			this.items[i].name = iconPrefab.name+"_"+i.ToString();
			
			
			this.items[i].transform.localPosition = new Vector3(this.offset.x*i,this.offset.y*i,this.transform.position.z);
			
			
			
			this.items[i].color = this.colorForEmpty;
			
			if (i <= this.count) 
			{
				this.items[i].color = this.colorForFill;
			}
			
			if (this.label!=null) this.label.text = count.ToString();
			
			this.items[i].MakePixelPerfect();
		}
	}
	
}