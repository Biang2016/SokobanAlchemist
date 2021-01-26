public enum CastAreaType
{
    /// <summary>
    /// R=1单点；R=2十字形；……
    /// 要求中心点在施法范围内
    /// </summary>
    CircleCast,

    /// <summary>
    /// 要求整个效果矩形范围在施法范围内
    /// </summary>
    RectCast,

    /// <summary>
    /// 角色面前的带状区域，沿角色Forward方向为深度，垂直方向为宽度
    /// 无施法范围要求
    /// </summary>
    FrontRectCast,

    /// <summary>
    /// 角色面前的扇状区域，R=1为面前单点，R=2为面前第一排1格+第二排3格；……
    /// 无施法范围要求
    /// </summary>
    FrontFanCast,

    /// <summary>
    /// 角色面前的三角形区域，R=1为面前单点，R=2为面前第一排3格+第二排1格；……
    /// 无施法范围要求
    /// </summary>
    FrontTriangleCast
}