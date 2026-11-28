// Runs isolated Editor regression checks for compass notice.
#if UNITY_EDITOR
// Preserve the existing automation entry point. The retired frozen-arrow popup is now
// covered by the permanent-guidance and legacy-save checks in the Winter suite.
public static class CompassNoticeChecks
{
    public static void Run()=>WinterRefinementChecks.Run();
}
#endif
