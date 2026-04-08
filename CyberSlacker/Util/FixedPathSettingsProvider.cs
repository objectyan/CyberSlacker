using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Xml.Linq;

namespace CyberSlacker.Util
{
    public class FixedPathSettingsProvider : SettingsProvider
    {
        private static readonly string ConfigFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CyberSlacker");

        private static readonly string ConfigFilePath = Path.Combine(ConfigFolder, "user.config");

        public override string ApplicationName { get; set; } = "CyberSlacker";

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(ApplicationName, config);
        }

        // --- 1. 读取逻辑修正 ---
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            var values = new SettingsPropertyValueCollection();
            XDocument doc = null;

            if (File.Exists(ConfigFilePath))
            {
                try { doc = XDocument.Load(ConfigFilePath); } catch { }
            }

            foreach (SettingsProperty setting in collection)
            {
                var value = new SettingsPropertyValue(setting);
                var node = doc?.Root.Element(setting.Name);

                if (node != null)
                {
                    // 使用 TypeConverter 将字符串转回正确类型
                    var converter = TypeDescriptor.GetConverter(setting.PropertyType);
                    try
                    {
                        value.PropertyValue = converter.ConvertFromInvariantString(node.Value);
                    }
                    catch
                    {
                        // 如果转换失败（比如文件损坏），使用默认值
                        value.PropertyValue = DefaultValue(setting);
                    }
                }
                else
                {
                    value.PropertyValue = DefaultValue(setting);
                }

                value.IsDirty = false;
                values.Add(value);
            }
            return values;
        }

        // --- 2. 写入逻辑修正 ---
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            try
            {
                if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);

                XDocument doc;
                if (File.Exists(ConfigFilePath))
                {
                    try { doc = XDocument.Load(ConfigFilePath); }
                    catch { doc = new XDocument(new XElement("Settings")); }
                }
                else { doc = new XDocument(new XElement("Settings")); }

                foreach (SettingsPropertyValue value in collection)
                {
                    var element = doc.Root.Element(value.Name);
                    if (element == null)
                    {
                        element = new XElement(value.Name);
                        doc.Root.Add(element);
                    }

                    // 使用 TypeConverter 将值转为不随语言环境变化的字符串
                    var converter = TypeDescriptor.GetConverter(value.Property.PropertyType);
                    element.Value = converter.ConvertToInvariantString(value.PropertyValue);
                }

                doc.Save(ConfigFilePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Settings] 保存失败: {ex.Message}");
            }
        }

        // 辅助方法：处理默认值转换
        private object DefaultValue(SettingsProperty setting)
        {
            if (setting.DefaultValue == null) return null;
            var converter = TypeDescriptor.GetConverter(setting.PropertyType);
            return converter.ConvertFromInvariantString(setting.DefaultValue.ToString());
        }
    }
}