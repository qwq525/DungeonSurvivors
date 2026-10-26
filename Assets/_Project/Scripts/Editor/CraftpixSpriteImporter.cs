using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DungeonSurvivors.EditorTools
{
    /// <summary>
    /// Craftpix 角色精灵表导入规则（本地素材，不进公开仓库）：
    /// - Slime / BaseMale：64×64 网格、点采样、无压缩、按行切片（行 0 = 图片顶部）；
    /// - Dungeon：单 Sprite、16 PPU。
    /// 面试可讲点：AssetPostprocessor 批量导入规范，避免手动拖 100+ 张图。
    /// </summary>
    public class CraftpixSpriteImporter : AssetPostprocessor
    {
        private const string Root = "Assets/ThirdParty/Craftpix/";
        private const int CharacterCell = 64;

        private void OnPreprocessTexture()
        {
            bool craftpix = assetPath.StartsWith("Assets/ThirdParty/Craftpix/");
            bool kenney = assetPath.StartsWith("Assets/ThirdParty/Kenney/");
            if (!craftpix && !kenney) return;

            TextureImporter importer = (TextureImporter)assetImporter;
            if (assetPath.Contains("/Classes/"))
            {
                ConfigureCharacterSheet(importer);
                return;
            }

            if (assetPath.Contains("/GreenDungeon/") || assetPath.Contains("/MinersCave/"))
            {
                ConfigureTileset(importer, 32);
                return;
            }

            if (kenney)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 16f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = 2048;
                return;
            }

            if (assetPath.Contains("/Slime/") || assetPath.Contains("/BaseMale/"))
            {
                ConfigureCharacterSheet(importer);
            }
            else if (assetPath.Contains("/Dungeon/"))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 16f;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = 2048;
            }
        }

        private void ConfigureTileset(TextureImporter importer, int cell)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = cell;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048;

            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            if (width <= 0 || height <= 0) return;
            int columns = width / cell;
            int rows = height / cell;
            if (columns <= 0 || rows <= 0) return;

            string sheetName = Path.GetFileNameWithoutExtension(assetPath);
            List<SpriteMetaData> metas = new List<SpriteMetaData>(columns * rows);
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    metas.Add(new SpriteMetaData
                    {
                        name = sheetName + "_r" + row + "_c" + column,
                        rect = new Rect(column * cell, height - (row + 1) * cell, cell, cell),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });
                }
            }

            importer.spritesheet = metas.ToArray();
        }

        private void ConfigureCharacterSheet(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = CharacterCell;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048;

            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            if (width <= 0 || height <= 0) return;

            int columns = width / CharacterCell;
            int rows = height / CharacterCell;
            if (columns <= 0 || rows <= 0) return;

            string sheetName = Path.GetFileNameWithoutExtension(assetPath);
            List<SpriteMetaData> metas = new List<SpriteMetaData>(columns * rows);
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    metas.Add(new SpriteMetaData
                    {
                        name = sheetName + "_r" + row + "_c" + column,
                        rect = new Rect(
                            column * CharacterCell,
                            height - (row + 1) * CharacterCell,
                            CharacterCell,
                            CharacterCell),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });
                }
            }

            importer.spritesheet = metas.ToArray();
        }
    }
}
