using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FireTruck
{
    class Program
    {
        #region Constants
        const string FOLDER_NAME = "FireTruck";
        const string INPUT_TXT_FILE = "input.txt";
        const int FIRE_STREET_CORNER_QUANTITY_NUMBERS = 1;
        const int STOPPING_VALUE = 0;
        const string OUTPUT_TXT_FILE = "output.txt";
        const int FIRESTATION_STREET_CORNER_NUMBER = 1;
        #endregion

        static void Main(string[] args)
        {
            #region Read File
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FOLDER_NAME);

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            string inputFilePath = Path.Combine(filePath, INPUT_TXT_FILE);
            string outputFilePath = Path.Combine(filePath, OUTPUT_TXT_FILE);

            string fileData = File.ReadAllText(inputFilePath);
            #endregion

            #region 1st PASS
            // split input.txt by "0 0" string
            string[] caseStringsFromFileData = fileData.Split(new string[] { $"{STOPPING_VALUE} {STOPPING_VALUE}" }, StringSplitOptions.RemoveEmptyEntries);

            // 1st PASS:  First, need to create all Street Corners per Case in a List of Dictionarys
            List<Dictionary<int, StreetCorner>> numToStreetCornerDictCaseList = new List<Dictionary<int, StreetCorner>>();

            for (int i = 0; i < caseStringsFromFileData.Length; i++)
            {
                numToStreetCornerDictCaseList.Add(new Dictionary<int, StreetCorner>());
                string[] streetCornersStrArr = Regex.Replace(caseStringsFromFileData[i], "\r?\n", " "). // str array of all numbers in each case (with duplicates)
                                               Split(new string[] { " ", "  " }, StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < streetCornersStrArr.Length; j++)
                {
                    int num = int.Parse(streetCornersStrArr[j]);
                    if (!numToStreetCornerDictCaseList[i].ContainsKey(num)) // Only add unique keys to dictionary, avoiding duplicates
                        numToStreetCornerDictCaseList[i].Add(num, new StreetCorner(num)); // create/add new StreetCorner as Value
                }
            }
            #endregion

            #region 2nd PASS
            // 2nd PASS: Get fire number; add all ajdacent streetcorners per streetcorner, per case; Stopping Condition: apply DFS and get output
            string[] linesFromFileData = fileData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            int fireStreetCornerNumber = -1; // initialize to value outside the 1-20 set of streetcorner numbers
            int caseNum = 0;
            for (int i = 0; i < linesFromFileData.Length; i++)
            {
                string[] numOfNumberStrings = linesFromFileData[i].Split(new char[] { ' ' });

                if (numOfNumberStrings.Length == FIRE_STREET_CORNER_QUANTITY_NUMBERS)
                    fireStreetCornerNumber = int.Parse(numOfNumberStrings[0]);
                else // numStrings.Length == 2
                {
                    int num1 = int.Parse(numOfNumberStrings[0]);
                    int num2 = int.Parse(numOfNumberStrings[1]);

                    if (num1 == STOPPING_VALUE && num2 == STOPPING_VALUE) // "0 0"
                    {
                        File.AppendAllText(outputFilePath, FindAllFireTruckRoutesViaDFSForSimplePaths(
                            numToStreetCornerDictCaseList, caseNum, FIRESTATION_STREET_CORNER_NUMBER, fireStreetCornerNumber));
                        caseNum++;
                    }
                    else // not "0 0"
                    {
                        // Connect two adjacent streetcorners to one another (via their internal list of adjacent street corners)
                        numToStreetCornerDictCaseList[caseNum][num1].adjacentStreetCorners.Add(num2, numToStreetCornerDictCaseList[caseNum][num2]);
                        numToStreetCornerDictCaseList[caseNum][num2].adjacentStreetCorners.Add(num1, numToStreetCornerDictCaseList[caseNum][num1]);
                    }
                }
            }
        }
        #endregion

        #region Methods
        static public string FindAllFireTruckRoutesViaDFSForSimplePaths(List<Dictionary<int, StreetCorner>> n2scdcl, int caseNum, int stationNum, int fireNum)
                                                                   {
            // Helper objects for DFS
            List<StreetCorner> currentPath = new List<StreetCorner>();
            List<List<StreetCorner>> simplePaths = new List<List<StreetCorner>>(); // no repeated street corners

            // Main recursive algorithm
            DFSWithBacktrackingForAllSimplePaths(currentPath, simplePaths, n2scdcl[caseNum][stationNum], n2scdcl[caseNum][fireNum]);

            // Format/Sort Output for Current Case
            List<string> simplePathStrings = new List<string>();
            foreach (List<StreetCorner> simplePath in simplePaths)
            {
                string simplePathString = "";
                for (int i = 0; i < simplePath.Count; i++)
                    simplePathString += $"{simplePath[i].number}{(i == (simplePath.Count - 1) ? "\n" : " ")}";
                simplePathStrings.Add(simplePathString);
            }
            simplePathStrings.Sort();

            // Return entire case string
            return $"CASE {caseNum + 1}:\n{String.Join("", simplePathStrings.ToArray())}There are {simplePaths.Count} routes from the firestation to streetcorner {fireNum}.\n";
        }

        static public void DFSWithBacktrackingForAllSimplePaths(List<StreetCorner> cp, List<List<StreetCorner>> sp, StreetCorner u, StreetCorner v)
        {
            // https://www.baeldung.com/cs/simple-paths-between-two-vertices
            if (u.visited)
                return;

            u.visited = true;
            cp.Add(u);

            if (u.number == v.number)
            {
                sp.Add(new List<StreetCorner>(cp)); // don't pass by reference! We need to clone it here!
                u.visited = false;
                cp.RemoveAt(cp.Count - 1);
                return;
            }

            // Exhaustively probe depth
            foreach (StreetCorner adj in u.adjacentStreetCorners.Values) // one next for each of the recursive calls
                DFSWithBacktrackingForAllSimplePaths(cp, sp, adj, v);

            // Backtracking
            cp.RemoveAt(cp.Count - 1);
            u.visited = false;
        }
    }
    #endregion
}
