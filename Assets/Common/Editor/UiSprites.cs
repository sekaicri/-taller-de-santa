using System.IO;
using UnityEditor;
using UnityEngine;

namespace VrClass.Tools
{
    /// <summary>
    /// Cuts the Christmas UI sheet (one PNG with transparent background) into single sprites.
    /// Each piece is saved as its own PNG and imported as a Sprite, with 9-slice borders where it needs them.
    /// </summary>
    public static class UiSprites
    {
        const string SHEET_PATH = "Assets/Common/UI/UI_Navidad.png";
        const string FOLDER = "Assets/Common/UI/Sprites";

        // Name, position and size on the sheet (pixels, measured from the top-left corner), 9-slice border (left, bottom, right, top).
        struct Piece
        {
            public readonly string Name;
            public readonly int X, Y, W, H;
            public readonly Vector4 Border;
            public Piece(string name, int x, int y, int w, int h, Vector4 border) { Name = name; X = x; Y = y; W = w; H = h; Border = border; }
        }

        static readonly Vector4 NONE = Vector4.zero;
        static readonly Vector4 PILL = new Vector4(60f, 40f, 60f, 40f);

        static readonly Piece[] PIECES =
        {
            new Piece("Marco", 21, 14, 618, 425, NONE),
            new Piece("Cinta", 657, 22, 578, 163, NONE),
            new Piece("Regalo", 820, 181, 263, 268, NONE),
            new Piece("Pergamino", 19, 460, 594, 218, NONE),
            new Piece("Placa", 662, 473, 571, 110, new Vector4(60f, 30f, 60f, 30f)),
            new Piece("BarraFondo", 651, 610, 570, 45, new Vector4(25f, 15f, 25f, 15f)),
            new Piece("BarraRelleno", 651, 673, 570, 46, new Vector4(25f, 15f, 25f, 15f)),
            new Piece("BotonRojo", 29, 732, 423, 123, PILL),
            new Piece("BotonAzul", 483, 738, 244, 117, PILL),
            new Piece("BotonNaranja", 751, 738, 228, 117, PILL),
            new Piece("BotonRedondo", 1045, 730, 138, 141, NONE),
            new Piece("Ayuda", 14, 875, 507, 332, NONE),
            new Piece("CasillaVacia", 543, 897, 100, 99, NONE),
            new Piece("CasillaMarcada", 665, 897, 102, 99, NONE),
            new Piece("Bola", 543, 1032, 148, 180, NONE),
            new Piece("Baston", 711, 1015, 145, 204, NONE),
            new Piece("Osito", 880, 1007, 184, 224, NONE),
            new Piece("Estrella", 1074, 1015, 166, 162, NONE),
        };

        static bool _warned;

        // Cuts the sheet by itself every time scripts reload, as long as the sheet is in the project.
        [InitializeOnLoadMethod]
        static void AutoCut()
        {
            EditorApplication.delayCall += () => { if (File.Exists(SHEET_PATH)) Prepare(); };
        }

        // Also cuts it right after the sheet PNG is imported.
        public static void OnSheetImported()
        {
            EditorApplication.delayCall += Prepare;
        }

        // Returns the sprite with that name, cutting the sheet first if needed. Null when the sheet is missing.
        public static Sprite Get(string name)
        {
            Prepare();
            return AssetDatabase.LoadAssetAtPath<Sprite>(PathOf(name));
        }

        static string PathOf(string name)
        {
            return FOLDER + "/" + name + ".png";
        }

        // Cuts every missing piece out of the sheet and sets its import options.
        static void Prepare()
        {
            if (!File.Exists(SHEET_PATH))
            {
                if (!_warned) Debug.LogWarning("UI sheet not found at " + SHEET_PATH + ": the sign will use plain colors. Download UI_Navidad.png first.");
                _warned = true;
                return;
            }

            var missing = false;
            foreach (var piece in PIECES)
                if (!File.Exists(PathOf(piece.Name))) missing = true;

            if (missing)
            {
                if (!AssetDatabase.IsValidFolder(FOLDER)) AssetDatabase.CreateFolder("Assets/Common/UI", "Sprites");

                // LoadImage reads the PNG file directly, so the sheet does not need to be readable in its import settings.
                var sheet = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                sheet.LoadImage(File.ReadAllBytes(SHEET_PATH));

                foreach (var piece in PIECES)
                {
                    if (File.Exists(PathOf(piece.Name))) continue;
                    var cut = new Texture2D(piece.W, piece.H, TextureFormat.RGBA32, false);
                    // Unity counts rows from the bottom, the measures above count from the top.
                    cut.SetPixels(sheet.GetPixels(piece.X, sheet.height - piece.Y - piece.H, piece.W, piece.H));
                    cut.Apply();
                    File.WriteAllBytes(PathOf(piece.Name), cut.EncodeToPNG());
                    Object.DestroyImmediate(cut);
                }
                Object.DestroyImmediate(sheet);
                AssetDatabase.Refresh();
                Debug.Log("UI sprites cut into " + FOLDER);
            }

            foreach (var piece in PIECES) SetupImport(piece);
        }

        static void SetupImport(Piece piece)
        {
            var importer = AssetImporter.GetAtPath(PathOf(piece.Name)) as TextureImporter;
            if (importer == null) return;
            if (importer.textureType == TextureImporterType.Sprite && importer.spriteBorder == piece.Border) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = piece.Border;
            importer.SaveAndReimport();
        }
    }

    // Tells UiSprites when the sheet PNG has just been imported.
    class UiSheetWatcher : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (var path in imported)
                if (path.EndsWith("UI_Navidad.png")) { UiSprites.OnSheetImported(); return; }
        }
    }
}
