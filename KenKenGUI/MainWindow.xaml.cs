using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using KenKenLogic;
using System.ComponentModel;
using KenKenModel;
using System.Threading;

namespace KenKenGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            solvedCells = GenerateSolvedCells();
            SetBindings();
        }

        private ColorPalate myPalate = new ColorPalate();

        private SolvedCell[,] solvedCells;

        private SolvedCell[,] GenerateSolvedCells()
        {
            var cells = new SolvedCell[6, 6];
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    cells[i, j] = new SolvedCell();
                }
            }

            return cells;
        }

        private void SetBindings()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    var cell = solvedCells[i, j];
                    var square = GetKKSquare(i, j);
                    var b = new Binding();
                    b.Path = new PropertyPath("Value");
                    b.Source = cell;
                    square.SetBinding(KenKenSquare.CentreValueProperty, b);
                }
            }
        }

        private IEnumerable<KenKenSquare> KKSquares()
        {
            return kenKenGrid.Children.Cast<UIElement>().OfType<KenKenSquare>();
        }

        private KenKenSquare GetKKSquare(int row, int col)
        {
            return KKSquares().Where(square => Grid.GetRow(square) == row && Grid.GetColumn(square) == col).Single();
        }

        private IEnumerable<KenKenSquare> DefinedKKSquares()
        {
            return KKSquares().Where(kkSquare => kkSquare.IsDefined);
        }

        private IEnumerable<KenKenSquare> KKNeighbours(KenKenSquare square)
        {
            int row = Grid.GetRow(square);
            int col = Grid.GetColumn(square);

            return KKSquares().Where(kkSquare => {
                int r = Grid.GetRow(kkSquare);
                int c = Grid.GetColumn(kkSquare);

                int rowDiff = Math.Abs(r - row);
                int colDiff = Math.Abs(c - col);

                return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
            });
        }

        private KenKenSquare TopLeftKKSquare(IEnumerable<KenKenSquare> squareGroup)
        {
            int topMostRow = squareGroup.Min(square => Grid.GetRow(square));
            int leftMostCol = squareGroup.Where(square => Grid.GetRow(square) == topMostRow).Min(square => Grid.GetColumn(square));
            return squareGroup.Where(square => Grid.GetRow(square) == topMostRow && Grid.GetColumn(square) == leftMostCol).Single();
        }

        private List<CellCollection> cellColletionList = new List<CellCollection>();

        private List<KenKenSquare> EvolvingKKShape = new List<KenKenSquare>();


        private void UpdateEvolvingKKShape()
        {
            var surroundingSquares = EvolvingKKShape.ConvertAll(square => KKNeighbours(square)).Aggregate((allNeighbours, neighbours) => allNeighbours.Union(neighbours)).Except(EvolvingKKShape);
            foreach (KenKenSquare square in surroundingSquares.Except(DefinedKKSquares()))
            {
                square.IsEnabled = true;
                //square.ColorIn = square.ColorOut = Brushes.Yellow.Color;
            }
            var notInPlay = KKSquares().Except(surroundingSquares).Except(EvolvingKKShape).Except(DefinedKKSquares());
            foreach (KenKenSquare square in notInPlay)
            {
                square.IsEnabled = false;
            }
        }

        private void btnDefineRegion_Click(object sender, RoutedEventArgs e)
        {
            if (EvolvingKKShape.Count == 1)
            {
                SingleSquare singleSquare = new SingleSquare();
                if ((bool)singleSquare.ShowDialog())
                {
                    KenKenSquare onlySquare = EvolvingKKShape.Single();
                    onlySquare.TopLeftValue = singleSquare.Target.ToString();
                    onlySquare.ColorIn = myPalate.NextColor();
                    cellColletionList.Add(new CellCollection { KenKenSquares = EvolvingKKShape, TargetSum = singleSquare.Target });
                    EvolvingKKShape = new List<KenKenSquare>();
                }
            }
            else
            {
                NewRegion nr = new NewRegion(EvolvingKKShape.Count);
                if ((bool)nr.ShowDialog())
                {
                    KenKenSquare topLeft = TopLeftKKSquare(EvolvingKKShape);
                    topLeft.TopLeftValue = String.Format("{0} {1}", nr.Target, nr.Operation.StringFormat());
                    Color currentColor = myPalate.NextColor();
                    foreach (KenKenSquare square in EvolvingKKShape)
                    {
                        square.ColorIn = currentColor;
                    }
                    cellColletionList.Add(new CellCollection { KenKenSquares = EvolvingKKShape, MathematicalOp = nr.Operation, TargetSum = nr.Target });
                    EvolvingKKShape = new List<KenKenSquare>();
                }
            }

            foreach (KenKenSquare square in KKSquares().Except(DefinedKKSquares()))
            {
                square.IsEnabled = true;
                square.Reset();
            }

            if (DefinedKKSquares().Count() == 36)
            {
                this.btnSolve.IsEnabled = true;
            }
        }

        private async void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            //Disable the other buttons
            btnDefineRegion.IsEnabled = btnReset.IsEnabled = false;
            var arg = cellColletionList.ConvertAll(cellCollection => cellCollection.ToCellGroup()).ToArray();

            Animate();
            await DoWork(arg);
            cancellationToken.Cancel();
            btnDefineRegion.IsEnabled = btnReset.IsEnabled = true;
        }

        private Task DoWork(CellGroup[] cellGroup)
        {
            return Task.Run(() =>
           {
               GenericFns.publicMethod(cellGroup, solvedCells);
           });
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            EvolvingKKShape = new List<KenKenSquare>();
            cellColletionList = new List<CellCollection>();
            foreach (KenKenSquare square in KKSquares())
            {
                square.Reset();
            }
            solvedCells = GenerateSolvedCells();
            SetBindings();
            btnSolve.IsEnabled = false;
        }

        private CancellationTokenSource cancellationToken;

        private void AnimateCellGroup(Random random)
        {
            var kksList = cellColletionList[random.Next(cellColletionList.Count)];
            foreach (KenKenSquare square in kksList.KenKenSquares)
            {
                int parity = random.Next() % 2 == 0 ? 1 : -1;
                square.RotateSquare(parity * random.Next(360), random.Next((int)this.ActualWidth / 6), random.Next((int)this.ActualHeight / 6));
            }
        }

        private void Animate()
        {
            cancellationToken = new CancellationTokenSource();
            var random = new Random();
            try
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        this.Dispatcher.Invoke(() => AnimateCellGroup(random));
                        Thread.Sleep(200);
                        cancellationToken.Token.ThrowIfCancellationRequested();
                    }
                },
                cancellationToken.Token);
            }
            catch (Exception ex) { }
        }

        private void KenKenSquare_Click(object sender, RoutedEventArgs e)
        {
            KenKenSquare square = (KenKenSquare)sender;
            if (!square.IsDefined)
            {
                square.IsDefined = true;
                EvolvingKKShape.Add(square);
                UpdateEvolvingKKShape();
            }
        }

        private void KenKenSquare_MouseEnter(object sender, MouseEventArgs e)
        {
            KenKenSquare_MouseEnterLeave((KenKenSquare)sender, true);
        }

        private void KenKenSquare_MouseLeave(object sender, MouseEventArgs e)
        {
            KenKenSquare_MouseEnterLeave((KenKenSquare)sender, false);
        }

        private void KenKenSquare_MouseEnterLeave(KenKenSquare aSquare, bool IsEntering)
        {
            if (aSquare.IsDefined)
            {
                var kksList = cellColletionList.Find(cellCollection => cellCollection.KenKenSquares.Contains(aSquare));
                if (kksList != null)
                {
                    foreach (KenKenSquare square in kksList.KenKenSquares)
                    {
                        if (IsEntering)
                        {
                            square.button_MouseEnter(square, null);
                            
                        }
                        else
                        {
                            square.button_MouseLeave(square, null);
                        }
                        
                    }
                }
            }
        }
    }
}
