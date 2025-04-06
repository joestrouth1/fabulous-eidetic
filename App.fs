namespace FabulousAvaloniaBlank

open Avalonia.Themes.Fluent
open Fabulous
open Fabulous.Avalonia

open type Fabulous.Avalonia.View

module App =
    type Model =
        { Count: int; Step: int; TimerOn: bool }

    type Msg =
        | Increment
        | Decrement
        | Reset
        | SetStep of float
        | TimerToggled of bool
        | TimedTick

    let initModel = { Count = 0; Step = 1; TimerOn = false }

    let timerCmd () =
        async {
            do! Async.Sleep 200
            return TimedTick
        }
        |> Cmd.ofAsyncMsg

    let init () = initModel, Cmd.none

    let update msg model =
        match msg with
        | Increment ->
            { model with
                Count = model.Count + model.Step },
            Cmd.none
        | Decrement ->
            { model with
                Count = model.Count - model.Step },
            Cmd.none
        | Reset -> initModel, Cmd.none
        | SetStep n -> { model with Step = int (n + 0.5) }, Cmd.none
        | TimerToggled on -> { model with TimerOn = on }, (if on then timerCmd () else Cmd.none)
        | TimedTick ->
            if model.TimerOn then
                { model with
                    Count = model.Count + model.Step },
                timerCmd ()
            else
                model, Cmd.none

    let view model =
        (VStack 16.0 {
            TextBlock($"%d{model.Count}").centerText ()

            Button("Increment", Increment).centerHorizontal ()

            Button("Decrement", Decrement).centerHorizontal ()

            (HStack 16.0 {
                TextBlock("Timer").centerVertical ()

                ToggleSwitch(model.TimerOn, TimerToggled)
            })
                .margin(20.)
                .centerHorizontal ()

            Slider(1., 10, float model.Step, SetStep)

            TextBlock($"Step size: %d{model.Step}").centerText ()

            Button("Reset", Reset).centerHorizontal ()

        })
            .center ()


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
