using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EffigyMaker.Core
{
    /// <summary>
    /// Loads the given file from the vpk or the file system
    /// </summary>
    public class KvFileLoader
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="vpkLoader">The VpkLoader</param>
        public KvFileLoader(BasicVpkFileLoader vpkLoader)
        {
            VpkLoader = vpkLoader;
        }

        /// <summary>
        /// The loader for vpk files
        /// </summary>
        public BasicVpkFileLoader VpkLoader { get; }

        /// <summary>
        /// Convert some kv text to json text
        /// This implementation taken from my other project: https://github.com/mdiller/dotabase-builder
        /// </summary>
        /// <param name="text">The text to convert</param>
        /// <returns>The json text</returns>
        public string KvStringToJson(string text)
        {
            // To temporarily hide non-functional quotes
            text = Regex.Replace(text, @"\\""", "TEMP_QUOTE_TOKEN");
            // remove the null hex char at the end of some files
            text = Regex.Replace(text, @"\x00$", "");
            // get rid of troublesome comments
            text = Regex.Replace(text, @"\s//[^\n]+", "");

            // To convert Valve"s KeyValue format to Json
            text = Regex.Replace(text, "﻿", ""); // remove zero width no-brece

            text = Regex.Replace(text, @"""([^""]*)""(\s*){", @"""$1"": {");
            text = Regex.Replace(text, @"""([^""]*)""\s*""([^""]*)""", @"""$1"": ""$2"",");
            text = Regex.Replace(text, @",(\s*[}\]])", @"$1");
            text = Regex.Replace(text, @"([}\]])(\s*)(""[^""]*"":\s*)?([{\[])", @"$1,$2$3$4");
            text = Regex.Replace(text, @"}(\s*""[^""]*"":)", @"},$1");
            if (!Regex.IsMatch(text, @"^\s*\{"))
                text = "{ " + text + " }";
            // To re-include non-functional quotes (commented out here cuz the C# not working for it)
            //text = Regex.Replace(text, @"TEMP_QUOTE_TOKEN", @"\\""");

            return text;
        }

        /// <summary>
        /// Loads the kv file as a jobject
        /// </summary>
        /// <param name="path">The vpk-based path of the file</param>
        /// <returns>The loaded kvalues as json</returns>
        public JObject LoadFile(string path)
        {
            var text = VpkLoader.LoadFileText(path);

            text = KvStringToJson(text);

            return JObject.Parse(text, new JsonLoadSettings { 
                CommentHandling = CommentHandling.Ignore,
            });
        }

    }
}
