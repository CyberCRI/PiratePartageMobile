using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using UnityEngine.SceneManagement;

public class Coordinator : MonoBehaviour
{
	enum State { Shuffle, Play, Firing, Count, End };

	public Model m_model;

	public GameObject m_shuffleSection;
	public GameObject m_playSection;
	public GameObject m_countSection;
	public GameObject m_endSection;

	public float m_playTime = 4 * 60f;
	public int m_firingSuccessGoal = 3;
	public int m_firingFailureLimit = 3;
	public int m_firingSessionCount = 1;

	State m_state = State.Shuffle;
	List<Model.Card>[] m_distributedCards;
	Model.PieceCount[] m_finalPieceCounts;
	float m_elapsedPlayTime;
	float[] m_firingSessionStartTimes;
	int m_firingSessionsComplete;
	AsyncOperation m_sceneChangeAsyncOp;
	CannonsCoordinator m_cannonsCoordinator;


	static string MakeListOfCardIds(List<Model.Card> cards)
	{
		StringBuilder builder = new StringBuilder();
		foreach(var card in cards)
		{
			builder.Append(card.m_id).Append("\n");
		}
		return builder.ToString();
	}

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

		m_shuffleSection.transform.Find("ShuffleButton").GetComponent<Button>().onClick.AddListener(OnShuffleButtonClick);
		m_shuffleSection.transform.Find("StartButton").GetComponent<Button>().onClick.AddListener(OnStartButtonClick);
		m_countSection.transform.Find("DoneButton").GetComponent<Button>().onClick.AddListener(OnCountDoneButtonClick);
		m_endSection.transform.Find("DoneButton").GetComponent<Button>().onClick.AddListener(OnEndDoneButtonClick);
	}

	void Update()
	{
		switch(m_state)
		{
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

			case State.Firing:
				if(m_sceneChangeAsyncOp != null)
				{
					if(m_sceneChangeAsyncOp.isDone)
					{
						m_sceneChangeAsyncOp = null;
						StartFiringSession();
					}
				}
				break;
		}
	}

	void OnFiringRoundOver(bool succeeded, int successCount, int failureCount)
	{
		if(successCount >= m_firingSuccessGoal)
		{
			Debug.Log("Won firing session");
			EndFiringSession();

			m_state = State.Play;
		}
		else if(failureCount >= m_firingFailureLimit)
		{
			Debug.Log("Lost firing session");
			EndFiringSession();

			m_endSection.transform.Find("Results").GetComponent<Text>().text = "You lose";
			m_endSection.transform.Find("Explanation").GetComponent<Text>().text = "You were destroyed in battle";
			m_endSection.SetActive(true);
			m_state = State.End;
		}
	}

	void StartFiringSession()
	{
		// Hide timer
		m_playSection.SetActive(false);

		// Attach to event handler
		m_cannonsCoordinator = GameObject.Find("CannonsCoordinator").GetComponent<CannonsCoordinator>();
		m_cannonsCoordinator.m_onRoundOver += OnFiringRoundOver;
	}

	void EndFiringSession()
	{
		m_cannonsCoordinator.m_onRoundOver -= OnFiringRoundOver;
		m_sceneChangeAsyncOp = SceneManager.UnloadSceneAsync("Cannons");

		m_firingSessionsComplete++;
	}

	void UpdateInPlay()
	{
		// Update timer
		float timeLeft = m_playTime - m_elapsedPlayTime;
		int minutes = ((int) timeLeft) / 60;
		int seconds = ((int) timeLeft) % 60;
		m_playSection.transform.Find("Timer").GetComponent<Text>().text = string.Concat(minutes, ":", seconds < 10 ? "0" : "", seconds);
	}

	void OnShuffleButtonClick()
	{
		m_distributedCards = m_model.ReliablyDistributeCards();

		m_shuffleSection.transform.Find("EyesCards").GetComponent<Text>().text = MakeListOfCardIds(m_distributedCards[0]);
		m_shuffleSection.transform.Find("HandsCards").GetComponent<Text>().text = MakeListOfCardIds(m_distributedCards[1]);
		m_shuffleSection.transform.Find("EarsCards").GetComponent<Text>().text = MakeListOfCardIds(m_distributedCards[2]);
		m_shuffleSection.transform.Find("MouthCards").GetComponent<Text>().text = MakeListOfCardIds(m_distributedCards[3]);

		m_finalPieceCounts = m_model.CalculateFinalCounts(m_distributedCards);

		m_shuffleSection.transform.Find("StartButton").GetComponent<Button>().interactable = true;
	}

	void OnStartButtonClick()
	{
		m_shuffleSection.SetActive(false);
		m_playSection.SetActive(true);

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
			m_endSection.transform.Find("Results").GetComponent<Text>().text = "You won";
			m_endSection.transform.Find("Explanation").GetComponent<Text>().text = "";
		}
		else
		{
			m_endSection.transform.Find("Results").GetComponent<Text>().text = "You lose";
			m_endSection.transform.Find("Explanation").GetComponent<Text>().text = string.Concat(difference, " pieces are wrong");
		}
	}

	void OnEndDoneButtonClick()
	{
		m_endSection.SetActive(false);
		m_shuffleSection.SetActive(true);

		m_shuffleSection.transform.Find("EyesCards").GetComponent<Text>().text = "";
		m_shuffleSection.transform.Find("HandsCards").GetComponent<Text>().text = "";
		m_shuffleSection.transform.Find("EarsCards").GetComponent<Text>().text = "";
		m_shuffleSection.transform.Find("MouthCards").GetComponent<Text>().text = "";

		m_shuffleSection.transform.Find("StartButton").GetComponent<Button>().interactable = false;

		m_state = State.Shuffle;
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
