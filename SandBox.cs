using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Genkit;

using XRL;
using XRL.World;

namespace UD_Modding_Sandbox
{
    internal class SandBox
    {
        public static bool CellIsInLowerBoundX(Cell C)
            => C.X >= 5
            && C.X <= 30
            ;
        public static bool CellIsInUpperBoundX(Cell C)
            => C.X >= 35
            && C.X <= 70
            ;
        public static bool CellIsInXBounds(Cell C)
            => CellIsInLowerBoundX(C)
            || CellIsInLowerBoundX(C)
            ;

        public static bool CellIsInLowerBoundY(Cell C)
            => C.Y >= 5
            && C.Y <= 10
            ;
        public static bool CellIsInUpperBoundY(Cell C)
            => C.X >= 15
            && C.X <= 20
            ;
        public static bool CellIsInYBounds(Cell C)
            => CellIsInLowerBoundY(C)
            || CellIsInLowerBoundY(C)
            ;

        public static bool IsCellWithinBounds(Cell C)
            => CellIsInXBounds(C)
            && CellIsInYBounds(C)
            ;
        public static Point2D Point2DFromCell(Cell C)
            => C.Location.Point;

        public static Point2D GetPointAvoidMiddle(Zone Zone)
            => Zone.GetCells()
                .Where(IsCellWithinBounds)
                .Select(Point2DFromCell)
                .GetRandomElementCosmetic();
    }
}
