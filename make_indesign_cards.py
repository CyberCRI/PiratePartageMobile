import csv
import sys

reader = csv.reader(sys.stdin)
writer = csv.writer(sys.stdout)

# Copy header row
writer.writerow(reader.next())

# Copy other rows, adding in a .png extension to certain columns
for row in reader:
	middle_cols = [x + ".png" for x in row[1:-1]]
	writer.writerow(row[0:1] + middle_cols + row[-1:])
