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
        this._sapi = api;
        _sapi.Event.PlayerNowPlaying += EventPlayerJoin;
        
        _sapi.ChatCommands.GetOrCreate("playeralias")
            .RequiresPrivilege(Privilege.chat)
            .RequiresPlayer()
            .BeginSub("set_alias")
            .WithDescription("Sets the alias to your playername")
            .WithArgs(new StringArgParser("newAlias", true))
            .HandleWith(SetAlias);

        _sapi.ChatCommands.GetOrCreate("playeralias")
            .RequiresPrivilege(Privilege.chat)
            .RequiresPlayer()
            .BeginSub("remove_alias")
            .WithDescription("Resets the players name back to the default")
            .HandleWith(RemoveAlias);
    }

    private TextCommandResult SetAlias(TextCommandCallingArgs args)
    {
        var newName = args[0] as string;
        var player = args.Caller.Player as IServerPlayer;
        var nametagAttribute = player?.Entity.WatchedAttributes.GetTreeAttribute("nametag");
        nametagAttribute?.SetString("name", newName);
        nametagAttribute?.SetString("nameSave", newName);
        player?.Entity.WatchedAttributes.MarkPathDirty("nametag");
        
        return TextCommandResult.Success(Lang.Get("betteralias:setalias") + newName);
        
    }
    
    private TextCommandResult RemoveAlias(TextCommandCallingArgs args)
    {
        var player = args.Caller.Player as IServerPlayer;
        var nameTagAttribute = player?.Entity.WatchedAttributes.GetTreeAttribute("nametag");
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