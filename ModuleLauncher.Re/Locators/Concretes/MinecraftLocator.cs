﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModuleLauncher.Re.Models.Locators.Minecraft;
using ModuleLauncher.Re.Utils.Extensions;

namespace ModuleLauncher.Re.Locators.Concretes
{
    public class MinecraftLocator
    {
        private readonly LocalityLocator _locator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="locator">A string can be implicitly converted to LocalityLocator</param>
        public MinecraftLocator(LocalityLocator locator)
        {
            _locator = locator;
        }

        /// <summary>
        /// Get local minecraft entities
        /// </summary>
        /// <returns>
        /// If there are some values that are not included in the json file,
        /// the corresponding property value is null
        /// </returns>
        public IEnumerable<Minecraft> GetLocalMinecrafts()
        {
            var re = new List<Minecraft>();
            var versions = _locator.GetLocalVersions();
            
            foreach (var version in versions)
            {
                if (!version.Json.Exists)
                {
                    throw new IOException($"No json file exist for {version.Version.Name}!");
                }
                
                var json = version.Json.ReadAllText();
                var entity = json.ToJsonEntity<MinecraftJson>();

                var mc = new Minecraft
                {
                    Locality = version,
                    Raw = entity
                };
                
                re.Add(mc);
            }

            return re;
        }

        /// <summary>
        /// Get single local minecraft via specified minecraft id
        /// </summary>
        /// <param name="id">e.g. 1.8.9</param>
        /// <returns></returns>
        public Minecraft GetLocalMinecraft(string id)
        {
            try
            {
                var minecrafts = GetLocalMinecrafts();

                return minecrafts.First(x => x.Raw.Id == id);
            }
            catch (Exception e)
            {
                throw new ArgumentException("The specified Minecraft was not found!", e);
            }
        }
    }
}