using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using UnityEngine.SceneManagement;

public class Coordinator : MonoBehaviour
{
	enum State { Intro, Menu, FiringTutorial, Shuffle, Play, Firing, Count, End };

	public Model m_model;
	public AudioSource m_musicSource;
	public AudioClip m_introMusic;
	public AudioClip m_playMusic;

	public GameObject m_introSection;
	public GameObject m_shuffleSection;
	public GameObject m_playSection;
	public GameObject m_countSection;
	public GameObject m_endSection;

	public Sprite[] m_hourglassImages;

	public float m_introTime = 5f;
	public float m_playTime = 4 * 60f;
	public int m_firingSuccessGoal = 3;
	public int m_firingFailureLimit = 3;
	public int m_firingSessionCount = 1;

	public GameObject m_menuSection;

	State m_state = State.Intro;
	List<Model.Card>[] m_distributedCards;
	Model.PieceCount[] m_finalPieceCounts;
	float m_elapsedPlayTime;
	float[] m_firingSessionStartTimes;
	int m_firingSessionsComplete;
	AsyncOperation m_sceneChangeAsyncOp;
	CannonsCoordinator m_cannonsCoordinator;
	float m_gameStartTime;


	static string MakeListOfCounts(Model.PieceCount pieceCount)
	{
		StringBuilder builder = new StringBuilder();
		builder.Append(pieceCount.m_cannonballCount).Append(" cannonballs\n");
		builder.Append(pieceCount.m_parchmentCount).Append(" parchments\n");
		builder.Append(pieceCount.m_jewelCount).Append(" jewels\n");
		builder.Append(pieceCount.m_bottleCount).Append(" bottles");
		return builder.ToString();		
	}

	static Model.PieceCount ReadPieceCounts(GameObject panel)
	{
		return new Model.PieceCount(int.Parse(panel.transform.Find("CannonballInput").GetComponent<InputField>().text),
			int.Parse(panel.transform.Find("ParchmentInput").GetComponent<InputField>().text),
			int.Parse(panel.transform.Find("JewelInput").GetComponent<InputField>().text),
			int.Parse(panel.transform.Find("BottleInput").GetComponent<InputField>().text));	
	}
	

	void Start()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		m_menuSection.transform.Find("Tutorial1Button").GetComponent<Button>().onClick.AddListener(OnTutorial1ButtonClick);
		m_menuSection.transform.Find("Tutorial2Button").GetComponent<Button>().onClick.AddListener(OnTutorial2ButtonClick);
		m_menuSection.transform.Find("Level1Button").GetComponent<Button>().onClick.AddListener(OnLevel1ButtonClick);
		m_menuSection.transform.Find("Level2Button").GetComponent<Button>().onClick.AddListener(OnLevel2ButtonClick);
		m_menuSection.transform.Find("Level3Button").GetComponent<Button>().onClick.AddListener(OnLevel3ButtonClick);

		m_shuffleSection.transform.Find("ShuffleButton").GetComponent<Button>().onClick.AddListener(OnShuffleButtonClick);
		m_shuffleSection.transform.Find("StartButton").GetComponent<Button>().onClick.AddListener(OnStartButtonClick);
		m_playSection.transform.Find("DoneButton").GetComponent<Button>().onClick.AddListener(OnPlayDoneButtonClick);
		m_countSection.transform.Find("DoneButton").GetComponent<Button>().onClick.AddListener(OnCountDoneButtonClick);
		m_endSection.transform.Find("DoneButton").GetComponent<Button>().onClick.AddListener(OnEndDoneButtonClick);
		
		PrepareShuffleSection();

		m_gameStartTime = Time.time;

