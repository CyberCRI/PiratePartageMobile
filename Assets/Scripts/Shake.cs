using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour 
{
	public GameObject m_thing;
	public float m_shakeDistance = 1f;
	//public float m_shakeAngle;
	public float m_timeBetweenShakes = 1f;

	Vector3 m_defaultPosition;
	//Vector3 m_defaultAngles;
	float m_lastShakeTime = 0;


	public void Restore()
	{
		m_thing.transform.localPosition = m_defaultPosition;
		//m_thing.transform.localEulerAngles = m_defaultAngles;
	}


	void Start () 
	{
		m_defaultPosition = m_thing.transform.localPosition;
		//m_defaultAngles = m_thing.transform.localEulerAngles;
	}
	
	void Update () 
	{
		if(m_lastShakeTime + m_timeBetweenShakes > Time.time) return;

		m_thing.transform.localPosition = m_defaultPosition + (Vector3) (m_shakeDistance * Random.insideUnitCircle);
		m_lastShakeTime = Time.time;
	}

}
