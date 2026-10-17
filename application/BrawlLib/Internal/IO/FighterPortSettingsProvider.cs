// FighterPort Studio addition, 2026-10-17. Licensed under the repository LGPL-3.0 terms.
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;

namespace BrawlLib.Internal.IO
{
    /// <summary>
    /// Independent portable settings. The framework still serializes each setting according to its
    /// declared type; this provider only stores that representation. It never imports upstream profiles.
    /// </summary>
    public sealed class FighterPortSettingsProvider : SettingsProvider, IApplicationSettingsProvider
    {
        private string _directory;
        private string _mutexName;
        public override string ApplicationName { get; set; } = "FighterPort Studio";

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name ?? nameof(FighterPortSettingsProvider), config ?? new NameValueCollection());
            _directory = SourceWriteGuard.NormalizePath(Path.Combine(FighterPortPaths.DataRoot, "Settings"));
            using (SHA256 hash = SHA256.Create())
                _mutexName = "Local\\FighterPortStudio.Settings." + BitConverter.ToString(
                    hash.ComputeHash(Encoding.UTF8.GetBytes(_directory.ToUpperInvariant()))).Replace("-", "");
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context,
            SettingsPropertyCollection properties)
        {
            return WithLock(() =>
            {
                XmlDocument document = ReadDocument();
                XmlElement group = FindGroup(document, context, false);
                var result = new SettingsPropertyValueCollection();
                foreach (SettingsProperty property in properties)
                {
                    bool userScoped = IsUserScoped(property);
                    // Do not quietly fall back to defaults when a saved value cannot be decoded.
                    property.ThrowOnErrorDeserializing = true;
                    property.ThrowOnErrorSerializing = true;
                    var value = new SettingsPropertyValue(property);
                    XmlElement stored = userScoped ? FindSetting(group, property.Name) : null;
                    if (stored != null)
                    {
                        string kind = stored.GetAttribute("kind");
                        if (kind == "string") value.SerializedValue = stored.InnerText;
                        else if (kind == "binary") value.SerializedValue = Convert.FromBase64String(stored.InnerText);
                        else if (kind == "null") value.PropertyValue = null;
                        else throw new ConfigurationErrorsException("Unknown saved setting representation: " + property.Name);
                    }
                    value.IsDirty = false;
                    result.Add(value);
                }
                return result;
            });
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values)
        {
            WithLock(() =>
            {
                XmlDocument document = ReadDocument();
                XmlElement group = FindGroup(document, context, true);
                bool changed = false;
                foreach (SettingsPropertyValue value in values)
                {
                    if (!IsUserScoped(value.Property) || !value.IsDirty) continue;
                    value.Property.ThrowOnErrorSerializing = true;
                    object serialized = value.SerializedValue;
                    XmlElement stored = FindSetting(group, value.Name);
                    if (stored == null)
                    {
                        stored = document.CreateElement("setting");
                        stored.SetAttribute("name", value.Name);
                        group.AppendChild(stored);
                    }
                    if (serialized == null)
                    {
                        stored.SetAttribute("kind", "null");
                        stored.InnerText = string.Empty;
                    }
                    else if (serialized is byte[] bytes)
                    {
                        stored.SetAttribute("kind", "binary");
                        stored.InnerText = Convert.ToBase64String(bytes);
                    }
                    else if (serialized is string text)
                    {
                        stored.SetAttribute("kind", "string");
                        stored.InnerText = text;
                    }
                    else throw new ConfigurationErrorsException("Unsupported serialized setting: " + value.Name);
                    changed = true;
                }
                if (changed) WriteDocument(document);
                foreach (SettingsPropertyValue value in values) value.IsDirty = false;
                return true;
            });
        }

        public void Reset(SettingsContext context)
        {
            WithLock(() =>
            {
                XmlDocument document = ReadDocument();
                XmlElement group = FindGroup(document, context, false);
                if (group != null)
                {
                    group.ParentNode.RemoveChild(group);
                    WriteDocument(document);
                }
                return true;
            });
        }

        // The stable product store survives FighterPort version changes. No BrawlCrate migration occurs.
        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties) { }
        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property) => null;

        private static bool IsUserScoped(SettingsProperty property)
        {
            bool user = property.Attributes[typeof(UserScopedSettingAttribute)] != null;
            bool application = property.Attributes[typeof(ApplicationScopedSettingAttribute)] != null;
            if (user == application)
                throw new ConfigurationErrorsException("Setting must have exactly one scope: " + property.Name);
            return user;
        }

        private T WithLock<T>(Func<T> action)
        {
            using (var mutex = new Mutex(false, _mutexName))
            {
                bool acquired = false;
                try
                {
                    try { acquired = mutex.WaitOne(TimeSpan.FromSeconds(10)); }
                    catch (AbandonedMutexException) { acquired = true; }
                    if (!acquired) throw new IOException("FighterPort settings are busy in another process.");
                    return action();
                }
                finally { if (acquired) mutex.ReleaseMutex(); }
            }
        }

        private XmlDocument ReadDocument()
        {
            string path = Path.Combine(_directory, "settings.xml");
            // A process interruption between the two renames leaves the complete previous snapshot.
            if (!File.Exists(path) && File.Exists(path + ".previous")) path += ".previous";
            var document = new XmlDocument { XmlResolver = null };
            if (!File.Exists(path))
            {
                document.AppendChild(document.CreateElement("fighterportSettings"));
                document.DocumentElement.SetAttribute("version", "1");
                return document;
            }
            SourceWriteGuard.EnsureNoReparsePoints(path);
            if (new FileInfo(path).Length > 16 * 1024 * 1024)
                throw new ConfigurationErrorsException("FighterPort settings exceed the 16 MiB limit: " + path);
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(path, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 16 * 1024 * 1024
            })) document.Load(reader);
            if (document.DocumentElement?.Name != "fighterportSettings" ||
                document.DocumentElement.GetAttribute("version") != "1")
                throw new ConfigurationErrorsException("Unsupported FighterPort settings document: " + path);
            return document;
        }

        private void WriteDocument(XmlDocument document)
        {
            string path = Path.Combine(_directory, "settings.xml");
            string previous = path + ".previous";
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            SourceWriteGuard.ThrowIfProtected(_directory);
            SourceWriteGuard.ThrowIfProtected(path);
            SourceWriteGuard.ThrowIfProtected(previous);
            SourceWriteGuard.ThrowIfProtected(temporary);
            Directory.CreateDirectory(_directory);
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings
                    { Encoding = new UTF8Encoding(false), Indent = true, CloseOutput = false,
                        NewLineHandling = NewLineHandling.Entitize }))
                        document.Save(writer);
                    if (stream.Length > 16 * 1024 * 1024)
                        throw new ConfigurationErrorsException("FighterPort settings exceed the 16 MiB limit.");
                    stream.Flush(true);
                }
                if (File.Exists(path))
                {
                    if (File.Exists(previous)) File.Delete(previous);
                    File.Move(path, previous);
                }
                try { File.Move(temporary, path); }
                catch
                {
                    if (!File.Exists(path) && File.Exists(previous)) File.Move(previous, path);
                    throw;
                }
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        private static XmlElement FindGroup(XmlDocument document, SettingsContext context, bool create)
        {
            string name = Convert.ToString(context["GroupName"]);
            string key = Convert.ToString(context["SettingsKey"]);
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
                if (node is XmlElement group && group.Name == "group" &&
                    group.GetAttribute("name") == name && group.GetAttribute("key") == key) return group;
            if (!create) return null;
            XmlElement result = document.CreateElement("group");
            result.SetAttribute("name", name);
            result.SetAttribute("key", key);
            document.DocumentElement.AppendChild(result);
            return result;
        }

        private static XmlElement FindSetting(XmlElement group, string name)
        {
            if (group != null)
                foreach (XmlNode node in group.ChildNodes)
                    if (node is XmlElement setting && setting.Name == "setting" &&
                        setting.GetAttribute("name") == name) return setting;
            return null;
        }
    }
}
