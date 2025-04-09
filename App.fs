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
        { value: int
          label: string
          state: CellState }

    type Grid = Cell option list list
    let Dimensions = {| rows = 6; columns = 3 |}
    let ValueRange = [ 1..9 ]

    let init () : Grid =
        let random = Random()
        let shuffledValues = ValueRange |> List.sortBy (fun _ -> random.Next())

        let totalCells = Dimensions.rows * Dimensions.columns

        let filledCells: List<Cell option> =
            shuffledValues
            |> List.map (fun (value) ->
                { value = value
                  label = sprintf "%d" value
                  state = Visible })
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


    let transformGridToObscured (grid: Grid) =
        grid
        |> List.map (fun row ->
            row
            |> List.map (fun cell ->
                Option.map
                    (fun cell ->
                        if cell.state = Visible then
                            { cell with state = Obscured }
                        else
                            cell)
                    cell))

    let advanceState model =
        match model.State with
        | Start ->
            { model with
                State = Learning
                grid = Grid.init () },
            Cmd.none
        | Learning ->
            { model with
                State = Testing
                grid = transformGridToObscured model.grid },
            Cmd.none
        | Testing -> { model with State = End }, Cmd.none
        | End -> { model with State = Start }, Cmd.none

    let updateGridCell row col grid : Grid =
        grid
        |> List.mapi (fun rowIndex gridRow ->
            if rowIndex = row then
                gridRow
                |> List.mapi (fun colIndex cellOption ->
                    match cellOption with
                    | Some c when colIndex = col -> Some { c with state = Hidden }
                    | c -> c)
            else
                gridRow
                |> List.map (fun cell ->
                    Option.map
                        (fun cell ->
                            if cell.state = Hidden then
                                cell
                            else
                                { cell with state = Obscured })
                        cell))

    let pressCell row col model =
        match model.grid[row][col] with
        | Some cell ->
            let newGrid = updateGridCell row col model.grid

            { model with
                grid = newGrid
                State = Testing
                message = Some $"Pressed {cell.value}" },
            Cmd.none
        | None -> { model with message = None }, Cmd.none

    let update msg model =
        match msg with
        | SetState state -> { model with State = state }, Cmd.none
        | AdvanceState -> advanceState model
        | PressCell(row, col) -> pressCell row col model


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
