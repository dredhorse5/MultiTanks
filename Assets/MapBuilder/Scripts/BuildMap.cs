#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MultiTanks
{
	public class BuildMap : MonoBehaviour
	{
		public string MapPath;
		public string MapName;
		[Space]
		public GameObject spritePrefab;
		public string SummerAssetsPath;
		public string WinterAssetsPath;
		public string MaterialsFullPath;
		[Space] 
		public string ReserveTexturePath;
		
		
		private List<GameObject> instancedSprites = new List<GameObject>();
		private List<GameObject> instancedMeshes = new List<GameObject>();
		private List<Material> instancedMaterials = new List<Material>();
		private List<GameObject> instancedLights = new List<GameObject>();

		private Transform Parent;
		

		
		[Button()]
		public void LoadMap()
		{
			LoadMap(MapPath + MapName + ".xml");
		}
		public void LoadMap(string mapFile)
		{
			System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
			xmlDocument.Load(mapFile);
			Clear();
			CreateMap(GeneratePropDict(xmlDocument));
		}
		[Button()]
		private void Clear()
		{
			if(Parent)
			{
				if(Application.isPlaying)
					Destroy(Parent.gameObject);
				else
					DestroyImmediate(Parent.gameObject);
			}
			Resources.UnloadUnusedAssets();
		}


		private void CreateMap(Dictionary<string, List<PropEntry>> propDict)
		{
			Parent = new GameObject(MapName).transform;
			System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
			int errors = 0;
			
			
			foreach (var keyValuePair in propDict)
			{
				string assetsPath = (keyValuePair.Key.Contains("Winter") ? WinterAssetsPath : SummerAssetsPath) + keyValuePair.Key.ToLower() + "/";
				var library = (TextAsset)Resources.Load(assetsPath + "library", typeof(TextAsset));
				if (library == null)
				{
					errors++;
					continue;
				}
				System.IO.StringReader txtReader = new System.IO.StringReader(library.text); 
				xmlDocument.Load(txtReader);
				System.Xml.XmlElement documentElement = xmlDocument.DocumentElement;
				
				
				if (keyValuePair.Key == "PointLight")
				{
					using (List<PropEntry>.Enumerator enumerator2 = keyValuePair.Value.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							PropEntry propEntry = enumerator2.Current;
							try
							{
								System.Xml.XmlNode lightNode = documentElement.SelectSingleNode("color[@texture='" + propEntry.texture + "']");
								CreateLight(lightNode, propEntry.position);
							}
							catch
							{
							}
						}
						continue;
					}
				}
				
				Dictionary<string, Material> dictionary = new Dictionary<string, Material>();
				foreach (PropEntry propEntry2 in keyValuePair.Value)
				{
					try
					{
						System.Xml.XmlNode firstChild = documentElement
							.SelectSingleNode("prop-group[@name='" + propEntry2.group + "']")
							.SelectSingleNode("prop[@name='" + propEntry2.name + "']").FirstChild;
						if (firstChild.Name == "mesh")
						{
							string text = firstChild.Attributes["file"].Value;
							int num3 = text.LastIndexOf(".");
							if (num3 > 0)
								text = text.Remove(num3);
							
							var meshPrefab = (GameObject) Resources.Load(assetsPath + text, typeof(GameObject));
							var spawnedMesh = PrefabUtility.InstantiatePrefab(meshPrefab) as GameObject;
							
							Material material = null;
							if (propEntry2.texture != "")
							{
								string value = firstChild
									.SelectSingleNode("texture[@name='" + propEntry2.texture + "']")
									.Attributes["diffuse-map"].Value;
								value = value.Replace(".jpg", "");

								Texture mainTexture = (Texture) Resources.Load(assetsPath + value, typeof(Texture));
								if (mainTexture == null)
									mainTexture = (Texture) Resources.Load(ReserveTexturePath + value, typeof(Texture));
								if (mainTexture == null)
								{
									Debug.LogError($"Cant find texture: {value})");
									continue;
								}


								string matPath =
									$"{MaterialsFullPath}{spawnedMesh.GetComponent<Renderer>().sharedMaterial.name}_{mainTexture.name}.mat";
								var loadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
								if (!loadedMaterial)
								{
									material = new Material(spawnedMesh.GetComponent<Renderer>().sharedMaterial);
									material.mainTexture = mainTexture;
									instancedMaterials.Add(material);
									AssetDatabase.CreateAsset(material, matPath);
									Debug.Log($"New asset: {material.name}_{mainTexture.name}.mat");
								}
								else
									material = loadedMaterial;

							}
							spawnedMesh.transform.SetParent(Parent);
							spawnedMesh.transform.position = propEntry2.position;
							spawnedMesh.transform.rotation = Quaternion.Euler(0f, propEntry2.zrotation, 0f);
							if(material != null)
								spawnedMesh.GetComponent<Renderer>().sharedMaterial = material;
							
							instancedMeshes.Add(spawnedMesh);
						}
						else if (firstChild.Name == "sprite")
						{
							
							
							
							string textureName = firstChild.Attributes["file"].Value;
							textureName = textureName.Replace(".png", "");
							float scale = ToFloat(firstChild.Attributes["scale"].Value) * 0.4f;
							var spawnedSprite__ = PrefabUtility.InstantiatePrefab(spritePrefab) as GameObject;
							//GameObject spawnedSprite = Instantiate(spritePrefab, propEntry2.position + Vector3.up * 2f, spritePrefab.transform.rotation);
							spawnedSprite__.transform.SetParent(Parent);
							spawnedSprite__.transform.position = propEntry2.position;
							spawnedSprite__.transform.localScale = new Vector3(scale, scale, scale);

							
							Texture mainTexture = (Texture) Resources.Load(assetsPath + textureName, typeof(Texture));
							if (mainTexture == null)
								mainTexture = (Texture) Resources.Load(ReserveTexturePath + textureName, typeof(Texture));
							if (mainTexture == null)
							{
								Debug.LogError($"Cant find texture: {textureName})");
								continue;
							}

							Material material2 = null;
							string matPath = $"{MaterialsFullPath}{spawnedSprite__.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.name}_{mainTexture.name}.mat";
							var loadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
							if (!loadedMaterial)
							{
								material2 = new Material(spawnedSprite__.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial);
								material2.mainTexture = mainTexture;
								instancedMaterials.Add(material2);
								AssetDatabase.CreateAsset(material2, matPath);
								Debug.Log($"New asset: {material2.name}_{mainTexture.name}.mat");
							}
							else
								material2 = loadedMaterial;

							spawnedSprite__.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = material2;
							//Texture mainTexture2 = (Texture) Resources.Load(assetsPath + textureName, typeof(Texture));
							//Material material2 = spawnedSprite.GetComponent<Renderer>().sharedMaterial;
							//material2.mainTexture = mainTexture2;
							//dictionary.Add(textureName, material2);
							//instancedMaterials.Add(material2);
							//instancedSprites.Add(spawnedSprite);
						}
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
				}
			}
			
			
			if (errors > 0)
				Debug.LogError(errors + " libraries could not be loaded");
		}

		
		
		private void CreateLight(System.Xml.XmlNode lightNode, Vector3 position)
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.Translate(position);
			Light light = gameObject.AddComponent<Light>();
			light.type = LightType.Point;
			light.color = new Color(ToFloat(lightNode.Attributes["r"].Value),
				ToFloat(lightNode.Attributes["g"].Value), ToFloat(lightNode.Attributes["b"].Value));
			light.intensity = ToFloat(lightNode.Attributes["intensity"].Value);
			light.range = ToFloat(lightNode.Attributes["range"].Value);
			light.shadows = LightShadows.Hard;
			light.shadowStrength = 0.9f;
			instancedLights.Add(gameObject);
		}
		private Dictionary<string, List<PropEntry>> GeneratePropDict(System.Xml.XmlDocument mapXml)
		{
			System.Xml.XmlNode xmlNode = mapXml.DocumentElement.SelectSingleNode("static-geometry[1]");
			Dictionary<string, List<PropEntry>> dictionary =
				new Dictionary<string, List<PropEntry>>();
			using (System.Xml.XmlNodeList xmlNodeList = xmlNode.SelectNodes("prop"))
			{
				foreach (object obj in xmlNodeList)
				{
					System.Xml.XmlNode xmlNode2 = (System.Xml.XmlNode) obj;
					string innerText = xmlNode2.SelectSingleNode("texture-name")?.InnerText;
					if (innerText != "invisible")
					{
						string value = xmlNode2.Attributes["library-name"].Value;
						string value2 = xmlNode2.Attributes["group-name"].Value;
						string value3 = xmlNode2.Attributes["name"].Value;
						System.Xml.XmlNode xmlNode3 = xmlNode2.SelectSingleNode("position");
						Vector3 position = new Vector3(
							ToFloat(xmlNode3.SelectSingleNode("x")?.InnerText) / 100f,
							ToFloat(xmlNode3.SelectSingleNode("z")?.InnerText) / 100f,
							ToFloat(xmlNode3.SelectSingleNode("y")?.InnerText) / 100f);
						float zrotation =
							ToFloat(xmlNode2.SelectSingleNode("rotation")?.SelectSingleNode("z")?.InnerText) *
							-57.295776f + 180f;
						PropEntry item =
							new PropEntry(value2, value3, innerText, position, zrotation);
						if (dictionary.ContainsKey(value))
						{
							dictionary[value].Add(item);
						}
						else
						{
							dictionary.Add(value, new List<PropEntry>
							{
								item
							});
						}
					}
				}
			}

			return dictionary;
		}
		private float ToFloat(string text) => System.Convert.ToSingle(text, new System.Globalization.CultureInfo("en-US"));
		
		public static void SaveObjectToFile(Object obj, string fileName)
		{
			AssetDatabase.CreateAsset(obj, fileName);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		private struct PropEntry
		{
			public PropEntry(string group, string name, string texture, Vector3 position, float zrotation)
			{
				this.group = group;
				this.name = name;
				this.texture = texture;
				this.position = position;
				this.zrotation = zrotation;
			}

			public string group;
			public string name;
			public string texture;
			public Vector3 position;
			public float zrotation;
		}
		private struct MeshMaterial
		{
			public MeshMaterial(Mesh mesh, Material material)
			{
				this.mesh = mesh;
				this.material = material;
			}

			public Mesh mesh;
			public Material material;
		}
	}
	

}
#endif