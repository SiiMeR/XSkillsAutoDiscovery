using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using XSkills;

namespace XSkillsAutoDiscovery;

public class XSkillsAutoDiscoveryModSystem : ModSystem
{
    public override void AssetsFinalize(ICoreAPI api)
    {
        if (api.Side != EnumAppSide.Server) return;

        AutomapWoodBlocks(api);

        base.AssetsFinalize(api);
    }

    private void AutomapWoodBlocks(ICoreAPI api)
    {
         var woodBlocks = api.World.Blocks.Where(block =>
            block.Class == "BlockLog" && block.Code.PathStartsWith("log-grown") &&
            block.BlockBehaviors.All(behavior => behavior is not XSkillsWoodBehavior));

        // TODO: log-section

        foreach (var block in woodBlocks)
        {
            var woodBehavior = new XSkillsWoodBehavior(block);
            woodBehavior.Initialize(JsonObject.FromJson("{\"xp\": 0.4}"));

            block.BlockBehaviors = block.BlockBehaviors.Append(woodBehavior).ToArray();
            api.Logger.Debug($"Added XSkillsWoodBehavior to {block.Code.Path}");
        }
    }
}