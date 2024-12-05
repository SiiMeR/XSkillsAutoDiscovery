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


        // forestry
        AutomapWoodBlocks();
        AutomapSaplings();
        AutomapTreeCuttings();
        AutomapLeaves();


        AutomapOres();
        AutomapGems();
        AutomapBombs();

        AutomapBerryBushes();

        base.AssetsFinalize(api);
    }

    private void AutomapBerryBushes()
    {
        var berryBlocks = _api.World.Blocks.Where(block =>
            block.Class == "BlockBerryBush" &&
            block.BlockBehaviors.All(behavior => behavior is not XSkillsBerryBushBehavior) &&
            block.Code.Path.Contains("ripe"));

        foreach (var block in berryBlocks)
            AddBehaviorToBlock(block, new XSkillsBerryBushBehavior(block), "{\"xp\": 0.33}");
    }

    private void AutomapBombs()
    {
        var bombBlocks = _api.World.Blocks.Where(block =>
            block.Class == "BlockBomb" && block.EntityClass == "Bomb" &&
            block.BlockBehaviors.All(behavior => behavior is not XSkillsBombBehavior));

        foreach (var block in bombBlocks) AddBehaviorToBlock(block, new XSkillsBombBehavior(block), "{}");
    }

    private void AutomapGems()
    {
        // TODO: How to separate gems from ores?
    }

    private void AutomapTreeCuttings()
    {
        // TODO: Figure out how tree cuttings work
    }

    private void AutomapWoodBlocks()
    {
        var logBlocks = _api.World.Blocks.Where(block =>
            block.Class == nameof(BlockLog) && block.Code.PathStartsWith("log-grown") &&
            block.BlockBehaviors.All(behavior => behavior is not XSkillsWoodBehavior));

        foreach (var block in logBlocks) AddBehaviorToBlock(block, new XSkillsLeavesBehavior(block), "{\"xp\": 0.4}");

        var logSectionBlocks = _api.World.Blocks.Where(block =>
            block.Class == nameof(BlockLogSection) && block.Code.PathStartsWith("logsection-grown") &&
            block.BlockBehaviors.All(behavior => behavior is not XSkillsWoodBehavior));

        foreach (var block in logSectionBlocks)
            AddBehaviorToBlock(block, new XSkillsWoodBehavior(block), "{\"xp\": 0.25}");
    }

    private void AutomapSaplings()
    {
        var saplingBlocks = _api.World.Blocks.Where(block =>
            block.EntityClass == "Sapling" && block.Code.PathStartsWith("sapling") &&
            block.BlockBehaviors.All(behavior => behavior is not XSkillsSaplingBehavior));

        foreach (var block in saplingBlocks) AddBehaviorToBlock(block, new XSkillsSaplingBehavior(block), "{}");
    }

    private void AutomapLeaves()
    {
        // TODO: doesnt give xp for some reason even though the behavior is there

        // BlockLeavesDropCanes is a patch class by Wildcraft

        // normal and narrow leaves 0.01xp
        var regularLeavesBlocks = _api.World.Blocks.Where(block =>
            block.Class is "BlockLeavesDropCanes" or "BlockLeaves" or "BlockLeavesNarrow" &&
            block.Code.PathStartsWith("leaves") &&
            block.BlockBehaviors.All(behavior => behavior is not XSkillsLeavesBehavior));
        foreach (var block in regularLeavesBlocks)
            AddBehaviorToBlock(block, new XSkillsLeavesBehavior(block), "{\"xp\": 0.01}");

        // branchy leaves 0.02xp
        var branchyBlocks = _api.World.Blocks.Where(block =>
            block.Class is "BlockLeavesDropCanes" or "BlockLeaves" &&
            block.Code.PathStartsWith("branchyleaves") &&
            block.BlockBehaviors.All(behavior => behavior is not XSkillsLeavesBehavior));

        foreach (var block in branchyBlocks)
            AddBehaviorToBlock(block, new XSkillsLeavesBehavior(block), "{\"xp\": 0.02}");
    }

    private void AutomapOres()
    {
        var oreBlocks = _api.World.Blocks.Where(block =>
            block.Class == "BlockOre" && block.Code.PathStartsWith("ore"));

        // TODO: verify this works
        var properties = """
                                 {
                                   "xpByType": {
                                     "*-poor-*": "0.5",
                                     "*-medium-*": "0.7",
                                     "*-rich-*": "0.9",
                                     "*-bountiful-*": "1.0",
                                     "*": "0.3"
                                   }
                                 }
                         """;

        foreach (var block in oreBlocks)
            AddBehaviorToBlock(block, new XSkillsOreBehavior(block), properties);
    }

    private void AddBehaviorToBlock(Block block, BlockBehavior behavior, string properties)
    {
        behavior.Initialize(JsonObject.FromJson(properties));

        block.BlockBehaviors = block.BlockBehaviors.Append(behavior).ToArray();
        _api.Logger.Debug($"Added {behavior.GetType().Name} to {block.Code.Path}");
    }
}