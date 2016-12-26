namespace selfHost

open WebSharper

module Server =

    type Sample = {now : System.DateTime; nowUTC : System.DateTime; epochLocal : System.DateTime; epoch : System.DateTime}
    type Kinds = {nowK : string; nowKUTC : string; epochKLocal : string; epochK : string}
    with
      static member FromSample (i : Sample) =
        let getKind (i : System.DateTime) =
          if i.Kind = System.DateTimeKind.Utc then "UTC"
          elif i.Kind = System.DateTimeKind.Local then "Local"
          else "Unspecified"
        {nowK=getKind i.now;
         nowKUTC=getKind i.nowUTC
         epochKLocal=getKind i.epochLocal
         epochK=getKind i.epoch
        }

    [<Remote>]
    let DoSomething (i : Sample) =
        let inputKinds = [||]
        let i2 = {
          now=i.nowUTC.ToLocalTime();
          nowUTC=i.now.ToUniversalTime();
          epochLocal=i.epoch.ToLocalTime();
          epoch=i.epochLocal.ToUniversalTime()}
        async {
            return (Kinds.FromSample i,i,i2)
        }
