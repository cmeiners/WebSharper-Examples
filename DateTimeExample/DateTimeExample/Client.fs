namespace selfHost

open WebSharper
open WebSharper.JavaScript
open WebSharper.Html.Client

[<JavaScript>]
module Client =

    open Server

    let formatDate(i : System.DateTime) = i.ToShortDateString() + "." + i.ToShortTimeString()

    let PlaceHolder = 
      Div[
        H2 [Text "Remoting"]
      ]

    let RemoteIt (now : System.DateTime,nowUTC : System.DateTime,epochLocal : System.DateTime,epoch : System.DateTime) =
      let p ={now=now;nowUTC=nowUTC;epochLocal=epochLocal;epoch=epoch}
      async {
          let! (k,i1,i2)= Server.DoSomething p
          let r = 
            Table[
              TR[TD [Text <| "Local Time (" + k.nowK + ")"]; TD [Text (formatDate i1.now)];TD [Text " Local Time (output)"]; TD [Text (formatDate i2.now)]]
              TR[TD [Text <| "UTC Time (" + k.nowKUTC + ")"]; TD [Text (formatDate i1.nowUTC)];TD [Text " UTC Time (output)"]; TD [Text (formatDate i2.nowUTC)]]
              TR[TD [Text <| "Local Epoch (" + k.epochKLocal + ")"]; TD [Text (formatDate i1.epochLocal)];TD [Text " Local Epoch (output)"]; TD [Text (formatDate i2.epochLocal)]]
              TR[TD [Text <| "UTC Epoch (" + k.epochK + ")"]; TD [Text (formatDate i1.epoch)];TD [Text " UTC Epoch (output)"]; TD [Text (formatDate i2.epoch)]]
            ]
          do PlaceHolder.Append r
      }
      |> Async.Start 

    let Main (now : System.DateTime,nowUTC : System.DateTime,epochLocal : System.DateTime,epoch : System.DateTime) =
      do RemoteIt(now,nowUTC,epochLocal,epoch)
      Div [
            H2 [Text "Client Side"]
            Table[
              TR[TD [Text "Local Time"]; TD [Text (formatDate now)]]
              TR[TD [Text "UTC Time"]; TD [Text (formatDate nowUTC)]]
              TR[TD [Text "Local Epoch"]; TD [Text (formatDate epochLocal)]]
              TR[TD [Text "UTC Epoch"]; TD [Text (formatDate epoch)]]
            ]
            PlaceHolder
          ]
     
