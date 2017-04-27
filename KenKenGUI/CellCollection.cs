using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using KenKenLogic;
using System.Windows.Media;

namespace KenKenGUI
{
    class CellCollection
    {
        public List<KenKenSquare> KenKenSquares = new List<KenKenSquare>();
        public MathOp MathematicalOp;
        public int TargetSum;
        private Tuple<int,int> ConvertKKSquareToCoord(KenKenSquare square)
        {
            int row = Grid.GetRow(square);
            int col = Grid.GetColumn(square);

            return Tuple.Create(row, col);
        }
        public CellGroup ToCellGroup()
        {
            return new CellGroup { Coordinates = KenKenSquares.ConvertAll(square => ConvertKKSquareToCoord(square)).ToArray(), MathematicalOperation = MathematicalOp, Target = TargetSum };
        }
    }
}
