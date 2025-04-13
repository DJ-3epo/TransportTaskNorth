using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportTask
{
    public class TransportTaskSolver
    {
        
        public static bool CheckBalance(int[] supply, int[] demand)
        {
            int totalSupply = supply.Sum();
            int totalDemand = demand.Sum();

            return totalSupply == totalDemand;
        }

       
        public static int[,] SolveByNorthWestCorner(int[,] costMatrix, int[] supply, int[] demand)
        {
            int m = costMatrix.GetLength(0); 
            int n = costMatrix.GetLength(1); 
            int[,] solution = new int[m, n];

            int i = 0, j = 0;

            while (i < m && j < n)
            {
                
                int transportAmount = Math.Min(supply[i], demand[j]);

                
                solution[i, j] = transportAmount;

                supply[i] -= transportAmount;
                demand[j] -= transportAmount;

                
                if (supply[i] == 0)
                    i++;

                if (demand[j] == 0)
                    j++;
            }

            return solution;
        }
    }

}
