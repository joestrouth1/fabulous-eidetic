namespace FabulousAvaloniaBlank

open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia
open System

open type Fabulous.Avalonia.View

module Grid =
    type CellState =
        | Visible
        | Obscured
        | Hidden

    type Cell =
        Option<
            {| value: int
               label: string
               state: CellState |}
         >

    type Grid = Cell list list
    let Dimensions = {| rows = 6; columns = 3 |}
    let ValueRange = [ 1..9 ]

    let init () : Grid =
        let random = Random()
        let shuffledValues = ValueRange |> List.sortBy (fun _ -> random.Next())

        let totalCells = Dimensions.rows * Dimensions.columns

        let filledCells: List<Cell> =
            shuffledValues
            |> List.map (fun (value) ->
                {| value = value
                   label = sprintf "%d" value
                   state = Visible |})
            |> List.map Some

        let emptyCells = List.init (totalCells - List.length shuffledValues) (fun _ -> None)
        let allCells = filledCells @ emptyCells |> List.sortBy (fun _ -> random.Next())

        allCells |> List.chunkBySize Dimensions.columns

module App =
    open Grid

    type GameState =
        | Start
        | Learning
        | Testing
        | End

    let GameStateDisplayNames: Map<GameState, string> =
        [ (Start, "Game Start")
          (Learning, "Learn")
          (Testing, "Do your best!")
          (End, "Game Over") ]
        |> Map.ofList



    type Model =
        { State: GameState
          grid: Grid
          message: string option }
    // { Count: int; Step: int; TimerOn: bool }

    type Msg =
        | SetState of GameState
        | AdvanceState
        | PressCell of int * int

    let initModel =
        { State = Start
          grid = Grid.init ()
          message = None }

    // let timerCmd () =
    //     async {
    //         do! Async.Sleep 200
    //         return TimedTick
    //     }
    //     |> Cmd.ofAsyncMsg

    let init () = initModel, Cmd.none

    let update msg model =
        match msg with
        | SetState state -> { model with State = state }, Cmd.none
        | AdvanceState ->
            match model.State with
            | Start ->
                { model with
                    State = Learning
                    grid = Grid.init () },
                Cmd.none
            | Learning ->
                { model with
                    State = Testing
                    grid =
                        model.grid
                        |> List.map (fun row ->
                            row
                            |> List.map (fun cell ->
                                match cell with
                                | Some cell when cell.state = Visible ->
                                    Some
                                        {| label = cell.label
                                           state = Obscured
                                           value = cell.value |}
                                | None -> None
                                | _ -> cell)) },
                Cmd.none
            | Testing -> { model with State = End }, Cmd.none
            | End -> { model with State = Start }, Cmd.none
        | PressCell(row, col) ->
            let result =
                match model.grid[row][col] with
                | Some cell ->
                    let newGrid =
                        model.grid
                        |> List.mapi (fun rowIndex gridRow ->
                            if rowIndex <> row then
                                gridRow
                                |> List.map (fun cell ->
                                    match cell with
                                    | Some cell when cell.state = Hidden -> Some cell
                                    | Some cell -> Some {| cell with state = Obscured |}
                                    | None -> None)
                            else
                                gridRow
                                |> List.mapi (fun colIndex cellOption ->
                                    match cellOption with
                                    | Some c when colIndex = col -> Some {| c with state = Hidden |}
                                    | Some c when c.state = Visible -> Some {| c with state = Obscured |}
                                    | c -> c))

                    { model with
                        grid = newGrid
                        State = Testing
                        message = Some $"Pressed {cell.value}" }
                | None -> { model with message = None }

            result, Cmd.none

    let view model =
        SplitView(
            (Rectangle()).fill(Avalonia.Media.Colors.PaleVioletRed).width(24.0).height (24.0),
            (VStack 16.0 {
                TextBlock($"%s{GameStateDisplayNames[model.State]}").centerText ()
                Button("Advance", AdvanceState).centerHorizontal ()

                match model.State with
                | Learning
                | Testing ->
                    let coldefs = List.init Grid.Dimensions.columns (fun _ -> Dimension.Star)
                    let rowdefs = List.init Grid.Dimensions.rows (fun _ -> Dimension.Star)

                    match model.message with
                    | Some(msg) -> TextBlock(msg)
                    | _ -> ()

                    (Grid(coldefs, rowdefs) {
                        for row in 0 .. Grid.Dimensions.rows - 1 do
                            for col in 0 .. Grid.Dimensions.columns - 1 do
                                let cell = model.grid[row][col]

                                match cell with
                                | Some cell when cell.state = Visible ->
                                    Button(cell.label, PressCell(row, col))
                                        .gridRow(row)
                                        .gridColumn(col)
                                        .padding(24.0, 32.0)
                                        .horizontalContentAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                                        .horizontalAlignment(Avalonia.Layout.HorizontalAlignment.Stretch)
                                        .verticalAlignment(Avalonia.Layout.VerticalAlignment.Stretch)
                                        .fontSize (20.0)
                                | Some cell when cell.state = Obscured ->
                                    Button("?", PressCell(row, col))
                                        .gridRow(row)
                                        .gridColumn(col)
                                        .padding(24.0, 32.0)
                                        .horizontalContentAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                                        .horizontalAlignment(Avalonia.Layout.HorizontalAlignment.Stretch)
                                        .verticalAlignment(Avalonia.Layout.VerticalAlignment.Stretch)
                                        .fontSize (20.0)
                                | _ -> Rectangle().gridRow(row).gridColumn (col)
                    })
                        .horizontalAlignment(Avalonia.Layout.HorizontalAlignment.Stretch)
                        .verticalAlignment (Avalonia.Layout.VerticalAlignment.Stretch)
                | Start -> Button("You ready?", AdvanceState)
                | End -> Button("All Done!", AdvanceState)
            })
                .margin (48.0)
        )
            // .isPaneOpen(true)
            .displayMode (Avalonia.Controls.SplitViewDisplayMode.CompactInline)




#if MOBILE
    let app model = SingleViewApplication(view model)
#else
    let app model =
        DesktopApplication(
            Window(view model)
                .title("My app")
                .background("Transparent")
                .extendClientAreaToDecorationsHint(true)
                .transparencyLevelHint (
                    [ Avalonia.Controls.WindowTransparencyLevel.Mica
                      Avalonia.Controls.WindowTransparencyLevel.None ]
                )

        )
#endif


    let _theme = FluentTheme()
    _theme.DensityStyle <- DensityStyle.Compact
    let theme = _theme

    let program = Program.statefulWithCmd init update app
