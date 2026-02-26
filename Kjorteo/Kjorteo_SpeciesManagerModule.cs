using System;
using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;

using Kjorteo.SpeciesManager;
using Kjorteo.SpeciesManager.Patches;

namespace XRL.CharacterBuilds.Qud
{
    public class Kjorteo_SpeciesManager_PlayerSpeciesModule : EmbarkBuilderModule<PlayerSpeciesData>
    {
        /// <summary>
        /// Do not include the information from this module in build codes.
        /// </summary>
        public override bool IncludeInBuildCodes()
            => true;

        /// <summary>
        /// This module should automatically be enabled.
        /// </summary>
        public override bool shouldBeEnabled()
            => true;

        public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
        {
            if (id == QudGameBootModule.BOOTEVENT_GAMESTARTING
                && (Kjorteo_SpeciesManagerPatches.EmbarkSpeciesData ?? info.getData<PlayerSpeciesData>()) is var speciesData
                && The.Player?.RequirePart<Kjorteo_SpeciesManager>() is var speciesManager)
            {
                try
                {
                    speciesManager.Species = speciesData.TechnicalSpecies;
                    speciesManager.DisplaySpecies = speciesData.DisplaySpecies;

                }
                catch (Exception x)
                {
                    MetricsManager.LogInfo("Error setting final species values: " + x);
                }
            }
            return base.handleBootEvent(id, game, info, element);
        }
    }
}