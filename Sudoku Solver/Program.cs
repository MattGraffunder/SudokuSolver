using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku_Solver
{
    class Program
    {
        static int valuesToSet = 0;

        static void Main(string[] args)
        {
            int valuesSetLastRound = 0;
            
            Puzzles p = new Puzzles();
            p.LoadPuzzles(4);

            Solver solver = new Solver();
            Validator validator = new Validator();

            Sudoku puzzleToSolve = p.VeryHard;
            valuesToSet = puzzleToSolve.SpotsToSolve;

            //Start Puzzle
            Console.WriteLine("Start");
            Console.WriteLine();

            PrintSudoku(puzzleToSolve, false);

            Console.ReadKey();

            do
            {
                System.Threading.Thread.Sleep(1000);
                
                valuesSetLastRound = solver.SolveRound(puzzleToSolve);
                valuesToSet -= valuesSetLastRound;

                PrintSudoku(puzzleToSolve, true);
            } 
            while (!validator.IsValid(puzzleToSolve) && valuesSetLastRound > 0 );

            //Print Success or Failure
            if (validator.IsValid(puzzleToSolve) && valuesToSet == 0)
            {
                Console.WriteLine("Success!");
            }
            else
            {
                Console.WriteLine("Failed");
            }
                        
            Console.ReadKey();
        }

        static internal void PrintSudoku(Sudoku sudoku, bool reset)
        {
            if (reset)
            {
                ResetConsole();
            }

            for (int i = 0; i < sudoku.PuzzleSize; i++)
            {
                for (int j = 0; j < sudoku.PuzzleSize; j++)
                {
                    Console.Write(sudoku.Puzzle[i, j]);
                }

                Console.WriteLine();                
            }

            Console.WriteLine("Spots to Solve: {0}  ", valuesToSet);

            //Console.ReadKey();
        }

        static internal void ResetConsole()
        {
            Console.SetCursorPosition(0, 2);
        }
    }

    public class Sudoku
    {
        public int[,] Puzzle;

        public int SpotsToSolve = 0;

        public int PuzzleSize;

        public Sudoku(int puzzleSize)
        {
            PuzzleSize = puzzleSize;
            Puzzle = new int[puzzleSize, puzzleSize];
        }

        public static int BlockStartCol(int col)
        {
            return col - (col % 3);
        }

        public static int BlockStartRow(int row)
        {
            return row - (row % 3);
        }

        public HashSet<int> CellPosibilities(int row, int col)
        {
            if (Puzzle[row, col] != 0)
            {
                return new HashSet<int>();
            }

            HashSet<int> rowMatches = RowPosibilities(row);
            HashSet<int> colMatches = ColPosibilities(col);
            HashSet<int> blockMatches = BlockPosibilities(row, col);

            //Set Operations
            HashSet<int> posibilities = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 }; ;
            posibilities.IntersectWith(rowMatches);
            posibilities.IntersectWith(colMatches);
            posibilities.IntersectWith(blockMatches);

            return posibilities;
        }

        private HashSet<int> RowPosibilities(int row)
        {
            HashSet<int> matches = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int foundNumber = 0;

            for (int col = 0; col < 9; col++)
            {
                foundNumber = Puzzle[row, col];

                if (foundNumber != 0)
                {
                    matches.Remove(foundNumber);
                }
            }

            return matches;
        }

        private HashSet<int> ColPosibilities(int col)
        {
            HashSet<int> matches = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int foundNumber = 0;

            for (int row = 0; row < 9; row++)
            {
                foundNumber = Puzzle[row, col];

                if (foundNumber != 0)
                {
                    matches.Remove(foundNumber);
                }
            }

            return matches;
        }

        private HashSet<int> BlockPosibilities(int row, int col)
        {
            HashSet<int> matches = new HashSet<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int foundNumber = 0;

            int blockStartX = Sudoku.BlockStartCol(col);
            int blockStartY = Sudoku.BlockStartRow(row);

            for (int blockRow = blockStartY; blockRow < blockStartY + 3; blockRow++)
            {
                for (int blockCol = blockStartX; blockCol < blockStartX + 3; blockCol++)
                {
                    foundNumber = Puzzle[blockRow, blockCol];

                    if (foundNumber != 0)
                    {
                        matches.Remove(foundNumber);
                    }
                }
            }

            return matches;
        }
    }
    
    public class Puzzles
    {
        private const string src = "C:/Sudoku/Sudoku.txt";
        private const int SIZE = 9;

        public Sudoku Easy { get; set; }
        public Sudoku Medium { get; set; }
        public Sudoku Hard { get; set; }
        public Sudoku VeryHard { get; set; }

        public void LoadPuzzles(int numberOfPuzzles)
        {
            List<Sudoku> puzzles = new List<Sudoku>();
            
            string[] lines = System.IO.File.ReadAllLines(src);
                        
            for (int block = 0; block < numberOfPuzzles; block++)
            {
                Sudoku sudoku = new Sudoku(SIZE);

                int offset = block * (SIZE + 1);

                for (int row = 0; row < SIZE; row++)
                {
                    for (int i = 0; i < SIZE; i++)
                    {
                        sudoku.Puzzle[row, i] = int.Parse(lines[offset + row][i].ToString());
                        
                        if (sudoku.Puzzle[row, i] == 0)
                        {
                            sudoku.SpotsToSolve++;
                        }                        
                    }
                }

                puzzles.Add(sudoku);
            }

            Easy = puzzles[0];
            Medium = puzzles[1];
            Hard = puzzles[2];
            VeryHard = puzzles[3];
        }
    }

    public class Solver
    {
        private List<SearchRule> rules = new List<SearchRule>()
        { 
            new OnlyOnePossibleValueRule(), 
            new ValueCanOnlyBeInOneCellInRow(), 
            new ValueCanOnlyBeInOneCellInColumn(), 
            new ValueCanOnlyBeInOneCellInBlock() 
        };
        //private List<SearchRule> rules = new List<SearchRule>() { new ValueCanOnlyBeInOneCellInBlock() };
        
        public void Solve()
        {

        }

        public int SolveRound(Sudoku sudoku)
        {
            int valuesSet = 0;

            rules.ForEach(rule => valuesSet += rule.SetKnowValues(sudoku));

            return valuesSet;
        }
    }


    #region Validation

    public class Validator
    {
        private List<ValidationRule> rules;

        public Validator()
        {
            rules = new List<ValidationRule>() { new RowValidationRule(), new ColumnValidationRule(), new BlockValidationRule() };
        }

        public bool IsValid(Sudoku sudoku)
        {
            foreach (ValidationRule rule in rules)
            {
                if (!rule.RulePasses(sudoku))
                {
                    return false;
                }
            }

            return true;
        }
    }
        
    public abstract class ValidationRule
    {
        public abstract bool RulePasses(Sudoku sudoku);
    }

    public class RowValidationRule : ValidationRule
    {
        public override bool RulePasses(Sudoku sudoku)
        {
            HashSet<int> values;
            int value;

            for (int row = 0; row < sudoku.PuzzleSize; row++)
            {
                values = new HashSet<int>();

                for (int col = 0; col < sudoku.PuzzleSize; col++)
                {
                    value = sudoku.Puzzle[row, col];

                    if (value == 0 || values.Contains(value))
                    {
                        return false;
                    }

                    values.Add(value);
                }
            }

            return true;
        }
    }

    public class ColumnValidationRule : ValidationRule
    {
        public override bool RulePasses(Sudoku sudoku)
        {
            HashSet<int> values;
            int value;

            for (int col = 0; col < sudoku.PuzzleSize; col++)
            {
                values = new HashSet<int>();

                for (int row = 0; row < sudoku.PuzzleSize; row++)
                {
                    value = sudoku.Puzzle[row, col];

                    if (value == 0 || values.Contains(value))
                    {
                        return false;
                    }

                    values.Add(value);
                }
            }

            return true;
        }
    }

    public class BlockValidationRule : ValidationRule
    {
        public override bool RulePasses(Sudoku sudoku)
        {
            //Check each block
            HashSet<int> blockValues;
            int blockValue;

            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    blockValues = new HashSet<int>();

                    for (int row = blockRow * 3; row < (blockRow + 1) * 3; row++)
                    {
                        for (int col = blockCol * 3; col < (blockCol + 1) * 3; col++)
                        {                            
                            blockValue = sudoku.Puzzle[row, col];

                            if (blockValue == 0 || blockValues.Contains(blockValue))
                            {
                                return false;
                            }

                            blockValues.Add(blockValue);
                        }
                    }
                }
            }

            return true;
        }
    }

    #endregion
    
    #region Search Rules

    public abstract class SearchRule
    {
        protected int knownValue = 0;

        /// <summary>
        /// Returns Number of Values Set By Rule
        /// </summary>
        /// <param name="sudoku"></param>
        /// <returns></returns>
        public abstract int SetKnowValues(Sudoku sudoku);
    }

    public class OnlyOnePossibleValueRule : SearchRule
    {
        public override int SetKnowValues(Sudoku sudoku)
        {
            int numberOfValuesSet = 0;

            for (int row = 0; row < sudoku.PuzzleSize; row++)
            {
                for (int col = 0; col < sudoku.PuzzleSize; col++)
                {
                    HashSet<int> cellPosibilities = sudoku.CellPosibilities(row, col);

                    if (sudoku.Puzzle[row, col] == 0 && cellPosibilities.Count == 1)
                    {
                        sudoku.Puzzle[row, col] = cellPosibilities.First();
                        numberOfValuesSet++;
                    }
                }
            }

            return numberOfValuesSet;
        }
    }

    public class ValueCanOnlyBeInOneCellInRow : SearchRule
    {
        public override int SetKnowValues(Sudoku sudoku)
        {
            int numberOfValuesSet = 0;
            Dictionary<int, int> valueOccurances = new Dictionary<int, int>();
            HashSet<int>[] rowPosibilities = new HashSet<int>[sudoku.PuzzleSize]; //Hold the posibilites for the row, indexed by column number

            for (int row = 0; row < sudoku.PuzzleSize; row++)
            {
                valueOccurances.Clear();

                for (int col = 0; col < sudoku.PuzzleSize; col++)
                {
                    rowPosibilities[col] = sudoku.CellPosibilities(row, col);

                    foreach (int value in rowPosibilities[col])
                    {
                        //Load posibilities into dictionary
                        if (valueOccurances.ContainsKey(value))
                        {
                            //Add 1 for each occurance of value
                            valueOccurances[value]++;
                        }
                        else
                        {
                            valueOccurances[value] = 1;
                        }
                    }
                }

                //If value occures once, find cell and set value  
                foreach (int value in valueOccurances.Keys)
                {
                    if (valueOccurances[value] == 1)
                    {
                        //Find  Cell with Value and set value
                        for (int col = 0; col < sudoku.PuzzleSize; col++)
                        {
                            if (rowPosibilities[col].Contains(value))
                            {
                                sudoku.Puzzle[row, col]= value;
                                numberOfValuesSet++;
                            }
                        }
                    }
                }
            }

            return numberOfValuesSet;
        }
    }

    public class ValueCanOnlyBeInOneCellInColumn : SearchRule
    {
        public override int SetKnowValues(Sudoku sudoku)
        {
            int numberOfValuesSet = 0;
            Dictionary<int, int> valueOccurances = new Dictionary<int, int>();
            HashSet<int>[] colPosibilities = new HashSet<int>[sudoku.PuzzleSize];

            for (int col = 0; col < sudoku.PuzzleSize; col++)
            {
                valueOccurances.Clear();

                for (int row = 0; row < sudoku.PuzzleSize; row++)
                {
                    colPosibilities[row] = sudoku.CellPosibilities(row, col);

                    foreach (int value in colPosibilities[row])
                    {
                        //Load posibilities into dictionary
                        if (valueOccurances.ContainsKey(value))
                        {
                            //Add 1 for each occurance of value
                            valueOccurances[value]++;
                        }
                        else
                        {
                            valueOccurances[value] = 1;
                        }
                    }
                }

                //If value occures once, find cell and set value  
                foreach (int value in valueOccurances.Keys)
                {
                    if (valueOccurances[value] == 1)
                    {
                        //Find  Cell with Value and set value
                        for (int row = 0; row < sudoku.PuzzleSize; row++)
                        {
                            if (colPosibilities[row].Contains(value))
                            {
                                sudoku.Puzzle[row, col]= value;
                                numberOfValuesSet++;
                            }
                        }
                    }
                }
            }

            return numberOfValuesSet;
        }
    }

    public class ValueCanOnlyBeInOneCellInBlock : SearchRule
    {
        public override int SetKnowValues(Sudoku sudoku)
        {
            int numberOfValuesSet = 0;
            Dictionary<int, int> valueOccurances = new Dictionary<int, int>();
            HashSet<int>[,] blockPosibilities = new HashSet<int>[3, 3];

            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    blockPosibilities = new HashSet<int>[3, 3];
                    valueOccurances.Clear();

                    for (int row = blockRow * 3; row < (blockRow + 1) * 3; row++)
                    {
                        for (int col = blockCol * 3; col < (blockCol + 1) * 3; col++)
                        {
                            blockPosibilities[row % 3, col % 3] = sudoku.CellPosibilities(row, col);

                            foreach (int value in blockPosibilities[row % 3, col % 3])
                            {
                                //Load posibilities into dictionary
                                if (valueOccurances.ContainsKey(value))
                                {
                                    //Add 1 for each occurance of value
                                    valueOccurances[value]++;
                                }
                                else
                                {
                                    valueOccurances[value] = 1;
                                }
                            }
                        }
                    }

                    //If value occures once, find cell and set value  
                    foreach (int value in valueOccurances.Keys)
                    {
                        if (valueOccurances[value] == 1)
                        {
                            for (int rowOffset = 0; rowOffset < 3; rowOffset++)
                            {
                                for (int colOffset = 0; colOffset < 3; colOffset++)
                                {
                                    if (blockPosibilities[rowOffset, colOffset].Contains(value))
                                    {
                                        sudoku.Puzzle[blockRow * 3 + rowOffset, blockCol * 3 + colOffset] = value;
                                        numberOfValuesSet++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return numberOfValuesSet;
        }
    }

    #endregion
}
