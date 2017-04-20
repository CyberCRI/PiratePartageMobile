using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour 
{
	public float m_lifeTime;

	float m_spawnTime;


	public bool ShouldDie() { return Time.time >= m_spawnTime + m_lifeTime; }


	void Start () 
	{
		m_spawnTime = Time.time;
	}
	
	void Update () 
	{
		// TODO: move		
	}
}
