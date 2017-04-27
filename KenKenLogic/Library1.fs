namespace KenKenLogic
open System

type MathOp = Addition | Subtraction | Multiplication | Division
type CellGroup() =
    member val Coordinates = [|(0,0)|] with get, set
    member val Target = 0 with get, set
    member val MathematicalOperation = MathOp.Addition with get, set

type GridPuzzle = Map<int,int> 
type GridPuzzles = Set<GridPuzzle>
type Coordinate = int*int
type Coordinates = Set<Coordinate>
type CellCollection = { Coords: Coordinates; GridPuzzleSet: GridPuzzles}

module private SetHelpers = 

    //using bit pattern to generate subsets
    let max_bits x = 
        let rec loop acc = if (1 <<< acc ) > x then acc else loop (acc + 1)
        loop 0
        
    let bit_setAt i x = ((1 <<< i) &&& x) <> 0
    let subsets s = 
            let a = List.toArray s in
            let len = (Array.length a)
            let as_set x = [for i in 0 .. (max_bits x) do 
                                    if (bit_setAt i x) && (i < len) then  yield a.[i]]
        
            seq{for i in 0 .. (1 <<< len)-1 -> as_set i}

    let CompleteGraph op setA setB =
            setA
            |> Set.fold(fun sumSet a -> Set.union sumSet (setB
                                                          |> Set.fold(fun sumSubSet b -> Set.add (op a b) sumSubSet) Set.empty)) Set.empty

    let CompleteBipartite op setA setB = 
        setA
        |> Set.toArray
        |> Array.Parallel.map(fun a -> setB |> Set.toArray |> Array.Parallel.map(fun b -> op a b) |> Set.ofArray)
        |> Seq.ofArray
        |> Set.unionMany

