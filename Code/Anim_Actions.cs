﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cultivation_Way;
using Cultivation_Way.Utils;
using Cultivation_Way.Animation;
using UnityEngine;
using ReflectionUtility;
using Cultivation_Way.Extensions;
namespace Extreme_Spells.Code
{
    internal static class Anim_Actions
    {
        public static void extreme_void_frame(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            if (cur_frame_idx == 17) anim.cur_frame_idx = 6;
            WorldTile center = MapBox.instance.GetTile((int)src_vec.x, (int)src_vec.y);
            if (center == null || anim.src_object == null || !anim.src_object.base_data.alive) { anim.force_stop(false); return; }


            List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(center, (int)(Mathf.Log10(anim.cost_for_spell / 1000) * 12));

            List<BaseSimObject> enemies = CW_SpellHelper.find_enemies_in_tiles(tiles, anim.src_object.kingdom);
            float dist;
            foreach (BaseSimObject enemy in enemies)
            {
                dist = Toolbox.DistVec2Float(dst_vec, enemy.currentPosition);
                CW_SpellHelper.cause_damage_to_target(anim.src_object, enemy, anim.cost_for_spell / ((1 + dist)*10000f));
                if (enemy.objectType != MapObjectType.Actor || enemy==  anim.src_object) continue;
                if (!enemy.base_data.alive)
                {
                    // 夺取灵气
                    anim.end_froze_time += CW_Utils_Others.get_raw_wakan(((CW_Actor)enemy).cw_status.wakan, ((CW_Actor)enemy).cw_status.wakan_level);
                    ((CW_Actor)enemy).cw_status.wakan = 0;
                }
                else
                {
                    // 拖拽
                    ((CW_Actor)enemy).add_force((dst_vec.x-enemy.currentPosition.x) * 0.1f, (dst_vec.y-enemy.currentPosition.y) * 0.1f, 0.05f);
                }
                

            }
            foreach(WorldTile tile in tiles)
            {
                CW_MapChunk chunk = tile.get_cw_chunk();
                if(Toolbox.randomChance(0.01f))MapAction.terraformMain(tile, TileLibrary.pit_deep_ocean, TerraformLibrary.destroy_no_flash);
                if (chunk == null) continue;
                anim.end_froze_time += CW_Utils_Others.get_raw_wakan(chunk.wakan * 0.05f, chunk.wakan_level);
                chunk.wakan *= 0.95f;
                chunk.update(true);
            }
            Cultivation_Way.World_Data.instance.map_chunk_manager.force_update_mapmode();
        }
        public static void extreme_void_end(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            WorldTile center = MapBox.instance.GetTile((int)src_vec.x, (int)src_vec.y);
            if (center == null || anim.src_object == null || !anim.src_object.base_data.alive) return;
            List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(center, (int)(Mathf.Log10(anim.cost_for_spell / 1000) * 12));
            foreach(WorldTile tile in tiles)
            {
                MapAction.terraformMain(tile, TileLibrary.pit_deep_ocean, TerraformLibrary.destroy_no_flash);
            }
            CW_Actor actor = (CW_Actor)anim.src_object;
            actor.cw_status.wakan += CW_Utils_Others.compress_raw_wakan(anim.end_froze_time, actor.cw_status.wakan_level);
            Debug.Log(CW_Utils_Others.compress_raw_wakan(anim.end_froze_time, actor.cw_status.wakan_level));
            for(int i=0;i<20;i++)actor.check_level_up();
        }

