using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestShader
{
    /// <summary>
    /// Tool for selecting tiles automatically
    /// The method follows the wangid idea from Tiled.
    /// TODO: Implement wang id importer from tmx
    /// </summary>
    class WangTool
    {

        /// <summary>
        /// The legend for reading the wang id
        /// </summary>
        public enum WangID {
            Top = 0,
            TopRight = 1,
            Right = 2,
            BottomRight = 3,
            Bottom = 4,
            BottomLeft = 5,
            Left = 6,
            TopLeft = 7
        }

        public static readonly Dictionary<WangID, int> WangIDy = new Dictionary<WangID, int>()
        {
            {WangID.Top, -1},
            {WangID.TopRight, -1},
            {WangID.Right, 0},
            {WangID.BottomRight, 1},
            {WangID.Bottom, 1},
            {WangID.BottomLeft, 1},
            {WangID.Left, 0},
            {WangID.TopLeft, -1}
        };

        public static readonly Dictionary<WangID, int> WangIDx = new Dictionary<WangID, int>()
        {
            {WangID.Top, 0},
            {WangID.TopRight, 1},
            {WangID.Right, 1},
            {WangID.BottomRight, 1},
            {WangID.Bottom, 0},
            {WangID.BottomLeft, -1},
            {WangID.Left, -1},
            {WangID.TopLeft, -1}
        };

        /// <summary>
        /// Creates the wang id of the neccesery tile at a position from the map of solid/non-solid tiles
        /// </summary>
        /// <param name="map"> The map of tiles, true = solid, false = non-solid</param>
        /// <returns> The wang id for each tile</returns>
        static public List<List<List<bool>>> wangIDMap(List<List<bool>> map)
        {
            //iterate through the map
            List<List<List<bool>>> wangIDMap = new List<List<List<bool>>>();
            for (int i = 0; i < map.Count; i++)
            {
                wangIDMap.Add(new List<List<bool>>());

                for (int j = 0; j < map[i].Count; j++)
                {
                    //create the wang id list
                    wangIDMap[i].Add(new List<bool>() { false, false, false, false, false, false, false, false });

                    //Fill the wang id list if the tile is solid
                    if (map[i][j])
                    {

                        //Iterate through the 8 directions
                        for (int k = 0; k < 8; k++)
                        {
                            //check if the tile is in the map
                            if (i + WangIDx[(WangID)k] < 0 ||
                                i + WangIDx[(WangID)k] >= map.Count ||
                                j + WangIDy[(WangID)k] < 0 ||
                                j + WangIDy[(WangID)k] >= map[i + WangIDx[(WangID)k]].Count)
                                continue;

                            //check if the tile is solid and add to the wang id
                            if (map[i + WangIDx[(WangID)k]][j + WangIDy[(WangID)k]])
                            {
                                wangIDMap[i][j][k] = true;
                            }
                        }
                    }
                }
            }
            return wangIDMap;
        }

        static public List<List<int>> TranslateWangIDmap(List<List<List<bool>>> wangIDMap,Dictionary<List<bool>, int> dictionary, int defaultValue = -1)
        {
            List<List<int>> map= new List<List<int>>();

            //Iterate through the map
            for(int i = 0; i < wangIDMap.Count; i++)
            {
                map.Add(new List<int>());
                for(int j = 0; j < wangIDMap[i].Count; j++)
                {
                    map[i].Add(1+dictionary.GetValueOrDefault(wangIDMap[i][j], defaultValue));

//                    map[i].Add(dictionary[wangIDMap[i][j]]);
                }
            }

            return map;
        }


    }

    class ListComparer<T> : IEqualityComparer<List<T>>
    {
        public ListComparer() {
        }

        public bool Equals(List<T> x, List<T> y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(List<T> obj)
        {
            int hashcode = 0;
            foreach (T t in obj)
            {
                hashcode *= 2;
                hashcode += t.GetHashCode();
            }
            return hashcode;
        }
    }
}
