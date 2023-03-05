using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MultiTanks
{

	public class BuildMap : MonoBehaviour
	{
		public TextAsset schema1x;
		public TextAsset schema3x;
		public GameObject spritePrefab;
		public string MapPath;

		public string PrefabsPath;
		//public Text infoPanelText;
		
		
		
		private string schema1xString;
		private string schema3xString;
		private bool xmlErrors;
		private bool lightsEnabled;
		private bool spritesEnabled;
		private List<GameObject> instancedSprites = new List<GameObject>();
		private List<GameObject> instancedMeshes = new List<GameObject>();
		private List<Material> instancedMaterials = new List<Material>();
		private List<GameObject> instancedLights = new List<GameObject>();
		private System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
		private Dictionary<MeshMaterial, List<Matrix4x4>> props = new Dictionary<MeshMaterial, List<Matrix4x4>>();
		

		

		private void ErrorMessage(string text)
		{
			Debug.LogError(text);
		}

		private void InfoPanelWrite(int propCount, int spriteCount, int lightCount)
		{
			/*infoPanelText.text = string.Concat(new object[]
			{
				"Props: ",
				propCount,
				"\nSprites: ",
				spriteCount,
				"\nLights: ",
				lightCount
			});*/
		}

		public void LoadMap()
		{
			LoadMap(MapPath);
		}
		public void LoadMap(string mapFile)
		{
			System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
			xmlDocument.Load(mapFile);
			Clear();
			CreateMap(GeneratePropDict(xmlDocument));
			xmlDocument = null;
		}

		private void Clear()
		{
			if (instancedMeshes != null)
			{
				foreach (var mesh in instancedMeshes)
				{
					Destroy(mesh);
				}
				instancedMeshes.Clear();
			}
			if (instancedMaterials != null)
			{
				foreach (Material obj in instancedMaterials)
				{
					Destroy(obj);
				}

				instancedMaterials.Clear();
			}

			if (instancedLights != null)
			{
				foreach (GameObject obj2 in instancedLights)
				{
					Object.Destroy(obj2);
				}

				instancedLights.Clear();
			}

			if (instancedSprites != null)
			{
				foreach (GameObject obj3 in instancedSprites)
				{
					Object.Destroy(obj3);
				}

				instancedSprites.Clear();
			}

			if (props != null)
			{
				props.Clear();
			}

			Resources.UnloadUnusedAssets();
		}


		private void CreateMap(Dictionary<string, List<PropEntry>> propDict)
		{
			System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
			int errors = 0;
			foreach (KeyValuePair<string, List<PropEntry>> keyValuePair in propDict)
			{
				
				AssetBundle assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Application.streamingAssetsPath, keyValuePair.Key.ToLower()));
				if (!assetBundle)
				{
					errors++;
					continue;
				}
				
				

				System.IO.StringReader txtReader = new System.IO.StringReader((assetBundle.LoadAsset("library.xml") as TextAsset).text);
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

						goto IL_454;
					}

					goto IL_E8;
				}

				goto IL_E8;
				IL_454:
				assetBundle.Unload(false);
				continue;
				IL_E8:
				
				
				
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

							//GameObject gm_ = assetBundle.LoadAsset(text + ".prefab") as GameObject;
							var meshPrefab = (GameObject)Resources.Load(PrefabsPath + text, typeof(GameObject));
							Mesh sharedMesh = meshPrefab.GetComponent<MeshFilter>().sharedMesh;
							Material material;
							if (propEntry2.texture != "")
							{
								string value = firstChild
									.SelectSingleNode("texture[@name='" + propEntry2.texture + "']")
									.Attributes["diffuse-map"].Value;
								
								if (dictionary.ContainsKey(value))
									material = dictionary[value];
								else
								{
									if (!assetBundle.Contains(value))
										continue;

									Texture mainTexture = assetBundle.LoadAsset(value) as Texture;
									material = new Material(meshPrefab.GetComponent<Renderer>().sharedMaterial);
									material.mainTexture = mainTexture;
									dictionary.Add(value, material);
									instancedMaterials.Add(material);
								}
							}
							else
								material = meshPrefab.GetComponent<Renderer>().sharedMaterial;

							MeshMaterial key = new MeshMaterial(sharedMesh, material);
							Matrix4x4 item = Matrix4x4.TRS(propEntry2.position, Quaternion.Euler(0f, propEntry2.zrotation, 0f), new Vector3(1f, 1f, 1f));
							if (props.ContainsKey(key))
							{
								props[key].Add(item);
							}
							else
							{
								List<Matrix4x4>
									list = new List<Matrix4x4>();
								list.Add(item);
								props.Add(key, list);
							}

							var spawnedMesh = PrefabUtility.InstantiatePrefab(meshPrefab) as GameObject;
							spawnedMesh.transform.SetParent(transform);
							spawnedMesh.transform.position = item.GetPosition();
							spawnedMesh.transform.rotation = item.rotation;
							spawnedMesh.GetComponent<Renderer>().sharedMaterial = key.material;
							
							instancedMeshes.Add(spawnedMesh);
							//var instantiate = Instantiate(gm_, item.GetPosition(), item.rotation);
							//instantiate.GetComponent<Renderer>().sharedMaterial = key.material;
						}
						else if (firstChild.Name == "sprite")
						{
							string value2 = firstChild.Attributes["file"].Value;
							float scale = ToFloat(firstChild.Attributes["scale"].Value) * 0.4f;
							GameObject spawnedSprite = Instantiate(spritePrefab, propEntry2.position, Quaternion.identity);
							spawnedSprite.transform.SetParent(transform);
							spawnedSprite.transform.localScale = new Vector3(scale, scale, 1f);
							
							if (dictionary.ContainsKey(value2))
							{
								spawnedSprite.GetComponent<Renderer>().material = dictionary[value2];
							}
							else if (assetBundle.Contains(value2))
							{
								Texture mainTexture2 = assetBundle.LoadAsset(value2) as Texture;
								Material material2 = spawnedSprite.GetComponent<Renderer>().material;
								material2.mainTexture = mainTexture2;
								dictionary.Add(value2, material2);
								instancedMaterials.Add(material2);
							}
							instancedSprites.Add(spawnedSprite);
						}
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
				}

				goto IL_454;
			}


			xmlDocument = null;
			if (errors > 0)
				ErrorMessage(errors + " libraries could not be loaded");
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
			if (!lightsEnabled)
			{
				gameObject.SetActive(false);
			}
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
		private float ToFloat(string text) => System.Convert.ToSingle(text, culture);
		
		private void Update()
		{
			return;
			if (props != null)
			{
				foreach (KeyValuePair<MeshMaterial, List<Matrix4x4>> keyValuePair
				         in props)
				{
					int count = keyValuePair.Value.Count;
					if (count > 1023)
					{
						int num = count / 1023 + 1;
						for (int i = 0; i < num; i++)
						{
							int count2 = Mathf.Min(1023, count - 1023 * i);
							int index = i * 1023;
							List<Matrix4x4> list = new List<Matrix4x4>();
							list.AddRange(keyValuePair.Value.GetRange(index, count2));
							Graphics.DrawMeshInstanced(keyValuePair.Key.mesh, 0,
								keyValuePair.Key.material, list, null,
								UnityEngine.Rendering.ShadowCastingMode.TwoSided);
						}
					}
					else
					{
						Graphics.DrawMeshInstanced(keyValuePair.Key.mesh, 0,
							keyValuePair.Key.material, keyValuePair.Value, null,
							UnityEngine.Rendering.ShadowCastingMode.TwoSided);
					}
				}
			}
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