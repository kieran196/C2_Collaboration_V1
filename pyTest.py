import random

intTest = 5

def test():
    return random.randint(1, 100)

def main():
    while True:
        intTest = random.randint(1, 100)
        print(intTest);
        
if __name__ == "__main__":
    main()
