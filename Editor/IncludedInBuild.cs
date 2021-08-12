namespace UniModules.Tools.Editor
{
    public enum IncludedInBuild
    {
        Unknown       = 0,
        NotIncludable = 1,
        NotIncluded   = 2,
        SceneInBuild  = 10,
        RuntimeScript = 11,
        ResourceAsset = 12,
        Referenced    = 13
    }
}