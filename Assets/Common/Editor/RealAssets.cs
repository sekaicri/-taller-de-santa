using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace VrClass.Tools
{
    /// <summary>
    /// Cambia los cubos y esferas de las escenas por los modelos reales importados de la Asset Store
    /// (pack "Christmas Presents": Present, Christmas-Ball, Star, Teddybear, Candy-Cane) y arregla sus
    /// materiales para URP (si no, se ven rosados). Si el pack no esta, deja las primitivas y avisa.
    /// </summary>
    public static class RealAssets
    {
        const string URL_PRESENTS = "https://assetstore.unity.com/packages?q=christmas%20presents";
        const string MODEL_SUFFIX = " (modelo real)";
        const string LOG_PREFIX = "[Assets reales] ";
        const string URP_LIT = "Universal Render Pipeline/Lit";
        const string MATERIALS_FOLDER = "Assets/Common/Materials";
        const float UNIFORM_TOLERANCE = 0.001f;
        const float TINT_STRENGTH = 0.5f;

        // Carpetas que NUNCA se consideran paquetes de la Asset Store.
        static readonly string[] IGNORED_PATHS = { "Packages/", "Assets/Samples/", "Assets/TextMesh Pro/", "Assets/XR/", "Assets/XRI/" };

        // Cada regla: que objeto de la escena se cambia y que prefab (nombre exacto) lo reemplaza.
        class Rule
        {
            public string Label;
            public Func<string, bool> Matches;
            public string PrefabName;
            public bool Tint;
        }

        static readonly Rule[] RULES =
        {
            new Rule { Label = "Regalos", Matches = n => n.StartsWith("Regalo", StringComparison.Ordinal), PrefabName = "Present", Tint = true },
            new Rule { Label = "Bola (juguete)", Matches = n => n.StartsWith("Bola", StringComparison.Ordinal), PrefabName = "Christmas-Ball", Tint = false },
            new Rule { Label = "Osito (juguete)", Matches = n => n.StartsWith("Osito", StringComparison.Ordinal), PrefabName = "Teddybear", Tint = false },
            new Rule { Label = "Baston (juguete)", Matches = n => n.StartsWith("Baston", StringComparison.Ordinal), PrefabName = "Candy-Cane", Tint = false },
            new Rule { Label = "Campana (estrella)", Matches = n => n.StartsWith("Campana", StringComparison.Ordinal), PrefabName = "Star", Tint = false },
        };

        [MenuItem("VR Class/Assets reales/Aplicar modelos reales a la escena actual")]
        public static void UpgradeMenu()
        {
            var count = UpgradeActiveScene();
            if (count > 0) Debug.Log(LOG_PREFIX + count + " objeto(s) cambiados. Guarda la escena con Ctrl+S.");
        }

        [MenuItem("VR Class/Assets reales/Convertir materiales de Christmas-Presents a URP")]
        public static void ConvertPackMaterials()
        {
            var count = 0;
            var urp = Shader.Find(URP_LIT);
            if (urp == null) { Debug.LogWarning(LOG_PREFIX + "No se encontro el shader URP/Lit: revisa que el proyecto use URP."); return; }

            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/Christmas-Presents" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (material == null || IsUrpCompatible(material.shader)) continue;

                // Se leen textura y color ANTES de cambiar el shader, porque el shader viejo ya no es valido.
                var (texture, color) = ReadSavedProperties(material);
                material.shader = urp;
                if (texture != null) material.SetTexture("_BaseMap", texture);
                material.SetColor("_BaseColor", color);
                material.SetFloat("_Smoothness", 0.2f);
                EditorUtility.SetDirty(material);
                count++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log(LOG_PREFIX + count + " material(es) de Christmas-Presents convertidos a URP.");
        }

        /// <summary>
        /// Recorre la escena activa y cambia el aspecto de los objetos conocidos por su modelo real.
        /// Nunca lanza excepciones: si algo falla, deja la escena con las primitivas.
        /// </summary>
        public static int UpgradeActiveScene()
        {
            var changed = 0;
            try
            {
                var scene = SceneManager.GetActiveScene();
                var targets = new List<(GameObject Object, Rule Rule)>();
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    {
                        var rule = RULES.FirstOrDefault(r => r.Matches(t.name));
                        if (rule != null && t.GetComponent<MeshRenderer>() != null && !HasModel(t))
                            targets.Add((t.gameObject, rule));
                    }

                var missing = new HashSet<string>();
                foreach (var target in targets)
                {
                    var prefab = FindPrefab(target.Rule.PrefabName);
                    if (prefab == null) { missing.Add(target.Rule.Label + " (prefab '" + target.Rule.PrefabName + "')"); continue; }
                    if (Apply(target.Object, prefab, target.Rule.Tint)) changed++;
                }

                foreach (var line in missing)
                    Debug.Log(LOG_PREFIX + "Sin modelo real para " + line + ": se deja la primitiva. Importa el pack 'Christmas Presents' desde Package Manager > My Assets. " + URL_PRESENTS);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(LOG_PREFIX + "No se pudo aplicar modelos reales, se dejan las primitivas: " + exception.Message);
            }
            return changed;
        }

        /// <summary>
        /// Coloca un modelo decorativo (sin fisica) apoyado en una posicion, con la altura indicada.
        /// Devuelve null si el prefab no esta importado.
        /// </summary>
        public static GameObject Place(string prefabName, Vector3 basePosition, Vector3 euler, float height, Transform parent = null)
        {
            try
            {
                var prefab = FindPrefab(prefabName);
                if (prefab == null) return null;

                var instance = Object.Instantiate(prefab);
                instance.name = prefabName + MODEL_SUFFIX;
                StripPhysics(instance);
                ApplyUrpMaterials(instance, null);

                instance.transform.rotation = Quaternion.Euler(euler);
                var bounds = BoundsOf(instance);
                if (bounds.size.y < 1e-4f) { Object.DestroyImmediate(instance); return null; }
                instance.transform.localScale *= height / bounds.size.y;

                bounds = BoundsOf(instance);
                instance.transform.position += new Vector3(basePosition.x - bounds.center.x,
                                                           basePosition.y - bounds.min.y,
                                                           basePosition.z - bounds.center.z);
                if (parent != null) instance.transform.SetParent(parent, true);
                return instance;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(LOG_PREFIX + "No se pudo colocar '" + prefabName + "': " + exception.Message);
                return null;
            }
        }

        static bool HasModel(Transform t)
        {
            foreach (Transform child in t)
                if (child.name.EndsWith(MODEL_SUFFIX, StringComparison.Ordinal)) return true;
            return false;
        }

        // Busca el prefab por nombre exacto, prefiriendo la version URP y excluyendo carpetas del curso.
        static GameObject FindPrefab(string prefabName)
        {
            // Busca primero prefabs y, si no hay, modelos (.fbx) con ese nombre.
            var path = AssetDatabase.FindAssets(prefabName + " t:Prefab")
                .Concat(AssetDatabase.FindAssets(prefabName + " t:Model"))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/", StringComparison.Ordinal) && !IGNORED_PATHS.Any(i => p.StartsWith(i, StringComparison.Ordinal)))
                .Where(p => string.Equals(System.IO.Path.GetFileNameWithoutExtension(p), prefabName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.IndexOf("/URP/", StringComparison.OrdinalIgnoreCase) >= 0)
                .ThenBy(p => p.Length)
                .FirstOrDefault();
            return path == null ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // Pone el modelo real encima de la primitiva, conservando fisica e interaccion de la primitiva.
        static bool Apply(GameObject target, GameObject prefab, bool tint)
        {
            var renderer = target.GetComponent<MeshRenderer>();
            var targetBounds = renderer.bounds;
            var tintColor = tint ? ReadTint(renderer) : (Color?)null;

            var instance = Object.Instantiate(prefab);
            instance.name = target.name + MODEL_SUFFIX;
            StripPhysics(instance);

            var modelBounds = BoundsOf(instance);
            if (modelBounds.size.x < 1e-4f || modelBounds.size.y < 1e-4f || modelBounds.size.z < 1e-4f)
            {
                Object.DestroyImmediate(instance);
                return false;
            }

            // Regalo Rojo / Verde / Azul: textura repintada del pack (el tinte solo ensuciaba el rojo y azul originales).
            var giftVariant = tint ? GiftVariantOf(target.name) : null;
            if (giftVariant != null) tintColor = null;
            ApplyUrpMaterials(instance, tintColor.HasValue ? Color.Lerp(Color.white, tintColor.Value, TINT_STRENGTH) : (Color?)null, target.name);
            if (giftVariant != null) ApplyGiftMaterial(instance, giftVariant);

            // Escala uniforme para que el modelo quepa dentro de la primitiva.
            var factor = Mathf.Min(targetBounds.size.x / modelBounds.size.x,
                                   targetBounds.size.y / modelBounds.size.y,
                                   targetBounds.size.z / modelBounds.size.z);
            instance.transform.localScale = instance.transform.localScale * factor;

            // Centrado en X/Z y apoyado sobre la base de la primitiva.
            modelBounds = BoundsOf(instance);
            instance.transform.position += new Vector3(targetBounds.center.x - modelBounds.center.x,
                                                       targetBounds.min.y - modelBounds.min.y,
                                                       targetBounds.center.z - modelBounds.center.z);

            var scale = target.transform.lossyScale;
            var uniform = Mathf.Abs(scale.x - scale.y) < UNIFORM_TOLERANCE && Mathf.Abs(scale.y - scale.z) < UNIFORM_TOLERANCE;
            // Con escala no uniforme el modelo va al lado para no deformarse.
            instance.transform.SetParent(uniform ? target.transform : target.transform.parent, true);

            renderer.enabled = false;
            return true;
        }

        static Color ReadTint(Renderer renderer)
        {
            var material = renderer.sharedMaterial;
            if (material != null && material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
            if (material != null && material.HasProperty("_Color")) return material.GetColor("_Color");
            return Color.white;
        }

        static void StripPhysics(GameObject root)
        {
            foreach (var body in root.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(body);
            foreach (var collider in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
        }

        // Cambia los materiales del modelo por copias URP (los del pack son del pipeline antiguo y salen rosados).
        static void ApplyUrpMaterials(GameObject root, Color? tint, string tintKey = null)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var shared = r.sharedMaterials;
                for (var i = 0; i < shared.Length; i++)
                    shared[i] = ToUrp(shared[i], tint, tintKey);
                r.sharedMaterials = shared;
            }
        }

        static Material ToUrp(Material source, Color? tint, string tintKey)
        {
            if (source == null) return null;
            if (!tint.HasValue && IsUrpCompatible(source.shader)) return source;

            EnsureFolder(MATERIALS_FOLDER);
            var suffix = tint.HasValue && !string.IsNullOrEmpty(tintKey) ? "_" + Sanitize(tintKey) : string.Empty;
            var path = MATERIALS_FOLDER + "/" + Sanitize(source.name) + "_URP" + suffix + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            var shader = Shader.Find(URP_LIT);
            if (shader == null) return source;

            var (texture, color) = ReadSavedProperties(source);
            var material = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(path) };
            if (texture != null) material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", tint.HasValue ? color * tint.Value : color);
            material.SetFloat("_Smoothness", 0.25f);
            if (source.shader != null && source.shader.name.Contains(PACK_LIGHT_SHADER)) MakeGlow(material, texture);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        // The tree lights glow: the same picture as light.
        static void MakeGlow(Material material, Texture texture)
        {
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            material.SetColor("_EmissionColor", Color.white * 1.5f);
            if (texture != null) material.SetTexture("_EmissionMap", texture);
        }

        // ---- Regalos de colores: se repinta la textura del pack (azul con cinta roja) ----

        const string TEXTURES_FOLDER = "Assets/Common/Textures";
        const string PACK_TEXTURE_NAME = "Christmas-Presents-01";
        const float GIFT_STRIP_FRACTION = 0.20f; // los regalos ocupan la franja inferior de la textura
        const float GREEN_HUE = 0.36f, GOLD_HUE = 0.12f;

        [MenuItem("VR Class/Assets reales/Pintar regalos (rojo, verde y azul)")]
        public static void PaintGiftsMenu()
        {
            var made = 0;
            foreach (var variant in new[] { "Rojo", "Verde", "Azul" })
                if (GiftMaterial(variant) != null) made++;
            AssetDatabase.SaveAssets();
            Debug.Log(LOG_PREFIX + (made == 3
                ? "Listo: materiales Rojo, Verde y Azul en " + MATERIALS_FOLDER + ". Arrastra cada uno al regalo que le corresponde."
                : "No se pudieron crear los materiales: importa antes el pack FREE Christmas Presents / Low Poly."));
        }

        static string GiftVariantOf(string name)
        {
            if (name.IndexOf("Rojo", StringComparison.Ordinal) >= 0) return "Rojo";
            if (name.IndexOf("Verde", StringComparison.Ordinal) >= 0) return "Verde";
            if (name.IndexOf("Azul", StringComparison.Ordinal) >= 0) return "Azul";
            return null;
        }

        static void ApplyGiftMaterial(GameObject root, string variant)
        {
            var material = GiftMaterial(variant);
            if (material == null) return;
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var shared = r.sharedMaterials;
                for (var i = 0; i < shared.Length; i++) shared[i] = material;
                r.sharedMaterials = shared;
            }
        }

        // Crea (o actualiza) el material URP del regalo con su textura repintada.
        static Material GiftMaterial(string variant)
        {
            var texture = EnsureGiftTexture(variant);
            if (texture == null) return null;
            EnsureFolder(MATERIALS_FOLDER);
            var path = MATERIALS_FOLDER + "/" + PACK_TEXTURE_NAME + "_URP_Regalo_" + variant + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find(URP_LIT);
                if (shader == null) return null;
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Smoothness", 0.25f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Texture2D EnsureGiftTexture(string variant)
        {
            EnsureFolder(TEXTURES_FOLDER);
            var outPath = TEXTURES_FOLDER + "/Present_" + variant + ".png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(outPath);
            if (existing != null) return existing;

            string sourcePath = null;
            foreach (var guid in AssetDatabase.FindAssets(PACK_TEXTURE_NAME + " t:Texture2D"))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.StartsWith(TEXTURES_FOLDER, StringComparison.Ordinal)) continue;
                if (System.IO.Path.GetFileNameWithoutExtension(p) == PACK_TEXTURE_NAME) { sourcePath = p; break; }
            }
            if (sourcePath == null) { Debug.LogWarning(LOG_PREFIX + "No encuentro la textura " + PACK_TEXTURE_NAME + ": importa el pack FREE Christmas Presents / Low Poly."); return null; }

            // La textura del pack no es legible por defecto: se abre un momento sin compresion y se restaura.
            var importer = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            var wasReadable = importer.isReadable; var oldCompression = importer.textureCompression;
            importer.isReadable = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            var pixels = source.GetPixels32(); var width = source.width; var height = source.height;
            importer.isReadable = wasReadable; importer.textureCompression = oldCompression;
            importer.SaveAndReimport();

            if (variant != "Azul") PaintStrip(pixels, width, height, variant);

            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.SetPixels32(pixels); result.Apply();
            System.IO.File.WriteAllBytes(outPath, result.EncodeToPNG());
            Object.DestroyImmediate(result);
            AssetDatabase.ImportAsset(outPath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(outPath);
        }

        // Cambia el color del regalo (azul) y de su cinta (roja) solo en la franja de los regalos.
        static void PaintStrip(Color32[] pixels, int width, int height, string variant)
        {
            var rows = Mathf.RoundToInt(height * GIFT_STRIP_FRACTION);
            for (var y = 0; y < rows; y++)
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x; var c = pixels[index];
                Color.RGBToHSV(new Color32(c.r, c.g, c.b, 255), out var h, out var s, out var v);
                var body = h >= 0.52f && h <= 0.72f && s > 0.35f;
                var ribbon = (h < 0.05f || h > 0.94f) && s > 0.5f;
                float? newHue = null;
                if (body) newHue = variant == "Verde" ? GREEN_HUE : 0f;
                else if (ribbon && variant == "Rojo") newHue = GOLD_HUE;
                if (!newHue.HasValue) continue;
                var rgb = (Color32)Color.HSVToRGB(newHue.Value, s, v);
                pixels[index] = new Color32(rgb.r, rgb.g, rgb.b, c.a);
            }
        }

        const string PACK_LIGHT_SHADER = "M_ChristmasLight";   // the pack's tree-lights shader

        static bool IsUrpCompatible(Shader shader)
        {
            if (shader == null) return false;
            var name = shader.name;
            if (name.Contains(PACK_LIGHT_SHADER)) return false;   // a Built-in Shader Graph: it shows pink in URP
            return name.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal)
                || name.StartsWith("Shader Graphs/", StringComparison.Ordinal)
                || name.StartsWith("Polyart/", StringComparison.Ordinal)
                || name.StartsWith("TextMeshPro/", StringComparison.Ordinal);
        }

        // Lee textura y color guardados en el .mat aunque su shader original ya no sea valido.
        static (Texture, Color) ReadSavedProperties(Material material)
        {
            Texture texture = null;
            var color = Color.white;
            var so = new SerializedObject(material);

            var textures = so.FindProperty("m_SavedProperties.m_TexEnvs");
            if (textures != null)
                for (var i = 0; i < textures.arraySize; i++)
                {
                    var entry = textures.GetArrayElementAtIndex(i);
                    var key = entry.FindPropertyRelative("first").stringValue;
                    if (key != "_MainTex" && key != "_BaseMap" && key != "_ColorMap") continue;
                    var value = entry.FindPropertyRelative("second.m_Texture").objectReferenceValue as Texture;
                    if (value != null) { texture = value; break; }
                }

            var colors = so.FindProperty("m_SavedProperties.m_Colors");
            if (colors != null)
                for (var i = 0; i < colors.arraySize; i++)
                {
                    var entry = colors.GetArrayElementAtIndex(i);
                    var key = entry.FindPropertyRelative("first").stringValue;
                    if (key != "_Color" && key != "_BaseColor") continue;
                    color = entry.FindPropertyRelative("second").colorValue;
                    break;
                }

            return (texture, color);
        }

        static string Sanitize(string value)
        {
            foreach (var c in System.IO.Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value.Replace(' ', '_');
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        static Bounds BoundsOf(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.zero);
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }
    }
}
