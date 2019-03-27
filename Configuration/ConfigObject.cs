using System.Drawing;

namespace DGServerControllerGUI.Configuration
{
    public class ConfigObject
    {
        private static ConfigObject instance;
        public static ConfigObject Instance { get { return instance ?? (instance = new ConfigObject()); } }

        /// <summary>
        /// 最大的日志缓存数
        /// </summary>
        public int MaxLogCount { get; set; }
        /// <summary>
        /// 重启服务器时清除所有日志
        /// </summary>
        public bool ClearLogOnReboot { get; set; }
        public Color DebugForeColor { get; set; }
        public Color DebugBackColor { get; set; }
        public Color NoticForeColor { get; set; }
        public Color NoticBackColor { get; set; }
        public Color WarnForeColor { get; set; }
        public Color WarnBackColor { get; set; }
        public Color ErrorForeColor { get; set; }
        public Color ErrorBackColor { get; set; }
        public Color FatalForeColor { get; set; }
        public Color FatalBackColor { get; set; }

        public ConfigObject()
        {
            MaxLogCount = 5000;
            ClearLogOnReboot = true;

            DebugForeColor = Color.White; DebugBackColor = Color.Black;
            NoticForeColor = Color.White; NoticBackColor = Color.Green;
            WarnForeColor = Color.Orange; WarnBackColor = Color.Black;
            ErrorForeColor = Color.Red; ErrorBackColor = Color.White;
            FatalForeColor = Color.White; FatalBackColor = Color.Red;
        }
    }
}