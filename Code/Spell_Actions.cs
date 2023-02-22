using Cultivation_Way.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cultivation_Way.Animation;
using Cultivation_Way.Utils;
using UnityEngine;

namespace Extreme_Spells.Code
{
    internal static class Spell_Actions
    {
        public static void extreme_void_spell_action(CW_Asset_Spell spell_asset, BaseSimObject pUser, BaseSimObject pTarget, WorldTile pTargetTile, float cost)
        {
            if (pUser == null || !pUser.base_data.alive) return;
            CW_SpriteAnimation anim = CW_EffectManager.instance.spawn_anim(spell_asset.anim_id, pTargetTile, pTargetTile, pUser, pTarget, Mathf.Log10(cost / 1000));
            if (anim == null) return;
            anim.cost_for_spell = cost;
        }

        internal static void extreme_meteorolite_spell_action(CW_Asset_Spell spell_asset, BaseSimObject pUser, BaseSimObject pTarget, WorldTile pTargetTile, float cost)
        {
            if (pUser == null || !pUser.base_data.alive) return;
            CW_SpriteAnimation anim = CW_EffectManager.instance.spawn_anim(spell_asset.anim_id, pTargetTile.posV+new Vector3(300,500), pTargetTile.posV, pUser, pTarget, Mathf.Log10(cost / 1000));
            if (anim == null) return;
            anim.cost_for_spell = cost;
            anim.set_alpha(0);
            anim.set_position(pTargetTile.posV + new Vector3(50, 100));
        }

        internal static void extreme_fire_spell_action(CW_Asset_Spell spell_asset, BaseSimObject pUser, BaseSimObject pTarget, WorldTile pTargetTile, float cost)
        {
            if (pUser == null || !pUser.base_data.alive) return;
            float radius = Mathf.Log10(cost)*3;
            List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(pTargetTile, radius);
            //Debug.Log(tiles.Count);
            foreach(WorldTile tile in tiles)
            {
                if (Toolbox.randomChance(0.6f)) continue;
                CW_SpriteAnimation anim = CW_EffectManager.instance.spawn_anim(spell_asset.anim_id, tile.posV+new Vector3(0,Toolbox.randomFloat(25f,62f)), tile.posV, pUser, null, 1);
                if (anim == null) { Debug.Log("Fail to spawn anim"); return; }

                anim.cost_for_spell = Mathf.Sqrt(cost);
                anim.cur_frame_idx = Toolbox.randomInt(0, 3);
                //anim.set_alpha(0);
                //anim.set_position(tile.posV + new Vector3(0, 50));
                //Debug.Log(anim.gameObject.transform.localPosition);
            }
        }

        internal static void extreme_gold_sword_b_spell_action(CW_Asset_Spell spell_asset, BaseSimObject pUser, BaseSimObject pTarget, WorldTile pTargetTile, float cost)
        {
            if (pUser == null || !pUser.base_data.alive) return;
            float radius = Mathf.Log10(cost) * 3;
            List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(pTargetTile, radius);
            //Debug.Log(tiles.Count);
            foreach (WorldTile tile in tiles)
            {
                if (Toolbox.randomChance(0.1f)) continue;
                int count = (int)(Mathf.Sqrt(radius - Toolbox.DistTile(pTargetTile, tile)));
                while (count-- > 0)
                {
                    float x_offset = Toolbox.randomFloat(-0.5f, 0.5f);
                    float y_offset = Toolbox.randomFloat(-0.5f, 0.5f);
                    CW_SpriteAnimation anim = CW_EffectManager.instance.spawn_anim(spell_asset.anim_id, tile.posV+new Vector3(x_offset, Toolbox.randomFloat(10f, 40f)), tile.posV+new Vector3(x_offset, y_offset), pUser, null, 1);
                    if (anim == null) { Debug.Log("Fail to spawn anim"); return; }

                    anim.cost_for_spell = radius * radius;
                    anim.cur_frame_idx = 0;
                }
            }
        }

        internal static void extreme_gold_sword_a_spell_action(CW_Asset_Spell spell_asset, BaseSimObject pUser, BaseSimObject pTarget, WorldTile pTargetTile, float cost)
        {
            if (pUser == null || !pUser.base_data.alive) return;
            float radius = Mathf.Log10(cost) * 3;
            List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(pTargetTile, radius);
            //Debug.Log(tiles.Count);
            List<WorldTile> edge_tiles = new List<WorldTile>();
            foreach (WorldTile tile in tiles)
            {
                //if (Toolbox.randomChance(0.6f)) continue;
                int count = (int)(radius - Toolbox.DistTile(pTargetTile, tile));
                if (count != 1) continue;

                edge_tiles.Add(tile);

                Vector3 src_vec = tile.posV;
                Vector3 dst_vec = 2 * pTargetTile.posV - src_vec;

                CW_SpriteAnimation anim = CW_EffectManager.instance.spawn_anim(spell_asset.anim_id, src_vec, dst_vec, pUser, null, 1);
                if (anim == null) { Debug.Log("Fail to spawn anim"); return; }

                anim.cost_for_spell = radius * radius;
                anim.cur_frame_idx = 0;
                anim.free_val = Toolbox.DistTile(pTargetTile, tile) * 2;

                for (int i = 0; i < 5; i++)
                {
                    WorldTile target_tile = edge_tiles.GetRandom(); if (target_tile == tile) break;

                    anim = CW_EffectManager.instance.spawn_anim(spell_asset.anim_id, src_vec, target_tile.posV, pUser, null, 1);
                    if (anim == null) { Debug.Log("Fail to spawn anim"); return; }

                    anim.cost_for_spell = radius * radius;
                    anim.cur_frame_idx = 0;
                    anim.free_val = Toolbox.DistTile(target_tile, tile) * 2;
                }
            }
        }
    }
}
