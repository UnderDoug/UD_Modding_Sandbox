using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL;
using XRL.Language;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

namespace UD_Modding_Sandbox.Garresh
{
    [HasWishCommand]
    public static class FoodEffectFinder
    {
        [WishCommand(Command = "gimme the food effects")]
        public static void FoodEffectFinder_WishHandler()
        {
            GameObject campfireObject = null;
            try
            {
                List<string> domains = new();
                foreach (GameObjectBlueprint ingredientMapping in GameObjectFactory.Factory.GetBlueprintsInheritingFrom("IngredientMapping"))
                {
                    string[] pieces = ingredientMapping.Name.Split("_");
                    if (pieces.Length > 1 && !pieces[1].Contains("random"))
                    {
                        domains.Add(pieces[1]);
                        UnityEngine.Debug.LogError($"{pieces[1]}");
                        Loading.SetLoadingStatus($"Adding domain {pieces[1]}...");
                    }
                }
                if (domains.IsNullOrEmpty())
                {
                    return;
                }
                List<List<string>> ingredientsLists = new();
                for (int i = 0; i < domains.Count; i++)
                {
                    for (int j = 0; j < domains.Count; j++)
                    {
                        for (int k = 0; k < domains.Count; k++)
                        {
                            Loading.SetLoadingStatus($"Compiling ingredient domains {domains[i]}, {domains[j]}, {domains[k]}...");
                            if (domains[i] == domains[j] && domains[j] ==  domains[k])
                            {
                                continue;
                            }
                            ingredientsLists.Add(new() { domains[i], domains[j], domains[k] });
                        }
                    }
                }
                campfireObject = GameObjectFactory.Factory.CreateSampleObject("Campfire");
                if (campfireObject is not null
                    && campfireObject.TryGetPart(out Campfire campfire))
                {
                    Dictionary<ProceduralCookingEffect, List<string>> effectsIngredientsDict = new();

                    foreach (List<string> ingredientsList in ingredientsLists)
                    {
                        foreach (ProceduralCookingEffect effect in campfire.GenerateEffectsFromTypeList(ingredientsList, 10))
                        {
                            if (effectsIngredientsDict.Keys.IsNullOrEmpty() 
                                || !effectsIngredientsDict.Keys.Any(pce => pce == null || pce.SameAs(effect)))
                            {
                                Loading.SetLoadingStatus($"Generating effects from {ingredientsList[0]}, {ingredientsList[1]}, {ingredientsList[2]}...");
                                effectsIngredientsDict.Add(effect, ingredientsList);
                            }
                        }
                    }
                    if (!effectsIngredientsDict.IsNullOrEmpty())
                    {
                        foreach ((ProceduralCookingEffect effect, List<string> ingredients) in effectsIngredientsDict)
                        {
                            string ingredientsOutput = null;
                            string effectsOutput = null;
                            foreach (string ingredient in ingredients)
                            {
                                if (!ingredientsOutput.IsNullOrEmpty())
                                {
                                    ingredientsOutput += ",";
                                }
                                ingredientsOutput += ingredient;
                            }
                            if (effect.units != null && effect.units.Count > 0)
                            {
                                foreach (ProceduralCookingEffectUnit effectUnit in effect.units)
                                {
                                    if (!effectsOutput.IsNullOrEmpty())
                                    {
                                        effectsOutput += ",";
                                    }
                                    effectsOutput += effectUnit?.GetType()?.Name;
                                }
                            }
                            UnityEngine.Debug.LogError($"Domains [{ingredientsOutput ?? "empty"}]; EffectUnits: [{effectsOutput ?? "empty"}]");
                            Loading.SetLoadingStatus($"Outputting: {$"Domains [{ingredientsOutput ?? "empty"}]; EffectUnits: [{effectsOutput ?? "empty"}]"}...");
                        }
                    }
                }
            }
            finally
            {
                if (GameObject.Validate(ref campfireObject))
                {
                    campfireObject.Obliterate();
                }
                Loading.SetLoadingStatus(null);
            }
        }
    }
}
