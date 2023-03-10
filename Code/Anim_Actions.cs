using System;
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
        private static TerraformOptions tmp_terraform_options;
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
                if(Cultivation_Way.Content.W_Content_WorldLaws.is_law_enable(Addon_Main_Class.instance.destroy_world_law_id) && Toolbox.randomChance(0.01f))MapAction.terraformMain(tile, TileLibrary.pit_deep_ocean, TerraformLibrary.destroy_no_flash);
                if (chunk == null) continue;
                anim.end_froze_time += CW_Utils_Others.get_raw_wakan(chunk.wakan * 0.001f, chunk.wakan_level);
                chunk.wakan *= 0.999f;
                chunk.update(true);
            }
            Cultivation_Way.World_Data.instance.map_chunk_manager.force_update_mapmode();
        }
        public static void extreme_void_end(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            WorldTile center = MapBox.instance.GetTile((int)src_vec.x, (int)src_vec.y);
            if (center == null || anim.src_object == null || !anim.src_object.base_data.alive) return;
            List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(center, (int)(Mathf.Log10(anim.cost_for_spell / 1000) * 12));
            if (Cultivation_Way.Content.W_Content_WorldLaws.is_law_enable(Addon_Main_Class.instance.destroy_world_law_id))
            {
                foreach(WorldTile tile in tiles)
                {
                    MapAction.terraformMain(tile, TileLibrary.pit_deep_ocean, TerraformLibrary.destroy_no_flash);
                }
            }
            
            CW_Actor actor = (CW_Actor)anim.src_object;
            actor.cw_status.wakan += CW_Utils_Others.compress_raw_wakan(anim.end_froze_time, actor.cw_status.wakan_level);
            //Debug.Log(CW_Utils_Others.compress_raw_wakan(anim.end_froze_time, actor.cw_status.wakan_level));
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
                    if(Cultivation_Way.Content.W_Content_WorldLaws.is_law_enable(Addon_Main_Class.instance.destroy_world_law_id)) MapAction.terraformMain(tile, TileLibrary.pit_deep_ocean, TerraformLibrary.destroy_no_flash);

                    chunk = tile.get_cw_chunk();
                    if (chunk.wakan <= 0) { count.Value--; continue; }

                    MapBox.instance.dropManager.spawnBurstPixel(tile, "extreme_meteorolite_drop", 0, 2, 0, -1);

                    tile.delayedTimerBomb = anim.cost_for_spell/10 + CW_Utils_Others.get_raw_wakan(chunk.wakan*0.2f, chunk.wakan_level);

                    tile.delayedBombType = _tmp_actor.fast_data.actorID;

                    chunk.wakan *= 0.999f;

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
        internal static void extreme_lightning_frame(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            //throw new NotImplementedException();
            if (cur_frame_idx != 5) return;
            List<WorldTile> tiles = new List<WorldTile>();

            List<Vector2> src_list = new List<Vector2>();
            int i;
            float scale = anim.get_scale().x;

            Vector2Int delta = new Vector2Int((int)((dst_vec.x - src_vec.x) * scale * 5), (int)((dst_vec.y - src_vec.y) * scale * 5));

            Vector2 crude_dir = new Vector2(Mathf.Sign(delta.x), Mathf.Sign(delta.y));

            for (i = -(int)(scale * 4); i < (int)(scale * 4); i++)
            {
                src_list.Add(src_vec + i*crude_dir);
            }

            int dx = Math.Abs(delta.x);
            int dy = Math.Abs(delta.y);
            float k = delta.y / (float)delta.x;

            Vector2 tmp_vec = new Vector2(0, 0);
            WorldTile tmp_tile;
            foreach (Vector2 cur_src in src_list)
            {
                float b = -k * cur_src.x + cur_src.y;
                for (i = 0; i < dx; i++)
                {
                    tmp_vec.x = cur_src.x + i * crude_dir.x;
                    tmp_vec.y = k * tmp_vec.x + b;
                    tmp_tile = MapBox.instance.GetTile((int)tmp_vec.x, (int)tmp_vec.y);
                    if (tmp_tile == null) continue;
                    tiles.Add(tmp_tile);
                }
                for (i = 0; i < dy; i++)
                {
                    tmp_vec.y = cur_src.y + i * crude_dir.y;
                    tmp_vec.x = (tmp_vec.y - cur_src.y) / k + cur_src.x;
                    tmp_tile = MapBox.instance.GetTile((int)tmp_vec.x, (int)tmp_vec.y);
                    if (tmp_tile == null) continue;
                    tiles.Add(tmp_tile);
                }
            }

            foreach(WorldTile tile in tiles) { MapAction.removeLiquid(tile); tile.setBurned(); }

            List<BaseSimObject> enemies = CW_SpellHelper.find_enemies_in_tiles(tiles, anim.src_object == null ? null : anim.src_object.kingdom);
            foreach(BaseSimObject enemy in enemies)
            {
                if (!enemy.base_data.alive) continue;
                CW_SpellHelper.cause_damage_to_target(anim.src_object, enemy, anim.cost_for_spell);
            }

            tmp_tile = MapBox.instance.GetTile((int)anim.src_vec.x, (int)anim.src_vec.y);
            if (tmp_tile != null)
            {
                enemies = CW_SpellHelper.find_enemies_in_circle(tmp_tile, anim.src_object == null ? null : anim.src_object.kingdom, (int)(scale * 8));
                foreach (BaseSimObject enemy in enemies)
                {
                    if (!enemy.base_data.alive) continue;
                    CW_SpellHelper.cause_damage_to_target(anim.src_object, enemy, anim.cost_for_spell);
                }
            }
            anim.free_val = Toolbox.randomFloat(0.8f, 3f);
            float radius = Toolbox.DistVec2Float(src_vec, dst_vec);
            float theta = Toolbox.randomFloat(0, 2 * Mathf.PI);
            dst_vec.x = src_vec.x + Mathf.Cos(theta) * radius;
            dst_vec.y = src_vec.y + Mathf.Sin(theta) * radius;
            anim.renderer.flipX = Toolbox.randomBool();
        }

        internal static void extreme_lightning_end(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            //throw new NotImplementedException();
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

        internal static void extreme_fire_end(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            //throw new NotImplementedException();
            WorldTile tile = MapBox.instance.GetTile((int)anim.gameObject.transform.localPosition.x, (int)anim.gameObject.transform.localPosition.y);
            if (tile == null) return;
            if (Toolbox.randomChance(0.99f)) return;

            if (anim.src_object != null && (tmp_terraform_options == null || tmp_terraform_options.id != anim.src_object.name))
            {
                tmp_terraform_options = new TerraformOptions
                {
                    id = anim.src_object.name,
                    removeBorders = true,
                    removeBurned = false,
                    removeBuilding = true,
                    removeFrozen = true,
                    removeLava = false,
                    removeRoads = true,
                    removeRuins = true,
                    removeTopTile = true,
                    removeTornado = false,
                    removeTreesFully = true,
                    removeWater = true,
                    destroyBuildings = true,
                    addBurned = true,
                    addHeat = 2,
                    applies_to_high_flyers = true,
                    damage = 0,
                    explode_and_set_random_fire = false,
                    explode_strength = 0,
                    explode_tile = false,
                    explosion_pixel_effect = false,
                    flash = false,
                    ignoreKingdoms = new string[] { anim.src_object.kingdom.id },
                    setFire = true
                };
            }
            MapAction.terraformMain(tile, tile.main_type.ground ? AssetManager.tiles.get("sand") : tile.main_type, tmp_terraform_options);
        }

        internal static void extreme_fire_frame(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            bool valid = false;
            // 采用end_froze_time>0作为落地标识
            if (anim.end_froze_time <= 0 && Mathf.Sqrt(Mathf.Pow(anim.gameObject.transform.localPosition.x - dst_vec.x, 2) + Mathf.Pow(anim.gameObject.transform.localPosition.y - dst_vec.y, 2)) > anim.get_setting().trace_grad * anim.get_setting().frame_interval)
            {
                if (Toolbox.randomChance(0.5f)) anim.set_position(new Vector3(anim.dst_vec.x + Toolbox.randomFloat(-0.1f, 0.1f), anim.gameObject.transform.localPosition.y));
            }
            else
            {

                WorldTile tile = MapBox.instance.GetTile((int)anim.gameObject.transform.localPosition.x, (int)anim.gameObject.transform.localPosition.y);
                if (tile == null) return;

                CW_MapChunk chunk = tile.get_cw_chunk();

                if (anim.end_froze_time <= 0)
                {
                    if (Toolbox.randomChance(0.255f) && chunk.wakan >= 120)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 new_dst = new Vector3(dst_vec.x + Toolbox.randomFloat(-5f, 5f), dst_vec.y + Toolbox.randomFloat(-5f, 5f));
                            CW_SpriteAnimation new_anim = CW_EffectManager.instance.spawn_anim("extreme_fire_anim", new_dst + new Vector2(0, 50 + Toolbox.randomFloat(-25f, 12f)), new_dst, anim.src_object, null, 1);

                            if (new_anim != null)
                            {
                                valid = true;
                                new_anim.cost_for_spell = anim.cost_for_spell;
                                new_anim.cur_frame_idx = Toolbox.randomInt(0, 3);
                            }
                        }
                    }
                    anim.end_froze_time = 1;
                }


                anim.next_frame_time -= Toolbox.randomFloat(0, anim.next_frame_time / 5f);

                if (anim.src_object != null && (tmp_terraform_options == null || tmp_terraform_options.id != anim.src_object.name))
                {
                    tmp_terraform_options = new TerraformOptions
                    {
                        id = anim.src_object.name,
                        removeBorders = true,
                        removeBurned = false,
                        removeBuilding = true,
                        removeFrozen = true,
                        removeLava = false,
                        removeRoads = true,
                        removeRuins = true,
                        removeTopTile = true,
                        removeTornado = false,
                        removeTreesFully = true,
                        removeWater = true,
                        destroyBuildings = true,
                        addBurned = true,
                        addHeat = 2,
                        applies_to_high_flyers = true,
                        damage = 0,
                        explode_and_set_random_fire = false,
                        explode_strength = 0,
                        explode_tile = false,
                        explosion_pixel_effect = false,
                        flash = false,
                        ignoreKingdoms = new string[] { anim.src_object.kingdom.id },
                        setFire = true
                    };
                }

                if (Toolbox.randomChance(0.4f))
                {
                    tile.setBurned();
                    MapAction.terraformMain(tile, tile.main_type.ground ? AssetManager.tiles.get("sand") : tile.main_type, tmp_terraform_options);
                }
                foreach (WorldTile near_tile in tile.neighboursAll)
                {
                    if (Toolbox.randomChance(0.2f))
                    {
                        near_tile.setBurned();
                        MapAction.terraformMain(near_tile, near_tile.main_type.ground ? AssetManager.tiles.get("sand") : near_tile.main_type, tmp_terraform_options);
                    }
                }

                float cost = anim.cost_for_spell * CW_Utils_Others.get_raw_wakan(chunk.wakan * 0.001f, chunk.wakan_level) * 10;
                foreach (CW_Actor unit in tile.units)
                {
                    if (unit.fast_data.alive && unit != anim.src_object && CW_SpellHelper.is_enemy(unit, anim.src_object))
                    {
                        valid = true;
                        unit.get_hit(cost, true, Cultivation_Way.Others.CW_Enums.CW_AttackType.Spell, anim.src_object, false);
                        ActionLibrary.addBurningEffectOnTarget(unit, tile);
                    }
                }
                foreach (WorldTile near_tile in tile.neighboursAll)
                {
                    foreach (CW_Actor unit in near_tile.units)
                    {
                        if (unit.fast_data.alive && unit != anim.src_object && CW_SpellHelper.is_enemy(unit, anim.src_object))
                        {
                            valid = true;
                            unit.get_hit(cost, true, Cultivation_Way.Others.CW_Enums.CW_AttackType.Spell, anim.src_object, false);
                            ActionLibrary.addBurningEffectOnTarget(unit, near_tile);
                        }
                    }
                }
                if (chunk.wakan > 0)
                {
                    if (valid)
                    {
                        chunk.wakan -= Mathf.Max(0.1f, 0.001f * chunk.wakan);
                        chunk.update(true);
                    }
                    else
                    {
                        chunk.wakan -= Mathf.Max(0.1f, 0.0001f * chunk.wakan);
                        chunk.update(true);
                    }
                }
            }

        }

        internal static void extreme_tornado_end(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            //throw new NotImplementedException();
        }

        internal static void extreme_tornado_frame(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            //throw new NotImplementedException();
            WorldTile tile = MapBox.instance.GetTile((int)anim.gameObject.transform.localPosition.x, (int)anim.gameObject.transform.localPosition.y);
            if (tile == null) return;

            float scale = anim.get_scale().x;
            float damage = anim.cost_for_spell * scale * scale;
            List<WorldTile> tiles = CW_SpellHelper.get_circle_tiles(tile, (int)(scale * 8));
            List<BaseSimObject> enemies = CW_SpellHelper.find_enemies_in_tiles(tiles, anim.src_object == null ? null : anim.src_object.kingdom);

            foreach(BaseSimObject enemy in enemies)
            {
                if (enemy.objectType != MapObjectType.Actor || !enemy.base_data.alive) continue;
                CW_Actor cw_enemy = (CW_Actor)enemy;

                cw_enemy.addForce(Mathf.Clamp(anim.src_vec.x - enemy.currentPosition.x, - scale * 8, scale * 8) * scale * 0.1f, Mathf.Clamp(anim.src_vec.y - enemy.currentPosition.y, -scale * 8, scale * 8) * scale * 0.1f, scale*4);

                CW_SpellHelper.cause_damage_to_target(anim.src_object, cw_enemy, damage, Cultivation_Way.Others.CW_Enums.CW_AttackType.Spell, true);
                
            }
        }

        internal static void extreme_gold_sword_a_end(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
        }

        internal static void extreme_gold_sword_a_frame(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            WorldTile tile = MapBox.instance.GetTile((int)anim.gameObject.transform.localPosition.x, (int)anim.gameObject.transform.localPosition.y);
            if (tile == null) return;
            float damage = anim.cost_for_spell;
            foreach (CW_Actor unit in tile.units)
            {
                if (unit.fast_data.alive && unit != anim.src_object && CW_SpellHelper.is_enemy(unit, anim.src_object))
                {
                    unit.get_hit(damage, true, Cultivation_Way.Others.CW_Enums.CW_AttackType.God, anim.src_object, false);
                }
            }
        }

        internal static void extreme_gold_sword_b_end(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {

            WorldTile tile = MapBox.instance.GetTile((int)anim.gameObject.transform.localPosition.x, (int)anim.gameObject.transform.localPosition.y);
            if (tile == null) return;
            //throw new NotImplementedException();
            if (Toolbox.randomChance(0.48f))
            {
                CW_SpriteAnimation new_anim = CW_EffectManager.instance.spawn_anim("gold_sword_b_anim", src_vec, dst_vec, anim.src_object, null, 1);

                if (new_anim != null)
                {
                    new_anim.cost_for_spell = anim.cost_for_spell;
                }
            }
            else
            {
                //MapBox.instance.dropManager.spawnBurstPixel(tile, "extreme_gold_drop", 0.2f, Toolbox.randomFloat(0.5f + anim.get_scale().x, anim.get_scale().x), 0);
            }
        }

        internal static void extreme_gold_sword_b_frame(int cur_frame_idx, ref Vector2 src_vec, ref Vector2 dst_vec, CW_SpriteAnimation anim)
        {
            if (Toolbox.Dist(anim.gameObject.transform.localPosition.x, anim.gameObject.transform.localPosition.y, dst_vec.x, dst_vec.y) >= Cultivation_Way.Others.CW_Constants.anim_dst_error) return;
            anim.play_time += Toolbox.randomFloat(0, 0.05f);

            if (anim.free_val < 1)
            {
                anim.free_val = 2;
                anim.gameObject.transform.localRotation = Quaternion.Euler(0, 0, Toolbox.randomFloat(-120, -60));
            }
            WorldTile tile = MapBox.instance.GetTile((int)anim.gameObject.transform.localPosition.x, (int)anim.gameObject.transform.localPosition.y);
            float damage = anim.cost_for_spell * anim.get_scale().x;
            foreach (CW_Actor unit in tile.units)
            {
                if (unit.fast_data.alive && unit != anim.src_object && CW_SpellHelper.is_enemy(unit, anim.src_object))
                {
                    unit.get_hit(damage, true, Cultivation_Way.Others.CW_Enums.CW_AttackType.God, anim.src_object, false);
                    if (unit.fast_data.alive&&Toolbox.randomChance(0.017f))
                    {
                        CW_SpriteAnimation new_anim = CW_EffectManager.instance.spawn_anim("gold_sword_b_anim", src_vec, dst_vec, anim.src_object, null, 1);

                        if (new_anim != null)
                        {
                            new_anim.cost_for_spell = anim.cost_for_spell;
                        }
                    }
                }
            }
            foreach (WorldTile near_tile in tile.neighboursAll)
            {
                foreach (CW_Actor unit in near_tile.units)
                {
                    if (unit.fast_data.alive && unit != anim.src_object && CW_SpellHelper.is_enemy(unit, anim.src_object))
                    {
                        unit.get_hit(damage / 3, true, Cultivation_Way.Others.CW_Enums.CW_AttackType.God, anim.src_object, false);
                        if (unit.fast_data.alive && Toolbox.randomChance(0.017f))
                        {
                            CW_SpriteAnimation new_anim = CW_EffectManager.instance.spawn_anim("gold_sword_b_anim", src_vec, dst_vec, anim.src_object, null, 1);

                            if (new_anim != null)
                            {
                                new_anim.cost_for_spell = anim.cost_for_spell;
                            }
                        }
                    }
                }
            }
        }

    }
}
