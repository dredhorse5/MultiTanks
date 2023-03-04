using System.Collections.Generic;
using System.IO;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace MultiTanks.MapBuilder
{
#if UNITY_EDITOR
    
    public class PrefabCreator : MonoBehaviour
    {
        public List<GameObject> ModelsInScene;
        public bool DisableChildren = true;
        public Transform FolderForPrefabs;

        [Button()]
        public void Create()
        {
            foreach (var model in ModelsInScene)
            {
                if(model)
                    CreatePrefab(model);
            }
        }

        private void CreatePrefab(GameObject ofModel)
        {
            if (DisableChildren)
            {
                var children = ofModel.transform.GetComponentsInChildren<Transform>().ToList();
                children.Remove(ofModel.transform);
                children.ForEach(_ => _.gameObject.SetActive(false));
            }

            var parent = new GameObject(ofModel.name);
            parent.transform.position = ofModel.transform.position;
            ofModel.transform.SetParent(parent.transform);
            parent.transform.SetParent(FolderForPrefabs);

            SavePrefab(parent.gameObject);
        }

        private void SavePrefab(GameObject gm)
        {
            
            //MapBuilder/Prefabs/MapParts
            string localPath = "Assets/MapBuilder/Prefabs/MapParts/" + gm.name + ".prefab";

            // Make sure the file name is unique, in case an existing Prefab has the same name.
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

            // Create the new Prefab and log whether Prefab was saved successfully.
            bool prefabSuccess;
            PrefabUtility.SaveAsPrefabAssetAndConnect(gm, localPath,InteractionMode.AutomatedAction, out prefabSuccess);
            if (prefabSuccess == true)
                Debug.Log("Prefab was saved successfully");
            else
                Debug.LogError("Prefab failed to save" + prefabSuccess);
        }
    }
    
#endif
}