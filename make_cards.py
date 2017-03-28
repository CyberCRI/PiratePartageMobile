import itertools
import random


players = ("Eyes", "Hands", "Ears", "Mouth")
item_types = ("Cannonball", "Parchment", "Jewel", "Bottle")

min_item_count = 1
max_item_count = 6

def generate_item_counts(player_item_types):
	player_item_counts = {}
	for player_item_type_index in range(len(player_item_types)):
		player_item_type = player_item_types[player_item_type_index]
		# pick a random number, leaving some for the rest
		remaining_item_types = len(player_item_types) - player_item_type_index + 1
		item_count = random.randint(1, max_item_count - remaining_item_types)
		player_item_counts[player_item_type] = item_count
	return player_item_counts

def generate_card(item_types_count=None):
	if item_types_count == None:
		# pick a random number of item types
		item_types_count = random.randint(2, 4)

	# pick that number of items
	picked_item_types = random.sample(item_types, item_types_count)
	# separate into two lists
	split_point = random.randint(1, item_types_count - 1)
	player_a_item_types = picked_item_types[:split_point]
	player_b_item_types = picked_item_types[split_point:]

	# print("picked item types %s" % (str(picked_item_types)))
	# print("split item types %s and %s" % (str(player_a_item_types), str(player_b_item_types)))

	# pick a random number of items for each side
	player_a_item_counts = generate_item_counts(player_a_item_types)
	player_b_item_counts = generate_item_counts(player_b_item_types)

	# print("assigned item counts %s and %s" % (str(player_a_item_counts), str(player_b_item_counts)))

	return (player_a_item_counts, player_b_item_counts)

# difficulty = total kind of items to exchange (2-4) * 13 + total number of items (2-12)
def calculate_card_difficulty(card):
	(player_a, player_b, player_a_item_counts, player_b_item_counts) = card
	total_item_types = len(player_a_item_counts) + len(player_b_item_counts)
	total_items = sum(player_a_item_counts.values()) + sum(player_b_item_counts.values())
	return total_item_types * 13 + total_items

# def generate_cards_in_difficulty_range(n, min_difficulty, max_difficulty):
# 	cards = []
# 	while len(cards) < n:
# 		card = generate_card()
# 		difficulty = calculate_card_difficulty(card)
# 		if difficulty >= min_difficulty and difficulty <= max_difficulty:
# 			cards.append(card)
# 	return cards

def generate_cards_for_player_pair():
	cards = []
	for i in range(7):
		cards.append(generate_card(2))
	for i in range(7):
		cards.append(generate_card(3))
	for i in range(6):
		cards.append(generate_card(4))
	return cards

def generate_all_cards():
	cards = []
	for player_combinations in itertools.combinations(players, 2):
		player_cards = generate_cards_for_player_pair()
		for player_card in player_cards:
			cards.append((player_combinations[0], player_combinations[1], player_card[0], player_card[1]))
	return cards


cards = generate_all_cards()
print("generated %s cards" % (len(cards)))

for card in cards:
	(player_a, player_b, player_a_item_counts, player_b_item_counts) = card
	difficulty = calculate_card_difficulty(card)
	print("card %s %s %s %s -> %s" % (player_a, str(player_a_item_counts), player_b, str(player_b_item_counts), difficulty))

# TODO: generate CSV code? -> can then import in game and export to make card files

