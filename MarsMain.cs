using MagmaDataMiner;
using System.Runtime.InteropServices;

namespace MarsExplorer
{
    public partial class MarsMain : Form
    {
        public MarsMain()
        {
            InitializeComponent();

            MarsModel model = new();

            List<(string ActionName, MinedGraph Graph)>? abilityGraphs = null;

            using var _ = MineDb.ActivateLanguage("en-US");

            var charRoot = toc.Nodes.Add("Aspects");
            var vestigesRoot = toc.Nodes.Add("Vestiges");

            foreach (var charClass in MineDb.CharacterClasses)
            {
                var charNode = charRoot.Nodes.Add(charClass["className"].Translated());
                charNode.Expand();

                foreach (var abilityData in charClass.EnumerateAssetLinks("starterAbilities"))
                {
                    var name = abilityData["abilityName"].Translated();
                    var abilityNode = charNode.Nodes.Add(name);
                    abilityGraphs = AresSimulator.ParseAbilityGraph(abilityData);

                    foreach (var (actionName, graph) in abilityGraphs)
                    {
                        var graphNode = abilityNode.Nodes.Add(Path.GetFileName(actionName));
                        graphNode.Tag = graph;
                    }
                }
            }
            charRoot.Expand();

            foreach (var vestige in MineDb.AssetsByType("EquipmentData").OrderBy(x => x["rarity"].Value).ThenBy(x => x["equipmentName"].String))
            {

                var eType = vestige["equipmentType"].Value.ToString();

                if (eType != "Accessory")
                {
                    continue;
                }

                var vestigeNode = vestigesRoot.Nodes.Add(vestige["equipmentName"].Translated());

                foreach (var statusEntry in vestige["statusEffectEntries"]["statusEffectEntries"].Enumerate())
                {
                    var effect = statusEntry.Deref("statusEffect");

                    foreach (var proc in effect["procEffects"].Enumerate())
                    {
                        foreach (var actionData in proc["actionDataList"].EnumerateAssetLinks())
                        {
                            var graphData = actionData.Deref("actionGraph");
                            var graph = AresParser.ParseGraph(graphData);
                            var name = $"{proc["procType"].Value} -> {Path.GetFileName(graph.Name)}";
                            graph.Name = name;
                            vestigeNode.Nodes.Add(name).Tag = graph;
                        }
                    }

                }

                //foreach (var abilityData in charClass.EnumerateAssetLinks("starterAbilities"))
                //{
                //    var name = abilityData["abilityName"].Translated();
                //    var abilityNode = charNode.Nodes.Add(name);
                //    abilityGraphs = AresSimulator.ParseAbilityGraph(abilityData);

                //    foreach (var (actionName, graph) in abilityGraphs)
                //    {
                //        var graphNode = abilityNode.Nodes.Add(Path.GetFileName(actionName));
                //        graphNode.Tag = graph;
                //    }
                //}


            }

            toc.AfterSelect += (sender, args) =>
            {
                if (args?.Node?.Tag is MinedGraph graph)
                {
                    model.Reset(graph);
                    selectedProps.Rows.Clear();
                    marsCanvas1.Invalidate();

                    Text = graph.Name;
                }
            };


            marsCanvas1.OnNodeSelected += node =>
            {
                if (model.Selected == node)
                {
                    return;
                }

                if (model.Selected != null)
                {
                    model.Selected.Highlight = false;
                }

                model.Selected = node;
                node.Highlight = true;

                selectedProps.Rows.Clear();

                foreach (var (k, v) in node.Props)
                {
                    selectedProps.Rows.Add(k, v);
                }

                marsCanvas1.Invalidate();
            };


            marsCanvas1.Model = model;

        }
    }
}