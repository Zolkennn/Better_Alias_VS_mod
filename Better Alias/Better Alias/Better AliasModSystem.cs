using System.Linq;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;

namespace Better_Alias;

public class Better_AliasModSystem : ModSystem
{
    private ICoreServerAPI _sapi;
    
    public override bool ShouldLoad(EnumAppSide forSide)
    {
        return forSide == EnumAppSide.Server;
    }
    
    public override void StartServerSide(ICoreServerAPI api)
    {
        _sapi = api;
        _sapi.Event.PlayerNowPlaying += EventPlayerJoin;
        
        _sapi.ChatCommands.GetOrCreate("alias")
            .RequiresPrivilege(Privilege.chat)
            .RequiresPlayer()
            .WithDescription("Modify player aliases")
            
            .BeginSub("set")
            .WithAlias("change")
            .WithAlias("-s")
            .WithDescription(Lang.Get("betteralias:setdesc"))
            .WithArgs(_sapi.ChatCommands.Parsers.OptionalWord("New_alias"), new OnlinePlayerArgParser("PlayerName", api, false))
            .HandleWith(SetPlayerAlias)
            .EndSubCommand()
            
            .BeginSub("remove")
            .WithAlias("rm")
            .WithAlias("-r")
            .WithDescription(Lang.Get("betteralias:rmdesc"))
            .WithArgs(new OnlinePlayerArgParser("PlayerName", api, false)) //todo
            .HandleWith(RemoveAlias)
            .EndSubCommand()
            
            .Validate();
       
    }
    
    private TextCommandResult SetPlayerAlias(TextCommandCallingArgs args)
    {
        var newName = args[0] as string;
        var player = (args[1] ?? args.Caller.Player) as IServerPlayer;
        if (player != args.Caller.Player && !args.Caller.Player!.Privileges.Contains(Privilege.commandplayer))
        {
            return TextCommandResult.Error(Lang.Get("betteralias:Insufficient Privileges to set another players alias"));
        }
        var nametagAttribute = player?.Entity.WatchedAttributes.GetTreeAttribute("nametag");
        
        nametagAttribute?.SetString("name", newName);
        nametagAttribute?.SetString("nameSave", newName);
        player?.Entity.WatchedAttributes.MarkPathDirty("nametag");
        
        return TextCommandResult.Success(Lang.Get("betteralias:presetalias") + player?.PlayerName + Lang.Get("betteralias:setalias") + newName);
    }

    private TextCommandResult RemoveAlias(TextCommandCallingArgs args)
    {
        var player = (args[0] ?? args.Caller.Player) as IServerPlayer;
        if (player != args.Caller.Player && !args.Caller.Player!.Privileges.Contains(Privilege.commandplayer))
        {
            return TextCommandResult.Error(Lang.Get("betteralias:Insufficient Privileges to remove another players alias"));
        }
        var nameTagAttribute = player?.Entity.WatchedAttributes.GetTreeAttribute("nametag");
        
        //todo remove dupe
        nameTagAttribute?.SetString("name", player.PlayerName);
        nameTagAttribute?.SetString("nameSave", player.Entity.GetName());
        player?.Entity.WatchedAttributes.MarkPathDirty("nametag");
        
        return TextCommandResult.Success(Lang.Get("betteralias:removealias") + player?.PlayerName);
    }

    private void EventPlayerJoin(IServerPlayer byplayer)
    {
        var nameTagAttribute = byplayer.Entity.WatchedAttributes.GetTreeAttribute("nametag");
        nameTagAttribute?.SetString("name", nameTagAttribute.GetString("nameSave") ?? byplayer.PlayerName);
        byplayer?.Entity.WatchedAttributes.MarkPathDirty("nametag");
        byplayer.SendMessage(GlobalConstants.GeneralChatGroup, Lang.Get("betteralias:joinMessage") + nameTagAttribute.GetString("name"), EnumChatType.OwnMessage);
    }
}