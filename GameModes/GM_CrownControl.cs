using RWF.GameModes;

namespace GameModeCollection.GameModes
{
    /// <summary>
    /// 
    /// A game mode which can be played as FFA or in teams, where players fight for control of a crown.
    /// The crown starts in the middle of each battlefield and a player picks it up by walking into it.
    /// When the crowned player dies, they drop the crown - with it keeping the momentum of the now dead player.
    /// A team wins when they have held the crown for a requisite amount of time.
    /// 
    /// - Players respawn after a few seconds during battles
    /// - If the crown goes OOB off the right, left, or bottom while it is not controlled by a player, it is respawned at a random point above ground on the battlefield
    /// - If the crown goes untouched for too long, it is respawned at the center of the battlefield
    /// 
    /// </summary>
    public class GM_CrownControl : RWFGameMode
    {
    }
}
