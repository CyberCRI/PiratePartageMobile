import csv
import sys

reader = csv.reader(sys.stdin)
writer = csv.writer(sys.stdout)

# Copy header row, adding @ signs in the middle columns
header_row = reader.next()
middle_cols = ["@" + x for x in header_row[1:-1]]
writer.writerow(header_row[0:1] + middle_cols + header_row[-1:])

# Copy other rows, adding in a .png extension to the middle columns
for row in reader:
	middle_cols = [x + ".png" for x in row[1:-1]]
	writer.writerow(row[0:1] + middle_cols + row[-1:])
