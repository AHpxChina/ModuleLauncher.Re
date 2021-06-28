﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Downloader;
using ModuleLauncher.Re.Authenticators;
using ModuleLauncher.Re.Downloaders;
using ModuleLauncher.Re.Launcher;
using ModuleLauncher.Re.Locators;
using ModuleLauncher.Re.Locators.Concretes;
using ModuleLauncher.Re.Locators.Dependencies;
using ModuleLauncher.Re.Models.Downloaders;
using ModuleLauncher.Re.Models.Downloaders.Minecraft;
using ModuleLauncher.Re.Utils;
using ModuleLauncher.Re.Utils.Extensions;

namespace ModuleLauncher.Test
{
    class Program
    {
        //java executable file: 
        static async Task Main(string[] args)
        {
            const string root = @"/home/ahpx/.minecraft";
            const string ver = "fabric-loader-0.11.6-1.15.2";

            try
            {
                var minecraftDownloader = new MinecraftDownloader(root);
            
                minecraftDownloader.DownloadStarted += DownloadStarted;
                minecraftDownloader.DownloadCompleted += DownloadCompleted;
                minecraftDownloader.DownloadProgressChanged += DownloadProgressChanged;

                await minecraftDownloader.Download(ver);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            var librariesLocator = new LibrariesLocator(root);
            var assetsLocator = new AssetsLocator(root);

            var dependencies = await librariesLocator.GetDependencies(ver);

            dependencies = dependencies.Union(await assetsLocator.GetDependencies(ver));

            var dependenciesDownloader = new DependenciesDownloader(dependencies);
            
            dependenciesDownloader.DownloadStarted += DownloadStarted;
            dependenciesDownloader.DownloadCompleted += DownloadCompleted;
            dependenciesDownloader.DownloadProgressChanged += DownloadProgressChanged;

            await dependenciesDownloader.Download(32);
            
            var launcher = new Launcher(root)
            {
                Java = "/home/ahpx/.minecraft/runtime/jre-legacy/linux/jre-legacy/bin/java",
                Authentication = "AHpx"
            };

            var process = await launcher.Launch(ver);

            while (await process.StandardOutput.ReadLineAsync() != null)
            {
                Console.WriteLine(await process.StandardOutput.ReadLineAsync());
            }
        }

        private static void DownloadStarted(DownloadStartedEventArgs e)
        {
            Console.WriteLine($"{e.FileName} started to download!");
        }

        private static void DownloadCompleted(AsyncCompletedEventArgs e)
        {
            Console.WriteLine("Download accomplished!");
        }


        private static void DownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(
                $"Progress: {e.ProgressPercentage:F1}% - {e.ReceivedBytesSize / 1024 :F1}KB/{e.TotalBytesToReceive / 1024 :F1}KB");
        }
    }
}