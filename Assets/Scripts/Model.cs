using UnityEngine;
using System.Collections.Generic;

public class Model : MonoBehaviour
{
	public struct PieceCount 
	{
		public PieceCount(int cannonballCount, int partchmentCount, int jewelCount, int bottleCount)
		{
			m_cannonballCount = cannonballCount;
			m_parchmentCount = partchmentCount;
			m_jewelCount = jewelCount;
			m_bottleCount = bottleCount;
		}

		override public string ToString()
		{
			return string.Concat(m_cannonballCount, " ", m_parchmentCount, " ", m_jewelCount, " ", m_bottleCount);
		}

		public int m_cannonballCount;
		public int m_parchmentCount;
		public int m_jewelCount;
		public int m_bottleCount;
	}

	public enum Player { Eyes = 0, Hands, Ears, Mouth }; 

	public struct PieceCounts {
		public PieceCount[] m_counts; 
	}

	public struct Card
	{
		public Card(int id, Player playerA, PieceCount aWillGive, Player playerB, PieceCount bWillGive)
		{
			m_id = id;
			m_playerA = playerA;
			m_aWillGive = aWillGive;
			m_playerB = playerB;
			m_bWillGive = bWillGive;
		}

		public int m_id;

		public Player m_playerA;
		public PieceCount m_aWillGive;

		public Player m_playerB;
		public PieceCount m_bWillGive;	
	}

	public static PieceCount AddPieceCounts(PieceCount a, PieceCount b)
	{
		PieceCount c;
		c.m_cannonballCount = a.m_cannonballCount + b.m_cannonballCount;	
		c.m_parchmentCount = a.m_parchmentCount + b.m_parchmentCount;	
		c.m_jewelCount = a.m_jewelCount + b.m_jewelCount;	
		c.m_bottleCount = a.m_bottleCount + b.m_bottleCount;
		return c;
	}

	public static PieceCount SubtractPieceCounts(PieceCount a, PieceCount b)
	{
		PieceCount c;
		c.m_cannonballCount = a.m_cannonballCount - b.m_cannonballCount;	
		c.m_parchmentCount = a.m_parchmentCount - b.m_parchmentCount;	
		c.m_jewelCount = a.m_jewelCount - b.m_jewelCount;	
		c.m_bottleCount = a.m_bottleCount - b.m_bottleCount;
		return c;
	}

	// Returns true if the piece counts in a are greater-than or equal to b
	public static bool GtePieceCounts(PieceCount a, PieceCount b)
	{
		return a.m_cannonballCount >= b.m_cannonballCount	
			&& a.m_parchmentCount >= b.m_parchmentCount	
			&& a.m_jewelCount >= b.m_jewelCount	
			&& a.m_bottleCount >= b.m_bottleCount;
	}

	public static void ProcessCard(PieceCount[] pieceCounts, Card card)
	{
		pieceCounts[(int) card.m_playerA] = SubtractPieceCounts(pieceCounts[(int) card.m_playerA], card.m_aWillGive);
		pieceCounts[(int) card.m_playerB] = AddPieceCounts(pieceCounts[(int) card.m_playerB], card.m_aWillGive);

		pieceCounts[(int) card.m_playerB] = SubtractPieceCounts(pieceCounts[(int) card.m_playerB], card.m_bWillGive);
		pieceCounts[(int) card.m_playerA] = AddPieceCounts(pieceCounts[(int) card.m_playerA], card.m_bWillGive);
	}

	public static bool HaveEnoughPiecesForCard(PieceCount[] pieceCounts, Card card)
	{
		return GtePieceCounts(pieceCounts[(int) card.m_playerA], card.m_aWillGive)
			&& GtePieceCounts(pieceCounts[(int) card.m_playerB], card.m_bWillGive);
	}

	public static Player ParsePlayer(string playerName) 
	{
		if(playerName == "Eyes") return Player.Eyes;
		if(playerName == "Hands") return Player.Hands;
		if(playerName == "Ears") return Player.Ears;
		if(playerName == "Mouth") return Player.Mouth;
		throw new System.ArgumentException("Cannot find player");
	}

	public int m_starting_item_count = 6;

	public TextAsset m_cardsCsv;
	public int m_cardsForSelf = 8;
	public int m_cardsForOthers = 4;

