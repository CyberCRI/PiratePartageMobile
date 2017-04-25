import itertools
import random
import csv
import sys


players = ("Eyes", "Hands", "Ears", "Mouth")
item_types = ("Cannonball", "Parchment", "Jewel", "Bottle")
suits = ("A", "B", "C", "D")

# for each card, give the card count, item_types_count, item_count
card_specs = [
	# 3 cards of 2 item types
	(1, 2, 2),
	(1, 2, 3),
	(1, 2, 4),
	# 2 cards of 3 item types
	(1, 3, 3),
	(1, 3, 4),
	# 1 cards of 4 item types
	(1, 4, 4),
	]

def generate_item_counts(player_item_types, item_count):
	# Start with by giving 1 item to each item type
	player_item_counts = dict(zip(player_item_types, itertools.repeat(1)))
	remaining_item_count = item_count - len(player_item_types)

	# Give 1 item at a time one of the item types, until none are left
	while remaining_item_count > 0:
		player_item_counts[random.choice(player_item_types)] += 1
		remaining_item_count = remaining_item_count - 1
		
	return player_item_counts

def generate_card(item_types_count, item_count):
	# pick a number of items
	picked_item_types = random.sample(item_types, item_types_count)
	# separate into two lists
	split_point = random.randint(1, item_types_count - 1)
	player_a_item_types = picked_item_types[:split_point]
	player_b_item_types = picked_item_types[split_point:]

	# print("picked item types %s" % (str(picked_item_types)))
	# print("split item types %s and %s" % (str(player_a_item_types), str(player_b_item_types)))

	# pick a random number of items for each side
	player_a_item_counts = generate_item_counts(player_a_item_types, item_count)
	player_b_item_counts = generate_item_counts(player_b_item_types, item_count)

	# print("assigned item counts %s and %s" % (str(player_a_item_counts), str(player_b_item_counts)))

	return (player_a_item_counts, player_b_item_counts)

# difficulty = total kind of items to exchange (2-4) * 13 + total number of items (2-12)
def calculate_card_difficulty(card):
	(player_a, player_b, player_a_item_counts, player_b_item_counts) = card
	total_item_types = len(player_a_item_counts) + len(player_b_item_counts)
	total_items = sum(player_a_item_counts.values()) + sum(player_b_item_counts.values())
	return total_item_types * 13 + total_items

def generate_cards_for_player_pair():
	cards = []

	for (card_count, item_type_count, item_count) in card_specs:
		for i in xrange(card_count):
			# Avoid duplicates by looping until a unique card is found
			while True:
				card = generate_card(item_type_count, item_count)
				if card not in cards:
					cards.append(card)
					break

	return cards

def generate_all_cards():
	cards = []
	for player_combinations in itertools.combinations(players, 2):
		player_cards = generate_cards_for_player_pair()
		for player_card in player_cards:
			cards.append((player_combinations[0], player_combinations[1], player_card[0], player_card[1]))
	return cards

def make_csv_item_counts(player_item_counts):
	return [player_item_counts.get("Cannonball", 0), 
		player_item_counts.get("Parchment", 0), 
		player_item_counts.get("Jewel", 0), 
		player_item_counts.get("Bottle", 0)]

cards = generate_all_cards()
# print("generated %s cards" % (len(cards)))

writer = csv.writer(sys.stdout)
writer.writerow(["id", "player_a", "a_cannonball_count", "a_parchment_count", "a_jewel_count", "a_bottle_count", 
	"player_b", "b_cannonball_count", "b_parchment_count", "b_jewel_count", "b_bottle_count", "difficulty"])
for (card_index, card) in zip(itertools.count(), cards):
	difficulty = calculate_card_difficulty(card)

	cards_per_suit = len(cards) / len(suits)
	suit = suits[card_index / cards_per_suit]
	number = card_index % cards_per_suit + 1
	card_id = suit + str(number)
 
	(player_a, player_b, player_a_item_counts, player_b_item_counts) = card
	writer.writerow([card_id, player_a] + make_csv_item_counts(player_a_item_counts) + 
		[player_b] + make_csv_item_counts(player_b_item_counts) + [difficulty])
