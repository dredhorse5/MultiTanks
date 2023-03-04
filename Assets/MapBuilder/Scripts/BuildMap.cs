using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MultiTanks
{

	public class BuildMap : MonoBehaviour
	{
		public TextAsset schema1x;
		public TextAsset schema3x;
		public GameObject spritePrefab;
		public string MapPath;
		//public Text infoPanelText;
		
		
		
		private string schema1xString;
		private string schema3xString;
		private bool xmlErrors;
		private bool lightsEnabled;
		private bool spritesEnabled;
		private List<GameObject> instancedSprites = new List<GameObject>();
		private List<Material> instancedMaterials = new List<Material>();
		private List<GameObject> instancedLights = new List<GameObject>();
		private System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
		private Dictionary<MeshMaterial, List<Matrix4x4>> props = new Dictionary<MeshMaterial, List<Matrix4x4>>();

		
		private void Start()
		{
			return;
			schema1xString = schema1x.text;
			schema3xString = schema3x.text;
			schema1x = null;
			schema3x = null;
			InfoPanelWrite(0, 0, 0);
		}

		private float ToFloat(string text)
		{
			return System.Convert.ToSingle(text, culture);
		}

		private void ShowErrorMessage(string text)
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

		private bool ValidateXml(System.Xml.XmlDocument xmlDoc, string schemaString)
		{
			using (System.Xml.XmlReader xmlReader =
			       System.Xml.XmlReader.Create(new System.IO.StringReader(schemaString)))
			{
				System.Xml.Schema.XmlSchema schema = System.Xml.Schema.XmlSchema.Read(xmlReader,
					new System.Xml.Schema.ValidationEventHandler(schemaValidationEventHandler));
				xmlErrors = false;
				xmlDoc.Schemas.Add(schema);
				xmlDoc.Validate(new System.Xml.Schema.ValidationEventHandler(ValidationEventHandler));
			}

			return !xmlErrors;
		}

		public void LoadMap()
		{
			CheckMap(MapPath);
		}
		public void CheckMap(string mapFile)
		{
			System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
			try
			{
				xmlDocument.Load(mapFile);
			}
			catch
			{
				ShowErrorMessage("Could not load file");
				return;
			}

			clear();
			buildMap(GeneratePropDict1X(xmlDocument));
			
			/*
			if (ValidateXml(xmlDocument, schema1xString))
			{
				clear();
				buildMap(GeneratePropDict1X(xmlDocument));
			}
			else if (ValidateXml(xmlDocument, schema3xString))
			{
				clear();
				buildMap(GeneratePropDict3X(xmlDocument));
			}
			else
			{
				ShowErrorMessage("Not a valid map type");
			}
			*/

			xmlDocument = null;
		}

		private void clear()
		{
			if (instancedMaterials != null)
			{
				foreach (Material obj in instancedMaterials)
				{
					Object.Destroy(obj);
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

		private Dictionary<string, List<PropEntry>> GeneratePropDict1X(System.Xml.XmlDocument mapXml)
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

		private Dictionary<string, List<PropEntry>> GeneratePropDict3X(
			System.Xml.XmlDocument mapXml)
		{
			System.Xml.XmlElement documentElement = mapXml.DocumentElement;
			Dictionary<string, List<PropEntry>> dictionary =
				new Dictionary<string, List<PropEntry>>();
			using (System.Xml.XmlNodeList xmlNodeList = documentElement.SelectNodes("mesh | sprite"))
			{
				using (System.Xml.XmlNodeList xmlNodeList2 = documentElement.SelectNodes("prop"))
				{
					foreach (object obj in xmlNodeList)
					{
						System.Xml.XmlNode xmlNode = (System.Xml.XmlNode) obj;
						System.Xml.XmlNode xmlNode2 = xmlNodeList2[
							(int) System.Convert.ToInt16(xmlNode.Attributes["prop-index"].Value, culture)];
						string texture;
						if (xmlNode.SelectSingleNode("texture-index") == null)
						{
							texture = "";
						}
						else
						{
							if (xmlNode.SelectSingleNode("texture-index")?.InnerText == "invisible")
							{
								continue;
							}

							string innerText = xmlNode.SelectSingleNode("texture-index")?.InnerText;
							using (System.Xml.XmlNodeList xmlNodeList3 = xmlNode2.SelectNodes("texture-name"))
							{
								texture = xmlNodeList3[(int) System.Convert.ToInt16(innerText, culture)]
									.InnerText;
							}
						}

						string value = xmlNode2.Attributes["library-name"].Value;
						string value2 = xmlNode2.Attributes["group-name"].Value;
						string value3 = xmlNode2.Attributes["name"].Value;
						System.Xml.XmlNode xmlNode3 = xmlNode.SelectSingleNode("position");
						Vector3 position = new Vector3(
							ToFloat(xmlNode3.Attributes["x"].Value) / 100f,
							ToFloat(xmlNode3.Attributes["z"].Value) / 100f,
							ToFloat(xmlNode3.Attributes["y"].Value) / 100f);
						float num;
						if (xmlNode.Name == "mesh")
						{
							num = ToFloat(xmlNode.SelectSingleNode("rotation-z")?.InnerText);
							num = num * -57.295776f + 180f;
						}
						else
						{
							num = 0f;
						}

						PropEntry item =
							new PropEntry(value2, value3, texture, position, num);
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

		private void buildMap(Dictionary<string, List<PropEntry>> propDict)
		{
			System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
			int num = 0;
			int num2 = 0;
			foreach (KeyValuePair<string, List<PropEntry>> keyValuePair in propDict)
			{
				AssetBundle assetBundle = loadAssetBundle(keyValuePair.Key.ToLower());
				if (!assetBundle)
					continue;
				Debug.Log(assetBundle.name + "is loaded...");
				
				

				System.IO.StringReader txtReader = new System.IO.StringReader((assetBundle.LoadAsset("library.xml") as TextAsset).text);
				xmlDocument.Load(txtReader);
				System.Xml.XmlElement documentElement = xmlDocument.DocumentElement;
				if (keyValuePair.Key == "PointLight")
				{
					using (List<PropEntry>.Enumerator enumerator2 =
					       keyValuePair.Value.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							PropEntry propEntry = enumerator2.Current;
							try
							{
								System.Xml.XmlNode lightNode =
									documentElement.SelectSingleNode("color[@texture='" + propEntry.texture + "']");
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
				Dictionary<string, Material> dictionary =
					new Dictionary<string, Material>();
				foreach (PropEntry propEntry2 in keyValuePair.Value)
				{
					try
					{
						System.Xml.XmlNode firstChild = documentElement
							.SelectSingleNode("prop-group[@name='" + propEntry2.group + "']")
							.SelectSingleNode("prop[@name='" + propEntry2.name + "']").FirstChild;
						if (firstChild.Name == "mesh")
						{
							num2++;
							string text = firstChild.Attributes["file"].Value;
							int num3 = text.LastIndexOf(".");
							if (num3 > 0)
							{
								text = text.Remove(num3);
							}

							GameObject gameObject =
								assetBundle.LoadAsset(text + ".prefab") as GameObject;
							Mesh sharedMesh =
								gameObject.GetComponent<MeshFilter>().sharedMesh;
							Material material;
							if (propEntry2.texture != "")
							{
								string value = firstChild
									.SelectSingleNode("texture[@name='" + propEntry2.texture + "']")
									.Attributes["diffuse-map"].Value;
								if (dictionary.ContainsKey(value))
								{
									material = dictionary[value];
								}
								else
								{
									if (!assetBundle.Contains(value))
									{
										continue;
									}

									Texture mainTexture =
										assetBundle.LoadAsset(value) as Texture;
									material = new Material(gameObject
										.GetComponent<Renderer>().sharedMaterial);
									material.mainTexture = mainTexture;
									dictionary.Add(value, material);
									instancedMaterials.Add(material);
								}
							}
							else
							{
								material = gameObject.GetComponent<Renderer>().sharedMaterial;
							}

							MeshMaterial key =
								new MeshMaterial(sharedMesh, material);
							Matrix4x4 item =
								Matrix4x4.TRS(propEntry2.position,
									Quaternion.Euler(0f, propEntry2.zrotation, 0f),
									new Vector3(1f, 1f, 1f));
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
						}
						else if (firstChild.Name == "sprite")
						{
							string value2 = firstChild.Attributes["file"].Value;
							float num4 = ToFloat(firstChild.Attributes["scale"].Value) * 0.4f;
							GameObject gameObject2 =
								Object.Instantiate<GameObject>(
									spritePrefab, propEntry2.position,
									Quaternion.Euler(0f, 0f, 0f));
							gameObject2.transform.localScale = new Vector3(num4, num4, 1f);
							if (dictionary.ContainsKey(value2))
							{
								gameObject2.GetComponent<Renderer>().material =
									dictionary[value2];
							}
							else if (assetBundle.Contains(value2))
							{
								Texture mainTexture2 =
									assetBundle.LoadAsset(value2) as Texture;
								Material material2 =
									gameObject2.GetComponent<Renderer>().material;
								material2.mainTexture = mainTexture2;
								dictionary.Add(value2, material2);
								instancedMaterials.Add(material2);
							}

							if (!spritesEnabled)
							{
								gameObject2.SetActive(false);
							}

							instancedSprites.Add(gameObject2);
						}
					}
					catch
					{
					}
				}

				goto IL_454;
			}

			num++;


			xmlDocument = null;
			InfoPanelWrite(num2, instancedSprites.Count, instancedLights.Count);
			if (num > 0)
			{
				ShowErrorMessage(num + " libraries could not be loaded");
			}
		}

		private AssetBundle loadAssetBundle(string bundleName)
		{
			return AssetBundle.LoadFromFile(
				System.IO.Path.Combine(Application.streamingAssetsPath, bundleName));
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

		public void EnableLights(bool state)
		{
			foreach (GameObject gameObject in instancedLights)
			{
				gameObject.SetActive(state);
			}

			lightsEnabled = state;
		}

		public void EnableSprites(bool state)
		{
			foreach (GameObject gameObject in instancedSprites)
			{
				gameObject.SetActive(state);
			}

			spritesEnabled = state;
		}

		private void ValidationEventHandler(object sender, System.Xml.Schema.ValidationEventArgs e)
		{
			if (e.Severity == System.Xml.Schema.XmlSeverityType.Warning ||
			    e.Severity == System.Xml.Schema.XmlSeverityType.Error)
			{
				xmlErrors = true;
			}
		}

		private void schemaValidationEventHandler(object sender, System.Xml.Schema.ValidationEventArgs e)
		{
		}

		private void Update()
		{
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
			public PropEntry(string group, string name, string texture, Vector3 position,
				float zrotation)
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