﻿using System;
using System.Collections.Generic;
using Android.Text;
using Java.Lang;
using Exception = System.Exception;

namespace PixelPhoto.NiceArt
{
    public class CustomEffect
    {
        public string MEffectName;
        public Dictionary<string, Java.Lang.Object> ParametersMap;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder">Define your custom effect using {@link Builder} class</param>
        public CustomEffect(Builder builder)
        {
            try
            {
                MEffectName = builder.MEffectName;
                ParametersMap = builder.ParametersMap;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
        }

        public string GetEffectName()
        {
            try
            {
                return MEffectName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;

            }
        }


        /// <summary>
        /// Get Parameters
        /// </summary>
        /// <returns>map of key and value of parameters for {@link android.media.effect.Effect#setParameter(String, Object)}</returns>
        public Dictionary<string, Java.Lang.Object> GetParameters()
        {
            try
            {
                return ParametersMap;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;

            }
        }


        /// <summary>
        /// Set customize effect to image using this builder class
        /// </summary>
        public class Builder
        {
            public string MEffectName;
            public Dictionary<string, Java.Lang.Object> ParametersMap = new Dictionary<string, Java.Lang.Object>();

            /// <summary>
            /// Initiate your custom effect
            /// </summary>
            /// <param name="effectName">custom effect name from {@link android.media.effect.EffectFactory#createEffect(String)}</param>
            public Builder(string effectName)
            {
                try
                {
                    if (TextUtils.IsEmpty(effectName))
                    {
                        throw new RuntimeException("Effect name cannot be empty.Please provide effect name from EffectFactory");
                    }
                    else
                    {
                        MEffectName = effectName;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                }
            }

            /// <summary>
            /// Set parameter to the attributes with its value
            /// </summary>
            /// <param name="paramKey">attribute key for {@link android.media.effect.Effect#setParameter(String, Object)}</param>
            /// <param name="paramValue">value for {@link android.media.effect.Effect#setParameter(String, Object)}</param>
            /// <returns>builder instance to setup multiple parameters</returns>
            public Builder SetParameter(string paramKey, Java.Lang.Object paramValue)
            {
                try
                {
                    ParametersMap.Add(paramKey, paramValue);
                    return this;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;

                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns>instance for custom effect</returns>
            public CustomEffect Build()
            {
                try
                {
                    return new CustomEffect(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;

                }
            }
        }
    }
}