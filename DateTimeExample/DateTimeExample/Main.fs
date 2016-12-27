namespace selfHost

open WebSharper.Html.Server
open WebSharper
open WebSharper.Sitelets

type EndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /about">] About

module Templating =
    open System.Web

    type Page =
        {
            Title : string
            MenuBar : list<Element>
            Body : list<Element>
        }

    let MainTemplate =
        Content.Template<Page>("~/Main.html")
            .With("title", fun x -> x.Title)
            .With("menubar", fun x -> x.MenuBar)
            .With("body", fun x -> x.Body)

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint =
        let ( => ) txt act =
             LI [if endpoint = act then yield Attr.Class "active"] -< [
                A [Attr.HRef (ctx.Link act)] -< [Text txt]
             ]
        [
            LI ["Home" => EndPoint.Home]
            LI ["About" => EndPoint.About]
        ]

    let Main ctx endpoint title body : Async<Content<EndPoint>> =
        Content.WithTemplate MainTemplate
            {
                Title = title
                MenuBar = MenuBar ctx endpoint
                Body = body
            }

module Site =

    let HomePage ctx =
        let now = System.DateTime.Now
        let nowUTC = System.DateTime.UtcNow

        let epochLocal = System.DateTime(1970,1,1,0,0,0,System.DateTimeKind.Local)
        let epoch      = System.DateTime(1970,1,1,0,0,0,System.DateTimeKind.Utc)

        let formatDate(i : System.DateTime) = i.ToShortDateString() + "." + i.ToShortTimeString()

        let getKind (i : System.DateTime) =
          if i.Kind = System.DateTimeKind.Utc then "UTC"
          elif i.Kind = System.DateTimeKind.Local then "Local"
          else "Unspecified"

        Templating.Main ctx EndPoint.Home "Home" [
            H1 [Text "Time Tests!"]
            Div [
              H2 [Text "Server Side"]
              Table[
                TR[TD [Text <| (getKind now) + " Time"]; TD [Text (formatDate now)]]
                TR[TD [Text <| (getKind nowUTC) + " Time"]; TD [Text (formatDate nowUTC)]]
                TR[TD [Text <| (getKind epochLocal) + " Epoch"]; TD [Text (formatDate epochLocal)]]
                TR[TD [Text <| (getKind epoch) + " Epoch"]; TD [Text (formatDate epoch)]]
              ]
            ]
            Div [ClientSide <@ Client.Main(now,nowUTC,epochLocal,epoch) @>]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            H1 [Text "About"]
            P [Text "This is a template self-hosted WebSharper client-server application."]
        ]

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx action ->
            match action with
            | Home -> HomePage ctx
            | About -> AboutPage ctx
        )


module SelfHostedServer =

    open global.Owin
    open Microsoft.Owin.Hosting
    open Microsoft.Owin.StaticFiles
    open Microsoft.Owin.FileSystems
    open WebSharper.Owin

    [<EntryPoint>]
    let Main args =
        let rootDirectory, url =
            match args with
            | [| rootDirectory; url |] -> rootDirectory, url
            | [| url |] -> "..", url
            | [| |] -> "..", "http://localhost:9000/"
            | _ -> eprintfn "Usage: selfHost ROOT_DIRECTORY URL"; exit 1
        use server = WebApp.Start(url, fun appB ->
            appB.UseStaticFiles(
                    StaticFileOptions(
                        FileSystem = PhysicalFileSystem(rootDirectory)))
                .UseSitelet(rootDirectory, Site.Main)
            |> ignore)
        stdout.WriteLine("Serving {0}", url)
        stdin.ReadLine() |> ignore
        0
