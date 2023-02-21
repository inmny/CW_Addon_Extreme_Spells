using NCMS;
using UnityEngine;
using Cultivation_Way;
using Cultivation_Way.Actions;
using Cultivation_Way.Library;
using Cultivation_Way.Animation;
using System;
using ReflectionUtility;
using System.Collections.Generic;

namespace Extreme_Spells.Code
{
    internal class Pair<TI, TF>
    {
        public TI Key { get; }
        public TF Value { get; set; }
        public Pair(TI Key, TF Value){
            this.Key = Key;
            this.Value = Value;
        }
    }
    internal class Simple_SmokeController : SmokeController
    {
        public override void spawn()
        {
            return;
        }
    }
	[ModEntry]
	public class Addon_Main_Class : CW_Addon
	{
        internal StackEffects stack_effects;
        internal Simple_SmokeController fire_smoke_controller;
        internal BaseEffectController nuke_controller;
        internal BaseEffectController bomb_controller;
        internal Dictionary<string, Pair<CW_Actor, int>> tmp_actors;
        internal static Addon_Main_Class instance;
		public override void awake(){
			// 不要在此处添加代码，除非你知道你在做什么
			// DO NOT code here.
			load_mod_info(System.Type.GetType("Mod"));
		}
		public override void initialize(){
			Log("添加超级法术!");
            // 在这里初始化模组内容
            // Initalize your mod content here
            Drops.add_drops();
			add_spells();
            instance = this;
            stack_effects = MapBox.instance.stackEffects;
            // 获取原版烟效果
            BaseEffectController base_controller = (BaseEffectController)stack_effects.CallMethod("get", "fireSmoke");

            GameObject effects = new GameObject("effects");
            effects.transform.SetParent(this.transform);
            GameObject fire_smoke = new GameObject("fire_smokes");
            fire_smoke.transform.SetParent(effects.transform);

            fire_smoke_controller = fire_smoke.AddComponent<Simple_SmokeController>();
            fire_smoke_controller.prefab = GameObject.Instantiate(base_controller.prefab,fire_smoke_controller.transform);
            fire_smoke_controller.setWorld();
            fire_smoke_controller.prefab.GetComponent<SpriteRenderer>().sortingLayerName = "EffectsTop";
            fire_smoke_controller.prefab.transform.localPosition = new Vector3(-1000, -1000);
            // 获取原版核弹、炸弹效果
            nuke_controller = (BaseEffectController)stack_effects.CallMethod("get", "explosionNuke");
            bomb_controller = (BaseEffectController)stack_effects.CallMethod("get", "explosionSmall");

            tmp_actors = new Dictionary<string, Pair<CW_Actor, int>>();
        }

