using Cultivation_Way;
using Cultivation_Way.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extreme_Spells.Code
{
    internal static class Tester
    {
        private static void default_extreme_gold_sword_b()
        {
            force_spell("extreme_gold_sword_b");
        }
        private static void default_extreme_meteorolite()
        {
            extreme_meteorolite();
        }
        private static void default_extreme_fire()
        {
            extreme_fire();
        }
        private static void extreme_meteorolite(string src = "u_0", string dst = "u_1", float cost = 20000000)
        {
            force_spell("extreme_meteorolite", src, dst, cost);
        }
        private static void extreme_fire(string src = "u_0", string dst = "u_1", float cost = 20000000)
        {
            force_spell("extreme_fire", src, dst, cost);
        }
        private static bool force_spell(string spell_id, string src_id = "u_0", string dst_id = "u_1", float cost = 20000000)
        {
            CW_Asset_Spell spell_asset = CW_Library_Manager.instance.spells.get(spell_id);
            Actor src_actor = MapBox.instance.getActorByID(src_id);
            Actor dst_actor = MapBox.instance.getActorByID(dst_id);
            WorldTile tile = dst_actor.currentTile;
            if (spell_asset.anim_type != CW_Spell_Animation_Type.CUSTOM)
            {
                if (spell_asset.damage_action != null) spell_asset.damage_action(spell_asset, src_actor, dst_actor, tile, 10);
                if (spell_asset.anim_action != null) spell_asset.anim_action(spell_asset, src_actor, dst_actor, tile, 10);
            }
            else
            {
                spell_asset.spell_action(spell_asset, src_actor, dst_actor, tile, cost);
            }
            return true;
        }
    }
}