	public List<Card>[] DistributeCards()
	{
		PieceCount[] pieceCounts = new PieceCount[4] { 
			new PieceCount(m_starting_item_count, m_starting_item_count, m_starting_item_count, m_starting_item_count), 
			new PieceCount(m_starting_item_count, m_starting_item_count, m_starting_item_count, m_starting_item_count), 
			new PieceCount(m_starting_item_count, m_starting_item_count, m_starting_item_count, m_starting_item_count), 
			new PieceCount(m_starting_item_count, m_starting_item_count, m_starting_item_count, m_starting_item_count), };

		List<Card>[] distributedCards = new List<Card>[4]{ new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
		IList<Card> remainingCards = new List<Card>(m_cards);

		// Each player takes turns getting 1 card that concerns themselves
		for(int turn = 0; turn < m_cardsForSelf; turn++)
		{
			for(int player = 0; player < 4; player++)
			{
				// Take cards that concern self and that we have the pieces for
				int cardIndex = 0;
				int attempt = 0;
				while(true) 
				{
					cardIndex = Random.Range(0, remainingCards.Count - 1);
					if(((int) remainingCards[cardIndex].m_playerA == player || (int) remainingCards[cardIndex].m_playerB == player) &&
					  HaveEnoughPiecesForCard(pieceCounts, remainingCards[cardIndex])) break;

					if(attempt++ > remainingCards.Count) throw new DistributionFailureException();
				}

				ProcessCard(pieceCounts, remainingCards[cardIndex]);
				distributedCards[player].Add(remainingCards[cardIndex]);
				remainingCards.RemoveAt(cardIndex);
			}
		}

		for(int turn = 0; turn < m_cardsForOthers; turn++)
		{
			for(int player = 0; player < 4; player++)
			{
				// Take cards that DON'T concern self and that we have the pieces for
				int cardIndex = 0;
				int attempt = 0;
				while(true) 
				{
					cardIndex = Random.Range(0, remainingCards.Count - 1);
					if((int) remainingCards[cardIndex].m_playerA != player && (int) remainingCards[cardIndex].m_playerB != player &&
					  HaveEnoughPiecesForCard(pieceCounts, remainingCards[cardIndex])) break;

					if(attempt++ > remainingCards.Count) throw new DistributionFailureException();
				}

				ProcessCard(pieceCounts, remainingCards[cardIndex]);
				distributedCards[player].Add(remainingCards[cardIndex]);
				remainingCards.RemoveAt(cardIndex);
			}
		}

		return distributedCards;
	}

	// Calls DistributeCards() until it works
	public List<Card>[] ReliablyDistributeCards()
	{
		while(true)
		{
			try 
			{
				return DistributeCards();
			}
			catch(DistributionFailureException) 
			{
				Debug.Log("DistributeCards failed, trying again");
			}
		}
	}

	public PieceCount[] CalculateFinalCounts(List<Card>[] cards) 
	{
		PieceCount[] pieceCounts = new PieceCount[4] { 
			new PieceCount(m_starting_item_count, m_starting_item_count, m_starting_item_count, m_starting_item_count), 
			new PieceCount(m_starting_item_count, m_starting_item_count, m_starting_item_count, m_starting_item_count), 
			new PieceCount(m_starting_item_count, m_starting_item_count, m_starting_item_count, m_starting_item_count), 
			new PieceCount(m_starting_item_count, m_starting_item_count, m_starting_item_count, m_starting_item_count), };
		for(int i = 0; i < 4; i++)
		{
			foreach(var card in cards[i])
			{
				ProcessCard(pieceCounts, card);
			}
		}

		return pieceCounts;
	}


	enum CsvColumn { Id = 0, PlayerA, ACannonballCount, AParchmentCount, AJewelCount, ABottleCount, 
		PlayerB, BCannonballCount, BParchmentCount, BJewelCount, BBottleCount, Difficulty };

	class DistributionFailureException : System.Exception {};

	List<Card> m_cards;


	void Start()
	{
		// Load from CSV file
		m_cards = new List<Card>();

		string[] lines = m_cardsCsv.text.Split('\n');
		// Skip header line
		for(int i = 1; i < lines.Length; i++)
		{
			if(lines[i].Length == 0) continue; // Skip empty lines

			var columns = lines[i].Split(',');
			m_cards.Add(new Card(int.Parse(columns[(int) CsvColumn.Id]), 
				ParsePlayer(columns[(int) CsvColumn.PlayerA]), 
				new PieceCount(int.Parse(columns[(int) CsvColumn.ACannonballCount]), int.Parse(columns[(int) CsvColumn.AParchmentCount]), int.Parse(columns[(int) CsvColumn.AJewelCount]), int.Parse(columns[(int) CsvColumn.ABottleCount])),
				ParsePlayer(columns[(int) CsvColumn.PlayerB]), 
				new PieceCount(int.Parse(columns[(int) CsvColumn.BCannonballCount]), int.Parse(columns[(int) CsvColumn.BParchmentCount]), int.Parse(columns[(int) CsvColumn.BJewelCount]), int.Parse(columns[(int) CsvColumn.BBottleCount]))));
		}
	}
}