        public override void update(float elapsed)
        {
            fire_smoke_controller.update(elapsed);
        }
        private void add_spells()
        {
            /**
            gold_sword_a();
			extreme_water();
			extreme_tornado();
            */

            extreme_void();
            extreme_fire();
            extreme_meteorolite();
            gold_sword_b();
        }
        // 吞噬天地
        private void extreme_void()
        {
            CW_AnimationSetting anim_setting = new CW_AnimationSetting();
            anim_setting.frame_interval = 0.05f;
            anim_setting.loop_limit_type = AnimationLoopLimitType.TIME_LIMIT;
            anim_setting.loop_time_limit = 6f;
            anim_setting.layer_name = "EffectsBack";
            anim_setting.point_to_dst = false;
            anim_setting.anim_froze_frame_idx = -1;
            anim_setting.trace_grad = 20;
            anim_setting.frame_action = Anim_Actions.extreme_void_frame;
            anim_setting.end_action = Anim_Actions.extreme_void_end;
            anim_setting.set_trace(AnimationTraceType.NONE);
            CW_EffectManager.instance.load_as_controller("extreme_void_anim", "effects/anti_matter/", anim_limit: 10, controller_setting: anim_setting, base_scale: 0.5f);

            CW_Asset_Spell spell = new CW_Asset_Spell(
                id: "extreme_void", anim_id: "extreme_void_anim",
                new CW_Element(new int[] { 20, 20, 20, 20, 20 }), element_type_limit: null,
                rarity: 295, free_val: 1, cost: 0.98f, min_cost: 1000,
                learn_level: 10, cast_level: 10, can_get_by_random: true,
                cultisys_black_or_white_list: true, cultisys_list: null,
                banned_races: null,
                target_type: CW_Spell_Target_Type.ACTOR,
                target_camp: CW_Spell_Target_Camp.ENEMY,
                triger_type: CW_Spell_Triger_Type.ATTACK,
                anim_type: CW_Spell_Animation_Type.CUSTOM,
                damage_action: null,
                anim_action: null,
                spell_action: Code.Spell_Actions.extreme_void_spell_action,
                check_and_cost_action: Cultivation_Way.Actions.CW_SpellAction_Cost.default_check_and_cost
                );
            spell.add_tag(CW_Spell_Tag.IMMORTAL);
            spell.add_tag(CW_Spell_Tag.ATTACK);
            add_spell(spell);
        }
        // 地爆天星
        private void extreme_meteorolite()
        {
            CW_AnimationSetting anim_setting = new CW_AnimationSetting();
            anim_setting.frame_interval = 0.05f;
            anim_setting.loop_limit_type = AnimationLoopLimitType.TIME_LIMIT;
            anim_setting.loop_time_limit = 3f;
            anim_setting.layer_name = "Objects";
            anim_setting.always_roll = true;
            anim_setting.always_roll_axis = new Vector3(0, 0, 1);
            anim_setting.roll_angle_per_frame = 2000;
            anim_setting.point_to_dst = false;
            anim_setting.anim_froze_frame_idx = -1;
            anim_setting.trace_grad = 100;
            anim_setting.frame_action = Anim_Actions.extreme_meteorolite_frame;
            anim_setting.end_action = Anim_Actions.extreme_meteorolite_end;
            anim_setting.set_trace(AnimationTraceType.TRACK);
            CW_EffectManager.instance.load_as_controller("extreme_meteorolite_anim", "effects/extreme_meteorolite/", 10, 1, anim_setting);

            CW_Asset_Spell spell = new CW_Asset_Spell(
                id: "extreme_meteorolite", anim_id: "extreme_meteorolite_anim",
                new CW_Element(new int[] { 0, 50, 0, 0, 50 }), element_type_limit: null,
                rarity: 295, free_val: 1, cost: 0.8f, min_cost: 1000,
                learn_level: 10, cast_level: 10, can_get_by_random: true,
                cultisys_black_or_white_list: true, cultisys_list: null,
                banned_races: null,
                target_type: CW_Spell_Target_Type.ACTOR,
                target_camp: CW_Spell_Target_Camp.ENEMY,
                triger_type: CW_Spell_Triger_Type.ATTACK,
                anim_type: CW_Spell_Animation_Type.CUSTOM,
                damage_action: null,
                anim_action: null,
                spell_action: Code.Spell_Actions.extreme_meteorolite_spell_action,
                check_and_cost_action: Cultivation_Way.Actions.CW_SpellAction_Cost.default_check_and_cost
                );
            spell.add_tag(CW_Spell_Tag.IMMORTAL);
            spell.add_tag(CW_Spell_Tag.ATTACK);
            add_spell(spell);
        }
        // 天火焚世
        private void extreme_fire()
        {
            CW_AnimationSetting anim_setting = new CW_AnimationSetting();
            anim_setting.frame_interval = 0.3f;
            anim_setting.loop_limit_type = AnimationLoopLimitType.TIME_LIMIT;
            anim_setting.loop_time_limit = 10f;
            anim_setting.layer_name = "Objects";
            anim_setting.point_to_dst = false;
            anim_setting.anim_froze_frame_idx = -1;
            anim_setting.trace_grad = 15;
            anim_setting.frame_action = Anim_Actions.extreme_fire_frame;
            anim_setting.end_action = Anim_Actions.extreme_fire_end;
            anim_setting.set_trace(AnimationTraceType.TRACK);
            CW_EffectManager.instance.load_as_controller("extreme_fire_anim", "drops/drop_fire/", 10000, 0.2f, anim_setting);

            CW_Asset_Spell spell = new CW_Asset_Spell(
                id: "extreme_fire", anim_id: "extreme_fire_anim",
                new CW_Element(new int[] { 0, 100, 0, 0, 0 }), element_type_limit: null,
                rarity: 295, free_val: 1, cost: 0.8f, min_cost: 1000,
                learn_level: 10, cast_level: 10, can_get_by_random: true,
                cultisys_black_or_white_list: true, cultisys_list: null,
                banned_races: null,
                target_type: CW_Spell_Target_Type.ACTOR,
                target_camp: CW_Spell_Target_Camp.ENEMY,
                triger_type: CW_Spell_Triger_Type.ATTACK,
                anim_type: CW_Spell_Animation_Type.CUSTOM,
                damage_action: null,
                anim_action: null,
                spell_action: Code.Spell_Actions.extreme_fire_spell_action,
                check_and_cost_action: Cultivation_Way.Actions.CW_SpellAction_Cost.default_check_and_cost
                );
            spell.add_tag(CW_Spell_Tag.IMMORTAL);
            spell.add_tag(CW_Spell_Tag.ATTACK);
            add_spell(spell);
        }
        // 金光剑阵
        private void gold_sword_b()
        {
            CW_AnimationSetting anim_setting = new CW_AnimationSetting();
            anim_setting.frame_interval = 1f;
            anim_setting.loop_limit_type = AnimationLoopLimitType.DST_LIMIT;
            anim_setting.layer_name = "Objects";
            anim_setting.point_to_dst = true;
            anim_setting.always_point_to_dst = true;
            anim_setting.anim_froze_frame_idx = -1;
            anim_setting.trace_grad = 100;
            anim_setting.frame_action = Anim_Actions.extreme_gold_sword_b_frame;
            anim_setting.end_action = Anim_Actions.extreme_gold_sword_b_end;

            anim_setting.set_trace(Anim_Traces.trace_gold_sword_b);

            CW_EffectManager.instance.load_as_controller("gold_sword_b_anim", "effects/single_gold_sword/", 10000, 0.2f, anim_setting);

            CW_Asset_Spell spell = new CW_Asset_Spell(
                id: "extreme_gold_sword_b", anim_id: "gold_sword_b_anim",
                new CW_Element(new int[] { 0, 0, 0, 100, 0 }), element_type_limit: null,
                rarity: 295, free_val: 1, cost: 0.8f, min_cost: 1000,
                learn_level: 10, cast_level: 10, can_get_by_random: true,
                cultisys_black_or_white_list: true, cultisys_list: null,
                banned_races: null,
                target_type: CW_Spell_Target_Type.ACTOR,
                target_camp: CW_Spell_Target_Camp.ENEMY,
                triger_type: CW_Spell_Triger_Type.ATTACK,
                anim_type: CW_Spell_Animation_Type.CUSTOM,
                damage_action: null,
                anim_action: null,
                spell_action: Code.Spell_Actions.extreme_gold_sword_b_spell_action,
                check_and_cost_action: Cultivation_Way.Actions.CW_SpellAction_Cost.default_check_and_cost
                );
            spell.add_tag(CW_Spell_Tag.IMMORTAL);
            spell.add_tag(CW_Spell_Tag.ATTACK);
            add_spell(spell);
        }
        // 飓风领域
        private void extreme_tornado()
        {
            throw new NotImplementedException();
        }
        // 天河之水
        private void extreme_water()
        {
            throw new NotImplementedException();
        }
        // 万剑归宗
        private void gold_sword_a()
        {
            throw new NotImplementedException();
        }

        private void add_spell(CW_Asset_Spell spell)
        {
            try
            {
				CW_Library_Manager.instance.spells.add(spell);
				Log("成功添加法术'{0}'", LocalizedTextManager.getText("spell_" + spell.id));
			}
			catch(Exception e)
            {
				Error("添加法术'{0}'失败，错误信息如下:", spell.id);
				Error(e.Message);
				Error(e.Source);
            }
        }
    }
}