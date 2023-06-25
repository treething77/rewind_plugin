using System.IO;
using aeric.rewind_plugin.RewindStorageDataTypes;
using UnityEngine;

namespace aeric.rewind_plugin {
    public partial class RewindStorage {
        public string writeToJson() {
            var storageData = convertToStorage();
            string jsonStr = JsonUtility.ToJson(storageData, true);
            return jsonStr;
        }
        
        public void loadFromJsonFile(string fullPath) {
            string jsonTxt = File.ReadAllText(fullPath);
            var storageData = JsonUtility.FromJson<RewindStorageData>(jsonTxt);
            loadFromStorage(storageData);
        }

        public void writeToJsonFile(string fileName) {
            string jsonStr = writeToJson();
            File.WriteAllText(fileName, jsonStr);
        }


    }
}