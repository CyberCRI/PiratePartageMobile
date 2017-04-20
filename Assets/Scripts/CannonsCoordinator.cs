using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonsCoordinator : MonoBehaviour 
{
	// Must be declared in L B R T order
	public GameObject[] m_fireButtons;
	// Must be declared in same order as Spots enums
	public Transform[] m_spots;
	public GameObject m_boatPrefab; 
	public GameObject m_successText;
	public GameObject m_failureText;
	public Color m_buttonDefaultColor = Color.white;
	public Color m_buttonToPressColor = Color.black;
	public Color m_buttonGoodPressColor = Color.green;
	public Color m_buttonBadPressColor = Color.red;

	public float m_timePerRound = 3f;
	public float m_timeBetweenRounds = 3f;

	public delegate void OnRoundOver(bool succeeded, int successCount, int failureCount);
	public event OnRoundOver m_onRoundOver;	

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
		BeforeRound,
		InRound
	}

	bool[] m_fireButtonsWereHit = new bool[4];
	GameObject m_boat;
	State m_state = State.BeforeRound;
	float m_startedStateTime;
	SpotIndex m_spotIndex;
	int m_successCount = 0;
	int m_failureCount = 0;


	void Start () 
	{
		m_startedStateTime = Time.time;
	}
	
	void Update () 
	{
		switch(m_state)
		{
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
		}
	}

	void StartRound()
	{
		// Hide the texts
		m_successText.SetActive(false);
		m_failureText.SetActive(false);

		// Clear the buttons list
		for(var i = 0; i < 4; i++) m_fireButtonsWereHit[i] = false;

		// Pick a spot
		m_spotIndex = PickSpotIndex();
		Transform spot = m_spots[(int) m_spotIndex];

		// Move boat there
		m_boat = Object.Instantiate(m_boatPrefab, spot.position, Quaternion.identity);

		// Set the button color
		bool[] buttonsToPress = GetButtonsToPress(m_spotIndex);
		for(var i = 0; i < 4; i++)
		{
			m_fireButtons[i].GetComponent<SpriteRenderer>().color = (buttonsToPress[i] ? m_buttonToPressColor : m_buttonDefaultColor);
		}
	}

	void StopRound()
	{
		if(HitRightButtons())
		{
			m_successText.SetActive(true);
			m_successCount++;
			if(m_onRoundOver != null) m_onRoundOver(true, m_successCount, m_failureCount);
		}
		else
		{
			m_failureText.SetActive(true);
			m_failureCount++;
			if(m_onRoundOver != null) m_onRoundOver(false, m_successCount, m_failureCount);
		}

		Object.Destroy(m_boat);
		m_boat = null;
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
					m_fireButtons[i].GetComponent<SpriteRenderer>().color = m_buttonGoodPressColor;
				}
				else
				{
					m_fireButtons[i].GetComponent<SpriteRenderer>().color = m_buttonBadPressColor;					
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
