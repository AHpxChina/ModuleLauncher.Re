﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModuleLauncher.Re.Locators.Concretes;
using ModuleLauncher.Re.Models.Locators.Dependencies;
using ModuleLauncher.Re.Models.Locators.Minecraft;
using ModuleLauncher.Re.Utils;
using ModuleLauncher.Re.Utils.Extensions;
using Newtonsoft.Json.Linq;

namespace ModuleLauncher.Re.Locators.Dependencies
{
    public class AssetsLocator : IDependenciesLocator
    {
        private readonly MinecraftLocator _locator;

        public AssetsLocator(MinecraftLocator locator)
        {
            _locator = locator;
        }
        
        public static implicit operator AssetsLocator(string s)
        {
            return new AssetsLocator(s);
        }

        /// <summary>
        /// Get asset dependencies via local minecraft id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Dependency>> GetDependencies(string id)
        {
            var mc = await _locator.GetLocalMinecraft(id);

            return await GetDependencies(mc);
        }

        /// <summary>
        /// Get asset dependencies via minecraft entity
        /// </summary>
        /// <param name="mc"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IEnumerable<Dependency>> GetDependencies(Minecraft mc)
        {
            var re = new List<Dependency>();
            
            if (mc.Raw.AssetId.IsNullOrEmpty())
            {
                if (mc.Raw.InheritsFrom.IsNullOrEmpty())
                {
                    throw new Exception($"{mc.Raw.Id} has no any valid assets!");
                }

                mc = await _locator.GetLocalMinecraft(mc.Raw.InheritsFrom);
            }
            
            var assetIndex = mc.Locality.AssetsIndexes.GetSubFileInfo($"{mc.Raw.AssetId}.json");

            if (!assetIndex.Exists)
            {
                assetIndex.Directory?.Create();

                var response = await HttpUtility.Get(mc.Raw.AssetIndexUrl ??
                                                     throw new Exception($"{mc.Raw.Id} without any assets index!"));

                var content = await response.Content.ReadAsStringAsync();
                await assetIndex.WriteAllText(content);
            }

            var assetIndexContent = await assetIndex.ReadAllText();
            var objects = assetIndexContent.Fetch("objects");

            var hashTable = objects.ToObject<Hashtable>();
            
            foreach (DictionaryEntry entry in hashTable!)
            {
                var token = entry.Value.ToString().ToJObject();
                
                var hash = token.Fetch("hash");
                var url = $"{hash.Substring(0, 2)}/{hash}";

                var dependency = new Dependency
                {
                    Name = hash,
                    RelativeUrl = url,
                    File = mc.Locality.Assets.GetSubFileInfo(url.BuildPath())
                };
                
                re.Add(dependency);
            }

            return re;
        }
    }
}