		m_musicSource.clip = m_introMusic;
		m_musicSource.Play();
	}

	void Update()
	{
		switch(m_state)
		{
			case State.Intro:
				if(Time.time >= m_gameStartTime + m_introTime)
				{
					m_state = State.Menu;
					m_introSection.SetActive(false);
					m_menuSection.SetActive(true);
				}
				break;

			case State.Play:
				if(m_sceneChangeAsyncOp != null)
				{
					if(m_sceneChangeAsyncOp.isDone)
					{
						m_sceneChangeAsyncOp = null;
						
						// Show timer
						m_playSection.SetActive(true);
					}
				} 
				else
				{
					m_elapsedPlayTime += Time.deltaTime;
					if(m_elapsedPlayTime >= m_playTime)
					{
						m_state = State.Count;
						m_playSection.SetActive(false);
						m_countSection.SetActive(true);
					}
					else if(m_firingSessionsComplete < m_firingSessionCount && m_elapsedPlayTime >= m_firingSessionStartTimes[m_firingSessionsComplete])
					{
						m_state = State.Firing;
						m_sceneChangeAsyncOp = SceneManager.LoadSceneAsync("Cannons", LoadSceneMode.Additive);
					}
					else
					{
						UpdateInPlay();				
					}					
				}
				break;
			
			case State.FiringTutorial:
				if(m_sceneChangeAsyncOp != null)
				{
					if(m_sceneChangeAsyncOp.isDone)
					{
						m_sceneChangeAsyncOp = null;
						StartFiringSession(OnFiringTutorialSessionOver);
					}
				}
				break;

			case State.Firing:
				if(m_sceneChangeAsyncOp != null)
				{
					if(m_sceneChangeAsyncOp.isDone)
					{
						m_sceneChangeAsyncOp = null;
						StartFiringSession(OnFiringSessionOver);
					}
				}
				break;
		}
	}

	void OnFiringSessionOver(bool succeeded, int successCount, int failureCount)
	{
		if(succeeded)
		{
			Debug.Log("Won firing session");
			EndFiringSession();

			m_state = State.Play;
		}
		else
		{
			Debug.Log("Lost firing session");
			EndFiringSession();

			m_endSection.SetActive(true);

			m_endSection.transform.Find("Congratulations").gameObject.SetActive(false);
			m_endSection.transform.Find("Try Again").gameObject.SetActive(true);

			m_endSection.transform.Find("Explanation").GetComponent<Text>().text = "You were destroyed in battle";

			// Update star count
			var starContainer = m_endSection.transform.Find("Stars");
			foreach(Transform child in starContainer)
			{
				child.gameObject.SetActive(false);
			}

			// Report no errors
			for(int playerIndex = 0; playerIndex < 4; playerIndex++)
			{
				GameObject errorBoxes = GetErrorBoxes((Model.Player) playerIndex);

				UpdateErrorBox(errorBoxes, "CannonballBox", 0);
				UpdateErrorBox(errorBoxes, "ParchmentBox", 0);
				UpdateErrorBox(errorBoxes, "JewelBox", 0);
				UpdateErrorBox(errorBoxes, "BottleBox", 0);
			}
			
			m_state = State.End;
		}
	}

	void OnFiringTutorialSessionOver(bool succeeded, int successCount, int failureCount)
	{
		EndFiringSession();

		m_state = State.Menu;
		m_menuSection.SetActive(true);
	}

	void StartFiringSession(CannonsCoordinator.OnSessionOver onSessionOver)
	{
		// Hide timer
		m_playSection.SetActive(false);

		// Stop music
		m_musicSource.Pause();

		// Attach to event handler
		m_cannonsCoordinator = GameObject.Find("CannonsCoordinator").GetComponent<CannonsCoordinator>();
		m_cannonsCoordinator.m_onSessionOver += onSessionOver; 
		m_cannonsCoordinator.m_successGoal = m_firingSuccessGoal; 
		m_cannonsCoordinator.m_failureLimit = m_firingFailureLimit; 

		// Stop audio listener in cannons scene
		GameObject.Find("Cannons Main Camera").GetComponent<AudioListener>().enabled = false;
	}

	void EndFiringSession()
	{
		m_sceneChangeAsyncOp = SceneManager.UnloadSceneAsync("Cannons");

		// Start music again
		m_musicSource.Play();

		m_firingSessionsComplete++;
	}

	void UpdateInPlay()
	{
		// Update timer
		float timeLeft = m_playTime - m_elapsedPlayTime;
		int minutes = ((int) timeLeft) / 60;
		int seconds = ((int) timeLeft) % 60;
		m_playSection.transform.Find("Timer").GetComponent<Text>().text = string.Concat(minutes, ":", seconds < 10 ? "0" : "", seconds);

		// Update hourglass
		int imageIndex = System.Math.Min(m_hourglassImages.Length - 1, (int) (m_elapsedPlayTime / (m_playTime / m_hourglassImages.Length))); 
		m_playSection.transform.Find("Hourglass").GetComponent<Image>().sprite = m_hourglassImages[imageIndex];
	}

	void OnTutorial1ButtonClick()
	{
		m_model.m_cardsForSelf = 1;
		m_model.m_cardsForOthers = 0;
		m_model.m_starting_item_count = 8;
		m_playTime = 600;
		m_firingSessionCount = 0;

		m_menuSection.SetActive(false);
		m_shuffleSection.SetActive(true);
	}

	void OnTutorial2ButtonClick()
	{
		m_state = State.FiringTutorial;
		m_sceneChangeAsyncOp = SceneManager.LoadSceneAsync("Cannons", LoadSceneMode.Additive);

		m_menuSection.SetActive(false);
	}

	void OnLevel1ButtonClick()
	{
		m_model.m_cardsForSelf = 2;
		m_model.m_cardsForOthers = 0;
		m_model.m_starting_item_count = 8;
		m_playTime = 600;
		m_firingSessionCount = 0;

		m_menuSection.SetActive(false);
		m_shuffleSection.SetActive(true);
	}

	void OnLevel2ButtonClick()
	{
		m_model.m_cardsForSelf = 2;
		m_model.m_cardsForOthers = 1;
		m_model.m_starting_item_count = 8;
		m_playTime = 600; 
		m_firingSessionCount = 2;

		m_menuSection.SetActive(false);
		m_shuffleSection.SetActive(true);
	}

	void OnLevel3ButtonClick()
	{
		m_model.m_cardsForSelf = 2;
		m_model.m_cardsForOthers = 2;
		m_model.m_starting_item_count = 8;
		m_playTime = 600;
		m_firingSessionCount = 3;

		m_menuSection.SetActive(false);
		m_shuffleSection.SetActive(true);
	}

	void OnShuffleButtonClick()
	{
		// Shuffle the cards
		m_distributedCards = m_model.ReliablyDistributeCards();

		// Update the UI
		ClearShuffleSectionText();
		for(int playerIndex = 0; playerIndex < 4; playerIndex++)
		{
			GameObject cardBlock = GetCardBlock((Model.Player) playerIndex);
			for(int cardIndex = 0; cardIndex < m_distributedCards[playerIndex].Count; cardIndex++)
			{
				cardBlock.transform.GetChild(cardIndex).GetComponent<Text>().text = m_distributedCards[playerIndex][cardIndex].m_id;
			}
		}

		m_finalPieceCounts = m_model.CalculateFinalCounts(m_distributedCards);

		m_shuffleSection.transform.Find("StartButton").GetComponent<Button>().interactable = true;
	}

	void OnStartButtonClick()
	{
		m_shuffleSection.SetActive(false);
		m_playSection.SetActive(true);

		m_musicSource.clip = m_playMusic;
		m_musicSource.Play();

		m_firingSessionStartTimes = CalculateFiringSessionTimes(m_playTime, m_firingSessionCount);
		m_firingSessionsComplete = 0;

		m_elapsedPlayTime = 0;
		m_state = State.Play;
	}

	void OnCountDoneButtonClick()
	{
		m_countSection.SetActive(false);
		m_endSection.SetActive(true);

		m_state = State.End;

		// Read in piece counts from UI
		Model.PieceCount[] actualPieceCounts = new Model.PieceCount[4] { 
			ReadPieceCounts(m_countSection.transform.Find("EyesPieceInputPanel").gameObject),
			ReadPieceCounts(m_countSection.transform.Find("HandsPieceInputPanel").gameObject),
			ReadPieceCounts(m_countSection.transform.Find("EarsPieceInputPanel").gameObject),
			ReadPieceCounts(m_countSection.transform.Find("MouthPieceInputPanel").gameObject)
		};

		// Were the players correct?
		int difference = Model.DifferencePieceCounts(m_finalPieceCounts, actualPieceCounts);
		if(difference == 0)
		{
			m_endSection.transform.Find("Explanation").GetComponent<Text>().text = "You were perfect!";

			m_endSection.transform.Find("Congratulations").gameObject.SetActive(true);
			m_endSection.transform.Find("Try Again").gameObject.SetActive(false);
		}
		else
		{
			m_endSection.transform.Find("Explanation").GetComponent<Text>().text = string.Concat(difference, " pieces were wrong");

			m_endSection.transform.Find("Congratulations").gameObject.SetActive(false);
			m_endSection.transform.Find("Try Again").gameObject.SetActive(true);
		}

		// Update star count
		var starContainer = m_endSection.transform.Find("Stars");
		foreach(Transform child in starContainer)
		{
			child.gameObject.SetActive(false);
		}
		if(difference == 0) starContainer.GetChild(2).gameObject.SetActive(true); // 3 stars
		else if(difference < 5) starContainer.GetChild(1).gameObject.SetActive(true); // 2 stars
		else if (difference < 15) starContainer.GetChild(0).gameObject.SetActive(true); // 1 stars

		// Report errors
		for(int playerIndex = 0; playerIndex < 4; playerIndex++)
		{
			GameObject errorBoxes = GetErrorBoxes((Model.Player) playerIndex);
			var pieceCountDifference = Model.SubtractPieceCounts(m_finalPieceCounts[playerIndex], actualPieceCounts[playerIndex]);

			UpdateErrorBox(errorBoxes, "CannonballBox", pieceCountDifference.m_cannonballCount);
			UpdateErrorBox(errorBoxes, "ParchmentBox", pieceCountDifference.m_parchmentCount);
			UpdateErrorBox(errorBoxes, "JewelBox", pieceCountDifference.m_jewelCount);
			UpdateErrorBox(errorBoxes, "BottleBox", pieceCountDifference.m_bottleCount);
		}
	}

	void OnPlayDoneButtonClick()
	{
		m_state = State.Count;
		m_playSection.SetActive(false);
		m_countSection.SetActive(true);
	}

	void OnEndDoneButtonClick()
	{
		m_endSection.SetActive(false);
		m_menuSection.SetActive(true);

		PrepareShuffleSection();

		m_musicSource.clip = m_introMusic;
		m_musicSource.Play();
		
		m_state = State.Menu;
	}

	void PrepareShuffleSection()
	{
		m_shuffleSection.transform.Find("StartButton").GetComponent<Button>().interactable = false;
		ClearShuffleSectionText();
	}

	void ClearShuffleSectionText()
	{
		for(int playerIndex = 0; playerIndex < 4; playerIndex++)
		{
			GameObject cardBlock = GetCardBlock((Model.Player) playerIndex);
			for(int cardIndex = 0; cardIndex < 4; cardIndex++)
			{
				cardBlock.transform.GetChild(cardIndex).GetComponent<Text>().text = "";
			}
		}
	}

	GameObject GetCardBlock(Model.Player player)
	{
		switch(player)
		{
			case Model.Player.Eyes: return m_shuffleSection.transform.Find("EyesCardBlock").gameObject; 
			case Model.Player.Hands: return m_shuffleSection.transform.Find("HandsCardBlock").gameObject; 
			case Model.Player.Ears: return m_shuffleSection.transform.Find("EarsCardBlock").gameObject; 
			case Model.Player.Mouth: return m_shuffleSection.transform.Find("MouthCardBlock").gameObject; 
			default: throw new System.ArgumentException();
		}
	}

	GameObject GetErrorBoxes(Model.Player player)
	{
		switch(player)
		{
			case Model.Player.Eyes: return m_endSection.transform.Find("EyesErrorBoxes").gameObject; 
			case Model.Player.Hands: return m_endSection.transform.Find("HandsErrorBoxes").gameObject; 
			case Model.Player.Ears: return m_endSection.transform.Find("EarsErrorBoxes").gameObject; 
			case Model.Player.Mouth: return m_endSection.transform.Find("MouthErrorBoxes").gameObject; 
			default: throw new System.ArgumentException();
		}
	}

	void UpdateErrorBox(GameObject errorBoxes, string boxName, int difference)
	{
		var box = errorBoxes.transform.Find(boxName).gameObject;
		if(difference == 0)
		{
			box.SetActive(false);
		}
		else
		{
			box.SetActive(true);
			box.GetComponentInChildren<Text>().text = System.Math.Abs(difference).ToString();				
		}
	}

	static float[] CalculateFiringSessionTimes(float playTime, int firingSessionCount)
	{
		// Evenly place them in the play time
		float interval = playTime / (firingSessionCount + 1);
		float[] times = new float[firingSessionCount];
		for(var i = 0; i < firingSessionCount; i++) 
		{
			times[i] = (i + 1) * interval;
		}
		return times;
	}
}
