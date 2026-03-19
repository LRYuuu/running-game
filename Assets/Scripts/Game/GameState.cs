namespace SquareFireline.Game
{
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        /// <summary>等待开始（主菜单状态）</summary>
        Waiting,
        /// <summary>游戏中（正常运行）</summary>
        Playing,
        /// <summary>死亡动画中（不可操作）</summary>
        Dying,
        /// <summary>重生中（重置位置）</summary>
        Respawning
    }
}
