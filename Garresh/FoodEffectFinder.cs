using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.Effects;

namespace UD_Modding_Sandbox.Garresh
{
    [HasWishCommand]
    public static class FoodEffectFinder
    {
        public const string WISH_COMMAND = "gimme the food effects";

        [WishCommand(Command = WISH_COMMAND)]
        public static void GimmeTheFoodEffects_WishHandler()
        {
            GimmeTheFoodEffectsFor_WishHandler();
        }
        [WishCommand(Command = WISH_COMMAND)]
        public static void GimmeTheFoodEffectsFor_WishHandler(string For = null)
        {
            try
            {
                int currentStep = 0;
                List<string> domains = new();
                string domainsListOutput = "";
                foreach (GameObjectBlueprint ingredientMapping in GameObjectFactory.Factory.GetBlueprintsInheritingFrom("IngredientMapping"))
                {
                    string[] pieces = ingredientMapping.Name.Split("_");
                    if (pieces.Length > 1 && !pieces[1].Contains("random"))
                    {
                        Loading.SetLoadingStatus($"Step ({currentStep++}): Adding domain {pieces[1]}...");
                        domains.Add(pieces[1]);
                        if (!domainsListOutput.IsNullOrEmpty())
                        {
                            domainsListOutput += "\n";
                        }
                        domainsListOutput += pieces[1];
                    }
                }
                if (domains.IsNullOrEmpty())
                {
                    return;
                }
                List<string> fors = new();
                if (!For.IsNullOrEmpty())
                {
                    if (For.Contains(", "))
                    {
                        fors = For.Split(", ").ToList();
                    }
                    else
                    if (For.Contains(","))
                    {
                        fors = For.Split(",").ToList();
                    }
                    else
                    if (For.Contains(" "))
                    {
                        fors = For.Split(" ").ToList();
                    }
                    else
                    {
                        fors.Add(For);
                    }
                    if (!domains.Any(s => fors.Contains(s)))
                    {
                        domainsListOutput = "";
                        foreach (string domain in domains)
                        {
                            if (!domainsListOutput.IsNullOrEmpty())
                            {
                                domainsListOutput += "\n";
                            }
                            domainsListOutput += domain;
                        }
                        Popup.Show($"\"{For}\" not in domains list:\n{domainsListOutput}");
                        UnityEngine.Debug.LogError($"Domains:\n{domainsListOutput}");
                        return;
                    }
                }
                List<List<string>> ingredientsLists = new();
                for (int i = 0; i < domains.Count; i++)
                {
                    for (int j = i; j < domains.Count; j++)
                    {
                        for (int k = j; k < domains.Count; k++)
                        {
                            Loading.SetLoadingStatus($"Step ({currentStep++}): Compiling ingredient domains {domains[i]}, {domains[j]}, {domains[k]}...");
                            if (i == j && j == k)
                            {
                                continue;
                            }
                            if (!fors.IsNullOrEmpty() && !fors.Contains(domains[i]) && !fors.Contains(domains[j]) && !fors.Contains(domains[k]))
                            {
                                continue;
                            }
                            ingredientsLists.Add(new() { domains[i], domains[j], domains[k] });
                        }
                    }
                }
                Dictionary<ProceduralCookingEffect, List<string>> effectsDomainsDict = new();

                foreach (List<string> ingredientsList in ingredientsLists)
                {
                    ProceduralCookingEffect cookingEffect = ProceduralCookingEffect.CreateJustUnits(ingredientsList);
                    if (effectsDomainsDict.Keys.IsNullOrEmpty()
                        || !effectsDomainsDict.Keys.Any(e => e == null || e.SameAs(cookingEffect)))
                    {
                        Loading.SetLoadingStatus($"Step ({currentStep++}): Generating effects from {ingredientsList[0]}, {ingredientsList[1]}, {ingredientsList[2]}...");
                        effectsDomainsDict.Add(cookingEffect, ingredientsList);
                    }
                }
                if (!effectsDomainsDict.IsNullOrEmpty())
                {
                    int outputCounter = 0;
                    int outputCounterPadding = effectsDomainsDict.Count.ToString().Length;
                    UnityEngine.Debug.LogError($"count,dom1,dom2,dom3,unit1,unit2,unit3");
                    foreach ((ProceduralCookingEffect effect, List<string> ingredients) in effectsDomainsDict)
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
                        string output = $"{outputCounter++},{ingredientsOutput ?? ",,"},{effectsOutput ?? ",,"}";
                        UnityEngine.Debug.LogError(output);
                        Loading.SetLoadingStatus($"Step ({currentStep++}): Outputting: [{output}]...");
                    }
                }
            }
            finally
            {
                Loading.SetLoadingStatus(null);
            }
        }
    }
}
