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

		public string ToString()
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

	public static void ProcessCard(PieceCount[] pieceCounts, Card card)
	{
		pieceCounts[(int) card.m_playerA] = SubtractPieceCounts(pieceCounts[(int) card.m_playerA], card.m_aWillGive);
		pieceCounts[(int) card.m_playerB] = AddPieceCounts(pieceCounts[(int) card.m_playerB], card.m_aWillGive);

		pieceCounts[(int) card.m_playerB] = SubtractPieceCounts(pieceCounts[(int) card.m_playerB], card.m_bWillGive);
		pieceCounts[(int) card.m_playerA] = AddPieceCounts(pieceCounts[(int) card.m_playerA], card.m_bWillGive);
	}

	public static PieceCount[] CalculateFinalCounts(List<Card>[] cards) 
	{
		PieceCount[] pieceCounts = new PieceCount[4] { 
			new PieceCount(starting_item_count, starting_item_count, starting_item_count, starting_item_count), 
			new PieceCount(starting_item_count, starting_item_count, starting_item_count, starting_item_count), 
			new PieceCount(starting_item_count, starting_item_count, starting_item_count, starting_item_count), 
			new PieceCount(starting_item_count, starting_item_count, starting_item_count, starting_item_count), };
		for(int i = 0; i < 4; i++)
		{
			foreach(var card in cards[i])
			{
				ProcessCard(pieceCounts, card);
			}
		}

		return pieceCounts;
	}

// { Eyes = 0, Hands, Ears, Mouth }; 
	public static Player ParsePlayer(string playerName) 
	{
		if(playerName == "Eyes") return Player.Eyes;
		if(playerName == "Hands") return Player.Hands;
		if(playerName == "Ears") return Player.Ears;
		if(playerName == "Mouth") return Player.Mouth;
		throw new System.ArgumentException("Cannot find player");
	}

	public enum CsvColumn { Id = 0, PlayerA, ACannonballCount, AParchmentCount, AJewelCount, ABottleCount, 
		PlayerB, BCannonballCount, BParchmentCount, BJewelCount, BBottleCount, Difficulty };

	public const int starting_item_count = 20;


	public TextAsset m_cardsCsv;

	private List<Card> m_cards;

	public List<Card>[] DistributeCards()
	{
		// Give 12 cards per player, 10 that concern themselves and 2 that don't 

		List<Card>[] distributedCards = new List<Card>[4]{ new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
		IList<Card> remainingCards = new List<Card>(m_cards);

		// Each player takes turns getting 1 card that concerns themselves
		// Take 10 turns of cards that concern others
		for(int turn = 0; turn < 10; turn++)
		{
			for(int player = 0; player < 4; player++)
			{
				// Try to take cards that concern self
				int cardIndex = 0;
				for(int attempt = 0; attempt < 10; attempt++)
				{
					cardIndex = Random.Range(0, remainingCards.Count - 1);
					if((int) remainingCards[cardIndex].m_playerA == player || (int) remainingCards[cardIndex].m_playerB == player) break;
				}

				distributedCards[player].Add(remainingCards[cardIndex]);
				remainingCards.RemoveAt(cardIndex);
			}
		}

		for(int turn = 10; turn < 12; turn++)
		{
			for(int player = 0; player < 4; player++)
			{
				// Try to take cards that don't concern self
				int cardIndex = 0;
				for(int attempt = 0; attempt < 10; attempt++)
				{
					cardIndex = Random.Range(0, remainingCards.Count - 1);
					if((int) remainingCards[cardIndex].m_playerA != player && (int) remainingCards[cardIndex].m_playerB != player) break;
				}
				
				distributedCards[player].Add(remainingCards[cardIndex]);
				remainingCards.RemoveAt(cardIndex);
			}
		}

		return distributedCards;
	}

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
