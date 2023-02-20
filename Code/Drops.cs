using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cultivation_Way;
using Cultivation_Way.Utils;
namespace Extreme_Spells.Code
{
    internal static class Drops
    {
        public static void add_drops()
        {
            AssetManager.drops.add(new DropAsset
            {
                id = "extreme_meteorolite_drop",
                path_texture = "drops/drop_lava",
                animated = true,
                animation_speed = 0.03f,
                default_scale = 0.5f,
                sound_drop = "lava",
                action_landed = new DropsAction((tile, id) =>
                {
                    float damage = tile.delayedTimerBomb;

                    CW_Actor user = null;
                    Pair<CW_Actor, int> count;
                    if(Addon_Main_Class.instance.tmp_actors.TryGetValue(tile.delayedBombType, out count))
                    {
                        count.Value--;
                        user = count.Key;
                        if (count.Value <= 0)
                        {
                            Addon_Main_Class.instance.tmp_actors[tile.delayedBombType] = null;
                            Addon_Main_Class.instance.tmp_actors.Remove(tile.delayedBombType);
                        }
                    }

                    tile.delayedTimerBomb = 0;
                    tile.delayedBombType = "";
                    List<List<BaseSimObject>> enemies = CW_SpellHelper.find_kingdom_enemies_in_chunk(tile.chunk, user == null ? null : user.kingdom);
                    Addon_Main_Class.instance.bomb_controller.spawnAtRandomScale(tile, 0.45f, 0.6f);
                    //if(Toolbox.randomChance(0.001f))UnityEngine.Debug.LogFormat("Damage:{0},units_count:{1}", damage,tile.units.Count);
                    foreach(List<BaseSimObject> list in enemies)
                    {
                        foreach(BaseSimObject enemy in list)
                        {

                            if (enemy != user && enemy.base_data.alive && CW_SpellHelper.cause_damage_to_target(user, enemy, damage, Cultivation_Way.Others.CW_Enums.CW_AttackType.Spell, true)) { }//UnityEngine.Debug.LogFormat("{0} cause damage {1} to {2}", user==null?"null":user.fast_data.actorID,damage,enemy.name);
                        }
                    }
                })
            });
        }
    }
}
