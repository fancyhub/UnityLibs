using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FH.DayNightWeather
{
    /// <summary>
    /// E_RENDER_SLOT.light
    /// </summary>
    public enum ERenderSlotLight
    {
        main_light_rot, //旋转,四元数
        main_light_color, // MainLightColor 结构
        point_light_active, //点光源, bool
    }

    /// <summary>
    /// 后期相关的
    /// </summary>
    public enum ERenderSlotPP
    {
        profiler,
        bloom,  //RenderBloomCfg
    }

    /// <summary>
    /// E_RENDER_SLOT.env
    /// </summary>
    public enum ERenderSlotEnv
    {
        main,   //RenderEnvCfg
        fog,   //RenderFogCfg
        global_controller,
    }

    [System.Serializable]
    public struct MainLightColor : IRDSStruct<MainLightColor>
    {
        public Color _color;
        public float _intensity;
        public float _indirect_multipiler;

        public bool IsDataEuqal(ref MainLightColor other)
        {
            if (!RDSUtil.IsEuqal(_color, other._color)) return false;
            if (!RDSUtil.IsEuqal(_intensity, other._intensity)) return false;
            if (!RDSUtil.IsEuqal(_indirect_multipiler, other._indirect_multipiler)) return false;
            return true;
        }

        public void LerpData(ref MainLightColor from, ref MainLightColor to, float t)
        {
            _color = RDSUtil.Lerp(from._color, to._color, t);
            _intensity = RDSUtil.Lerp(from._intensity, to._intensity, t);
            _indirect_multipiler = RDSUtil.Lerp(from._indirect_multipiler, to._indirect_multipiler, t);
        }

        public void ReadFromRealLight()
        {
            Light main_light = GetMainLight();
            if (main_light == null)
            {
                Log.I("找不到 影响Default层的 主光源");
                return;
            }
            _color = main_light.color;
            _intensity = main_light.intensity;
            _indirect_multipiler = main_light.bounceIntensity;
        }

        public void ReadFromGlobalData()
        {
            if (!Application.isPlaying)
            {
                Log.I("当前不是处于运行模式");
                return;
            }

            if (RenderDataMgr.Inst.CurData.Get(ERenderSlot.light, ERenderSlotLight.main_light_color, false, out MainLightColor c))
            {
                _color = c._color;
                _indirect_multipiler = c._indirect_multipiler;
                _intensity = c._intensity;
            }
        }

        public static Light GetMainLight()
        {
            for (int i = 0; i < 32; i++)
            {
                Light[] lights = Light.GetLights(LightType.Directional, i);
                if (lights == null || lights.Length == 0)
                    continue;

                for (int j = 0; j < lights.Length; j++)
                {
                    if ((lights[j].cullingMask & 1) != 0)
                    {
                        return lights[j];
                    }
                }
            }
            return null;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////


    [System.Serializable]
    public struct RenderBloomCfg : IRDSStruct<RenderBloomCfg>
    {
        public bool active;

        //[Sirenix.OdinInspector.MinValue(0)]
        [Range(0, 2)]
        public float threshold;

        [Range(0, 10)]
        //[Sirenix.OdinInspector.MinValue(0)]
        public float intensity;

        [Range(0, 1)]
        public float scatter;

        //[Sirenix.OdinInspector.MinValue(0)]
        [Min(0)]
        public float clamp;

        [ColorUsage(false, false)]
        public Color tint;

        public bool highQualityFiltering;

        [Range(0, 16)]
        public int skipIterations;

        public Texture dirtTexture;

        //[Sirenix.OdinInspector.MinValue(0)]
        public float dirtIntensity;

        public bool IsDataEuqal(ref RenderBloomCfg other)
        {
            if (!RDSUtil.IsEuqal(active, other.active)) return false;
            if (!RDSUtil.IsEuqal(threshold, other.threshold)) return false;
            if (!RDSUtil.IsEuqal(intensity, other.intensity)) return false;
            if (!RDSUtil.IsEuqal(scatter, other.scatter)) return false;
            if (!RDSUtil.IsEuqal(clamp, other.clamp)) return false;
            if (!RDSUtil.IsEuqal(tint, other.tint)) return false;
            if (!RDSUtil.IsEuqal(highQualityFiltering, other.highQualityFiltering)) return false;
            if (!RDSUtil.IsEuqal(skipIterations, other.skipIterations)) return false;
            if (!RDSUtil.IsEuqal(dirtTexture, other.dirtTexture)) return false;
            if (!RDSUtil.IsEuqal(dirtIntensity, other.dirtIntensity)) return false;
            return true;
        }

        public void LerpData(ref RenderBloomCfg from, ref RenderBloomCfg to, float t)
        {
            active = RDSUtil.Lerp(from.active, to.active, t);
            threshold = RDSUtil.Lerp(from.threshold, to.threshold, t);
            intensity = RDSUtil.Lerp(from.intensity, to.intensity, t);
            scatter = RDSUtil.Lerp(from.scatter, to.scatter, t);
            clamp = RDSUtil.Lerp(from.clamp, to.clamp, t);
            tint = RDSUtil.Lerp(from.tint, to.tint, t);
            highQualityFiltering = RDSUtil.Lerp(from.highQualityFiltering, to.highQualityFiltering, t);
            skipIterations = RDSUtil.Lerp(from.skipIterations, to.skipIterations, t);
            dirtTexture = RDSUtil.LerpHalf(from.dirtTexture, to.dirtTexture, t);
            dirtIntensity = RDSUtil.Lerp(from.dirtIntensity, to.dirtIntensity, t);
        }

        public static RenderBloomCfg Creaet()
        {
            return new RenderBloomCfg()
            {
                active = true,
                threshold = 0.9f,
                intensity = 0,
                scatter = 0.7f,
                clamp = 65472f,
                tint = Color.white,
                highQualityFiltering = false,
                skipIterations = 1,
                dirtIntensity = 0,
            };
        }


        public void ApplyTo(Bloom inst)
        {
            inst.active = active;
            inst.threshold.Override(threshold);
            inst.intensity.Override(intensity);
            inst.scatter.Override(scatter);
            inst.clamp.Override(clamp);
            inst.tint.Override(tint);
            inst.highQualityFiltering.Override(highQualityFiltering);
            inst.skipIterations.Override(skipIterations);
            inst.dirtTexture.Override(dirtTexture);
            inst.dirtIntensity.Override(dirtIntensity);
        }


        //[Sirenix.OdinInspector.Button]
        public void ReadFromEnv()
        {
            Volume v = GetMainVolume();
            if (v == null)
            {
                Log.I("找不到影响 Default Layer的 Volume");
                return;
            }

            Bloom inst_p = null;
            v.sharedProfile.TryGet(out Bloom shared_p);
            if (v.HasInstantiatedProfile())
                v.profile.TryGet(out inst_p);

            //复制 shared            
            if (inst_p == null)
            {
                active = shared_p.active;
                threshold = shared_p.threshold.value;
                intensity = shared_p.intensity.value;
                scatter = shared_p.scatter.value;
                clamp = shared_p.clamp.value;
                tint = shared_p.tint.value;
                highQualityFiltering = shared_p.highQualityFiltering.value;
                skipIterations = shared_p.skipIterations.value;
                dirtTexture = shared_p.dirtTexture.value;
                dirtIntensity = shared_p.dirtIntensity.value;
            }
            else
            {
                active = inst_p.active;
                threshold = inst_p.threshold.overrideState ? inst_p.threshold.value : shared_p.threshold.value;
                intensity = inst_p.intensity.overrideState ? inst_p.intensity.value : shared_p.intensity.value;
                scatter = inst_p.scatter.overrideState ? inst_p.scatter.value : shared_p.scatter.value;
                clamp = inst_p.clamp.overrideState ? inst_p.clamp.value : shared_p.clamp.value;
                tint = inst_p.tint.overrideState ? inst_p.tint.value : shared_p.tint.value;
                highQualityFiltering = inst_p.highQualityFiltering.overrideState ? inst_p.highQualityFiltering.value : shared_p.highQualityFiltering.value;
                skipIterations = inst_p.skipIterations.overrideState ? inst_p.skipIterations.value : shared_p.skipIterations.value;
                dirtTexture = inst_p.dirtTexture.overrideState ? inst_p.dirtTexture.value : shared_p.dirtTexture.value;
                dirtIntensity = inst_p.dirtIntensity.overrideState ? inst_p.dirtIntensity.value : shared_p.dirtIntensity.value;
            }
        }

        public Volume GetMainVolume()
        {
            Volume[] volumes = VolumeManager.instance.GetVolumes(1);
            if (volumes == null || volumes.Length == 0)
                return null;

            Volume ret = null;
            for (int i = 0; i < volumes.Length; i++)
            {
                var t = volumes[i];
                if (!t.isGlobal || t.sharedProfile == null)
                    continue;

                if (ret == null)
                {
                    ret = t;
                    continue;
                }

                if (ret.priority < t.priority)
                    ret = t;
            }
            return ret;
        }
    }

    [System.Serializable]
    public struct RenderEnvCfg : IRDSStruct<RenderEnvCfg>
    {
        public Material skybox;
        public Color subtractiveShadowColor;

        public AmbientMode ambientMode;

        //Mode: SkyBox start
        public float ambientIntensity;
        //SkyBox end

        //Mode: Gradient Trilight start
        [ColorUsage(false, true)]
        public Color ambientSkyColor;
        [ColorUsage(false, true)]
        public Color ambientEquatorColor;
        [ColorUsage(false, true)]
        public Color ambientGroundColor;

        public DefaultReflectionMode defaultReflectionMode;
        public Cubemap customReflection;
        public float reflectionIntensity;
        public int reflectionBounces;
        public int defaultReflectionResolution;

        public bool IsDataEuqal(ref RenderEnvCfg other)
        {
            if (skybox != other.skybox) return false;
            if (ambientMode != other.ambientMode) return false;
            if (!RDSUtil.IsEuqal(ambientIntensity, other.ambientIntensity)) return false;
            if (!RDSUtil.IsEuqal(ambientSkyColor, other.ambientSkyColor)) return false;
            if (!RDSUtil.IsEuqal(ambientEquatorColor, other.ambientEquatorColor)) return false;
            if (!RDSUtil.IsEuqal(ambientGroundColor, other.ambientGroundColor)) return false;

            if (defaultReflectionMode != other.defaultReflectionMode) return false;
            if (!RDSUtil.IsEuqal(customReflection, other.customReflection)) return false;
            if (!RDSUtil.IsEuqal(reflectionIntensity, other.reflectionIntensity)) return false;
            if (!RDSUtil.IsEuqal(reflectionBounces, other.reflectionBounces)) return false;
            if (!RDSUtil.IsEuqal(defaultReflectionResolution, other.defaultReflectionResolution)) return false;
            return true;
        }

        public void LerpData(ref RenderEnvCfg from, ref RenderEnvCfg to, float t)
        {
            skybox = RDSUtil.LerpHalf(from.skybox, to.skybox, t);
            ambientMode = RDSUtil.LerpHalf(from.ambientMode, to.ambientMode, t);
            ambientIntensity = RDSUtil.Lerp(from.ambientIntensity, to.ambientIntensity, t);
            ambientSkyColor = RDSUtil.Lerp(from.ambientSkyColor, to.ambientSkyColor, t);
            ambientEquatorColor = RDSUtil.Lerp(from.ambientEquatorColor, to.ambientEquatorColor, t);
            ambientGroundColor = RDSUtil.Lerp(from.ambientGroundColor, to.ambientGroundColor, t);

            defaultReflectionMode = RDSUtil.LerpHalf(from.defaultReflectionMode, to.defaultReflectionMode, t);
            customReflection = RDSUtil.LerpHalf(from.customReflection, to.customReflection, t);
            reflectionIntensity = RDSUtil.Lerp(from.reflectionIntensity, to.reflectionIntensity, t);
            reflectionBounces = RDSUtil.Lerp(from.reflectionBounces, to.reflectionBounces, t);
            defaultReflectionResolution = RDSUtil.LerpHalf(from.defaultReflectionResolution, to.defaultReflectionResolution, t);

        }

        //Gradient Trilight end

        public void ReadFromEnv()
        {
            skybox = RenderSettings.skybox;
            subtractiveShadowColor = RenderSettings.subtractiveShadowColor;
            ambientMode = RenderSettings.ambientMode;
            ambientIntensity = RenderSettings.ambientIntensity;
            ambientSkyColor = RenderSettings.ambientSkyColor;
            ambientEquatorColor = RenderSettings.ambientEquatorColor;
            ambientGroundColor = RenderSettings.ambientGroundColor;

            defaultReflectionMode = RenderSettings.defaultReflectionMode;
            customReflection = RenderSettings.customReflection;
            reflectionIntensity = RenderSettings.reflectionIntensity;
            reflectionBounces = RenderSettings.reflectionBounces;
            defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
        }

        public void WriteToEnv()
        {
            RenderSettings.skybox = skybox;
            RenderSettings.subtractiveShadowColor = subtractiveShadowColor;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;

            RenderSettings.defaultReflectionMode = defaultReflectionMode;
            RenderSettings.customReflection = customReflection;
            RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.reflectionBounces = reflectionBounces;
            RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
        }
    }

    [System.Serializable]
    public struct RenderFogCfg : IRDSStruct<RenderFogCfg>
    {
        public bool fog;
        public FogMode fogMode;
        public Color fogColor;
        public float fogStartDistance;
        public float fogEndDistance;

        public bool IsDataEuqal(ref RenderFogCfg other)
        {
            if (fog != other.fog) return false;
            if (fogMode != other.fogMode) return false;
            if (!RDSUtil.IsEuqal(fogColor, other.fogColor)) return false;
            if (!RDSUtil.IsEuqal(fogStartDistance, other.fogStartDistance)) return false;
            if (!RDSUtil.IsEuqal(fogEndDistance, other.fogEndDistance)) return false;

            return true;
        }

        public void LerpData(ref RenderFogCfg from, ref RenderFogCfg to, float t)
        {
            fog = RDSUtil.Lerp(from.fog, to.fog, t);
            fogMode = RDSUtil.LerpHalf(from.fogMode, to.fogMode, t);
            fogColor = RDSUtil.Lerp(from.fogColor, to.fogColor, t);
            fogStartDistance = RDSUtil.Lerp(from.fogStartDistance, to.fogStartDistance, t);
            fogEndDistance = RDSUtil.Lerp(from.fogEndDistance, to.fogEndDistance, t);
        }

        public void ReadFromEnv()
        {
            fog = RenderSettings.fog;
            fogMode = RenderSettings.fogMode;
            fogColor = RenderSettings.fogColor;
            fogStartDistance = RenderSettings.fogStartDistance;
            fogEndDistance = RenderSettings.fogEndDistance;
        }


        public void WriteToEnv()
        {
            RenderSettings.fog = fog;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }
    }
}
