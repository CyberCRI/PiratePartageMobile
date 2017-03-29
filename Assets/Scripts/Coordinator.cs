using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;

public class Coordinator : MonoBehaviour
{
	enum State { Shuffle, Play, Count, End };

	public Model m_model;

	public GameObject m_shuffleSection;
	public GameObject m_playSection;
	public GameObject m_countSection;
	public GameObject m_endSection;

	public float m_playTime = 4 * 60f;

	State m_state = State.Shuffle;
	List<Model.Card>[] m_distributedCards;
	Model.PieceCount[] m_finalPieceCounts;
	float m_startTime;


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
				if(m_startTime + m_playTime < Time.time)
				{
					m_state = State.Count;
					m_playSection.SetActive(false);
					m_countSection.SetActive(true);
				}
				else
				{
					UpdateInPlay();				
				}
				break;
		}
	}

	void UpdateInPlay()
	{
		// Update timer
		float timeLeft = m_startTime + m_playTime - Time.time;
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

		m_startTime = Time.time;
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
		string results = (difference == 0 ? "You won" : "You lose");
		m_endSection.transform.Find("Results").GetComponent<Text>().text = results;
	}

	void OnEndDoneButtonClick()
	{
		m_endSection.SetActive(false);
		m_shuffleSection.SetActive(true);

		m_shuffleSection.transform.Find("StartButton").GetComponent<Button>().interactable = false;

		m_state = State.Shuffle;
	}
}
