using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAround : MonoBehaviour 
{
	public GameObject m_thing;

	public float m_distance = 1f;
	public float m_speed = 0.5f;

	Vector3 m_initialPos;
	Vector3 m_targetPos;


	void Start () 
	{
		m_initialPos = m_thing.transform.localPosition;	
		PickTarget();
	}
	
	void Update () 
	{
		if(Vector3.Distance(m_thing.transform.localPosition, m_targetPos) < Mathf.Epsilon)
		{
			PickTarget();
		} 
		else 
		{
			m_thing.transform.localPosition = Vector3.MoveTowards(m_thing.transform.localPosition, m_targetPos, m_speed * Time.deltaTime);
		}
	}

	void PickTarget()
	{
		m_targetPos = m_initialPos + (Vector3) (m_distance * Random.insideUnitCircle);
	}
}
