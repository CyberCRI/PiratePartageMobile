using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonsCoordinator : MonoBehaviour 
{
	public int m_successGoal = 3;
	public int m_failureLimit = 3;

	// Must be declared in L B R T order
	public GameObject[] m_fireButtons;
	public SpriteRenderer m_boat; 
	public GameObject m_transitionInImage;
	public ManualAnimator m_blastAnimation;
	public GameObject m_hurtImage;
	public UnityStandardAssets.ImageEffects.ScreenOverlay m_screenOverlay;

	public Sprite m_buttonDefaultSprite;
	public Sprite m_buttonToPressSprite;
	public Sprite[] m_buttonBadPressedAnimation;

	public Sprite m_cannonDefaultSprite;
	public Sprite m_cannonGoodPressedSprite;
	public Sprite m_cannonBadPressedSprite;

	public Sprite m_boatDefaultSprite;
	public Sprite m_boatBrokenSprite;

	public float m_timePerRound = 3f;
	public float m_timeBetweenRounds = 3f;
	public float m_transitionInTime = 3f;
	public float m_transitionOutTime = 3f;

	public delegate void OnSessionOver(bool succeeded, int successCount, int failureCount);
	public event OnSessionOver m_onSessionOver;	

	// Declared in counter-clockwise order
	enum SpotIndex 
	{
		// Single-person spots
		L = 0,
		B,
		R,
		T,

		// 2-person spots
		LB,
		BR,
		RT,
		TL,

		// 3-person spots
		LBT,
		BRT,

		// 4-person spot
		LBRT
	}

	enum State
	{
		TransitionIn,
		BeforeRound,
		InRound,
		TransitionOut,
		Done
	}

	bool[] m_fireButtonsWereHit = new bool[4];
	State m_state = State.TransitionIn;
	float m_startedStateTime;
	SpotIndex m_spotIndex;
	int m_successCount = 0;
	int m_failureCount = 0;

	bool m_won;


	void Start () 
	{
		m_startedStateTime = Time.time;

		m_transitionInImage.SetActive(true);

		m_boat.sprite = m_boatDefaultSprite;
		m_screenOverlay.enabled = false;
	}
	
	void Update () 
	{
		switch(m_state)
		{
			case State.TransitionIn:
				if(Time.time >= m_startedStateTime + m_transitionInTime)
				{
					m_transitionInImage.SetActive(false);

					m_state = State.BeforeRound;
					m_startedStateTime = Time.time;
				}
				break;

			case State.BeforeRound:
				if(Time.time >= m_startedStateTime + m_timeBetweenRounds)
				{
					m_state = State.InRound;
					m_startedStateTime = Time.time;

					StartRound();
				}
				break;

			case State.InRound:
				if(Time.time >= m_startedStateTime + m_timePerRound)
				{
					m_state = State.BeforeRound;
					m_startedStateTime = Time.time;

					StopRound();
				}
				else
				{
					UpdateInRound();
				}
				break;	

			case State.TransitionOut:
				if(Time.time >= m_startedStateTime + m_transitionOutTime)
				{
					m_state = State.Done;
					m_startedStateTime = Time.time;

					if(m_onSessionOver != null) m_onSessionOver(m_won, m_successCount, m_failureCount);
				}
				break;
		}
	}

	void StartRound()
	{
		// Clear the buttons list
		for(var i = 0; i < 4; i++) m_fireButtonsWereHit[i] = false;

		// Pick a spot (not the same)
		SpotIndex lastSpotIndex = m_spotIndex;
		while(lastSpotIndex == m_spotIndex) m_spotIndex = PickSpotIndex();

		// Update the look of buttons
		ResetButtons();
		bool[] buttonsToPress = GetButtonsToPress(m_spotIndex);
		for(var i = 0; i < 4; i++)
		{
			if(buttonsToPress[i])
			{
				m_fireButtons[i].GetComponent<SpriteRenderer>().sprite = m_buttonToPressSprite;
			}
		}

		// Remove hurt animation if shown
		m_hurtImage.SetActive(false);
	}

	void StopRound()
	{
		if(HitRightButtons())
		{
			m_blastAnimation.gameObject.SetActive(true);
			m_blastAnimation.m_frame = 0;
			m_blastAnimation.m_playing = true;

			m_successCount++;

			if(m_successCount >= m_successGoal)
			{
				m_boat.sprite = m_boatBrokenSprite;
				m_state = State.TransitionOut;

				m_won = true;
			}
		}
		else
		{
			ShowBurstingBubbles();
			m_hurtImage.SetActive(true);

			m_failureCount++;

			if(m_failureCount >= m_failureLimit)
			{
				m_screenOverlay.enabled = true;
				m_state = State.TransitionOut;

				m_won = false;				
			}
		}
	}

	void UpdateInRound()
	{
		var buttonsPressed = GetButtonsPressed();
		bool[] buttonsToPress = GetButtonsToPress(m_spotIndex);
		for(var i = 0; i < 4; i++) 
		{
			if(buttonsPressed[i]) 
			{
				m_fireButtonsWereHit[i] = true;
				if(buttonsToPress[i]) 
				{
					m_fireButtons[i].transform.Find("Cannon").gameObject.GetComponent<SpriteRenderer>().sprite = m_cannonGoodPressedSprite;
				}
				else
				{
					m_fireButtons[i].transform.Find("Cannon").gameObject.GetComponent<SpriteRenderer>().sprite = m_cannonBadPressedSprite;
				}
			}
		} 
	}

	bool HitRightButtons()
	{
		bool[] buttonsToPress = GetButtonsToPress(m_spotIndex);
		for(var i = 0; i < 4; i++) 
		{
			if(buttonsToPress[i] != m_fireButtonsWereHit[i]) return false;
		} 
		return true;
	}

	void ShowBurstingBubbles()
	{
		bool[] buttonsToPress = GetButtonsToPress(m_spotIndex);
		for(var i = 0; i < 4; i++) 
		{
			// If the button was not pressed correctly ... 
			if(buttonsToPress[i] != m_fireButtonsWereHit[i]) 
			{
				// Hide the default "bubble" button
				var fireButton = m_fireButtons[i];
				fireButton.GetComponent<SpriteRenderer>().enabled = false;
				
				// Show the bubble bursting animation
				var bubbleBurst = fireButton.transform.Find("BubbleBurst").gameObject;
				bubbleBurst.SetActive(true);
				var animator = bubbleBurst.GetComponent<ManualAnimator>();
				animator.m_frame = 0;
				animator.m_playing = true;
			}
		} 
	}

	void ResetButtons()
	{
		foreach(GameObject fireButton in m_fireButtons) 
		{
			// Show the default "bubble" button
			fireButton.GetComponent<SpriteRenderer>().enabled = true;
			fireButton.GetComponent<SpriteRenderer>().sprite = m_buttonDefaultSprite;
			
			// Hide the bubble bursting animation
			var bubbleBurst = fireButton.transform.Find("BubbleBurst").gameObject;
			bubbleBurst.SetActive(false);

			// Show default cannon
			fireButton.transform.Find("Cannon").gameObject.GetComponent<SpriteRenderer>().sprite = m_cannonDefaultSprite;
		} 
	}

	static int CountPointsForSpot(SpotIndex spotIndex)
	{
		switch(spotIndex)
		{
			case SpotIndex.L: 
			case SpotIndex.B: 
			case SpotIndex.R: 
			case SpotIndex.T: 
				return 1;

			case SpotIndex.LB: 
			case SpotIndex.BR: 
			case SpotIndex.RT: 
			case SpotIndex.TL: 
				return 2;

			case SpotIndex.LBT: 
			case SpotIndex.BRT: 
				return 3;

			case SpotIndex.LBRT: 
				return 4;
		}

		throw new System.ArgumentException();		
	}

	static List<Vector2> GetTouchPositions()
	{
		if(Application.isEditor)
		{
			if(Input.GetMouseButtonDown(0)) return new List<Vector2>{ Input.mousePosition };
			else return new List<Vector2>();
		}
		else
		{
			List<Vector2> touchPositions = new List<Vector2>();
			for(var i = 0; i < Input.touchCount; ++i) 
			{
				if(Input.GetTouch(i).phase == TouchPhase.Began) 
				{
					touchPositions.Add(Input.GetTouch(i).position);
				}
			}
			return touchPositions;
		}
	}

	static SpotIndex PickSpotIndex()
	{
		int numberOfPlayers = Random.Range(1, 5); // Random.Range(int, int) has the 2nd argument exclusive
		switch(numberOfPlayers)
		{
			case 1: return (SpotIndex) Random.Range(0, 4);
			case 2: return (SpotIndex) Random.Range(4, 8);
			case 3: return (SpotIndex) Random.Range(8, 10);
			case 4: return SpotIndex.LBRT;
		}

		throw new System.ArgumentException();
	}

	static bool[] GetButtonsToPress(SpotIndex spotIndex)
	{
		switch(spotIndex)
		{
			case SpotIndex.L: return new bool[] { true, false, false, false};
			case SpotIndex.B: return new bool[] { false, true, false, false};
			case SpotIndex.R: return new bool[] { false, false, true, false};
			case SpotIndex.T: return new bool[] { false, false, false, true};

			case SpotIndex.LB: return new bool[] { true, true, false, false};
			case SpotIndex.BR: return new bool[] { false, true, true, false};
			case SpotIndex.RT: return new bool[] { false, false, true, true};
			case SpotIndex.TL: return new bool[] { true, false, false, true};

			case SpotIndex.LBT: return new bool[] { true, true, false, true};
			case SpotIndex.BRT: return new bool[] { false, true, true, true};

			case SpotIndex.LBRT: return new bool[] { true, true, true, true};
		}

		throw new System.ArgumentException();
	}

	bool[] GetButtonsPressed() 
	{
		List<Vector2> touchPositions = GetTouchPositions();
		bool[] buttonsPressed = new bool[4];
		foreach(var position in touchPositions)
		{
			RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero);
            if(hitInfo)
			{
				//Debug.Log(hitInfo.transform.gameObject.name);
				for(int i = 0; i < 4; i++)
				{
					if(hitInfo.transform.gameObject == m_fireButtons[i]) buttonsPressed[i] = true;
				}
			}
		}
		return buttonsPressed;
	}
}
