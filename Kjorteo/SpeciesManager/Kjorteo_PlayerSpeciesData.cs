using System;
using System.IO;
using System.IO.Compression;
using System.Text;

using Newtonsoft.Json;

using XRL.CharacterBuilds;

namespace Kjorteo.SpeciesManager
{
    public class PlayerSpeciesData : AbstractEmbarkBuilderModuleData
    {
        public static JsonSerializerSettings SERIALIZER_SETTINGS => CodeCompressor.SERIALIZER_SETTINGS;

        public string TechnicalSpecies;
        public string DisplaySpecies;

        public static string GenerateCode(PlayerSpeciesData PlayerSpeciesData)
            => CodeCompressor.Compress(JsonConvert.SerializeObject(
                value: PlayerSpeciesData,
                formatting: Formatting.Indented,
                settings: SERIALIZER_SETTINGS))
            ;
        public string GenerateCode()
            => GenerateCode(this);

        public static PlayerSpeciesData LoadCode(string Code)
            => JsonConvert.DeserializeObject<PlayerSpeciesData>(
                value: CodeCompressor.Decompress(Code),
                settings: SERIALIZER_SETTINGS)
            ;
    }
}