        internal static void extreme_meteorolite_end(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            WorldTile center = MapBox.instance.GetTile((int)dst_vec.x, (int)dst_vec.y);
            if (center == null) return;
            List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(center, (int)(Mathf.Log10(anim.cost_for_spell / 1000) * 12));

            List<BaseSimObject> enemies = CW_SpellHelper.find_enemies_in_tiles(tiles, anim.src_object==null?null:anim.src_object.kingdom);

            float dist;
            float new_cost;
            CW_MapChunk chunk;
            float base_cost = anim.cost_for_spell * anim.cost_for_spell;
            if ((anim.src_object != null && anim.src_object.objectType == MapObjectType.Actor))
            {
                CW_Actor _tmp_actor = (CW_Actor)anim.src_object;
                Pair<CW_Actor, int> count;
                if (!Addon_Main_Class.instance.tmp_actors.TryGetValue(_tmp_actor.fast_data.actorID, out count))
                {
                    count = new Pair<CW_Actor, int>(_tmp_actor, 0);
                    Addon_Main_Class.instance.tmp_actors[_tmp_actor.fast_data.actorID] = count;
                }
                count.Value += tiles.Count;

                foreach (WorldTile tile in tiles)
                {
                    MapAction.terraformMain(tile, TileLibrary.pit_deep_ocean, TerraformLibrary.destroy_no_flash);

                    chunk = tile.get_cw_chunk();
                    if (chunk.wakan <= 0) { count.Value--; continue; }

                    MapBox.instance.dropManager.spawnBurstPixel(tile, "extreme_meteorolite_drop", 0, 2, 0, -1);

                    tile.delayedTimerBomb = anim.cost_for_spell/10 + CW_Utils_Others.get_raw_wakan(chunk.wakan*0.2f, chunk.wakan_level);

                    tile.delayedBombType = _tmp_actor.fast_data.actorID;

                    chunk.wakan *= 0.8f;

                    if (chunk.wakan < 1) chunk.wakan = 0;
                }

            }
            
            BaseEffect nuke_explosion = Addon_Main_Class.instance.nuke_controller.spawnAtRandomScale(center, Mathf.Log10(anim.cost_for_spell) / 10f, Mathf.Log10(anim.cost_for_spell) / 10f);
            nuke_explosion.transform.localPosition = new Vector3(nuke_explosion.transform.localPosition.x, nuke_explosion.transform.localPosition.y + 3);

            foreach (BaseSimObject enemy in enemies)
            {
                if (enemy == anim.src_object) continue;

                dist = Toolbox.DistVec2Float(dst_vec, enemy.currentPosition);

                chunk = enemy.currentTile.get_cw_chunk();

                new_cost = anim.cost_for_spell*81 / (5 + dist);

                CW_SpellHelper.cause_damage_to_target(anim.src_object, enemy, new_cost);

                if (enemy.objectType != MapObjectType.Actor) continue;

                if (enemy.base_data.alive)
                {
                    ((CW_Actor)enemy).add_force(-(dst_vec.x - enemy.currentPosition.x), -(dst_vec.y - enemy.currentPosition.y), 0.5f);
                }

            }
        }

        internal static void extreme_meteorolite_frame(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            anim.set_alpha(Mathf.Min(1, anim.renderer.color.a + 0.1f));

            if (anim.loop_nr % 3 == 0) Addon_Main_Class.instance.fire_smoke_controller.spawnSmoke(anim.gameObject.transform.localPosition, anim.get_scale().x);

            if(Mathf.Sqrt(Mathf.Pow(anim.gameObject.transform.localPosition.x - dst_vec.x,2)+ Mathf.Pow(anim.gameObject.transform.localPosition.y - dst_vec.y, 2)) <= anim.get_setting().trace_grad * anim.get_setting().frame_interval && anim.end_froze_time <= 0)
            {
                anim.end_froze_time = 1;

                WorldTile center = MapBox.instance.GetTile((int)dst_vec.x, (int)dst_vec.y);

                List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(center, (int)(Mathf.Log10(anim.cost_for_spell / 1000) * 12));
                List<BaseSimObject> enemies = CW_SpellHelper.find_enemies_in_tiles(tiles, anim.src_object.kingdom);
                //Debug.LogFormat("Tiles:{0},enemies:{1},cost:{2}", tiles.Count, enemies.Count, anim.cost_for_spell);
                float dist;
                foreach (BaseSimObject enemy in enemies)
                {
                    dist = Toolbox.DistVec2Float(dst_vec, enemy.currentPosition);
                    CW_SpellHelper.cause_damage_to_target(anim.src_object, enemy, anim.cost_for_spell / (5 + dist*3f));
                    if (enemy.objectType != MapObjectType.Actor || enemy == anim.src_object) continue;

                    if (enemy.base_data.alive)
                    {
                        ((CW_Actor)enemy).add_force(-(dst_vec.x - enemy.currentPosition.x) * 0.1f, -(dst_vec.y - enemy.currentPosition.y) * 0.1f, 0.05f);
                    }

                }
            }
        }
    }
}
