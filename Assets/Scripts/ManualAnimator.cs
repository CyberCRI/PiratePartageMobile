using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualAnimator : MonoBehaviour {
	public SpriteRenderer m_spriteRenderer;
	public Sprite[] m_sprites;
	public float m_secondsPerFrame = 1f;
	public bool m_playing = true;
	public bool m_loop = false;
	public int m_frame = 0;

	float m_lastFrameAt = 0;

	// Use this for initialization
	void Start () 
	{
		m_spriteRenderer.sprite = m_sprites[0];
		m_lastFrameAt = Time.time;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(m_playing)
		{
			if(m_lastFrameAt + m_secondsPerFrame <= Time.time && (m_loop || m_frame < m_sprites.Length - 1))
			{
				m_frame = (m_frame + 1) % m_sprites.Length;
				m_lastFrameAt = Time.time;
			}
		}

		m_spriteRenderer.sprite = m_sprites[m_frame];
	}
}
