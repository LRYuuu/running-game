using UnityEngine;
using System.Collections.Generic;

namespace RunnersJourney.Map
{
    /// <summary>
    /// 生物群系序列配置（ScriptableObject）
    /// 用于配置混合模式下随距离自动切换的群系序列
    /// </summary>
    [CreateAssetMenu(fileName = "NewBiomeSequence", menuName = "Runner's Journey/Biome Sequence")]
    public class BiomeSequence : ScriptableObject
    {
        [Header("基本信息")]
        [Tooltip("序列名称")]
        public string sequenceName = "FullProgression";

        [Header("群系阶段")]
        [Tooltip("群系阶段列表（按距离排序）")]
        public List<BiomeStage> biomeStages = new List<BiomeStage>();

        [Header("模式开关")]
        [Tooltip("是否启用序列模式，false 则使用固定单一群系")]
        public bool useSequence = true;
    }

    /// <summary>
    /// 群系阶段配置
    /// </summary>
    [System.Serializable]
    public class BiomeStage
    {
        [Tooltip("该阶段使用的群系配置")]
        public BiomeConfig biome;

        [Tooltip("该阶段持续的 Chunk 数量")]
        public int transitionChunks = 10;
    }
}
