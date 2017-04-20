using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedingCoordinator : MonoBehaviour 
{
	public Bounds[] m_spawnAreas = new Bounds[4];
	public float m_delayBetweenSpawn = 1f;
	public float m_movementTime = 2f;
	public Fish m_fishPrefab;

	public List<Fish> m_fish;
	
	Dictionary<int, Fish> m_fingerIdToFish = new Dictionary<int, Fish>();
	float m_lastSpawnTime;


	void Update () 
	{
		HandleDeath();
		HandleSpawn();
		HandleTouch();
	}

	void HandleDeath()
	{
		for(int i = 0; i < m_fish.Count; i++)
		{
			var fish = m_fish[i];

			// TODO: check that the fish is not being controlled by a finger
			if(fish.ShouldDie())
			{
				m_fish.Remove(fish);
				Object.Destroy(fish);
			}
		}
	}

	void HandleSpawn()
	{
		if(Time.time <= m_lastSpawnTime + m_delayBetweenSpawn) return;

		// Pick a spawn area and point with it
		Bounds spawnArea = m_spawnAreas[Random.Range(0, m_spawnAreas.Length - 1)];
		Vector2 spawnPoint = new Vector2(Random.Range(spawnArea.min.x, spawnArea.max.x), Random.Range(spawnArea.min.y, spawnArea.max.y));
		Fish newFish = (Fish) Object.Instantiate(m_fishPrefab, spawnPoint, Quaternion.identity);	
		m_fish.Add(newFish);

		m_lastSpawnTime = Time.time;
	}

	void HandleTouch()
	{
		// For mouse debug, we use only the element 0 of m_fingerIdToFish
		if(Application.isEditor)
		{
			if(Input.GetMouseButtonDown(0))
			{
				foreach(var fish in m_fish)
				{
					// Check if the player is touching the fish
					// The world pos of the screen point will have the wrong Z coordinate
					Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					Bounds bounds = fish.gameObject.GetComponent<SpriteRenderer>().bounds;
					worldPos.z = bounds.center.z;

					if(bounds.Contains(worldPos))
					{
						m_fingerIdToFish[0] = fish;
					}
				}
			}
			else if(Input.GetMouseButton(0) && m_fingerIdToFish.ContainsKey(0))
			{
				Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				// The world pos of the screen point will have the wrong Z coordinate
				worldPos.z = m_fingerIdToFish[0].transform.position.z;

				m_fingerIdToFish[0].transform.position = worldPos;
			}
			else if(Input.GetMouseButtonUp(0))
			{
				m_fingerIdToFish.Remove(0);
			}
		}
		else
		{
			for(int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				if(touch.phase == TouchPhase.Began)
				{
					foreach(var fish in m_fish)
					{
						// TODO: don't allow the same fish to be touched by two different fingers

						// Check if the player is touching the fish
						// The world pos of the screen point will have the wrong Z coordinate
						Vector3 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
						Bounds bounds = fish.gameObject.GetComponent<SpriteRenderer>().bounds;
						worldPos.z = bounds.center.z;
						
						if(bounds.Contains(worldPos))
						{
							m_fingerIdToFish[touch.fingerId] = fish;
						}
					}
				}
				else if(touch.phase == TouchPhase.Moved && m_fingerIdToFish.ContainsKey(touch.fingerId))
				{
					Vector3 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
					// The world pos of the screen point will have the wrong Z coordinate
					worldPos.z = m_fingerIdToFish[touch.fingerId].transform.position.z;

					m_fingerIdToFish[touch.fingerId].transform.position = worldPos;
				}
				else if((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && m_fingerIdToFish.ContainsKey(touch.fingerId))
				{
					m_fingerIdToFish.Remove(touch.fingerId);
				}
			}
		}
	}
}
