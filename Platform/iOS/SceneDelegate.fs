namespace FabulousAvaloniaBlank.iOS

open System
open Foundation
open Fabulous.Avalonia
open FabulousAvaloniaBlank

[<Register(nameof SceneDelegate)>]
type SceneDelegate() =
    inherit FabSceneDelegate()

    override this.CreateApp() =
        let app = Program.startApplication App.program
        app.Styles.Add(App.theme)
        app
