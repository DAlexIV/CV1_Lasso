using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LassoTool
{
    class ImGraph
    {
        public int[] d; //Distance to nodes
        int[] Djres; // Ancestors array
        int[,] m; // Bidimesional image presentation
        ver[][] g; //Image graph

        static public int load = 0; // Loadin process

        //Checks if dot gets to image
        public static bool InRange(int k, int W)
        {
            return (k - 1 >= 0 && k + 1 <= W + 1);
        }

        /// <summary>
        /// Processes neigbours of point into graph edges
        /// </summary>
        /// <returns> List with neighbours of this dot</returns>
        public static List<ver> LMaker(int[,] m, int i, int k, int H, int W)
        {
            List<ver> ret = new List<ver>();
            ver tmp; //Temporarily edge

            if (i - 1 > 0 && k + 1 < W + 1)
            {
                tmp.n = ((i - 1) - 1) * W + ((k + 1) - 1);
                tmp.len = (int)(255 - Math.Abs(m[i - 1, k] - m[i, k + 1]));
                ret.Add(tmp);
            }

            if (i + 1 < H + 1 && k + 1 < W + 1)
            {
                tmp.n = ((i + 1) - 1) * W + ((k + 1) - 1);
                tmp.len = (int)(255 - Math.Abs(m[i + 1, k] - m[i, k + 1]));
                ret.Add(tmp);
            }

            if (i + 1 < H + 1 && k - 1 > 0)
            {
                tmp.n = ((i + 1) - 1) * W + ((k - 1) - 1);
                tmp.len = (int)(255 - Math.Abs(m[i + 1, k] - m[i, k - 1]));
                ret.Add(tmp);
            }

            if (i - 1 > 0 && k - 1 > 0)
            {
                tmp.n = ((i - 1) - 1) * W + ((k - 1) - 1);
                tmp.len = (int)(255 - Math.Abs(m[i - 1, k] - m[i, k - 1]));
                ret.Add(tmp);
            }

            if (i + 1 < H + 1 && InRange(k, W))
            {
                tmp.n = ((i + 1) - 1) * W + ((k) - 1);
                tmp.len = (int)(255 - (1.0 / 2) * Math.Abs(m[i, k + 1] + m[i + 1, k + 1] - m[i, k - 1] - m[i + 1, k - 1]));
                ret.Add(tmp);
            }

            if (i - 1 > 0 && InRange(k, W))
            {
                tmp.n = ((i - 1) - 1) * W + ((k) - 1);
                tmp.len = (int)(255 - (1.0 / 2) * Math.Abs(m[i, k + 1] + m[i - 1, k + 1] - m[i, k - 1] - m[i - 1, k - 1]));
                ret.Add(tmp);
            }

            if (k + 1 < W + 1 && InRange(i, H))
            {
                tmp.n = ((i) - 1) * W + ((k + 1) - 1);
                tmp.len = (int)(255 - (1.0 / 2) * Math.Abs(m[i - 1, k] + m[i - 1, k + 1] - m[i + 1, k] - m[i + 1, k + 1]));
                ret.Add(tmp);
            }

            if (k - 1 > 0 && InRange(i, H))
            {
                tmp.n = ((i) - 1) * W + ((k - 1) - 1);
                tmp.len = (int)(255 - (1.0 / 2) * Math.Abs(m[i - 1, k] + m[i - 1, k - 1] - m[i + 1, k] - m[i + 1, k - 1]));
                ret.Add(tmp);
            }

            //if point has no neighbours
            if (ret.Count == 0)
                throw new Exception("Smth went very wrong");

            return ret;
        }

        //Represents edge of graph
        public struct ver
        {
            public int n; //Edge goes to dot with n number 
            public int len; // Weight of the edge
        }

        //Constructor makes graph of image
        public ImGraph(byte[] res, int H, int W)
        {
            m = ByteMeths.LinArrToMatrix(ByteMeths.BToInt(res), H, W);
            g = new ver[H * W][]; // Creates graph

            for (int i = 0; i < g.Length; i++)
            {

                List<ver> curcon = LMaker(m, i / W + 1, i % W + 1, H, W); // Getting neigbours

                //Pushes them into graph
                g[i] = new ver[curcon.Count];
                for (int k = 0; k < g[i].Length; ++k)
                    g[i][k] = curcon[k];
            }
        }

        //Djkstra all paths search algoritm for our graph
        public void Dj(object obj)
        {
            //Unpack H, W and start coods;
            List<int> inpvalues = (List<int>)obj;
            int x = inpvalues[0];
            int y = inpvalues[1];
            int H = inpvalues[2];
            int W = inpvalues[3];

            //Initializes arrays with our graph length
            d = Enumerable.Repeat(int.MaxValue, g.Length).ToArray();
            int[] p = new int[g.Length];
            bool[] ch = new bool[g.Length];

            //Sets start point
            d[y * W + x] = 0;
            p[y * W + x] = -1;

            //Creates list of unprocessed and reached nodes(queue)
            List<int> q = new List<int>();
            q.Add(y * W + x); //Adds start node
            
            //Main Dj alg with queue
            for (int i = 0; i < g.Length; ++i)
            {
                load = (i + 1) * 100 / g.Length; //Sets loading
                int v = -1;
                for (int k = 0; k < q.Count; ++k)
                    if (!ch[q[k]] && (v == -1 || d[q[k]] < d[v]))
                        v = q[k];

                if (v == -1 || d[v] == int.MaxValue)
                    break;

                ch[v] = true;

                for (int k = 0; k < g[v].Length; ++k)
                {
                    int curv = g[v][k].n;
                    if (d[v] + g[v][k].len < d[curv])
                    {
                        d[curv] = d[v] + g[v][k].len;
                        if (q.IndexOf(curv) == -1)
                            q.Add(curv);
                        p[curv] = v;
                    }
                    q.Remove(v);
                }
            }
            Djres = p;
        }

        //Gets path to node from start node
        public List<int> GetPath(int n)
        {
            List<int> ret = new List<int>();
            int times = 0;
            int cur = n;
            while (Djres[cur] != -1)
            {
                ret.Add(cur);
                cur = Djres[cur];
                ++times;
            }
            ret.Add(cur);
            return ret;
        }
    }
}
