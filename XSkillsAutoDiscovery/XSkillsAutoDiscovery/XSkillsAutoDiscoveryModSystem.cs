using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using XSkills;

namespace XSkillsAutoDiscovery;

public class XSkillsAutoDiscoveryModSystem : ModSystem
{
    private ICoreAPI _api;

    public override void AssetsFinalize(ICoreAPI api)
    {
        if (api.Side != EnumAppSide.Server) return;

        _api = api;


        AutomapWoodBlocks();
        AutomapSaplings();

        base.AssetsFinalize(api);
    }


    private void AutomapWoodBlocks()
    {
        var woodBlocks = _api.World.Blocks.Where(block =>
            block.Class == nameof(BlockLog) && block.Code.PathStartsWith("log-grown") &&
            block.BlockBehaviors.All(behavior => behavior is not XSkillsWoodBehavior));

        foreach (var block in woodBlocks)
        {
            var woodBehavior = new XSkillsWoodBehavior(block);
            woodBehavior.Initialize(JsonObject.FromJson("{\"xp\": 0.4}"));

            block.BlockBehaviors = block.BlockBehaviors.Append(woodBehavior).ToArray();
            _api.Logger.Debug($"Added {nameof(XSkillsWoodBehavior)} to {block.Code.Path}");
        }
    }

    private void AutomapSaplings()
    {
        var saplingBlocks = _api.World.Blocks.Where(block =>
            block.EntityClass == "Sapling" && block.Code.PathStartsWith("sapling") &&
            block.BlockBehaviors.All(behavior => behavior is not XSkillsSaplingBehavior));

        foreach (var block in saplingBlocks)
        {
            var behavior = new XSkillsSaplingBehavior(block);
            behavior.Initialize(JsonObject.FromJson("{}"));

            block.BlockBehaviors = block.BlockBehaviors.Append(behavior).ToArray();
            _api.Logger.Debug($"Added {nameof(XSkillsSaplingBehavior)} to {block.Code.Path}");
        }
    }
}