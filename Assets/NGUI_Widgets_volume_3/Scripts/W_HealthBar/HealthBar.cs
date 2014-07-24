using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour 
{
	
	private UISlider slider;
	
	// Use this for initialization
	void Start () 
	{
	
		this.slider = (UISlider)this.GetComponentInChildren<UISlider>();
		
		
	}
	
	
	public float GetValue()
	{
		return this.slider.sliderValue;	
	}
	
	public void SetValue( float _val)
	{
		this.slider.sliderValue = _val;	
	}
}
