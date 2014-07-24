using UnityEngine;
using System.Collections;

public class LevelStage : MonoBehaviour 
{
	
	public int stage_id = 0;
	public bool locked = false;
	public bool isDone = false;
	public string id_format = "{0:000}";
	public int stage_star_count = 1;
	public Color starColor = Color.white;
	public Color labelColor = Color.black;
	
	private UILabel label;
	private UISprite star_1;
	private UISprite star_2;
	private UISprite star_3;
	private UISprite stageIsDone;
	
	// Use this for initialization
	void Start () 
	{
		this.label = (UILabel)this.GetComponentInChildren<UILabel>();
		this.label.text = string.Format(this.id_format, this.stage_id);
		this.label.color = this.labelColor;
		
		this.star_1 = (UISprite)this.transform.FindChild("Stage_Star_1").gameObject.GetComponent<UISprite>();
		this.star_2 = (UISprite)this.transform.FindChild("Stage_Star_2").gameObject.GetComponent<UISprite>();
		this.star_3 = (UISprite)this.transform.FindChild("Stage_Star_3").gameObject.GetComponent<UISprite>();
		
		this.stageIsDone = (UISprite)this.transform.FindChild("IsDoneIcon").gameObject.GetComponent<UISprite>();
		
		this.SetStar(this.stage_star_count);
		
		if (this.locked) this.label.color = Color.black;
		
		if (isDone)
		{
			this.stageIsDone.alpha = 1f;
		}
		else
		{
			this.stageIsDone.alpha = 0f;
		}
	}
	
	
	
	public void SetStar(int count)
	{
		if ((count<0) || (count>3)) return;
			
		if (count==0)
		{
			this.star_1.color = Color.black;	
			this.star_2.color = Color.black;
			this.star_3.color = Color.black;
		}
		
		if (count==1)
		{
			this.star_1.color = this.starColor;	
			this.star_2.color = Color.black;
			this.star_3.color = Color.black;
		}
		
		if (count==2)
		{
			this.star_1.color = this.starColor;	
			this.star_2.color = this.starColor;
			this.star_3.color = Color.black;
		}
		
		if (count==3)
		{
			this.star_1.color = this.starColor;	
			this.star_2.color = this.starColor;
			this.star_3.color = this.starColor;
		}
	}
}
