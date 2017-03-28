using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;

public class Coordinator : MonoBehaviour
{
	public Model m_model;

	public Button m_button;

	public Text m_eyesCardsText;
	public Text m_handsCardsText;
	public Text m_earsCardsText;
	public Text m_mouthCardsText;

	public Text m_eyesCountsText;
	public Text m_handsCountsText;
	public Text m_earsCountsText;
	public Text m_mouthCountsText;


	void Start()
	{
		m_button.GetComponent<Button>().onClick.AddListener(OnClick);
	}

	void OnClick()
	{
		List<Model.Card>[] distributedCards = m_model.ReliablyDistributeCards();

		m_eyesCardsText.text = MakeListOfCardIds(distributedCards[0]);
		m_handsCardsText.text = MakeListOfCardIds(distributedCards[1]);
		m_earsCardsText.text = MakeListOfCardIds(distributedCards[2]);
		m_mouthCardsText.text = MakeListOfCardIds(distributedCards[3]);

		Model.PieceCount[] finalPieceCounts = m_model.CalculateFinalCounts(distributedCards);
		m_eyesCountsText.text = MakeListOfCounts(finalPieceCounts[0]);
		m_handsCountsText.text = MakeListOfCounts(finalPieceCounts[1]);
		m_earsCountsText.text = MakeListOfCounts(finalPieceCounts[2]);
		m_mouthCountsText.text = MakeListOfCounts(finalPieceCounts[3]);
	}

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
}
