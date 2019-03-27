using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DGServerControllerGUI
{
    public static class ImageLoader
    {
         static Dictionary<string, Image> cacheDict = new Dictionary<string, Image>();

        public static Image Load(string path)
        {
            if (!cacheDict.ContainsKey(path))
            {
                var all_path = string.Format("./Icon/{0}.png", path);
                if (!File.Exists(all_path))
                    return null;
                var img = Image.FromFile(all_path);
                cacheDict[path] = img;
            }
            return cacheDict[path];
        }
    }
}