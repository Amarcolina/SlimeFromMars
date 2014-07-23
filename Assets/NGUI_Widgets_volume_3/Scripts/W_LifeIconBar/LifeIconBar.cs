using UnityEngine;
using System.Collections;

public class LifeIconBar : MonoBehaviour 
{
	public GameObject prefab;
	public string spriteNameOn="HeartIcon_ON";
	public string spriteNameOff="HeartIcon_FF";
	
	
	public Vector2 offset = new Vector2(30,0);
	public int count = 5;
	public int selected=0;
	public UISprite[] items;
	
	// Use this for initialization
	void Start () 
	{
		this.items = new UISprite[this.count];
		for(int i=0;i<this.count;i++)
		{	
			GameObject star = NGUITools.AddChild(this.gameObject,this.prefab);			
			
			this.items[i] = (UISprite)star.gameObject.GetComponent<UISprite>();
			this.items[i].name = prefab.name+"_"+i.ToString();
			
			
			this.items[i].transform.localPosition = new Vector3(this.offset.x*i,this.offset.y*i,this.transform.position.z);
			
			
			
			this.items[i].spriteName = this.spriteNameOff;
			
			if (i <= this.selected) 
			{
				this.items[i].spriteName = this.spriteNameOn;
			}
			
			this.items[i].MakePixelPerfect();
		}
	}
	
}
