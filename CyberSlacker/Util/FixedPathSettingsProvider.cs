using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Xml.Linq;

namespace CyberSlacker.Util
{
    /// <summary>
    /// 自定义设置存储提供者：强制将配置文件存放在固定位置，避开版本号子文件夹
    /// </summary>
    public class FixedPathSettingsProvider : SettingsProvider
    {
        // 🌟 这里的路径你可以随便改，它是永恒不变的
        private static readonly string _configFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CyberSlacker");

        private static readonly string _configFilePath = Path.Combine(_configFolder, "user.config");

        public override string ApplicationName { get; set; } = "CyberSlacker";

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(ApplicationName, config);
        }

        // 读取设置
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            var values = new SettingsPropertyValueCollection();
            XDocument doc = null;

            if (File.Exists(_configFilePath))
            {
                try { doc = XDocument.Load(_configFilePath); } catch { }
            }

            foreach (SettingsProperty setting in collection)
            {
                var value = new SettingsPropertyValue(setting);
                if (doc != null)
                {
                    var node = doc.Root.Element(setting.Name);
                    if (node != null) value.SerializedValue = node.Value;
                }
                values.Add(value);
            }
            return values;
        }

        // 写入设置
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            if (!Directory.Exists(_configFolder)) Directory.CreateDirectory(_configFolder);

            XElement root = new XElement("Settings");
            foreach (SettingsPropertyValue value in collection)
            {
                if (value.IsDirty) // 只保存修改过的值
                {
                    root.Add(new XElement(value.Name, value.SerializedValue));
                }
            }
            root.Save(_configFilePath);
        }
    }
}