module GenericFns  =

    open System;
    open SetHelpers;
    open KenKenModel

    let private setOp (compFn: int -> int -> int) (gridPuz1: GridPuzzle) (gridPuz2: GridPuzzle) =
        let seqForm1 = Map.toSeq gridPuz1
        let seqForm2 = Map.toSeq gridPuz2  
        Seq.map2(fun (x, noOfx) (y, noOfy) -> (x, compFn noOfx noOfy)) seqForm1 seqForm2
        |> GridPuzzle

    let private max (x: int) (y: int) = if x > y then x else y
    let private min (x: int) (y: int) = if x < y then x else y
    let private exDiff (x: int) (y: int) = if x > y then x - y else 0

    let private intersection = setOp min
    let private union = setOp max
    let private difference = setOp exDiff

    let private gridPuzzToIntList (gp : GridPuzzle) = 
        Map.fold(fun accList key count-> accList@(List.init count (fun _-> key))) [] gp

    let rec private intListToGridPuzz (gridPuzz: GridPuzzle) (intList: list<int>)= 
        match intList with
        | [] -> gridPuzz
        | head::tail -> let gridPuzz' = gridPuzz.Add (head, (gridPuzz.[head] + 1))
                        intListToGridPuzz gridPuzz' tail

    let private emptyKenKen = GridPuzzle[(1,0); (2,0); (3,0); (4,0); (5,0); (6,0)]

    let private intListToKKGridPuzz = intListToGridPuzz emptyKenKen

    let private getSubsets (emptyGridPuzzle: GridPuzzle) (gp: GridPuzzle) = 
        gp
        |> gridPuzzToIntList
        |> subsets
        |> Seq.map(fun aList -> intListToKKGridPuzz aList)

    let private getKKSubsets = getSubsets emptyKenKen

    let private crossProduct op A B = 
        seq{for a in A do
                for b in B do
                    yield op a b}

    let private gridPuzzleLength (gp : GridPuzzle) = 
        gp
        |> Map.toSeq
        |> Seq.map(fun (key, v) -> v)
        |> Seq.reduce(fun v0 v1 -> v0 + v1)

    let private intersectSet length setA setB = 
        crossProduct intersection setA setB
        |> Seq.filter(fun gp -> gridPuzzleLength gp >= length)
        |> Seq.fold(fun (interSet : GridPuzzles) (gp: GridPuzzle) -> match gridPuzzleLength gp with
                                                                     | len when len = length -> Set.add gp interSet
                                                                     | len when len > length -> gp
                                                                                                |> getKKSubsets
                                                                                                |> Seq.filter(fun gpss -> gridPuzzleLength gpss = length)
                                                                                                |> Set.ofSeq
                                                                                                |> Set.union interSet
                                                                     | _ -> interSet) Set.empty

    let private differenceSet length setA setB =  
        crossProduct difference setA setB
        |> Seq.fold(fun (diffSet : GridPuzzles) (gp: GridPuzzle) -> match gridPuzzleLength gp with
                                                                    | len when len = length -> Set.add gp diffSet
                                                                    | _ -> diffSet) Set.empty 

    let private isDerivable length setA setB = 
        let seqSeqConstrainedGridPuzzles =  setA
                                            |> Seq.map(fun gp -> getKKSubsets gp
                                                                 |> Seq.filter(fun gp0 -> gridPuzzleLength gp0 = length))
        setB = Set.ofSeq (seq{for x in seqSeqConstrainedGridPuzzles do yield! x})

    let private coordsPartition coordSeta coordSetb = 
        (Set.intersect coordSeta coordSetb, Set.difference coordSeta coordSetb, Set.difference coordSetb coordSeta)

    let private extractSubcollections (a: CellCollection) (b: CellCollection) =
        let mutable ccIntersect = None  
        let mutable ccaNotb = None
        let mutable ccbNota = None
        let coordIntersect, coordaNotb, coordbNota = coordsPartition a.Coords b.Coords
        if coordIntersect.Count > 0 then
            let setIntersect = intersectSet coordIntersect.Count a.GridPuzzleSet b.GridPuzzleSet
            ccIntersect <- Some {Coords = coordIntersect; GridPuzzleSet = setIntersect}
            if coordaNotb.Count > 0 then
                let setaNotb = differenceSet coordaNotb.Count a.GridPuzzleSet setIntersect
                ccaNotb <- Some {Coords = coordaNotb; GridPuzzleSet = setaNotb}
            if coordbNota.Count > 0 then
                let setbNota = differenceSet coordbNota.Count b.GridPuzzleSet setIntersect
                ccbNota <- Some {Coords = coordbNota; GridPuzzleSet = setbNota}
        (ccIntersect, ccaNotb, ccbNota)


    let private examinePairwise (a: CellCollection) (b: CellCollection) =
        let mutable setToAdd = Set.empty
        let mutable setToRemove = Set.empty
        let (ccIntersect, ccaNotb, ccbNota) = extractSubcollections a b
        match ccIntersect with
        | None -> ()
        | Some x -> if x.GridPuzzleSet.Count = 1 then
                        setToAdd <- setToAdd |> Set.add x
                        setToRemove <- setToRemove |> Set.add a |> Set.add b
                        match ccaNotb with
                        | None -> ()
                        | Some x -> setToAdd <- setToAdd |> Set.add x                        
                        match ccbNota with
                        | None -> ()
                        | Some x -> setToAdd <- setToAdd |> Set.add x
                    else
                        let isIntersectDerivableFroma = isDerivable x.Coords.Count a.GridPuzzleSet x.GridPuzzleSet
                        let isIntersectDeriavbleFromb = isDerivable x.Coords.Count b.GridPuzzleSet x.GridPuzzleSet  
                        if not(isIntersectDerivableFroma) && not(isIntersectDeriavbleFromb) then
                            setToAdd <- setToAdd |> Set.add x
                        if isIntersectDerivableFroma && isIntersectDeriavbleFromb then
                            if x = a then setToRemove <- setToRemove |> Set.add a
                            elif x = b then setToRemove <- setToRemove |> Set.add b
                        match ccaNotb with
                        | None -> ()
                        | Some y -> if y.GridPuzzleSet.Count = 1 then
                                        setToAdd <- setToAdd |> Set.add y
                                        setToRemove <- setToRemove |> Set.add a
                                    else
                                        if not(isDerivable y.Coords.Count a.GridPuzzleSet y.GridPuzzleSet) then
                                            setToAdd <- setToAdd |> Set.add y
                        match ccbNota with
                        | None -> ()
                        | Some z -> if z.GridPuzzleSet.Count = 1 then
                                        setToAdd <- setToAdd |> Set.add z
                                        setToRemove <- setToRemove |> Set.add b
                                    else
                                        if not(isDerivable z.Coords.Count b.GridPuzzleSet z.GridPuzzleSet) then
                                            setToAdd <- setToAdd |> Set.add z
        (Set.difference setToRemove setToAdd, Set.difference setToAdd setToRemove)

    let private processCCSet ccElement ccSet =        
        let (setToRemove, setToAdd) = ccSet
                                      |> Set.filter(fun x -> let theIntersect = Set.intersect x.Coords ccElement.Coords
                                                             not(theIntersect.IsEmpty))
                                      |> Set.fold(fun (r,a) x -> if Set.contains ccElement r then
                                                                    (r, a)
                                                                 else
                                                                     let (remove, add) = examinePairwise x ccElement
                                                                     (Set.union r remove, Set.union a add)) (Set.empty, Set.empty)
        (setToRemove, setToAdd)

    let private getStartingElement aSet = 
        aSet
        |> Set.toSeq
        |> Seq.minBy(fun x -> x.GridPuzzleSet.Count*x.Coords.Count)


    let private SixRowCol = set [
                                   {
                                     Coords = set[(0,0); (0,1); (0,2); (0,3); (0,4); (0,5)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6])]
                                   };
                                   {
                                     Coords = set[(1,0); (1,1); (1,2); (1,3); (1,4); (1,5)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   };
                                   {
                                     Coords = set[(2,0); (2,1); (2,2); (2,3); (2,4); (2,5)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   };
                                   {
                                     Coords = set[(3,0); (3,1); (3,2); (3,3); (3,4); (3,5)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   };
                                   {
                                     Coords = set[(4,0); (4,1); (4,2); (4,3); (4,4); (4,5)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   };
                                   {
                                     Coords = set[(5,0); (5,1); (5,2); (5,3); (5,4); (5,5)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   };
                                   {
                                     Coords = set[(0,0); (1,0); (2,0); (3,0); (4,0); (5,0)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   };
                                   {
                                     Coords = set[(0,1); (1,1); (2,1); (3,1); (4,1); (5,1)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   };
                                   {
                                     Coords = set[(0,2); (1,2); (2,2); (3,2); (4,2); (5,2)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   };
                                   {
                                     Coords = set[(0,3); (1,3); (2,3); (3,3); (4,3); (5,3)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   };
                                   {
                                     Coords = set[(0,4); (1,4); (2,4); (3,4); (4,4); (5,4)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   };
                                   {
                                     Coords = set[(0,5); (1,5); (2,5); (3,5); (4,5); (5,5)];
                                     GridPuzzleSet = GridPuzzles[(intListToKKGridPuzz [1; 2; 3; 4; 5; 6] )]
                                   }
                                ]

    let rec private solveIncrementally aSet startElement prevStarts currentIncompleteAnswer reportProgress=  
        let r, a = processCCSet startElement (Set.remove startElement aSet)
        let aSet' = Set.difference (Set.union aSet a) r
        let nextStartElementPossibilities = Set.difference aSet' prevStarts
        let startElement' = if nextStartElementPossibilities.IsEmpty then getStartingElement aSet' else getStartingElement nextStartElementPossibilities
        let prevStarts' = if nextStartElementPossibilities.IsEmpty then Set.singleton startElement' else prevStarts.Add startElement'
        let potentialAnswer =  aSet' 
                               |> Set.filter(fun x -> x.Coords.Count = 1 && x.GridPuzzleSet.Count = 1)
        let newInfo = Set.difference potentialAnswer currentIncompleteAnswer
        if not newInfo.IsEmpty then reportProgress newInfo
        if potentialAnswer.Count < 36 then solveIncrementally aSet' startElement' prevStarts' potentialAnswer reportProgress
   
    let private matchAddMult noOfItems targetSum Op =
        [1.. (noOfItems+1)/2] |> List.fold(fun state _ -> state@[1..6]) []
        |> subsets 
        |> Seq.filter(fun aList -> aList |> List.length = noOfItems)
        |> Seq.filter(fun aList -> aList |> List.reduce(fun x y -> Op x y) = targetSum)
        |> Seq.distinctBy(fun aList -> aList |> List.sort)
        |> Seq.map(fun aList -> intListToKKGridPuzz aList )

    let private matchDev targetSum  =
        match targetSum with
        | 2 -> seq[(intListToKKGridPuzz [1;2] ); (intListToKKGridPuzz [2;4] ); (intListToKKGridPuzz [3;6] )]
        | 3 -> seq[(intListToKKGridPuzz [1;3] ); (intListToKKGridPuzz [2;6] )]
        | 4 -> seq[(intListToKKGridPuzz [1;4] )]
        | 5 -> seq[(intListToKKGridPuzz [1;5] )]
        | 6 -> seq[(intListToKKGridPuzz [1;6] )]
        | _ -> failwith (Printf.sprintf "targetSum %d is invalid" targetSum)

    let private matchMin targetSum  =
        match targetSum with
        | 1 -> seq[(intListToKKGridPuzz [1;2] ); (intListToKKGridPuzz [2;3] ); (intListToKKGridPuzz [3;4] ); (intListToKKGridPuzz [4;5] ); (intListToKKGridPuzz [5;6] )]
        | 2 -> seq[(intListToKKGridPuzz [1;3] ); (intListToKKGridPuzz [2;4] ); (intListToKKGridPuzz [3;5] ); (intListToKKGridPuzz [4;6] )]
        | 3 -> seq[(intListToKKGridPuzz [1;4] ); (intListToKKGridPuzz [2;5] ); (intListToKKGridPuzz [3;6] )]
        | 4 -> seq[(intListToKKGridPuzz [1;5] ); (intListToKKGridPuzz [2;6] )]
        | 5 -> seq[(intListToKKGridPuzz [1;6] )]
        | _ -> failwith (Printf.sprintf "targetSum %d is invalid" targetSum)

    let private createKKGridPuzzleSet noOfCoords targetSum mathOp =
        match noOfCoords with
        | 0 -> failwith "Coordinates may not be empty"
        | 1 -> GridPuzzles[(intListToKKGridPuzz [targetSum] )]
        | 2 -> let gridPuzzleSeq =  match mathOp with
                                     | MathOp.Addition -> matchAddMult 2 targetSum (+)
                                     | MathOp.Multiplication -> matchAddMult 2 targetSum (*)
                                     | MathOp.Subtraction -> matchMin targetSum
                                     | MathOp.Division -> matchDev targetSum
               gridPuzzleSeq
               |> Set.ofSeq
        | n when n < 7 -> let gridPuxzzleSeq =  match mathOp with
                                                | MathOp.Addition -> matchAddMult n targetSum (+)
                                                | MathOp.Multiplication -> matchAddMult n targetSum (*)
                                                | _ -> failwith "This CellGroup is incompatible with this operator"
                          gridPuxzzleSeq
                          |> Set.ofSeq
        | _ -> failwith "CellGroup may not have more than 6 Coordinates"

    let private createKKCellCollection (cellGroup: CellGroup) =
        let gridPuzzleSet = createKKGridPuzzleSet cellGroup.Coordinates.Length cellGroup.Target cellGroup.MathematicalOperation
        { Coords = cellGroup.Coordinates |> Set.ofArray; GridPuzzleSet = gridPuzzleSet}

    let publicMethod cellGroupArray (solvedCells: SolvedCell[,]) = 
        let ccSet = cellGroupArray
                    |> Array.map(fun cc -> createKKCellCollection cc)
                    |> Set.ofArray
        let start = getStartingElement ccSet
        let largeSet = Set.union ccSet SixRowCol
        let reportProgress (aSet: Set<CellCollection>) =
            aSet
            |> Set.iter(fun x -> let (coord: Coordinate) = x.Coords.MaximumElement
                                 solvedCells.[fst coord, snd coord].Value <- x.GridPuzzleSet.MaximumElement
                                                                             |> gridPuzzToIntList
                                                                             |> List.head
                                                                             |> Nullable<int>)
        solveIncrementally largeSet start Set.empty Set.empty reportProgress
