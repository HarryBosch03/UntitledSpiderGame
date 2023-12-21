using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace UntitledSpiderGame.Runtime.Data
{
    public static class SaveFile
    {
        public static readonly SaveFile<GameSettings> Settings = SaveFile<GameSettings>.Xml("settings.xml");
    }

    public sealed class SaveFile<T> where T : class, new()
    {
        private T cache;
        private string filename;
        private Action<FileStream, T> serialize;
        private Func<FileStream, T> deserialize;

        public event System.Action<T> FileSavedEvent;

        public SaveFile(string filename, Action<FileStream, T> serialize, Func<FileStream, T> deserialize)
        {
            this.filename = Path.Combine(Application.dataPath, filename);
            this.serialize = serialize;
            this.deserialize = deserialize;
        }

        public T Get()
        {
            if (cache == null) Load();
            return cache;
        }

        public void Load()
        {
            if (!File.Exists(filename))
            {
                Save();
                return;
            }

            var filestream = new FileStream(filename, FileMode.Open);
            cache = deserialize(filestream);
            filestream.Close();
        }

        public void Save()
        {
            if (cache == null)
            {
                cache = new T();
            }
            
            var filestream = new FileStream(filename, FileMode.OpenOrCreate);
            serialize(filestream, cache);
            filestream.Close();
            
            FileSavedEvent?.Invoke(cache);
        }

        public static implicit operator T(SaveFile<T> saveFile) => saveFile.Get();

        public static SaveFile<T> Xml(string filename)
        {
            return new SaveFile<T>(filename,
                (dst, target) =>
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(dst, target);
                },
                src =>
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(src);
                });
        }
    }
}