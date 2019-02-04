using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace SnakeGame
{
    struct Position
    {
        public int row;
        public int col;

        public Position(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }
    class Program
    {      
        static void Main(string[] args)
        {
            //Varialbles for directions
            byte right = 0;
            byte left = 1;
            byte down = 2;
            byte up = 3;

            //Variables for food and for points
            int lastFoodTime = 0;
            int foodDissapearTime = 10000;
            int negativePoints = 0;
            //Declaring obstecles
            List<Position> obstecles = new List<Position>();
            //Declaring array of directions so it is esier for me to change positions
            Position[] directions = new Position[]
            {
                new Position(0, 1),   //Right
                new Position(0, -1),  //Left
                new Position(1, 0),   //Down
                new Position(-1, 0),  //Up
            };

            double gameSpeed = 100;
            int direction = 0;

            Random randNum = new Random();
            //Fixing console size
            Console.BufferHeight = Console.WindowHeight;
            //Declaring snake elements
            Queue<Position> snakeElements = new Queue<Position>();
            Position food = SpawnFood(snakeElements, obstecles);                     
            for (int i = 0; i < 6; i++)
            {
                snakeElements.Enqueue(new Position(0, i));
            }
            //Creating food and apple
            PrintFood(food);
            lastFoodTime = Environment.TickCount;
            PrintSnake(snakeElements, direction);
            //Spawning and printing obstecles
            for (int i = 0; i < 25; i++)
            {
                var temp = new Position();
                do
                {
                    Random rand = new Random();
                    Random secRand = new Random();
                    temp = new Position(rand.Next(0, Console.WindowHeight), secRand.Next(0, Console.WindowWidth));
                } while (snakeElements.Contains(temp));
                obstecles.Add(temp);
            }
            foreach (var obstecle in obstecles)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(obstecle.col, obstecle.row);
                Console.Write("X");
            }
            
            
            
            while (true)
            {
                //Taking the direction of the snake by clicking one of the four arrow keys
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo userInput = Console.ReadKey();
                    if (userInput.Key == ConsoleKey.LeftArrow)
                    {
                        if (direction != right)
                        {
                            direction = left;
                        }
                        
                    }

                    if (userInput.Key == ConsoleKey.RightArrow)
                    {
                        if (direction != left)
                        {
                            direction = right;
                        }
                        
                    }

                    if (userInput.Key == ConsoleKey.UpArrow)
                    {
                        if (direction != down)
                        {
                            direction = up;
                        }
                    }

                    if (userInput.Key == ConsoleKey.DownArrow)
                    {
                        if (direction != up)
                        {
                            direction = down;
                        }
                    }
                    PrintSnake(snakeElements, direction);
                }
                              
                Position newHead = snakeElements.Last();
                //Feeding the snake
                if (newHead.col == food.col && newHead.row == food.row)
                {
                    FeedSnake(snakeElements, direction, directions);                  
                    food = SpawnFood(snakeElements, obstecles);
                    PrintFood(food);
                    lastFoodTime = Environment.TickCount;
                    gameSpeed--;
                }
                //Moving the snake and check if it bit itself
                else
                {
                    Position lastElement = snakeElements.Dequeue();
                    Console.SetCursorPosition(lastElement.col, lastElement.row);
                    Console.Write(" ");
                    Position currentDirection = directions[direction];
                    Position snakeNewPosition = new Position(newHead.row + currentDirection.row,
                        newHead.col + currentDirection.col);
                    if (snakeNewPosition.col < 0)
                    {
                        snakeNewPosition.col = Console.WindowWidth - 1;
                    }
                    if (snakeNewPosition.row < 0)
                    {
                        snakeNewPosition.row = Console.WindowHeight - 1;
                    }
                    if (snakeNewPosition.row >= Console.WindowHeight)
                    {
                        snakeNewPosition.row = 0;
                    }
                    if (snakeNewPosition.col >= Console.WindowWidth)
                    {
                        snakeNewPosition.col = 0;
                    }
                    //Game is over 
                    if (snakeElements.Contains(snakeNewPosition) ||
                        obstecles.Contains(snakeNewPosition))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.SetCursorPosition(0, 0);
                        int userPoints = ((snakeElements.Count - 5) * 10) - negativePoints;
                        userPoints = Math.Max(0, userPoints);
                        Console.WriteLine("Game Over!");
                        Console.WriteLine($"Your points are: {userPoints}");
                        return;
                    }

                    Console.SetCursorPosition(snakeNewPosition.col, snakeNewPosition.row);
                    Console.Write("*");

                    snakeElements.Enqueue(snakeNewPosition);
                    Console.SetCursorPosition(snakeNewPosition.col, snakeNewPosition.row);
                    PrintSnake(snakeElements, direction); //?
                }

                //Checking if the time for an apple is over
                if (Environment.TickCount - lastFoodTime >= foodDissapearTime)
                {
                    negativePoints += 5;
                    Console.SetCursorPosition(food.col, food.row);
                    Console.Write(" ");
                    food = SpawnFood(snakeElements, obstecles);
                    PrintFood(food);
                    lastFoodTime = Environment.TickCount;
                }

                //Changin the speed of the game
                gameSpeed -= 0.01;
                Thread.Sleep((int)gameSpeed);               
            }            
        }
        
        public static Position SpawnFood(Queue<Position> snakeElements, List<Position> obstecles)
        {
            Position food;
            do
            {
                Random randNum = new Random();          
                food = new Position(randNum.Next(0, Console.WindowHeight - 1), randNum.Next(0, Console.WindowWidth - 1));
            } while (snakeElements.Contains(food) || obstecles.Contains(food));
            

            return food;
        }
        public static void PrintFood(Position food)
        {
            Console.SetCursorPosition(food.col, food.row);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("@");
        }
        public static void PrintSnake(Queue<Position> snake, int direction)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            int count = 0;
            foreach (var position in snake)
            {
                Console.SetCursorPosition(position.col, position.row);
                if (count == snake.Count - 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    if (direction == 0)
                    {
                        Console.Write(">");
                    }
                    if (direction == 1)
                    {
                        Console.Write("<");
                    }
                    if (direction == 2)
                    {
                        Console.Write("v");
                    }
                    if (direction == 3)
                    {
                        Console.Write("^");
                    }

                }
                
                else Console.Write("*");
                count++;
            }
        }

        public static Queue<Position> FeedSnake(Queue<Position> snakeElements, int direction, Position[] directons)
        {
            Position snakeHead = snakeElements.Last();
            Position currentDirection = directons[direction];
            Position snakeNewPosition = new Position(snakeHead.row + currentDirection.row,
                snakeHead.col + currentDirection.col);
            snakeElements.Enqueue(snakeNewPosition);
            return snakeElements;
        }
    }
}
