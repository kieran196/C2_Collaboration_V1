import sys
import time

count = 0

def read_data():
    global count
    time.sleep(1)
    f = open("output.txt", "w")
    count = count + 1
    print(count)
    f.write(str(count))

def main():
    while True:
        read_data();

if __name__ == "__main__":
    main();
